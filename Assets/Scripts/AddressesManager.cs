using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using UnityEngine.UI;

public class AddressesManager : MonoBehaviour
{
    public List<string> addresses = new List<string>();
    public GameObject panelAddressesButtons;
    public GameObject buttonPrefab;

    public GameManager gameManager;

    private bool addressesChanged;

    private float elapsed = 6f;

    private string startAddress;

    public void Start()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                startAddress = ip.ToString();
            }
        }
        int index = startAddress.LastIndexOf('.');
        startAddress = startAddress.Substring(0, index+1);
        //Debug.Log(startAddress);
    }

    IEnumerator ScanNetworkCoroutine()
    {
        while (true)
        {
            Debug.LogError("New Scan");
            //ScanNetwork(GameManager.PORT);
            yield return new WaitForSeconds(6);
        }
    }

    public void Update()
    {
        elapsed += Time.deltaTime;
        if(elapsed > 6f)
        {
            elapsed = elapsed % (6f);
            Debug.Log("[AddressesManager] ScanNetwork");
            ScanNetwork(GameManager.PORT);
        }

        if (addressesChanged)
        {
            addressesChanged = false;

            // Destroy all child from panel
            foreach (Transform child in panelAddressesButtons.transform)
            {
                Destroy(child.gameObject);
            }

            int i = 0;
            foreach( string ad in addresses )
            {
                i++;
                GameObject button = Instantiate(buttonPrefab, panelAddressesButtons.transform);
                button.GetComponent<Button>().onClick.AddListener(delegate { OnClickAddressButton(ad); });
                button.GetComponentInChildren<Text>().text = "Server " + i + " - " + ad;
            }
        }
    }

    public void OnClickAddressButton(string ipAddress)
    {
        // Ask the GameManager to create a client with an ipAddress
        gameManager.CreateClientGameScene(ipAddress);
    }

    string GetAddress(string subnet, int i)
    {
        return new StringBuilder(subnet).Append(i).ToString();
    }

    private void ScanNetwork(int port)
    {
        Parallel.For(0, 254, async i =>
        {
            string address = GetAddress(startAddress, i);

            using (TcpClient client = new TcpClient())
            {
                try
                {
                    await client.ConnectAsync(IPAddress.Parse(address), port).ConfigureAwait(false);

                    await Task.Delay(100).ConfigureAwait(false);

                    if (!client.Connected) return;

                    Debug.Log($"[AddressesManager] Success @{address}");

                    if (!addresses.Contains(address))
                    {
                        addresses.Add(address);
                        addressesChanged = true;
                    }

                    client.Close();
                }
                catch (SocketException ex)
                {
                    Debug.Log($"[AddressesManager] Failed with error code: {ex.ErrorCode}");
                }
            }
        });
    }


}
