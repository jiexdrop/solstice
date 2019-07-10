using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class UDPClient
{
    public Message received = new Message();

    UdpClient client;

    IPEndPoint epServer;

    BinaryFormatter formatter = new BinaryFormatter();

    public void Client(string address)
    {
        client = new UdpClient();
        epServer = new IPEndPoint(IPAddress.Parse(address), GameManager.PORT);
        client.Connect(epServer);

        Thread thread = new Thread(new ThreadStart(ClientReceive));
        thread.Start();
    }

    private void ClientReceive()
    {
        while (true)
        {
            var data = client.Receive(ref epServer);
            MemoryStream ms = new MemoryStream(data);
      
            received = (Message)formatter.Deserialize(ms);

        }
    }

    public void ClientSend(Message message)
    {
        byte[] clientMessageAsByteArray = new byte[GameManager.PACKET_LENGTH];

        MemoryStream ms = new MemoryStream(clientMessageAsByteArray);

        formatter.Serialize(ms, message);

        client.Send(clientMessageAsByteArray, clientMessageAsByteArray.Length);
    }

    internal bool Connected()
    {
        return true;
    }
}
