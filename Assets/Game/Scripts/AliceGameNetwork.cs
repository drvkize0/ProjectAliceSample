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
        if( !isNetworkActive && discovery.serverAddress != string.Empty )
        {
            networkAddress = discovery.serverAddress;
            StartClient();
            Debug.Log("Connect to " + networkAddress + " as client");
        }
    }

    public override void OnStartServer()
    {
        // when server started, switch to discovery as server
        discovery.DiscoveryStop();
        discovery.DiscoveryStartServer();
        GetComponent<NetworkManagerHUD>().showGUI = false;
    }

    public override void OnStopHost()
    {
        // when server stopped, swithc to discovery as client
        discovery.DiscoveryStop();
        discovery.DiscoveryStartClient();
        GetComponent<NetworkManagerHUD>().showGUI = true;
    }

    public override void OnStartClient(NetworkClient client)
    {
        // when client started, stop discovery as client
        discovery.DiscoveryStop();
        GetComponent<NetworkManagerHUD>().showGUI = false;
    }

    public override void OnStopClient()
    {
        // when client stopped, start discovery as client
        discovery.DiscoveryStartClient();
        GetComponent<NetworkManagerHUD>().showGUI = true;
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        var player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        AliceNetworkPlayer netPlayer = player.GetComponent<AliceNetworkPlayer>();

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

        netPlayer.hmdDeviceCode = config.DeviceCode;

        Debug.Log("Add player with hmdDeviceCode " + netPlayer.hmdDeviceCode);
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }

    public string LocalIPAddress( string localAddress )
    {
        IPHostEntry host;
        string localIP = "";
        host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                
                localIP = ip.ToString();
                Debug.Log(localIP);
                //break;
            }
        }
        return localIP;
    }
}
