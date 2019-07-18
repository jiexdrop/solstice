using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject projectilePrefab;

    private int nbOfPlayers;
    private int playerId;
    private bool setPlayerId = false;
    private GameObject [] players = new GameObject[4];
    private Player player;
    private Vector2 [] startPlayersPositions = new Vector2[4];
    private Vector2 [] endPlayersPositions = new Vector2[4];
    private float [] startPlayersRotations = new float[4];
    private float [] endPlayersRotations = new float[4];
    private float [] timesStartedLerping = new float[4];

    private List<GameObject> projectiles = new List<GameObject>();

    [Header("Controls")]
    public VirtualJoystick joystick;
    public Button shootButton;
    private Vector3 speed = new Vector3();

    // Receive movement of server
    private Vector3 startServerPos;
    private Vector3 endServerPos;
    // Send movement of client
    MovementMessage clientMovement = new MovementMessage();

    private float elapsed;

    UDPClient c = new UDPClient();

    private GameState state = GameState.STOP;

    [Header("Lobby")]
    public PlayerPanel[] playerPanels;
    public Button playButton;
    public PanelsManager panelsManager;

    void Start()
    {
        if (GameManager.Instance.type.Equals(ConnectionType.CLIENT))
        {
            //Debug.LogError("Client connected with ip: " + GameManager.Instance.IP);
            c.Client(GameManager.Instance.IP);

            shootButton.onClick.AddListener(ClientShoot);

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

                speed = joystick.InputVector * Time.deltaTime * 5;
                if (speed.magnitude > 0)
                {
                    player.transform.position += speed;
                    player.SetRotation(joystick.InputVector);
                }

                elapsed += Time.deltaTime;

                if (elapsed >= GameManager.FREQUENCY)
                {
                    elapsed = elapsed % GameManager.FREQUENCY;
                    SendPosition(player.gameObject);
                }

                // Lerp server shared movements
                for (int i = 0; i < nbOfPlayers; i++)
                {
                    GameObject player = players[i];
                    Vector2 startServerPos = startPlayersPositions[i];
                    Vector2 endServerPos = endPlayersPositions[i];
                    float startServerRot = startPlayersRotations[i];
                    float endServerRot = endPlayersRotations[i];
                    float timeStartedLerping = timesStartedLerping[i];

                    if (i != playerId)
                    {
                        float lerpPercentage = (Time.time - timeStartedLerping) / GameManager.FREQUENCY;
                        // Position
                        player.transform.position = Vector3.Lerp(startServerPos, endServerPos, lerpPercentage);
                        // Rotation of the visor
                        float lerpedRotation = Mathf.LerpAngle(startServerRot, endServerRot, lerpPercentage);
                        players[i].GetComponent<Player>().SetRotation(lerpedRotation);
                    }
                }

                break;
        }


        // Client read received messages
        Debug.Log(c.received.type);
        switch (c.received.type)
        {
            case MessageType.NONE:
                break;
            case MessageType.SERVER_START_GAME:
                panelsManager.ShowGamePanel();
                state = GameState.GAME;
                break;
            case MessageType.SERVER_SHARE_PLAYERS:
                {
                    ServerSharePlayersMessage sharePlayersMessage = (ServerSharePlayersMessage)c.received;

                    // The first message sets the player number 
                    if (!setPlayerId)
                    {
                        playerId = sharePlayersMessage.playerId;
                        setPlayerId = true;
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

                    player = players[playerId].GetComponent<Player>();
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
                            timesStartedLerping[i] = Time.time;
                        }

                    }

                }
                break;
            case MessageType.SERVER_SHARE_SHOOT:
                {
                    ServerShareShootMessage ssm = (ServerShareShootMessage)c.received;
                    if (ssm.playerId != playerId)
                    {
                        ServerShoot(ssm.playerId);
                    }
                }
                break;
        }
        c.received.OnRead();

    }

    public void ClientShoot()
    {
        GameObject p = Instantiate(projectilePrefab, player.visor.transform.position, Quaternion.identity);
        Projectile projectile = p.GetComponent<Projectile>();
        projectile.duration = GameManager.SHOOT_DURATION;
        projectile.transform.rotation = player.center.transform.rotation;
        projectiles.Add(p);

        ShootMessage shoot = new ShootMessage();
        shoot.playerId = playerId;
        shoot.duration = GameManager.SHOOT_DURATION;
        c.ClientSend(shoot);
    }

    public void ServerShoot(int playerId)
    {
        GameObject p = Instantiate(projectilePrefab, players[playerId].GetComponent<Player>().visor.transform.position, Quaternion.identity);
        Projectile projectile = p.GetComponent<Projectile>();
        projectile.duration = GameManager.SHOOT_DURATION;
        projectile.transform.rotation = players[playerId].GetComponent<Player>().center.transform.rotation;
        projectiles.Add(p);
    }

    public void SendPosition(GameObject player)
    {
        //Debug.LogError("Sending position !");
        clientMovement.type = MessageType.MOVEMENT;
        clientMovement.x = player.transform.position.x;
        clientMovement.y = player.transform.position.y;
        clientMovement.visorRotation = player.GetComponent<Player>().visorRotation;
        clientMovement.playerId = playerId;

        c.ClientSend(clientMovement);
    }
}
