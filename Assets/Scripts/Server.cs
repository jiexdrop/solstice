﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Server : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject projectilePrefab;

    private int nbOfPlayers;
    private GameObject[] players = new GameObject[4];
    private Player player;

    // Receive movement of client
    private Vector2[] startPlayersPositions = new Vector2[4];
    private Vector2[] endPlayersPositions = new Vector2[4];
    private float[] startPlayersRotations = new float[4];
    private float[] endPlayersRotations = new float[4];
    private float[] timesStartedLerping = new float[4];
    private int lastClientMovement;

    private List<GameObject> projectiles = new List<GameObject>();

    [Header("Controls")]
    public VirtualJoystick joystick;
    public Button shootButton;
    private Vector2 speed = new Vector2();

    // Send movement of server
    MovementMessage serverMovement = new MovementMessage();

    private float elapsed;

    // This server shares movement data
    UDPServer s = new UDPServer();
    // This server is used for network ip address/port scan
    TCPServer os = new TCPServer();

    private GameState state = GameState.STOP;

    [Header("Lobby")]
    public PlayerPanel[] playerPanels;
    public Button playButton;
    public PanelsManager panelsManager;

    [Header("Generation")]
    public DungeonGeneration dungeonGeneration;
    public int seed;

    void Start()
    {
        if (GameManager.Instance.type.Equals(ConnectionType.SERVER))
        {
            s.Server(GameManager.Instance.IP);
            os.Server(GameManager.Instance.IP);

            AddPlayer();

            shootButton.onClick.AddListener(ServerShoot);

            playButton.onClick.AddListener(StartGame);

            panelsManager.ShowLobbyPanel();

            seed = Random.Range(0, Int32.MaxValue);
            dungeonGeneration.Generate(seed);
        }
        else
        {
            Destroy(this.gameObject);
            Destroy(this);
        }
    }

    private void StartGame()
    {
        panelsManager.ShowGamePanel();
        state = GameState.GAME;
        ServerStartGameMessage ssgm = new ServerStartGameMessage();
        s.ServerSend(ssgm);
    }

    void Update()
    {
        switch (state)
        {
            case GameState.STOP:
                
                break;
            case GameState.START:

                break;
            case GameState.GAME:
                
                speed = joystick.InputVector * Time.deltaTime * 12;

                // Get server player
                if (speed.magnitude > 0)
                {
                    //player.transform.position += speed;
                    Rigidbody2D playerRb2D = player.GetComponent<Rigidbody2D>();
                    playerRb2D.MovePosition(playerRb2D.position + speed);
                    player.SetRotation(joystick.InputVector);
                }

                elapsed += Time.deltaTime;

                if (elapsed >= GameManager.FREQUENCY)
                {
                    elapsed = elapsed % GameManager.FREQUENCY;
                    ShareMovements();
                }

                // Server movement Lerp 
                for (int i = 0; i < nbOfPlayers; i++)
                {
                    GameObject player = players[i];
                    Vector2 startClientPos = startPlayersPositions[i];
                    Vector2 endClientPos = endPlayersPositions[i];
                    float startClientRot = startPlayersRotations[i];
                    float endClientRot = endPlayersRotations[i];
                    float timeStartedLerping = timesStartedLerping[i];

                    if (i != 0)
                    {
                        float lerpPercentage = (Time.time - timeStartedLerping) / GameManager.FREQUENCY;
                        //Debug.Log(string.Format("lerpPercent[{0}] = (time[{1}] - tS[{2}]) / tTRG[{3}]", lerpPercentage, Time.time, timeStartedLerping, GameManager.FREQUENCY));
                        players[i].transform.position = Vector3.Lerp(startClientPos, endClientPos, lerpPercentage);
                        float lerpedRotation = Mathf.LerpAngle(startClientRot, endClientRot, lerpPercentage);
                        players[i].GetComponent<Player>().SetRotation(lerpedRotation);
                    }
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
                break;
            case MessageType.MOVEMENT:
                MovementMessage mm = (MovementMessage)s.received;
                //Debug.Log("Movement from " + mm.playerId);
                lastClientMovement = mm.playerId;
                startPlayersPositions[lastClientMovement] = players[lastClientMovement].transform.position;
                startPlayersRotations[lastClientMovement] = players[lastClientMovement].GetComponent<Player>().visorRotation;
                endPlayersRotations[lastClientMovement] = mm.visorRotation;
                endPlayersPositions[lastClientMovement] = new Vector3(mm.x, mm.y);

                timesStartedLerping[lastClientMovement] = Time.time;
                break;
            case MessageType.SHOOT:
                ShootMessage sm = (ShootMessage)s.received;
                ClientShoot(sm.playerId);
                ShareShoots(sm.playerId);
                break;
        }
        s.received.OnRead();


    }

    public void AddPlayer()
    {
        Vector3 randomPosition = Random.insideUnitCircle;

        if (nbOfPlayers < 4)
        {
            players[nbOfPlayers] = Instantiate(playerPrefab, randomPosition, Quaternion.identity);
            // Lobby
            playerPanels[nbOfPlayers].SetActivePlayer(true);
            nbOfPlayers++;
        }

        if (player == null) // Set server player 
        {
            player = players[0].GetComponent<Player>();
            // Set camera as a child of the player
            Camera.main.transform.parent = player.transform;
        }
    }

    public void SharePlayers()
    {
        ServerSharePlayersMessage newPlayerMessage = new ServerSharePlayersMessage();

        newPlayerMessage.x = new float[nbOfPlayers];
        newPlayerMessage.y = new float[nbOfPlayers];

        newPlayerMessage.playerId = nbOfPlayers - 1;

        newPlayerMessage.seed = seed;

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
        shareMovementsMessage.visorRotation = new float[nbOfPlayers];

        for (int i = 0; i < nbOfPlayers; i++)
        {
            shareMovementsMessage.x[i] = players[i].transform.position.x;
            shareMovementsMessage.y[i] = players[i].transform.position.y;
            shareMovementsMessage.visorRotation[i] = players[i].GetComponent<Player>().visorRotation;
        }

        s.ServerSend(shareMovementsMessage);
    }

    public void ServerShoot()
    {
        GameObject p = Instantiate(projectilePrefab, player.visor.transform.position, Quaternion.identity);
        Projectile projectile = p.GetComponent<Projectile>();
        projectile.duration = GameManager.SHOOT_DURATION;
        projectile.transform.rotation = player.center.transform.rotation;
        projectiles.Add(p);
        ShareShoots(0);
    }

    public void ClientShoot(int playerId)
    {
        GameObject p = Instantiate(projectilePrefab, players[playerId].GetComponent<Player>().visor.transform.position, Quaternion.identity);
        Projectile projectile = p.GetComponent<Projectile>();
        projectile.duration = GameManager.SHOOT_DURATION;
        projectile.transform.rotation = players[playerId].GetComponent<Player>().center.transform.rotation;
        projectiles.Add(p);
    }

    public void ShareShoots(int playerId)
    {
        ServerShareShootMessage shoot = new ServerShareShootMessage();
        shoot.playerId = playerId;
        s.ServerSend(shoot);
    }

}
