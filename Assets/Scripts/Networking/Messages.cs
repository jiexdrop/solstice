using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public enum MessageType
{
    NONE,

    CLIENT_NEW_PLAYER,
    SERVER_SHARE_PLAYERS,
    SERVER_SHARE_MOVEMENT,
    SHOOT,
    MOVEMENT,
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
    public int playerId;
    public float x;
    public float y;
}

[System.Serializable]
public class ServerSharePlayersMessage : Message
{
    
    public float[] x;
    public float[] y;

    public int playerNumber;

    public ServerSharePlayersMessage()
    {
        type = MessageType.SERVER_SHARE_PLAYERS;
    }
}


[System.Serializable]
public class ServerShareMovementMessage : Message
{
    public float [] x;
    public float [] y;


    public ServerShareMovementMessage()
    {
        type = MessageType.SERVER_SHARE_MOVEMENT;
    }
}


[System.Serializable]
public class ClientNewPlayerMessage : Message
{
    public ClientNewPlayerMessage()
    {
        type = MessageType.CLIENT_NEW_PLAYER;
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
