using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using UnityEngine;

public class TCPServer
{

    private TcpListener tcpListener;

    private Thread tcpListenerThread;

    private TcpClient connectedTcpClient;

    public const int PORT = 7345;

    public Message received = new Message();

    private string ip = "127.0.0.1";

    // Update is called once per frame
    void Update()
    {

    }

    public void Server(string ip)
    {
        this.ip = ip;
        tcpListenerThread = new Thread(new ThreadStart(ListenForIncomingRequests));
        tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();
    }

    private async void ListenForIncomingRequests()
    {
        try
        {
            tcpListener = new TcpListener(IPAddress.Parse(ip), PORT);
            
            tcpListener.Start();

            Debug.Log("Server is Listening");

            byte[] bytes = new byte[1024];

            while (true)
            {
                using (connectedTcpClient = tcpListener.AcceptTcpClient())
                {
                    using (NetworkStream stream = connectedTcpClient.GetStream())
                    {
                        int length;

                        while ((length = await stream.ReadAsync(bytes, 0, bytes.Length)) != 0)
                        {
                            var incommingData = new byte[length];
                            Array.Copy(bytes, 0, incommingData, 0, length);

                            BinaryFormatter formatter = new BinaryFormatter();
                            MemoryStream ms = new MemoryStream(incommingData);

                            Message msg = (Message)formatter.Deserialize(ms);

                            Debug.Log("--> Message from CLIENT: " + msg.ToString());

                            received = msg;
                        }
                    }
                }
            }

        }
        catch (SocketException e)
        {
            Debug.Log("SocketException " + e.ToString());
        }
    }

    public void ServerSend(Message message)
    {
        if (connectedTcpClient == null)
        {
            return;
        }

        try
        {
            // Get a stream object for writing. 			
            NetworkStream stream = connectedTcpClient.GetStream();
            if (stream.CanWrite)
            {
                
                byte[] serverMessageAsByteArray = new byte[1024];

                // Serialize message
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream ms = new MemoryStream(serverMessageAsByteArray);

                formatter.Serialize(ms, message);

                stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }
}
