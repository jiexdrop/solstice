using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using UnityEngine;

public class TCPClient
{
    private TcpClient socketConnection;
    private Thread clientReceiveThread;

    public const int PORT = 7345;

    public Message received = new Message();

    private string ip;

    public void Client(string ip)
    {
        this.ip = ip;
        try
        {
            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.Log("On client connect exception " + e);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void ListenForData()
    {
        try
        {
            socketConnection = new TcpClient(ip, PORT);

            byte[] bytes = new byte[1024];

            while (true)
            {
                using (NetworkStream stream = socketConnection.GetStream())
                {
                    int length;

                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        var incommingData = new byte[length];
                        Array.Copy(bytes, 0, incommingData, 0, length);

                        BinaryFormatter formatter = new BinaryFormatter();
                        MemoryStream ms = new MemoryStream(incommingData);

                        Message msg = (Message)formatter.Deserialize(ms);

                        Debug.Log("--> Message from SERVER: " + msg.ToString());

                        received = msg;
                    }
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    public bool Connected()
    {
        return socketConnection != null;
    }

    public void ClientSend(Message message)
    {
        if (socketConnection == null)
        {
            return;
        }

        try
        {
            NetworkStream stream = socketConnection.GetStream();

            if (stream.CanWrite)
            {
                // String converted
                byte[] clientMessageAsByteArray = new byte[1024];

                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream ms = new MemoryStream(clientMessageAsByteArray);

                formatter.Serialize(ms, message);

                stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
                //Debug.LogError("Client sent message " + message);
            }

        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }
}
