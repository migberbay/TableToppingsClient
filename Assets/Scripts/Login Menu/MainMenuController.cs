using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public ConnectionManager conn;
    public GameObject loginForm, worldGrid;
    public GameObject worldButtonPrefab;
    public GameObject worldsMenu, optionsMenu;

    public void Logout(){
        conn.SendMessageToServer("Logout:"+conn.logged.id);
    }

    public void LoadWorld(int worldID){
        conn.SendMessageToServer("004:"+worldID.ToString());
    }

    public IEnumerator LoadWorldInformation(){
        optionsMenu.SetActive(true);
        worldsMenu.SetActive(false);
        yield return null;
    }

    public void LoadMapEditor(){
        SceneManager.LoadScene("MapBuilder");
    }
    
    public void LoadGameView(){
        SceneManager.LoadScene("PlayScene");
    }

    public IEnumerator LoadMainMenuForMaster(List<ConnectionManager.world> worlds){
        worldsMenu.SetActive(true);
        foreach (var item in worlds)
        {
            var instance = Instantiate(worldButtonPrefab);
            instance.transform.parent = worldGrid.transform;
            instance.GetComponentInChildren<Text>().text = item.Name;
            var button = instance.GetComponent<Button>();
            button.onClick.AddListener(() => {LoadWorld(item.ID);});
        }
        yield return null;
    }

    public IEnumerator LoadMainMenuForPlayer(){

        yield return null;
    }

    public IEnumerator activeStateChange(){
        if(this.gameObject.activeSelf){
            this.gameObject.SetActive(false);
        }else{
            this.gameObject.SetActive(true);
        }
        yield return null;
    }
}
