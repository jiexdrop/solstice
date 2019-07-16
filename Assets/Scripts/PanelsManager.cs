using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelsManager : MonoBehaviour
{
    private GameObject lobbyPanel;
    private GameObject gamePanel;

    void Start()
    {
        lobbyPanel = transform.GetChild(0).gameObject;
        gamePanel = transform.GetChild(1).gameObject;
    }

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
