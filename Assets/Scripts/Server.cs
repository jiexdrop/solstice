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
    private Dictionary<int, GameObject> players = new Dictionary<int, GameObject>();

    private List<GameObject> projectiles = new List<GameObject>();

    [Header("Controls")]
    public VirtualJoystick joystick;
    public Button shootButton;
    private Vector3 speed = new Vector3();

    // Receive movement of client
    private int lastClientMovement = -1;
    private Vector3 startClientPos;
    private Vector3 endClientPos;
    // Send movement of server
    MovementMessage serverMovement = new MovementMessage();

    private float timeStartedLerping;
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

    GameObject getPlayer;

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
                players.TryGetValue(0, out getPlayer);
                getPlayer.transform.position += speed;


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
                //startClientPos = clientPlayer.transform.position;
                endClientPos = new Vector3(mm.x, mm.y);
                lastClientMovement = mm.playerId;
                //timeStartedLerping = Time.time;

                break;
            case MessageType.SHOOT:
                ClientShoot();
                break;
        }
        s.received.OnRead();

        // Server movement Lerp 
        //float lerpPercentage = (Time.time - timeStartedLerping) / GameManager.FREQUENCY;
        //Debug.Log(string.Format("lerpPercent[{0}] = (time[{1}] - tS[{2}]) / tTRG[{3}]", lerpPercentage, Time.time, timeStartedLerping, frequency));
        players.TryGetValue(lastClientMovement, out getPlayer);
        if(getPlayer != null)
        {
            getPlayer.transform.position = endClientPos;
        }
        //clientPlayer.transform.position = Vector3.Lerp(startClientPos, endClientPos, lerpPercentage);

    }

    public void AddPlayer()
    {
        Vector3 randomPosition = Random.insideUnitCircle;
        
        players.Add(nbOfPlayers, Instantiate(playerPrefab, randomPosition, Quaternion.identity));
        nbOfPlayers++;
    }

    public void SharePlayers()
    {
        ServerSharePlayersMessage newPlayerMessage = new ServerSharePlayersMessage();

        newPlayerMessage.x = new float[players.Count];
        newPlayerMessage.y = new float[players.Count];

        newPlayerMessage.playerNumber = players.Count - 1;

        for (int i = 0; i < players.Count; i++)
        {
            GameObject player;
            players.TryGetValue(i, out player);

            if (player != null) {
                newPlayerMessage.x[i] = player.transform.position.x;
                newPlayerMessage.y[i] = player.transform.position.y;
            }
        }

        s.ServerSend(newPlayerMessage);
    }

    public void ShareMovements()
    {
        ServerShareMovementMessage shareMovementsMessage = new ServerShareMovementMessage();

        shareMovementsMessage.x = new float[players.Count];
        shareMovementsMessage.y = new float[players.Count];

        for (int i = 0; i < players.Count; i++)
        {
            GameObject player;
            players.TryGetValue(i, out player);

            shareMovementsMessage.x[i] = player.transform.position.x;
            shareMovementsMessage.y[i] = player.transform.position.y;

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
