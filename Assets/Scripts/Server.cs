using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Server : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject projectilePrefab;

    private int nbOfPlayers;
    private GameObject[] players = new GameObject[4];

    // Receive movement of client
    private Vector2[] startPlayersPositions = new Vector2[4];
    private Vector2[] endPlayersPositions = new Vector2[4];
    private float[] timesStartedLerping = new float[4];
    private int lastClientMovement;

    private List<GameObject> projectiles = new List<GameObject>();

    [Header("Controls")]
    public VirtualJoystick joystick;
    public Button shootButton;
    private Vector3 speed = new Vector3();

    // Send movement of server
    MovementMessage serverMovement = new MovementMessage();

    private float elapsed;

    UDPServer s = new UDPServer();

    private GameState state = GameState.STOP;

    void Start()
    {
        if (GameManager.Instance.type.Equals(ConnectionType.SERVER))
        {
            s.Server(GameManager.Instance.IP);

            AddPlayer();

            shootButton.onClick.AddListener(ServerShoot);
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
                state = GameState.GAME;
                break;
            case GameState.START:

                break;
            case GameState.GAME:
                
                speed = joystick.InputVector * Time.deltaTime * 5;

                // Get server player
                players[0].transform.position += speed;


                elapsed += Time.deltaTime;

                if (elapsed >= GameManager.FREQUENCY)
                {
                    elapsed = elapsed % GameManager.FREQUENCY;
                    ShareMovements();
                }

                break;
        }

        // Server read received messages
        switch (s.received.type)
        {
            case MessageType.NONE:
                break;
            case MessageType.CLIENT_NEW_PLAYER:
                Debug.LogError("Recieved Start Server from client");
                AddPlayer();
                SharePlayers();
                elapsed = 0; // Interrupt ShareMovements to send new player
                break;
            case MessageType.MOVEMENT:
                MovementMessage mm = (MovementMessage)s.received;
                //Debug.Log("Movement from " + mm.playerId);
                lastClientMovement = mm.playerId;
                startPlayersPositions[lastClientMovement] = players[lastClientMovement].transform.position;
                endPlayersPositions[lastClientMovement] = new Vector3(mm.x, mm.y);
                timesStartedLerping[lastClientMovement] = Time.time;

                break;
            case MessageType.SHOOT:
                ClientShoot();
                break;
        }
        s.received.OnRead();

        // Server movement Lerp 
        for(int i = 0; i < nbOfPlayers; i++)
        {
            GameObject player = players[i];
            Vector2 startClientPos = startPlayersPositions[i];
            Vector2 endClientPos = endPlayersPositions[i];
            float timeStartedLerping = timesStartedLerping[i];

            if (i != 0)
            {
                float lerpPercentage = (Time.time - timeStartedLerping) / GameManager.FREQUENCY;
                //Debug.Log(string.Format("lerpPercent[{0}] = (time[{1}] - tS[{2}]) / tTRG[{3}]", lerpPercentage, Time.time, timeStartedLerping, frequency));
                players[i].transform.position = Vector3.Lerp(startClientPos, endClientPos, lerpPercentage);
            }
        }

    }

    public void AddPlayer()
    {
        Vector3 randomPosition = Random.insideUnitCircle;

        if (nbOfPlayers < 4)
        {
            players[nbOfPlayers] = Instantiate(playerPrefab, randomPosition, Quaternion.identity);
            nbOfPlayers++;
        }
    }

    public void SharePlayers()
    {
        ServerSharePlayersMessage newPlayerMessage = new ServerSharePlayersMessage();

        newPlayerMessage.x = new float[nbOfPlayers];
        newPlayerMessage.y = new float[nbOfPlayers];

        newPlayerMessage.playerNumber = nbOfPlayers - 1;

        for (int i = 0; i < nbOfPlayers; i++)
        {
            if (players[i] != null) {
                newPlayerMessage.x[i] = players[i].transform.position.x;
                newPlayerMessage.y[i] = players[i].transform.position.y;
            }
        }

        s.ServerSend(newPlayerMessage);
    }

    public void ShareMovements()
    {
        ServerShareMovementMessage shareMovementsMessage = new ServerShareMovementMessage();

        shareMovementsMessage.x = new float[nbOfPlayers];
        shareMovementsMessage.y = new float[nbOfPlayers];

        for (int i = 0; i < nbOfPlayers; i++)
        {
            shareMovementsMessage.x[i] = players[i].transform.position.x;
            shareMovementsMessage.y[i] = players[i].transform.position.y;
        }

        s.ServerSend(shareMovementsMessage);
    }

    public void ServerShoot()
    {
        //projectiles.Add(Instantiate(projectilePrefab, serverPlayer.transform.position, Quaternion.identity));
        //ShootMessage shoot = new ShootMessage();
        //s.ServerSend(shoot);
    }

    public void ClientShoot()
    {
        //projectiles.Add(Instantiate(projectilePrefab, clientPlayer.transform.position, Quaternion.identity));
    }

}
