using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UIChat;
using Newtonsoft.Json.Linq;

public class ConnectionManager : MonoBehaviour
{
    public string ipaddr = "127.0.0.1";
    public int port = 30069;
	public float timeout_seconds = 5f;

    public TcpClient socketConnection;
    public Thread clientRecieveThread;
	public LoginManager loginManager;
	public MainMenuController mmctr;
	public bool connected = false;
	MessagesController messages;
	public UnityMainThreadDispatcher dispatcher;
	public user logged;
	public world loadedWorld;
	public FileSync fileSync;

	public Dictionary<int, Dictionary<int, byte[]>> incoming_messages = new Dictionary<int, Dictionary<int, byte[]>>(); // for messages bigger than the buffer size.

	public Dictionary<int, int> message_parts_count = new Dictionary<int, int>();

	[Serializable]
	public class user{
		public int id;
		public string type, username;
	}

	[Serializable]
	public class world{
        public int ID;
        public string System;
		public string Name;
        public int owner;
		public int[] players;
		public scene[] scenes;
    }

	[Serializable]
	public class scene{
        public int ID;
        public string Name;
		public string FilePath;
    }

	private void Start() {
		DontDestroyOnLoad(this.gameObject);
		messages = loginManager.messages;
	}
	
	public IEnumerator MesaggeOnMainThread(string message) {
		messages.AddMessageToChat(message);
		yield return null;
 	}
 
	public void MainThreadMessage(string message){
		dispatcher.Enqueue(MesaggeOnMainThread(message));
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

		StartCoroutine(AwaitForConnection());
		StartCoroutine(loginManager.AwaitConnectionStablishmentAndSendLogin());
    }


    /// <summary>
	/// Runs in background clientReceiveThread; Listens for incomming data. 	
	/// </summary>
	private void ListenForData() {
		try {
			socketConnection = new TcpClient(ipaddr, port);
			Byte[] bytes = new Byte[16384];

			while (true) {
				// Get a stream object for reading
				using (NetworkStream stream = socketConnection.GetStream()) {
					int length;
					// Read incomming stream into byte arrary. 					
					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) { 						
						var incommingData = new byte[length]; 						
						Array.Copy(bytes, 0, incommingData, 0, length);
						
						// Convert byte array to string message. 						
						string serverMessage = Encoding.UTF8.GetString(incommingData);
						

						// add the files to the dictionary
						bool isLast = false;
						var aux = serverMessage.Split(';')[0];
						Debug.Log("server message header was: " + aux);
						if(aux[aux.Length-1] == 't'){
							isLast = true;
							aux = aux.Replace("t","");
						}

						var a = aux.Split(':');
						var padding = int.Parse(a[0]);
						var msgPart = int.Parse(a[1]);
						var msgID = int.Parse(a[2]);
						
						// Debug.Log("server message received as: " + serverMessage);
						// serverMessage = serverMessage.Substring(serverMessage.IndexOf(';') + 1, 16384-padding);
						var i = Array.IndexOf(incommingData, Convert.ToByte(';'))+1;
						var l = 16384-i-padding;
						Byte[] prunedData = new Byte[l];
						Array.Copy(incommingData, i, prunedData, 0, l); 

						try{
							incoming_messages[msgID][msgPart] = prunedData;
						}catch{
							incoming_messages[msgID] = new Dictionary<int, byte[]>();
							incoming_messages[msgID][msgPart] = prunedData;
						}
						
						if(isLast){ //if the server says this is the last part of the file, we get the number of the part and add it to the list.
							message_parts_count[msgID] = msgPart;
						}

						
						if(message_parts_count.ContainsKey(msgID)){
							//if message is complete proceed
							Debug.Log("total number of parts: " + (message_parts_count[msgID]+1).ToString() + 
							", recieved part: "+ incoming_messages[msgID].Values.Count.ToString());

							if(incoming_messages[msgID].Values.Count >= message_parts_count[msgID]){
								var msgbytes = CombineBytes(incoming_messages[msgID].Values.ToList());
								ResponseHandler(msgbytes, msgID);
							}
							
						}
					}
				} 			
			}   
		}         
		catch (SocketException socketException) {             
			Debug.Log("Socket exception: " + socketException);
			MainThreadMessage("Socket exception: " + socketException);
		}     
	}

    /// <summary> 	
	/// Send message to server using socket connection, the message must not contain newline elements.	
	/// </summary> 	
	public void SendMessageToServer(string clientMessage) {         
		if (socketConnection == null) {             
			return;         
		}  		

		clientMessage = clientMessage + "\n";

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
	private void ResponseHandler(byte[] msgbytes, int msgID){
			var message = Encoding.UTF8.GetString(msgbytes);
			Debug.Log("message length in bytes: " + msgbytes.Length + " content: " + message);
			
			char[] separator = {':'};
			String[] m = message.Split(separator, 2, StringSplitOptions.None);
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

		byte[] dst = new Byte[msgbytes.Length-4];
		Array.Copy(msgbytes, 4, dst, 0, dst.Length);
		
		// codes:
		// 0XX -> connection codes.
		// 1XX -> actions (in game operations).
		// 2XX -> audio.
		// 3XX -> chat codes.
		// 400 -> error code.

		switch (code[0])
		{
			case '0':
				ConnectionSubcodeHandler(subcode, info, dst, msgID);
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

	/// <summary> 	
	/// Handle the different Connection opperations.	
	/// </summary>
	private void ConnectionSubcodeHandler(string subcode, string info, byte[] msgBytes, int msgID){
		switch (subcode)
		{
			case "01"://Login
				string[] status_usr = info.Split(';');
				if(status_usr[0] == "accepted"){
					var usr_info = status_usr[1].Split(',');
					logged = new user();

					logged.id = int.Parse(usr_info[0]);
					logged.type = usr_info[1];
					logged.username = usr_info[2];
					
					MainThreadMessage("Success!");
					dispatcher.Enqueue(fileSync.SendFileInformationToServer());
				}
				if(status_usr[0] == "rejected"){
					Debug.Log("reject user login.");
					MainThreadMessage("Incorrect credentials, please try again...");
				}
				break;

			case "02"://Logout
				//Changes the state of the gameobjects from active to inactive or viceversa.
				dispatcher.Enqueue(loginManager.activeStateChange());
				dispatcher.Enqueue(mmctr.activeStateChange());
				
				// loginManager.gameObject.SetActive(true);
        		// mmctr.gameObject.SetActive(false);
				break;

			case "03": //World Information for master.
				Debug.Log("world information recieved: " + info);
				MainThreadMessage("world information recieved: " + info);
				List<world> worldsToSend = new List<world>();

				JObject worlds = JObject.Parse(info);
				
				foreach (var w in worlds["worlds"]){
					string worldString = w.ToString();
					Debug.Log(worldString);
					var worldToSend = JsonUtility.FromJson<world>(worldString);
					worldsToSend.Add(worldToSend);
				}

				dispatcher.Enqueue(loginManager.mainMenuController.LoadMainMenuForMaster(worldsToSend));
				break;

			case "04": // Loaded world accepted.
				Debug.Log("World information recieved is: "+ info);
				MainThreadMessage("scene information recieved is: "+ info);
				loadedWorld = JsonUtility.FromJson<world>(info);
				Debug.Log("Loaded world => ID:" + loadedWorld.ID +  " Name:"  +loadedWorld.Name);
				dispatcher.Enqueue(mmctr.LoadWorldInformation());
				break;

			case "05": //sending file or folder data
				var parts = info.Split(';');
				// Debug.Log("recieved file: "+ info);
				var trailstring = "";
				var protocol = parts[0];
				if(protocol == "info"){
					trailstring = parts[1];
					dispatcher.Enqueue(fileSync.SetInfo(trailstring));
				}else{
					// create, update, delete, info
					var type = parts[1]; // file, folder
					var path = parts[2]; // folderpath or filepath
					var datetime = DateTime.UtcNow;
					byte[] dst = new Byte[0];
					if((protocol == "create" || protocol == "update") && type == "file"){
						datetime = DateTime.Parse(parts[3]);
						trailstring = parts[4];

						var infosum = protocol.Length + path.Length + type.Length + parts[3].Length + 4; // +4 because of the ; separators
						try{
							dst = new Byte[msgBytes.Length-infosum];
							Array.Copy(msgBytes, infosum, dst, 0, dst.Length);
						}catch(OverflowException e){
							Debug.Log("recieved an empty file: "+ e.Message);
						}
					}

					dispatcher.Enqueue(fileSync.FileRecieve(msgID, protocol, type, path, datetime, dst, trailstring));
				}

				break;

			default:
				MainThreadMessage("subcode not handled.");
				break;
		}
	}

	private void ErrorHandler(string code, string info){
		MainThreadMessage("Error"+ code +": "+ info);
	}

	private static byte[] CombineBytes(List<Byte[]> arrays)
	{
		byte[] bytes = new byte[arrays.Sum(a => a.Length)];
		int offset = 0;
	
		foreach (byte[] array in arrays)
		{
			Buffer.BlockCopy(array, 0, bytes, offset, array.Length);
			offset += array.Length;
		}
	
		return bytes;
	}

	private string GetByteValues(byte[] toConv){
		string byte_represent = "";
		foreach (var b in toConv)
		{
			byte_represent += " "+b.ToString();
		}
		return byte_represent;
	}
}


public static class ListExtra
	{
		public static void Resize<T>(this List<T> list, int sz, T c)
		{
			int cur = list.Count;
			if(sz < cur)
				list.RemoveRange(sz, cur - sz);
			else if(sz > cur)
			{
				if(sz > list.Capacity)//this bit is purely an optimisation, to avoid multiple automatic capacity changes.
				list.Capacity = sz;
				list.AddRange(Enumerable.Repeat(c, sz - cur));
			}
		}
		public static void Resize<T>(this List<T> list, int sz) where T : new()
		{
			Resize(list, sz, new T());
		}
	}
