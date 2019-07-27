using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public enum MessageType
{
    NONE,

    SERVER_START_GAME,

    CLIENT_NEW_PLAYER,
    SERVER_SHARE_PLAYERS,

    MOVEMENT,
    SERVER_SHARE_MOVEMENT,

    CLIENT_GO_TO_NEXT_ROOM,
    SERVER_GO_TO_NEXT_ROOM,

    CLIENT_SHARE_MONSTERS_SPAWN,
    SERVER_SHARE_MONSTERS_SPAWN,

    SHOOT,
    SERVER_SHARE_SHOOT,
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
    public float visorRotation;
}

[System.Serializable]
public class ServerSharePlayersMessage : Message
{

    public float[] x;
    public float[] y;

    public int playerId;

    public int seed;

    public ServerSharePlayersMessage()
    {
        type = MessageType.SERVER_SHARE_PLAYERS;
    }
}


[System.Serializable]
public class ServerShareMovementMessage : Message
{
    public float[] x;
    public float[] y;
    public float[] visorRotation;

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
    public int playerId;
    public float duration;

    public ShootMessage()
    {
        type = MessageType.SHOOT;
    }
}

[System.Serializable]
public class ServerShareShootMessage : Message
{
    public int playerId;

    public ServerShareShootMessage()
    {
        type = MessageType.SERVER_SHARE_SHOOT;
    }
}

[System.Serializable]
public class ServerStartGameMessage : Message
{
    public int playerId;

    public ServerStartGameMessage()
    {
        type = MessageType.SERVER_START_GAME;
    }
}


[System.Serializable]
public class ServerGoToNextRoomMessage : Message
{
    public float[] x;
    public float[] y;
    public int seed;

    public ServerGoToNextRoomMessage()
    {
        type = MessageType.SERVER_GO_TO_NEXT_ROOM;
    }
}

[System.Serializable]
public class ClientGoToNextRoomMessage : Message
{
    public int seed;

    public ClientGoToNextRoomMessage()
    {
        type = MessageType.CLIENT_GO_TO_NEXT_ROOM;
    }
}


[System.Serializable]
public class ServerShareMonstersSpawnMessage : Message
{
    public int playerId;

    public int roomId;

    public int seed;

    public ServerShareMonstersSpawnMessage()
    {
        type = MessageType.SERVER_SHARE_MONSTERS_SPAWN;
    }
}

[System.Serializable]
public class ClientShareMonstersSpawnMessage : Message
{
    public int playerId;

    public int roomId;

    public int seed;

    public ClientShareMonstersSpawnMessage()
    {
        type = MessageType.CLIENT_SHARE_MONSTERS_SPAWN;
    }
}



