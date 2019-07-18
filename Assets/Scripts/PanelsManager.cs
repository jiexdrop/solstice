using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelsManager : MonoBehaviour
{
    public GameObject lobbyPanel;
    public GameObject gamePanel;

    public void ShowGamePanel()
    {
        HideAll();
        gamePanel.SetActive(true);
    }

    public void ShowLobbyPanel()
    {
        HideAll();
        lobbyPanel.SetActive(true);
    }

    public void HideAll()
    {
        lobbyPanel.SetActive(false);
        gamePanel.SetActive(false);
    }

}
