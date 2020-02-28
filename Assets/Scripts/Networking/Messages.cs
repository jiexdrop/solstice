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

    CLIENT_DIE,
    SERVER_DIE,
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
    public bool shooting;

    public MovementMessage()
    {
        type = MessageType.MOVEMENT;
    }
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
    // Players movements
    public float[] x;
    public float[] y;
    public float[] visorRotation;
    public bool[] shooting;

    // Monsters movements
    public float[] mx;
    public float[] my;
    public int[] health;

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

    public bool teleport;

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

    public bool teleport;

    public ClientShareMonstersSpawnMessage()
    {
        type = MessageType.CLIENT_SHARE_MONSTERS_SPAWN;
    }
}

[System.Serializable]
public class ClientDieMessage : Message
{
    public int playerId;

    public ClientDieMessage()
    {
        type = MessageType.CLIENT_DIE;
    }
}

[System.Serializable]
public class ServerDieMessage : Message
{
    public int playerId;

    public ServerDieMessage()
    {
        type = MessageType.SERVER_DIE;
    }
}

