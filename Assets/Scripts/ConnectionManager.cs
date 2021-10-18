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
	public float timeout_seconds = 5f;

    public TcpClient socketConnection;
    public Thread clientRecieveThread;
	public LoginManager loginManager;
	public bool connected = false;
	public bool[] flags;
	MessagesController messages;
	public UnityMainThreadDispatcher dispatcher;
	public user logged;

	public class user{
		public int id;
		public string type, username;
	}

	private void Start() {
		DontDestroyOnLoad(this.gameObject);
		messages = loginManager.messages;
		flags = new bool[] {connected}; // add more as needed.
	}

    /// <summary> 	
	/// Setup socket connection. 
	/// returns true when socket becomes connected.	
	/// </summary> 
    public void ConnectToTCPServer(){
        try {
			if(socketConnection == null){
				clientRecieveThread = new Thread (new ThreadStart(ListenForData));
				clientRecieveThread.IsBackground = true;
				clientRecieveThread.Start();
			}else{
				messages.AddMessageToChat("Connection already stablished");
			}
		}
		catch (Exception e) { 			
			Debug.LogException(e, this);
		}

		Coroutine rts = StartCoroutine(AwaitForConnection());
		Coroutine acssl = StartCoroutine(loginManager.AwaitConnectionStablishmentAndSendLogin());
		Coroutine[] rutines = {rts, acssl};
		StartCoroutine(TimeOutEvent(rutines, 0, true)); // stop the routines if connected flag is not true after 5 seconds.
    }

	public void DisconnectFromTcpServer(){
		SendMessageToServer("Logout:");
	}
	
	public IEnumerator MesaggeOnMainThread(string message) {
		messages.AddMessageToChat(message);
		yield return null;
 	}
 
	public void MainThreadMessage(string message){
		dispatcher.Enqueue(MesaggeOnMainThread(message));
	}

	public IEnumerator TimeOutEvent(Coroutine[] routines, int flagIndex, bool expected){
		yield return new WaitForSeconds(timeout_seconds);

		Debug.Log("Flag value after login is: " + flags[flagIndex] + " and connected value is: " + connected);

		foreach (var r in routines)
		{
			StopCoroutine(r);
		}

		if(flags[flagIndex] != expected){
			MainThreadMessage("Timeout...");
		}
	}

	public IEnumerator AwaitForConnection(){
		while(socketConnection == null){
			// yield return new WaitForSeconds(seconds);
			yield return null;
		}
		
		while(!socketConnection.Connected){
			// yield return new WaitForSeconds(seconds);
			yield return null;
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
				MainThreadMessage("Client sent message: " + clientMessage);         
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
			code = "400";
			subcode = code[1] + "" + code[2];
			info = "error handling response, generated the following exception\n"+ e.ToString();
		}
		

		// codes:
		// 0XX -> connection codes.
		// 1XX -> actions (in game operations).
		// 2XX -> audio.
		// 3XX -> chat codes.
		// 400 -> error code.

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
				ErrorHandler(code, info);
				break;

			default: // Unrecognized code pattern
				MainThreadMessage("Unrecognized code pattern from server.");
				break;
		}
	}

	private void ConnectionSubcodeHandler(string subcode, string info){
		switch (subcode)
		{
			case "01"://LoginHandler
				string[] status_usr = info.Split(';');
				if(status_usr[0] == "accepted"){
					// Debug.Log("log the user in.");
					var usr_info = status_usr[1].Split(',');
					logged.id = int.Parse(usr_info[0]);
					logged.type = usr_info[1];
					logged.username = usr_info[2];

					dispatcher.Enqueue(loginManager.LoadMainMenu());
					MainThreadMessage("Success!");
				}
				if(status_usr[0] == "rejected"){
					Debug.Log("reject user login.");
					MainThreadMessage("Incorrect credentials, please try again...");
				}
				break;
			case "02"://Logout

				break;
			default:
				MainThreadMessage("subcode not handled.");
				break;
		}
	}


	private void ErrorHandler(string code, string info){
		MainThreadMessage("Error"+ code +": "+ info);
	}

}
