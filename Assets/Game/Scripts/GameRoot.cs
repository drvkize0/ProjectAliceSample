using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectAlice;

public class GameRoot : Singleton<GameRoot> {

    public string settingsPath;

    GameSettings settings = new GameSettings();
    AliceInstanceManager aig;
    AliceGameNetwork agn;
    CameraManager cm;

    List<AliceNetworkPlayer> players = new List<AliceNetworkPlayer>();

    public GameSettings Settings { get { return settings; } }
    public AliceInstanceManager AIG { get { return aig; } }
    public AliceGameNetwork AGN { get { return agn; } }
    public CameraManager CM {  get { return cm; } }
    public List<AliceNetworkPlayer> Players { get { return players; } }

    private void OnEnable()
    {
        settings.LoadSettings(settingsPath);
    }

    private void OnDisable()
    {
        // settings.SaveSettings();
    }

    void Start () {

        DontDestroyOnLoad(this);

        // try get managers

        if ( aig == null )
        {
            aig = FindObjectOfType<AliceInstanceManager>();
        }

        if( agn == null )
        {
            agn = FindObjectOfType<AliceGameNetwork>();
        }

        if(cm == null )
        {
            cm = FindObjectOfType<CameraManager>();
        }

        if( aig != null )
        {
            aig.trackingOnStart = false;
            DontDestroyOnLoad(aig);
        }

        if(cm != null )
        {
            DontDestroyOnLoad(cm);
        }
    }

    private void Update()
    {
        // start tracking
        if( aig != null && Settings.Tracking.AliceServerAddress != string.Empty && aig.StreamIP == string.Empty )
        {
            aig.StreamIP = settings.Tracking.AliceServerAddress;
            aig.StartTrack(Settings.Tracking.AliceServerAddress);
            Debug.Log("Tracking started");
        }
    }

    private void OnApplicationQuit()
    {
        // stop tracking
        if( aig != null && aig.StreamIP != string.Empty )
        {
            aig.StopTrack();
            Debug.Log("Tracking stopped");
        }
    }

    public void AddPlayer( AliceNetworkPlayer player )
    {
        players.Add(player);
        Debug.Log("Player " + player.playerAddress + " added");
    }

    public void RemovePlayer( AliceNetworkPlayer player )
    {
        players.Remove(player);
        Debug.Log("Player " + player.playerAddress + " removed");
    }
}
