using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Server : MonoBehaviour
{
    public GameObject playerPrefab;

    private GameObject serverPlayer;
    private GameObject clientPlayer;

    // Joystick server
    public VirtualJoystick joystick;
    private Vector3 speed = new Vector3();

    // Movement of client
    private Vector3 startClientPos;
    private Vector3 endClientPos;

    private float timeStartedLerping;
    private float elapsed;

    TCPServer s = new TCPServer();

    private GameState state = GameState.STOP;

    void Start()
    {
        if (GameManager.Instance.type.Equals(ConnectionType.SERVER))
        {
            s.Server(GameManager.Instance.IP);

            clientPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            serverPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
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
                Debug.LogError("Recieved Start Server from client");
                state = GameState.START;
                break;
            case MessageType.CLIENT:
                MovementMessage mm = (MovementMessage)s.received;
                startClientPos = clientPlayer.transform.position;
                endClientPos = new Vector3(mm.x, mm.y);
                timeStartedLerping = Time.time;
                break;
        }
        s.received.OnRead();

        // Server movement Lerp 
        float lerpPercentage = (Time.time - timeStartedLerping) / GameManager.FREQUENCY;
        //Debug.Log(string.Format("lerpPercent[{0}] = (time[{1}] - tS[{2}]) / tTRG[{3}]", lerpPercentage, Time.time, timeStartedLerping, frequency));
        clientPlayer.transform.position = Vector3.Lerp(startClientPos, endClientPos, lerpPercentage);

    }

    public void SendPosition()
    {
        Debug.LogError("Sending position !");
        MovementMessage serverMovement = new MovementMessage();
        serverMovement.type = MessageType.SERVER;
        serverMovement.x = serverPlayer.transform.position.x;
        serverMovement.y = serverPlayer.transform.position.y;

        s.ServerSend(serverMovement);
    }
}
