using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Client : MonoBehaviour
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

            clientPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            serverPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

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
                c.ClientSend(new StartMessage());

                break;
            case GameState.START:

                state = GameState.GAME;

                break;
            case GameState.GAME:

                speed = joystick.InputVector * Time.deltaTime * 5;
                clientPlayer.transform.position += speed;

                elapsed += Time.deltaTime;

                if (elapsed >= GameManager.FREQUENCY)
                {
                    elapsed = elapsed % GameManager.FREQUENCY;
                    SendPosition();
                }

                break;
        }


        // Client read received messages
        switch (c.received.type)
        {
            case MessageType.NONE:
                break;
            case MessageType.START:
                state = GameState.START;
                break;
            case MessageType.SERVER:
                MovementMessage mm = (MovementMessage)c.received;
                startServerPos = serverPlayer.transform.position;
                endServerPos = new Vector3(mm.x, mm.y);
                timeStartedLerping = Time.time;
                break;
            case MessageType.SHOOT:
                ServerShoot();
                break;
        }
        c.received.OnRead();

        // Client movement Lerp 
        float lerpPercentage = (Time.time - timeStartedLerping) / GameManager.FREQUENCY;
        //Debug.Log(string.Format("lerpPercent[{0}] = (time[{1}] - tS[{2}]) / tTRG[{3}]", lerpPercentage, Time.time, timeStartedLerping, frequency));
        serverPlayer.transform.position = Vector3.Lerp(startServerPos, endServerPos, lerpPercentage);

    }

    public void ClientShoot()
    {
        projectiles.Add(Instantiate(projectilePrefab, clientPlayer.transform.position, Quaternion.identity));
        ShootMessage shoot = new ShootMessage();
        c.ClientSend(shoot);
    }

    public void ServerShoot()
    {
        projectiles.Add(Instantiate(projectilePrefab, serverPlayer.transform.position, Quaternion.identity));
    }

    public void SendPosition()
    {
        // Debug.LogError("Sending position !");
        clientMovement.type = MessageType.CLIENT;
        clientMovement.x = clientPlayer.transform.position.x;
        clientMovement.y = clientPlayer.transform.position.y;

        c.ClientSend(clientMovement);
    }
}
