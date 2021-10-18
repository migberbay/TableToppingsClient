using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UIChat;

public class LoginManager : MonoBehaviour
{
    public TMPro.TMP_InputField username, password, IP_addr, port;
    public Button loginButton;
    public ConnectionManager conn;
	public MessagesController messages;


    public void Login(){
        if(IP_addr.text == ""){
            Debug.Log("no ip address provided, using local address.");
			messages.AddMessageToChat("no ip address provided, using local address.");    
            conn.ipaddr = "127.0.0.1";
        }else{
            conn.ipaddr = IP_addr.text;
        }

        try{
            conn.port = int.Parse(port.text);
        }catch (Exception e){
            Debug.Log("no port was given using default 30069");
			messages.AddMessageToChat("no port was given using default 30069");    
            conn.port = 30069;
        }

        if(conn.socketConnection == null){
            Debug.Log("Connecting to server.");
            messages.AddMessageToChat("Connecting to server...");
            // Coroutine r = StartCoroutine(AwaitConnectionStablishmentAndSendLogin());
            conn.ConnectToTCPServer();
        }else{
            Debug.Log("Already connected to server.");
            conn.SendMessageToServer("Login:"+username.text+","+password.text);
        }
    }

    public void RemoveLoginPanelAndLoadMainMenu(){
        foreach (Transform child in this.gameObject.transform)
        {
            child.gameObject.SetActive(false);
        }
        //DO MORE STUFF
    }

    public IEnumerator AwaitConnectionStablishmentAndSendLogin(){
        while(!conn.connected){
            yield return null;
        }
        if(conn.socketConnection != null){
            messages.AddMessageToChat("Connection to server successfull.");
            conn.SendMessageToServer("Login:"+username.text+","+password.text);
        }
    }
}
