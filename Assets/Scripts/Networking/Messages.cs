using UnityEngine;
using UnityEditor;

public enum MessageType
{
    NONE,

    START,
    SHOOT,
    CLIENT,
    SERVER,
}

public enum GameState
{
    STOP,

    START,
    GAME,

}


[System.Serializable]
public class Message
{
    public MessageType type;
    public Message()
    {
        type = MessageType.NONE;
    }

    public void OnRead()
    {
        type = MessageType.NONE;
    }
}

[System.Serializable]
public class MovementMessage : Message
{
    public float x;
    public float y;
}

[System.Serializable]
public class StartMessage : Message
{
    public StartMessage()
    {
        type = MessageType.START;
    }
}

[System.Serializable]
public class ShootMessage : Message
{
    public ShootMessage()
    {
        type = MessageType.SHOOT;
    }
}
