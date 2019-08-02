using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelsManager : MonoBehaviour
{
    public GameObject lobbyPanel;
    public GameObject gamePanel;
    public GameObject menuPanel;

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

    public void ShowMenuPanel()
    {
        HideAll();
        menuPanel.SetActive(true);
    }

    public void GoToMenuScene()
    {
        GameManager.Instance.Destroy();
        SceneManager.LoadScene("MenuScene");
    }

    public void HideAll()
    {
        lobbyPanel.SetActive(false);
        gamePanel.SetActive(false);
        menuPanel.SetActive(false);
    }

}
