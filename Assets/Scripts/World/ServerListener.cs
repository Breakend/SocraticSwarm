
/*
	Listener to receive UDP messages
*/
using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class ServerListener : MonoBehaviour {
	
	// receiving Thread
	Thread receiveThread;
	
	// udpclient object
	UdpClient client;
	
	public const int LISTENER_PORT = 8052;
	// infos
	public string lastReceivedUDPPacket="";
	public string allReceivedUDPPackets=""; // clean up this from time to time!

	// start from unity3d
	public void Start()
	{
		init();
	}
	
	// GUI for testing the thing
	void OnGUI()
	{
		Rect rectObj=new Rect(40,10,200,400);
		GUIStyle style = new GUIStyle();
		style.alignment = TextAnchor.UpperLeft;
		GUI.Box(rectObj,"# UDPReceive\n127.0.0.1 "+LISTENER_PORT+" #\n"
		        + "shell> nc -u 127.0.0.1 : "+LISTENER_PORT+" \n"
		        + "\nLast Packet: \n"+ lastReceivedUDPPacket
		        + "\n\nAll Messages: \n"+allReceivedUDPPackets
		        ,style);
	}
	
	/*
	 * Launch thread for listening on port
	 **/
	private void init()
	{

		receiveThread = new Thread(new ThreadStart(ReceiveData));
		receiveThread.IsBackground = true;
		receiveThread.Start();
		
	}
	
	/*
	 * Receive data on the thread
	 * */
	private  void ReceiveData()
	{
		client = new UdpClient(LISTENER_PORT);
		while (true)
		{
			
			try
			{

				// Bytes empfangen.
				IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
				byte[] data = client.Receive(ref anyIP);
				
			
				string text = Encoding.UTF8.GetString(data);

				//TODO: remove this and have an enterpreter pass messages
				// to a unity game loop
				// latest UDPpacket
				lastReceivedUDPPacket=text;

				// ....
				allReceivedUDPPackets=allReceivedUDPPackets+text;
				
			}
			catch (Exception err)
			{
				print(err.ToString());
			}
		}
	}
	
	// getLatestUDPPacket
	// cleans up the rest
	public string getLatestUDPPacket()
	{
		allReceivedUDPPackets="";
		return lastReceivedUDPPacket;
	}
}