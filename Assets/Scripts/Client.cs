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
    private int myNumber;
    private bool setNumber = false;
    private Dictionary<int, GameObject> players = new Dictionary<int, GameObject>();

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

    private float timeStartedLerping;
    private float elapsed;

    UDPClient c = new UDPClient();

    private GameState state = GameState.STOP;

    void Start()
    {
        if (GameManager.Instance.type.Equals(ConnectionType.CLIENT))
        {
            //Debug.LogError("Client connected with ip: " + GameManager.Instance.IP);
            c.Client(GameManager.Instance.IP);

            shootButton.onClick.AddListener(ClientShoot);
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
                GameObject player;
                players.TryGetValue(myNumber, out player);
                if (player != null)
                {
                    player.transform.position += speed;
                }

                elapsed += Time.deltaTime;

                if (elapsed >= GameManager.FREQUENCY)
                {
                    elapsed = elapsed % GameManager.FREQUENCY;
                    if (player != null)
                    {
                        SendPosition(player);
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
            case MessageType.SERVER_SHARE_PLAYERS:
                {
                    ServerSharePlayersMessage sharePlayersMessage = (ServerSharePlayersMessage)c.received;

                    // The firt message sets the player number 
                    if (!setNumber)
                    {
                        myNumber = sharePlayersMessage.playerNumber;
                        setNumber = true;
                    }

                    // Destroy old players
                    for (int i = 0; i < nbOfPlayers; i++)
                    {
                        GameObject player;
                        players.TryGetValue(i, out player);
                        if (player != null)
                        {
                            Destroy(player);
                        }
                    }
                    players.Clear();

                    // Add new players
                    nbOfPlayers = sharePlayersMessage.x.Length;

                    for (int i = 0; i < nbOfPlayers; i++)
                    {
                        players.Add(i, Instantiate(playerPrefab, new Vector2(sharePlayersMessage.x[i], sharePlayersMessage.y[i]), Quaternion.identity));
                    }

                    state = GameState.GAME;
                }
                break;
            case MessageType.MOVEMENT:
                {
                    //MovementMessage mm = (MovementMessage)c.received;
                    //startServerPos = serverPlayer.transform.position;
                    //endServerPos = new Vector3(mm.x, mm.y);
                    //timeStartedLerping = Time.time;
                }
                break;
            case MessageType.SERVER_SHARE_MOVEMENT:
                {
                    ServerShareMovementMessage shareMovementsMessage = (ServerShareMovementMessage)c.received;
                    for (int i = 0; i < shareMovementsMessage.x.Length; i++)
                    {
                        GameObject player;
                        players.TryGetValue(i, out player);

                        if (player != null && i != myNumber)
                        {
                            player.transform.position = new Vector2(shareMovementsMessage.x[i], shareMovementsMessage.y[i]);
                        }

                    }

                }
                break;
            case MessageType.SHOOT:
                {
                    ServerShoot();
                }
                break;
        }
        c.received.OnRead();

        // Client movement Lerp 
        //float lerpPercentage = (Time.time - timeStartedLerping) / GameManager.FREQUENCY;
        //Debug.Log(string.Format("lerpPercent[{0}] = (time[{1}] - tS[{2}]) / tTRG[{3}]", lerpPercentage, Time.time, timeStartedLerping, frequency));
        //serverPlayer.transform.position = Vector3.Lerp(startServerPos, endServerPos, lerpPercentage);

    }

    public void ClientShoot()
    {
        //projectiles.Add(Instantiate(projectilePrefab, clientPlayer.transform.position, Quaternion.identity));
        //ShootMessage shoot = new ShootMessage();
        //c.ClientSend(shoot);
    }

    public void ServerShoot()
    {
        //projectiles.Add(Instantiate(projectilePrefab, serverPlayer.transform.position, Quaternion.identity));
    }

    public void SendPosition(GameObject player)
    {
        //Debug.LogError("Sending position !");
        clientMovement.type = MessageType.MOVEMENT;
        clientMovement.x = player.transform.position.x;
        clientMovement.y = player.transform.position.y;
        clientMovement.playerId = myNumber;

        c.ClientSend(clientMovement);
    }
}
