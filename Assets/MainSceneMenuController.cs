using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainSceneMenuController : MonoBehaviour
{
    public GameObject movableMenuPrefab;
    

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P)){
            SpawnNewMenu();
        }
    }

    public void SpawnNewMenu(){
        var menuInstance = Instantiate(movableMenuPrefab);
        var uiDoc = menuInstance.GetComponent<UIDocument>();
    }
}
