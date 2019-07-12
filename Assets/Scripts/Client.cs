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
    private GameObject [] players = new GameObject[4];
    private Vector2 [] startPlayersPositions = new Vector2[4];
    private Vector2 [] endPlayersPositions = new Vector2[4];
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
                players[myNumber].transform.position += speed;

                elapsed += Time.deltaTime;

                if (elapsed >= GameManager.FREQUENCY)
                {
                    elapsed = elapsed % GameManager.FREQUENCY;
                    SendPosition(players[myNumber]); 
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
                        Destroy(players[i]);
                        players[i] = null;
                    }

                    // Add new players
                    nbOfPlayers = sharePlayersMessage.x.Length;

                    for (int i = 0; i < nbOfPlayers; i++)
                    {
                        players[i] = Instantiate(playerPrefab, new Vector2(sharePlayersMessage.x[i], sharePlayersMessage.y[i]), Quaternion.identity);
                    }

                    state = GameState.GAME;
                }
                break;
            case MessageType.SERVER_SHARE_MOVEMENT:
                {
                    ServerShareMovementMessage shareMovementsMessage = (ServerShareMovementMessage)c.received;

                    for (int i = 0; i < shareMovementsMessage.x.Length; i++)
                    {

                        if (i != myNumber)
                        {
                            startPlayersPositions[i] = players[i].transform.position;
                            endPlayersPositions[i] = new Vector2(shareMovementsMessage.x[i], shareMovementsMessage.y[i]);
                            timesStartedLerping[i] = Time.time;
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

        // Lerp server shared movements
        for(int i = 0; i < nbOfPlayers; i++)
        {
            GameObject player = players[i];
            Vector2 startServerPos = startPlayersPositions[i];
            Vector2 endServerPos = endPlayersPositions[i];
            float timeStartedLerping = timesStartedLerping[i];

            if (i != myNumber)
            {
                float lerpPercentage = (Time.time - timeStartedLerping) / GameManager.FREQUENCY;
                player.transform.position = Vector3.Lerp(startServerPos, endServerPos, lerpPercentage);
            }
        }


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
