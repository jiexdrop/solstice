using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Scenes
{
    public const string MENU_SCENE = "MenuScene";
    public const string GAME_SCENE = "GameScene";
}

public enum ConnectionType
{
    UNDEFINED,

    CLIENT,
    SERVER
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField]
    private GameObject playerPrefab;

    [SerializeField]
    private InputField clientConnectIp;

    public ConnectionType type = ConnectionType.UNDEFINED;

    public const float FREQUENCY = 1f / 10f;
    public const float SHOOT_DURATION = 1f;
    public const int PACKET_LENGTH = 512;
    public const int PORT = 7345;

    public string IP = "127.0.0.1";

    void Start()
    {
        DontDestroyOnLoad(this);
        Instance = this;
    }

    void Update()
    {

    }

    public void CreateClientGameScene()
    {
        SceneManager.LoadScene(Scenes.GAME_SCENE);
        IP = clientConnectIp.text;
        type = ConnectionType.CLIENT;
    }

    public void CreateServerGameScene()
    {
        SceneManager.LoadScene(Scenes.GAME_SCENE);
        IP = clientConnectIp.text;
        type = ConnectionType.SERVER;
    }
}
