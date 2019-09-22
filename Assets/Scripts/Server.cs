using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Server : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject projectilePrefab;

    public int nbOfPlayers;
    public GameObject[] players = new GameObject[4];
    public Player player;

    // Receive movement of client
    private Vector2[] startPlayersPositions = new Vector2[4];
    private Vector2[] endPlayersPositions = new Vector2[4];
    private float[] startPlayersRotations = new float[4];
    private float[] endPlayersRotations = new float[4];
    private float[] timesStartedLerping = new float[4];
    private int lastClientMovement;

    private List<GameObject> projectiles = new List<GameObject>();

    [Header("Player UI and Controls")]
    public VirtualJoystick joystick;
    public ShootButton shootButton;
    public Slider healthBar;
    private Vector2 speed = new Vector2();

    // Send movement of server
    MovementMessage serverMovement = new MovementMessage();

    private float movementElapsed;
    private float shootingElapsed;
    private bool sharingMovements;
    private bool shooting;

    // This server shares movement data
    UDPServer s = new UDPServer();
    // This server is used for network ip address/port scan
    TCPServer os = new TCPServer();

    private GameState state = GameState.STOP;

    [Header("Lobby")]
    public PlayerPanel[] playerPanels;
    public Button playButton;
    public PanelsManager panelsManager;

    [Header("Generation")]
    public DungeonGeneration dungeonGeneration;
    public int seed;

    [Header("Spawner")]
    public Spawner spawner;

    void Start()
    {
        if (GameManager.Instance.type.Equals(ConnectionType.SERVER))
        {
            s.Server(GameManager.Instance.IP);
            os.Server(GameManager.Instance.IP);

            AddPlayer();

            shootButton.onDown.AddListener(StartShooting);
            shootButton.onUp.AddListener(StopShooting);

            playButton.onClick.AddListener(StartGame);

            panelsManager.ShowLobbyPanel();

            seed = Random.Range(0, Int32.MaxValue);
            dungeonGeneration.SetServer(this);
            spawner.SetServer(this);
            dungeonGeneration.Generate(seed);
            spawner.rooms = dungeonGeneration.rooms;
        }
        else
        {
            Destroy(this.gameObject);
            Destroy(this);
        }
    }

    private void StartGame()
    {
        panelsManager.ShowGamePanel();
        state = GameState.GAME;
        ServerStartGameMessage ssgm = new ServerStartGameMessage();
        os.Close();
        s.ServerSend(ssgm);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            // Testing
            seed = Random.Range(0, Int32.MaxValue);
            dungeonGeneration.Clear();
            spawner.SetServer(this);
            spawner.ClearMonsters();
            dungeonGeneration.Generate(seed);
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            //Testing
            for (int i = 0; i < spawner.monsters.Length; i++)
            {
                if (spawner.monsters[i] != null)
                {
                    spawner.monsters[i].GetComponent<Monster>().health = 0;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            panelsManager.ShowMenuPanel();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartShooting();
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            StopShooting();
        }

        switch (state)
        {
            case GameState.STOP:

                break;
            case GameState.START:

                break;
            case GameState.GAME:

                speed = joystick.InputVector * Time.deltaTime * 12;
                player.animator.SetBool("Walking", false);

                // Get server player
                if (speed.magnitude > 0 && !player.died)
                {
                    //player.transform.position += speed;
                    Rigidbody2D playerRb2D = player.GetComponent<Rigidbody2D>();
                    playerRb2D.MovePosition(playerRb2D.position + speed);
                    player.SetRotation(joystick.InputVector);
                    player.animator.SetBool("Walking", true);
                }

                movementElapsed += Time.deltaTime;
                shootingElapsed += Time.deltaTime;

                // Shooting
                if (!player.died && shooting)
                {
                    player.AnimateShooting(shootingElapsed);
                    if (shootingElapsed >= player.frequency)
                    {
                        shootingElapsed = shootingElapsed % player.frequency;
                        ServerShoot();
                    }
                }

                // Movement
                if (movementElapsed >= GameManager.FREQUENCY)
                {
                    movementElapsed = movementElapsed % GameManager.FREQUENCY;
                    dungeonGeneration.HighlightRoom(player);

                    if (sharingMovements)
                    {
                        ShareMovements();
                    }
                    else
                    {
                        sharingMovements = true;
                    }
                }


                // Server movement Lerp 
                for (int i = 0; i < nbOfPlayers; i++)
                {
                    GameObject player = players[i];
                    Vector2 startClientPos = startPlayersPositions[i];
                    Vector2 endClientPos = endPlayersPositions[i];
                    float startClientRot = startPlayersRotations[i];
                    float endClientRot = endPlayersRotations[i];
                    float timeStartedLerping = timesStartedLerping[i];

                    if (i != 0)
                    {
                        float lerpPercentage = (Time.time - timeStartedLerping) / GameManager.FREQUENCY;
                        //Debug.Log(string.Format("lerpPercent[{0}] = (time[{1}] - tS[{2}]) / tTRG[{3}]", lerpPercentage, Time.time, timeStartedLerping, GameManager.FREQUENCY));
                        players[i].transform.position = Vector3.Lerp(startClientPos, endClientPos, lerpPercentage);
                        float lerpedRotation = Mathf.LerpAngle(startClientRot, endClientRot, lerpPercentage);
                        players[i].GetComponent<Player>().SetRotation(lerpedRotation);

                        // If we havent moved then we are not walking
                        if (Vector2.Distance(startClientPos, endClientPos) < 0.1f)
                        {
                            players[i].GetComponent<Player>().animator.SetBool("Walking", false);
                        }
                        else
                        {
                            players[i].GetComponent<Player>().animator.SetBool("Walking", true);
                        }
                    }
                }

                break;
        }

        // Server read received messages
        switch (s.received.type)
        {
            case MessageType.NONE:
                {

                }
                break;
            case MessageType.CLIENT_NEW_PLAYER:
                {
                    Debug.LogError("Received Start Server from client");
                    AddPlayer();
                    SharePlayers();
                }
                break;
            case MessageType.MOVEMENT:
                {
                    // Received movement from client
                    MovementMessage mm = (MovementMessage)s.received;
                    //Debug.Log("Movement from " + mm.playerId);
                    lastClientMovement = mm.playerId;
                    startPlayersPositions[lastClientMovement] = players[lastClientMovement].transform.position;
                    startPlayersRotations[lastClientMovement] = players[lastClientMovement].GetComponent<Player>().visorRotation;
                    endPlayersRotations[lastClientMovement] = mm.visorRotation;
                    endPlayersPositions[lastClientMovement] = new Vector3(mm.x, mm.y);

                    timesStartedLerping[lastClientMovement] = Time.time;
                }
                break;
            case MessageType.SHOOT:
                {
                    ShootMessage sm = (ShootMessage)s.received;
                    ClientShoot(sm.playerId);
                    ShareShoots(sm.playerId);
                }
                break;
            case MessageType.CLIENT_GO_TO_NEXT_ROOM:
                {
                    ClientGoToNextRoomMessage goToNextRoomMessage = (ClientGoToNextRoomMessage)s.received;
                    GoToNextRoom(goToNextRoomMessage.seed);
                }
                break;
            case MessageType.CLIENT_SHARE_MONSTERS_SPAWN:
                {
                    ClientShareMonstersSpawnMessage shareMonstersSpawnMessage = (ClientShareMonstersSpawnMessage)s.received;
                    spawner.SpawnMonsters(shareMonstersSpawnMessage.roomId, shareMonstersSpawnMessage.seed);
                    // Teleportation
                    if (shareMonstersSpawnMessage.teleport)
                    {
                        Player player = players[shareMonstersSpawnMessage.playerId].GetComponent<Player>();
                        Vector2 teleportPosition = dungeonGeneration.GetPositionByPlayerDirection(0, shareMonstersSpawnMessage.roomId, player.GetVisorDirection());
                        players[0].transform.position = teleportPosition;
                    }

                    ShareSpawnMonsters(shareMonstersSpawnMessage.roomId, shareMonstersSpawnMessage.playerId, shareMonstersSpawnMessage.seed, shareMonstersSpawnMessage.teleport);
                }
                break;
            case MessageType.CLIENT_DIE:
                {
                    ClientDieMessage clientDieMessage = (ClientDieMessage)s.received;

                    players[clientDieMessage.playerId].GetComponent<Player>().SetDied();

                    ShareDeath(clientDieMessage.playerId);
                }
                break;
        }
        s.received.OnRead();


    }

    public void AddPlayer()
    {
        Vector3 randomPosition = Random.insideUnitCircle;

        if (nbOfPlayers < 4)
        {
            players[nbOfPlayers] = Instantiate(playerPrefab, randomPosition, Quaternion.identity);
            // Lobby
            playerPanels[nbOfPlayers].SetActivePlayer(true);

            nbOfPlayers++;
        }

        if (player == null) // Set server player 
        {
            player = players[0].GetComponent<Player>();
            player.server = this;
            player.isPlayed = true;
            // Set the healthbar of the server player
            player.healthBar = healthBar;
            player.healthBar.value = player.health;
            // Set camera as a child of the player
            Camera.main.transform.parent = player.transform;
        }
    }

    public void SharePlayers()
    {
        ServerSharePlayersMessage newPlayerMessage = new ServerSharePlayersMessage();

        newPlayerMessage.x = new float[nbOfPlayers];
        newPlayerMessage.y = new float[nbOfPlayers];

        newPlayerMessage.playerId = nbOfPlayers - 1;

        newPlayerMessage.seed = seed;

        for (int i = 0; i < nbOfPlayers; i++)
        {
            if (players[i] != null)
            {
                newPlayerMessage.x[i] = players[i].transform.position.x;
                newPlayerMessage.y[i] = players[i].transform.position.y;
            }
        }

        s.ServerSend(newPlayerMessage);
    }

    public void ShareMovements()
    {
        ServerShareMovementMessage shareMovementsMessage = new ServerShareMovementMessage();

        // Player movements
        shareMovementsMessage.x = new float[nbOfPlayers];
        shareMovementsMessage.y = new float[nbOfPlayers];
        shareMovementsMessage.visorRotation = new float[nbOfPlayers];

        for (int i = 0; i < nbOfPlayers; i++)
        {
            shareMovementsMessage.x[i] = players[i].transform.position.x;
            shareMovementsMessage.y[i] = players[i].transform.position.y;
            shareMovementsMessage.visorRotation[i] = players[i].GetComponent<Player>().visorRotation;
        }

        // Monsters movements
        shareMovementsMessage.mx = new float[GameManager.MAX_MONSTERS];
        shareMovementsMessage.my = new float[GameManager.MAX_MONSTERS];
        shareMovementsMessage.health = new int[GameManager.MAX_MONSTERS];
        if (spawner.monsters != null)
        {

            for (int i = 0; i < GameManager.MAX_MONSTERS; i++)
            {
                if (spawner.monsters[i] != null)
                {
                    shareMovementsMessage.mx[i] = spawner.monsters[i].transform.position.x;
                    shareMovementsMessage.my[i] = spawner.monsters[i].transform.position.y;
                    shareMovementsMessage.health[i] = spawner.monsters[i].health;
                }
            }
        }

        s.ServerSend(shareMovementsMessage);
    }

    public void StartShooting()
    {
        shootingElapsed = 0;
        shooting = true;
    }

    public void ServerShoot()
    {
        GameObject p = Instantiate(projectilePrefab, player.shootExit.transform.position, Quaternion.identity);
        Projectile projectile = p.GetComponent<Projectile>();
        projectile.duration = GameManager.SHOOT_DURATION;
        projectile.transform.rotation = player.center.transform.rotation;
        projectiles.Add(p);
        ShareShoots(0);
    }

    public void StopShooting()
    {
        player.StopShooting();
        shooting = false;
    }

    public void ClientShoot(int playerId)
    {
        GameObject p = Instantiate(projectilePrefab, players[playerId].GetComponent<Player>().visor.transform.position, Quaternion.identity);
        Projectile projectile = p.GetComponent<Projectile>();
        projectile.duration = GameManager.SHOOT_DURATION;
        projectile.transform.rotation = players[playerId].GetComponent<Player>().center.transform.rotation;
        projectiles.Add(p);
    }

    public void ShareShoots(int playerId)
    {
        ServerShareShootMessage shoot = new ServerShareShootMessage();
        shoot.playerId = playerId;
        s.ServerSend(shoot);
    }

    public void GoToNextRoom()
    {
        GoToNextRoom(Random.Range(0, Int32.MaxValue));
    }

    public void GoToNextRoom(int seed)
    {
        dungeonGeneration.Clear();
        spawner.ClearMonsters();
        dungeonGeneration.Generate(seed);

        ServerGoToNextRoomMessage goToNextRoomMessage = new ServerGoToNextRoomMessage();

        for (int i = 0; i < nbOfPlayers; i++)
        {
            players[i].transform.position = Random.insideUnitCircle;
        }

        goToNextRoomMessage.x = new float[nbOfPlayers];
        goToNextRoomMessage.y = new float[nbOfPlayers];

        for (int i = 0; i < nbOfPlayers; i++)
        {
            goToNextRoomMessage.x[i] = players[i].transform.position.x;
            goToNextRoomMessage.y[i] = players[i].transform.position.y;
        }

        goToNextRoomMessage.seed = seed;

        sharingMovements = false;

        s.ServerSend(goToNextRoomMessage);

    }

    public void ShareSpawnMonsters(int roomId, int playerId, int seed, bool teleport)
    {
        ServerShareMonstersSpawnMessage shareMonstersSpawnMessage = new ServerShareMonstersSpawnMessage();

        shareMonstersSpawnMessage.roomId = roomId; // the room where the monsters are spawned
        shareMonstersSpawnMessage.playerId = playerId; // the player who spawns the monsters
        shareMonstersSpawnMessage.seed = seed;
        shareMonstersSpawnMessage.teleport = teleport; // do I need to teleport the players to me

        sharingMovements = false;

        s.ServerSend(shareMonstersSpawnMessage);
    }

    public void ShareDeath(int playerId)
    {
        ServerDieMessage serverDieMessage = new ServerDieMessage();

        serverDieMessage.playerId = playerId;

        sharingMovements = false;

        s.ServerSend(serverDieMessage);
    }

    public void OnDestroy()
    {
        s.Close();
        os.Close();
    }
}
