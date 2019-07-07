using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Client : MonoBehaviour
{
    public GameObject playerPrefab;

    private GameObject serverPlayer;
    private GameObject clientPlayer;

    // Joystick client
    public VirtualJoystick joystick;
    private Vector3 speed = new Vector3();

    // Movement of server
    private Vector3 startServerPos;
    private Vector3 endServerPos;

    private float frequency;
    private float timeStartedLerping;

    TCPClient c = new TCPClient();

    private GameState state = GameState.STOP;

    void Start()
    {
        if (GameManager.Instance.type.Equals(ConnectionType.CLIENT))
        {
            Debug.LogError("Client connected with ip: " + GameManager.Instance.IP);
            c.Client(GameManager.Instance.IP);

            clientPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            serverPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        } else
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
                if (c.Connected())
                {
                    Debug.LogError("Send Start Server from client");
                    c.ClientSend(new StartMessage());
                }
                break;
            case GameState.START:

                StartCoroutine(SendPosition());
                state = GameState.GAME;
                

                break;
            case GameState.GAME:

                speed = joystick.InputVector * Time.deltaTime * 5;
                clientPlayer.transform.position += speed;

                break;
        }

        switch (c.received.type)
        {
            case MessageType.START:
                state = GameState.START;
                break;
            case MessageType.SERVER:
                MovementMessage mm = (MovementMessage)c.received;
                startServerPos = serverPlayer.transform.position;
                endServerPos = new Vector3(mm.x, mm.y);
                frequency = mm.frequency;
                timeStartedLerping = Time.time;
                break;
        }
        c.received.OnRead();

        // Client movement Lerp 
        float lerpPercentage = (Time.time - timeStartedLerping) / frequency;
        //Debug.Log(string.Format("lerpPercent[{0}] = (time[{1}] - tS[{2}]) / tTRG[{3}]", lerpPercentage, Time.time, timeStartedLerping, frequency));
        serverPlayer.transform.position = Vector3.Lerp(startServerPos, endServerPos, lerpPercentage);

    }

    public IEnumerator SendPosition()
    {
        while (true)
        {
            float frequency = 1.0f / 6.0f; // Sends 6 times a second

            yield return new WaitForSeconds(frequency);

            Debug.LogError("Sending position !");
            MovementMessage clientMovement = new MovementMessage();
            clientMovement.type = MessageType.CLIENT;
            clientMovement.x = clientPlayer.transform.position.x;
            clientMovement.y = clientPlayer.transform.position.y;
            clientMovement.frequency = frequency;

            c.ClientSend(clientMovement);
        }
    }
}
