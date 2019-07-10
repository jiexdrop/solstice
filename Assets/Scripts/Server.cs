using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Server : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject projectilePrefab;

    private GameObject serverPlayer;
    private GameObject clientPlayer;

    private List<GameObject> projectiles = new List<GameObject>();

    [Header("Controls")]
    public VirtualJoystick joystick;
    public Button shootButton;
    private Vector3 speed = new Vector3();

    // Receive movement of client
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

            clientPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            serverPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

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
                break;
            case GameState.START:
                s.ServerSend(new StartMessage());
                state = GameState.GAME;
                break;
            case GameState.GAME:

                speed = joystick.InputVector * Time.deltaTime * 5;
                serverPlayer.transform.position += speed;

                elapsed += Time.deltaTime;

                if (elapsed >= GameManager.FREQUENCY)
                {
                    elapsed = elapsed % GameManager.FREQUENCY;
                    SendPosition();
                }

                break;
        }

        // Server read received messages
        switch (s.received.type)
        {
            case MessageType.NONE:
                break;
            case MessageType.START:
                //Debug.LogError("Recieved Start Server from client");
                state = GameState.START;
                break;
            case MessageType.CLIENT:
                MovementMessage mm = (MovementMessage)s.received;
                startClientPos = clientPlayer.transform.position;
                endClientPos = new Vector3(mm.x, mm.y);
                timeStartedLerping = Time.time;
                break;
            case MessageType.SHOOT:
                ClientShoot();
                break;
        }
        s.received.OnRead();

        // Server movement Lerp 
        float lerpPercentage = (Time.time - timeStartedLerping) / GameManager.FREQUENCY;
        //Debug.Log(string.Format("lerpPercent[{0}] = (time[{1}] - tS[{2}]) / tTRG[{3}]", lerpPercentage, Time.time, timeStartedLerping, frequency));
        clientPlayer.transform.position = Vector3.Lerp(startClientPos, endClientPos, lerpPercentage);

    }

    public void ServerShoot()
    {
        projectiles.Add(Instantiate(projectilePrefab, serverPlayer.transform.position, Quaternion.identity));
        ShootMessage shoot = new ShootMessage();
        s.ServerSend(shoot);
    }

    public void ClientShoot()
    {
        projectiles.Add(Instantiate(projectilePrefab, clientPlayer.transform.position, Quaternion.identity));
    }


    public void SendPosition()
    {
        //Debug.LogError("Sending position !");
        serverMovement.type = MessageType.SERVER;
        serverMovement.x = serverPlayer.transform.position.x;
        serverMovement.y = serverPlayer.transform.position.y;

        s.ServerSend(serverMovement);
    }
}
