using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

public class UDPServer 
{
    public Message received = new Message();

    UdpClient server;

    List<IPEndPoint> epClients = new List<IPEndPoint>();

    BinaryFormatter formatter = new BinaryFormatter();

    public void Server(string address)
    {
        server = new UdpClient(GameManager.PORT);

        Thread thread = new Thread(new ThreadStart(ServerReceive));
        thread.Start();
    }

    public void ServerSend(Message message)
    {
        foreach (IPEndPoint client in epClients.ToArray())
        {
            byte[] clientMessageAsByteArray = new byte[GameManager.PACKET_LENGTH];

            MemoryStream ms = new MemoryStream(clientMessageAsByteArray);

            formatter.Serialize(ms, message);

            server.Send(clientMessageAsByteArray, clientMessageAsByteArray.Length, client);
        }
    }

    private void ServerReceive()
    {
        while (true)
        {
            var remoteEP = new IPEndPoint(IPAddress.Any, GameManager.PORT);
            var data = server.Receive(ref remoteEP); //Listen on port
            //Debug.LogError("-SERVER--RCV from " + remoteEP.ToString());
            MemoryStream ms = new MemoryStream(data);
      
            received = (Message)formatter.Deserialize(ms);

            epClients.Add(remoteEP);
        }
    }
}
