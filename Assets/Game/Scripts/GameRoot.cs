using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectAlice;

public class GameRoot : Singleton<GameRoot> {

    public string settingsPath;

    GameSettings settings = new GameSettings();
    AliceInstanceManager aig;
    AliceGameNetwork agn;

    public GameSettings Settings { get { return settings; } }

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

        if ( aig == null )
        {
            aig = FindObjectOfType<AliceInstanceManager>();
        }

        if( agn == null )
        {
            agn = FindObjectOfType<AliceGameNetwork>();
        }

        if( aig != null )
        {
            aig.trackingOnStart = false;
            DontDestroyOnLoad(aig);
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
}
