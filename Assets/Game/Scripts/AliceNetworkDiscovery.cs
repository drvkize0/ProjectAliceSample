using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class AliceNetworkDiscovery : NetworkDiscovery {

    public string serverAddress = string.Empty;

    private void Start()
    {
        showGUI = false;
    }

    public void DiscoveryStartServer()
    {
        Initialize();
        StartAsServer();
        Debug.Log("Start discovery as server");
    }

    public void DiscoveryStartClient()
    {
        Initialize();
        StartAsClient();
        Debug.Log("Start discovery as client");
    }

    public void DiscoveryStop()
    {
        if (running)
        {
            StopBroadcast();
            serverAddress = string.Empty;
            Debug.Log("Stop discovery");
        }
    }

    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        Debug.Log("Discovered server from " + fromAddress.Replace("::ffff:", "" ) );
        serverAddress = fromAddress;
    }

    private void OnApplicationQuit()
    {
        DiscoveryStop();
    }
}
