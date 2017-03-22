using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using ProjectAlice;

public class AliceGameNetwork : NetworkManager {

    public AliceNetworkDiscovery discovery;

    // main routine
    public void Start()
    {
        // start as discovery client
        discovery.DiscoveryStartClient();
    }

    public void Update()
    {
        // if we are not server, and discovery has return a valid server address, connect to this address
        if( !isNetworkActive && discovery.serverAddress != string.Empty )
        {
            networkAddress = discovery.serverAddress;
            StartClient();
            Debug.Log("Connect to " + networkAddress + " as client");
        }
    }

    public override void OnStartServer()
    {
        // when start as a server, switch discovery as server, broadcast my address
        discovery.DiscoveryStop();
        discovery.DiscoveryStartServer();
        GetComponent<NetworkManagerHUD>().showGUI = false;
    }

    public override void OnStopHost()
    {
        // when server stopped, switch discovery back as client
        discovery.DiscoveryStop();
        discovery.DiscoveryStartClient();
        GetComponent<NetworkManagerHUD>().showGUI = true;
    }

    public override void OnStartClient(NetworkClient client)
    {
        // when started as client, means we already had the server address, stop discovery.
        discovery.DiscoveryStop();
        GetComponent<NetworkManagerHUD>().showGUI = false;
    }

    public override void OnStopClient()
    {
        // when client stopped, turn on this discovery again
        discovery.DiscoveryStartClient();
        GetComponent<NetworkManagerHUD>().showGUI = true;
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        var player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        AliceNetworkPlayer netPlayer = player.GetComponent<AliceNetworkPlayer>();

        // find player hmd name by player IP address from settings
        PlayerSettings playerSettings = null;
        string addressInSettings = conn.address.Replace("::ffff:", "");
        GameRoot.Instance.Settings.Tracking.Players.TryGetValue( addressInSettings, out playerSettings );

        if( playerSettings == null )
        {
            Debug.LogWarning("Do no find settings for player " + addressInSettings);
            return;
        }

        AliceDeviceCfg config = AliceDeviceConfigs.Instance.GetConfigByDeviceName(playerSettings.HMDName);
        if( config == null )
        {
            Debug.LogWarning("Do no receive config for device " + playerSettings.HMDName);
            return;
        }

        // assign alice server address, and hmd device code to player
        netPlayer.aliceServerAddress = GameRoot.Instance.Settings.Tracking.AliceServerAddress;
        netPlayer.hmdDeviceCode = config.DeviceCode;
        Debug.Log("Add player with hmdDeviceCode " + netPlayer.hmdDeviceCode);

        // assign other values for the player here

        // network spawn
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }
}
