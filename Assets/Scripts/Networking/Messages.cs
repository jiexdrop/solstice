﻿using UnityEngine;
using UnityEditor;

public enum MessageType
{
    NONE,

    START,
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
    public float frequency;
}

[System.Serializable]
public class StartMessage : Message
{
    public StartMessage()
    {
        type = MessageType.START;
    }
}

