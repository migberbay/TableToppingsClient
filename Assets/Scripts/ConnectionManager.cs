using System.Collections;
using System.Collections.Generic;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UIChat;

public class ConnectionManager : MonoBehaviour
{
    public string ipaddr = "127.0.0.1";
    public int port = 30069;

    public TcpClient socketConnection;
    public Thread clientRecieveThread;
	public LoginManager loginManager;
	public bool connected = false;
	MessagesController messages;
	public UnityMainThreadDispatcher dispatcher;

	private void Start() {
		DontDestroyOnLoad(this.gameObject);
		messages = loginManager.messages;
	}

    /// <summary> 	
	/// Setup socket connection. 
	/// returns true when socket becomes connected.	
	/// </summary> 
    public void ConnectToTCPServer(){
        try {
			clientRecieveThread = new Thread (new ThreadStart(ListenForData));
			clientRecieveThread.IsBackground = true;
			clientRecieveThread.Start();
		}
		catch (Exception e) { 			
			Debug.LogException(e, this);
		}
		StartCoroutine(AwaitSocketForSeconds(1.5f));
    }
	
	public IEnumerator MesaggeOnMainThread(string message) {
		messages.AddMessageToChat(message);
		yield return null;
 	}
 
	public void MainThreadMessage(string message){
		dispatcher.Enqueue(MesaggeOnMainThread(message));
	}

	public IEnumerator AwaitSocketForSeconds(float seconds){
		while(socketConnection == null){
			yield return new WaitForSeconds(seconds);
		}
		
		while(!socketConnection.Connected){
			yield return new WaitForSeconds(seconds);
		}
		
		connected = true;
	}

    /// <summary>
	/// Runs in background clientReceiveThread; Listens for incomming data. 	
	/// </summary>
	private void ListenForData() {
		try {
			socketConnection = new TcpClient(ipaddr, port);
			Byte[] bytes = new Byte[1024];

			while (true) {
				// Get a stream object for reading
				using (NetworkStream stream = socketConnection.GetStream()) { 					
					int length;
					// Read incomming stream into byte arrary. 					
					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) { 						
						var incommingData = new byte[length]; 						
						Array.Copy(bytes, 0, incommingData, 0, length); 						
						// Convert byte array to string message. 						
						string serverMessage = Encoding.ASCII.GetString(incommingData); 						
						Debug.Log("server message received as: " + serverMessage); 
						ResponseHandler(serverMessage);					
					} 				
				} 			
			}   
		}         
		catch (SocketException socketException) {             
			Debug.Log("Socket exception: " + socketException);
		}     
	}

    /// <summary> 	
	/// Send message to server using socket connection. 	
	/// </summary> 	
	public void SendMessageToServer(string clientMessage) {         
		if (socketConnection == null) {             
			return;         
		}  		

		clientMessage = clientMessage +"\n";

		try { 			
			// Get a stream object for writing. 			
			NetworkStream stream = socketConnection.GetStream(); 			
			if (stream.CanWrite) {                 
				// Convert string message to byte array.                 
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage); 				
				// Write byte array to socketConnection stream.                 
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);                 
				Debug.Log("Client sent message "+ clientMessage);
			}         
		} 		
		catch (SocketException socketException) {
			Debug.Log("Socket exception: " + socketException);  
			MainThreadMessage("Socket exception: " + socketException);         
		}
	} 

	/// <summary> 	
	/// Handle the different Server responses. 	
	/// </summary>
	private void ResponseHandler(string message){
			var m = message.Split(':');
			string code, subcode, info;
		try{
			code = m[0];
			subcode = code[1] + "" + code[2];
			info = m[1];
		}catch (Exception e){
			code = "401";
			subcode = code[1] + "" + code[2];
			info = m[1];
		}
		

		// codes:
		// 0XX -> connection codes.
		// 1XX -> actions (in game operations).
		// 2XX -> audio.
		// 3XX -> chat codes.
		// 4XX -> error codes.

		switch (code[0])
		{
			case '0':
				ConnectionSubcodeHandler(subcode, info);
				break;
			case '1':
				Debug.Log("WIP");
				break;
			case '2':
				Debug.Log("WIP");
				break;
			case '3':
				Debug.Log("WIP");
				break;
			case '4':
				Debug.Log("WIP");
				ErrorSubcodeHandler(subcode, info);
				break;

			default: // Unrecognized code pattern
				Debug.Log("Unrecognized code pattern from server.");
				break;
		}
	}

	private void ConnectionSubcodeHandler(string subcode, string info){
		switch (subcode)
		{
			case "01":
				string[] status_usr = info.Split(';');
				if(status_usr[0] == "accepted"){
					Debug.Log("log the user in.");
					MainThreadMessage("Success!");
				}
				if(status_usr[1] == "rejected"){
					Debug.Log("reject user login.");
					MainThreadMessage("Incorrect credentials");
				}
				break;

			default:
				MainThreadMessage("subcode not handled.");
				break;
		}
	}


	private void ErrorSubcodeHandler(string subcode, string info){
		switch (subcode)
		{
			case "01":
				MainThreadMessage("Incorrect credentials in login.");
				break;

			default:
				Debug.Log("subcode not handled.");
				break;
		}
	}

}
