using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class NetworkManager : MonoBehaviour
{
    private const string typeName = "UniqueGameName";
    private const string gameName = "RoomName";

    private HostData[] hostList;

	/*
	 * This should be one of the few things instantiated in the editor other
	 * than maybe the terrain?
	 */
    public GameObject playerPrefab;

    private void StartServer()
    {
	
    }

    void OnServerInitialized()
    {
		//TODO: here, spawn camera player so you can get different camera angles
//        SpawnPlayer();
    }


    void Update()
    {
		//Check for connections?
    }
	
    void OnConnectedToServer()
    {
//        SpawnPlayer();
    }


    private void SpawnPlayer()
    {

//        Network.Instantiate(playerPrefab, Vector3.up * 5, Quaternion.identity, 0);
    }
}
