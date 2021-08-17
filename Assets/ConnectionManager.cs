using System.Collections;
using System.Collections.Generic;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    public string ipaddr = "127.0.0.1";
    public int port = 30069;

    private TcpClient socketConnection;
    private Thread clientRecieveThread;

    /// <summary> 	
	/// Setup socket connection. 	
	/// </summary> 
    public void connectToTCPServer(){
        try {  			
			clientRecieveThread = new Thread (new ThreadStart(ListenForData)); 			
			clientRecieveThread.IsBackground = true; 			
			clientRecieveThread.Start();
		} 		
		catch (Exception e) { 			
			Debug.LogException(e, this);	
		} 	
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
	new private void SendMessage(string clientMessage) {         
		if (socketConnection == null) {             
			return;         
		}  		

		try { 			
			// Get a stream object for writing. 			
			NetworkStream stream = socketConnection.GetStream(); 			
			if (stream.CanWrite) {                 
				// Convert string message to byte array.                 
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage); 				
				// Write byte array to socketConnection stream.                 
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);                 
				Debug.Log("Client sent his message - should be received by server");             
			}         
		} 		
		catch (SocketException socketException) {             
			Debug.Log("Socket exception: " + socketException);         
		}     
	} 

}
