using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    public InputField username, password, IP_addr, port;
    public ConnectionManager conn;

    public void Login(){
        if(IP_addr.text == ""){
            Debug.Log("no ip address provided.");
            conn.ipaddr = "127.0.0.1";
        }else{
            conn.ipaddr = IP_addr.text;
        }
        conn.port = int.Parse(port.text);
        

        conn.connectToTCPServer();
    }

}
