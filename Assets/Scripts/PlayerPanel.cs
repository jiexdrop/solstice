using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPanel : MonoBehaviour
{
    public GameObject playerGameObject;
    
    public void SetActivePlayer(bool activate)
    {
        playerGameObject.SetActive(activate);
    }

}
