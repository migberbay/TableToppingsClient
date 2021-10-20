using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public ConnectionManager conn;
    public GameObject loginForm, worldGrid;
    public GameObject worldButtonPrefab;

    public void Logout(){
        conn.SendMessageToServer("Logout:"+conn.logged.id);
    }

    public void LoadMainMenuForMaster(string worldinfo){
        Debug.Log("world information recieved: " + worldinfo);
        
    }

    public void LoadMainMenuForPlayer(){

    }
}
