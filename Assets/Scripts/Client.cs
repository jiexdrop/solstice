using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Client : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject projectilePrefab;

    public int nbOfPlayers;
    public int playerId;
    private bool setPlayerId = false;
    public GameObject[] players = new GameObject[4];
    public Player player;

    private Vector2[] startPlayersPositions = new Vector2[4];
    private Vector2[] endPlayersPositions = new Vector2[4];
    private float[] startPlayersRotations = new float[4];
    private float[] endPlayersRotations = new float[4];
    private float[] timesStartedLerping = new float[4];

    private Vector2[] startMonstersPositions;
    private Vector2[] endMonstersPositions;
    private float[] monstersTimesStartedLerping;

    private List<GameObject> projectiles = new List<GameObject>();

    [Header("Player UI and Controls")]
    public VirtualJoystick joystick;
    public ShootButton shootButton;
    public Slider healthBar;
    private Vector2 speed = new Vector2();

    // Receive movement of server
    private Vector3 startServerPos;
    private Vector3 endServerPos;
    // Send movement of client
    MovementMessage clientMovement = new MovementMessage();

    private float movementElapsed;
    private float shootingElapsed;
    private bool sharingMovements;

    UDPClient c = new UDPClient();

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
        if (GameManager.Instance.type.Equals(ConnectionType.CLIENT))
        {
            //Debug.LogError("Client connected with ip: " + GameManager.Instance.IP);
            c.Client(GameManager.Instance.IP);

            shootButton.onDown.AddListener(StartShooting);
            shootButton.onUp.AddListener(StopShooting);

            playButton.gameObject.SetActive(false);

            panelsManager.ShowLobbyPanel();

        }
        else
        {
            Destroy(this.gameObject);
            Destroy(this);
        }
    }

    void Update()
    {
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

                // Debug.LogError("Send Start Server from client");
                ClientNewPlayerMessage newPlayerMessage = new ClientNewPlayerMessage();
                if (c.Connected())
                {
                    c.ClientSend(newPlayerMessage);
                    state = GameState.START;
                }
                break;
            case GameState.START:



                break;
            case GameState.GAME:

                speed = joystick.InputVector * Time.deltaTime * 12;
                player.animator.SetBool("Walking", false);
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
                if (player.shooting)
                {
                    player.AnimateShooting(shootingElapsed);
                    if (shootingElapsed >= player.frequency)
                    {
                        shootingElapsed = shootingElapsed % player.frequency;
                        ClientShoot(playerId);
                    }
                }

                // Movement
                if (movementElapsed >= GameManager.FREQUENCY)
                {
                    movementElapsed = movementElapsed % GameManager.FREQUENCY;
                    dungeonGeneration.HighlightRoom(player);
                    if (sharingMovements)
                    {
                        SendPosition(player.gameObject);
                    }
                    else
                    {
                        sharingMovements = true;
                    }
                }


                // Lerp server shared movements
                for (int i = 0; i < nbOfPlayers; i++)
                {
                    Player playerIndex = players[i].GetComponent<Player>();
                    Vector2 startServerPos = startPlayersPositions[i];
                    Vector2 endServerPos = endPlayersPositions[i];
                    float startServerRot = startPlayersRotations[i];
                    float endServerRot = endPlayersRotations[i];
                    float timeStartedLerping = timesStartedLerping[i];

                    if (i != playerId)
                    {
                        float lerpPercentage = (Time.time - timeStartedLerping) / GameManager.FREQUENCY;
                        // Position
                        playerIndex.transform.position = Vector3.Lerp(startServerPos, endServerPos, lerpPercentage);
                        // Rotation of the visor
                        float lerpedRotation = Mathf.LerpAngle(startServerRot, endServerRot, lerpPercentage);
                        playerIndex.SetRotation(lerpedRotation);

                        // If we havent moved then we are not walking
                        if (Vector2.Distance(startServerPos, endServerPos) < 0.1f)
                        {
                            playerIndex.animator.SetBool("Walking", false);
                        }
                        else
                        {
                            playerIndex.animator.SetBool("Walking", true);
                        }

                        // Shooting
                        if (playerIndex.shooting)
                        {
                            playerIndex.AnimateShooting(shootingElapsed);
                            if (shootingElapsed >= playerIndex.frequency)
                            {
                                shootingElapsed = shootingElapsed % playerIndex.frequency;
                                ClientShoot(i);
                            }
                        } else
                        {
                            playerIndex.StopShooting();
                        }
                    }
                }

                // Lerp server shared monsters movement
                if (monstersTimesStartedLerping != null && monstersTimesStartedLerping.Length > 0)
                {
                    for (int i = 0; i < startMonstersPositions.Length; i++)
                    {
                        //Debug.Log("What do I have inside monsters ? " + spawner.monsters.transform.GetChild(i).gameObject.name);
                        if (spawner.monsters[i] != null)
                        {
                            GameObject monster = spawner.monsters[i].gameObject;
                            Vector2 startServerPos = startMonstersPositions[i];
                            Vector2 endServerPos = endMonstersPositions[i];
                            float timeStartedLerping = monstersTimesStartedLerping[i];

                            float lerpPercentage = (Time.time - timeStartedLerping) / GameManager.FREQUENCY;
                            //Position
                            monster.transform.position = Vector3.Lerp(startServerPos, endServerPos, lerpPercentage);
                        }
                    }
                }

                break;
        }


        // Client read received messages
        //Debug.Log(c.received.type);
        switch (c.received.type)
        {
            case MessageType.NONE:
                break;
            case MessageType.SERVER_START_GAME:
                {
                    panelsManager.ShowGamePanel();

                    // Define player and camera only once at the start of the game
                    player = players[playerId].GetComponent<Player>();

                    player.client = this;

                    // Set camera as a child of the player
                    Camera.main.transform.parent = player.transform;

                    player.isPlayed = true;

                    // Set the healthbar of the client player
                    player.healthBar = healthBar;
                    player.healthBar.value = player.health;

                    state = GameState.GAME;
                }
                break;
            case MessageType.SERVER_SHARE_PLAYERS:
                {
                    ServerSharePlayersMessage sharePlayersMessage = (ServerSharePlayersMessage)c.received;

                    // The first message sets the player number 
                    if (!setPlayerId)
                    {
                        playerId = sharePlayersMessage.playerId;
                        setPlayerId = true;
                        seed = sharePlayersMessage.seed;
                        dungeonGeneration.SetClient(this);
                        dungeonGeneration.Generate(seed);
                        spawner.SetClient(this);
                        spawner.ClearMonsters();
                        spawner.rooms = dungeonGeneration.rooms;
                    }


                    // Destroy old players
                    for (int i = 0; i < nbOfPlayers; i++)
                    {
                        Destroy(players[i]);
                        players[i] = null;
                    }

                    // Add new players
                    nbOfPlayers = sharePlayersMessage.x.Length;

                    for (int i = 0; i < nbOfPlayers; i++)
                    {
                        players[i] = Instantiate(playerPrefab, new Vector2(sharePlayersMessage.x[i], sharePlayersMessage.y[i]), Quaternion.identity);
                        // Lobby
                        playerPanels[i].SetActivePlayer(true);
                    }

                }
                break;
            case MessageType.SERVER_SHARE_MOVEMENT:
                {
                    ServerShareMovementMessage shareMovementsMessage = (ServerShareMovementMessage)c.received;

                    for (int i = 0; i < shareMovementsMessage.x.Length; i++)
                    {
                        if (i != playerId)
                        {
                            startPlayersPositions[i] = players[i].transform.position;
                            startPlayersRotations[i] = players[i].GetComponent<Player>().visorRotation;
                            endPlayersRotations[i] = shareMovementsMessage.visorRotation[i];
                            endPlayersPositions[i] = new Vector2(shareMovementsMessage.x[i], shareMovementsMessage.y[i]);
                            players[i].GetComponent<Player>().shooting = shareMovementsMessage.shooting[i];
                            timesStartedLerping[i] = Time.time;
                        }
                    }

                    startMonstersPositions = new Vector2[GameManager.MAX_MONSTERS];
                    endMonstersPositions = new Vector2[GameManager.MAX_MONSTERS];
                    monstersTimesStartedLerping = new float[GameManager.MAX_MONSTERS];

                    for (int i = 0; i < GameManager.MAX_MONSTERS; i++)
                    {
                        // If the monster exists set health and position from server
                        if (spawner.monsters[i] != null)
                        {
                            startMonstersPositions[i] = spawner.monsters[i].transform.position;
                            endMonstersPositions[i] = new Vector3(shareMovementsMessage.mx[i], shareMovementsMessage.my[i]);
                            monstersTimesStartedLerping[i] = Time.time;
                        }
                    }

                }
                break;
            case MessageType.SERVER_GO_TO_NEXT_ROOM:
                {
                    //Debug.Log("Server go to next room");
                    ServerGoToNextRoomMessage ssm = (ServerGoToNextRoomMessage)c.received;
                    for (int i = 0; i < ssm.x.Length; i++)
                    {
                        players[i].transform.position = new Vector3(ssm.x[i], ssm.y[i], 0);
                    }

                    spawner.ClearMonsters();
                    dungeonGeneration.Clear();
                    dungeonGeneration.Generate(ssm.seed);

                }
                break;
            case MessageType.SERVER_SHARE_MONSTERS_SPAWN:
                {
                    ServerShareMonstersSpawnMessage ssmsm = (ServerShareMonstersSpawnMessage)c.received;
                    // If I'm not the one that has sent the request to spawn monsters
                    if (ssmsm.playerId != playerId)
                    {
                        //spawner.SpawnMonsters(ssmsm.roomId, ssmsm.seed);
                        if (ssmsm.teleport)
                        {
                            Player player = players[ssmsm.playerId].GetComponent<Player>();
                            Vector2 teleportPosition = dungeonGeneration.GetPositionByPlayerDirection(playerId, ssmsm.roomId, player.GetVisorDirection());
                            players[playerId].transform.position = teleportPosition;
                        }
                    }
                }
                break;
            case MessageType.SERVER_DIE:
                {
                    ServerDieMessage sdm = (ServerDieMessage)c.received;

                    if (sdm.playerId != playerId)
                    {
                        players[sdm.playerId].GetComponent<Player>().SetDied();
                    }
                }
                break;
        }
        c.received.OnRead();

    }

    public void StartShooting()
    {
        shootingElapsed = 0;
        if (!player.died)
        {
            player.shooting = true;
        }
    }

    public void ClientShoot(int id)
    {
        switch (players[id].GetComponent<Player>().type)
        {
            case Pickable.Type.KATANA:
                break;
            default:
                GameObject p = Instantiate(projectilePrefab, players[id].GetComponent<Player>().visor.transform.position, Quaternion.identity);
                Projectile projectile = p.GetComponent<Projectile>();
                projectile.duration = GameManager.SHOOT_DURATION;
                projectile.transform.rotation = players[id].GetComponent<Player>().center.transform.rotation;
                projectiles.Add(p);
                break;
        }
    }

    public void StopShooting()
    {
        player.StopShooting();
        player.shooting = false;
    }


    public void ServerShoot(int playerId)
    {
        switch (players[playerId].GetComponent<Player>().type)
        {
            case Pickable.Type.KATANA:
                break;
            default:
                GameObject p = Instantiate(projectilePrefab, players[playerId].GetComponent<Player>().visor.transform.position, Quaternion.identity);
                Projectile projectile = p.GetComponent<Projectile>();
                projectile.duration = GameManager.SHOOT_DURATION;
                projectile.transform.rotation = players[playerId].GetComponent<Player>().center.transform.rotation;
                projectiles.Add(p);
                break;
        }
    }

    public void SendPosition(GameObject player)
    {
        //Debug.LogError("Sending position !");
        clientMovement.x = player.transform.position.x;
        clientMovement.y = player.transform.position.y;
        clientMovement.visorRotation = player.GetComponent<Player>().visorRotation;
        clientMovement.playerId = playerId;
        clientMovement.shooting = player.GetComponent<Player>().shooting;

        c.ClientSend(clientMovement);
    }

    public void GoToNextRoom()
    {
        seed = Random.Range(0, Int32.MaxValue);
        dungeonGeneration.Clear();
        dungeonGeneration.Generate(seed);

        ClientGoToNextRoomMessage goToNextRoomMessage = new ClientGoToNextRoomMessage();

        goToNextRoomMessage.seed = seed;

        sharingMovements = false;

        c.ClientSend(goToNextRoomMessage);
    }

    /// <summary>
    /// Send to the server a call to spawn monsters
    /// </summary>
    /// <param name="roomId"></param>
    /// <param name="seed"></param>
    /// <param name="teleport"></param>
    public void SpawnMonsters(int roomId, int seed, bool teleport)
    {
        ClientShareMonstersSpawnMessage shareMonstersSpawnMessage = new ClientShareMonstersSpawnMessage();

        shareMonstersSpawnMessage.roomId = roomId; // the room where the monsters are spawned
        shareMonstersSpawnMessage.playerId = playerId; // the player who spawns the monsters
        shareMonstersSpawnMessage.seed = seed;
        shareMonstersSpawnMessage.teleport = teleport; // do I need to teleport the players to me

        sharingMovements = false;

        c.ClientSend(shareMonstersSpawnMessage);
    }

    public void ShareDeath(int playerId)
    {
        ClientDieMessage dieMessage = new ClientDieMessage();

        dieMessage.playerId = playerId;

        sharingMovements = false;

        c.ClientSend(dieMessage);
    }

    public void OnDestroy()
    {
        c.Close();
    }


}
