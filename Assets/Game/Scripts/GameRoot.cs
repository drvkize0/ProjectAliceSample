using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectAlice;

public class GameRoot : Singleton<GameRoot> {

    public string settingsPath;

    AliceInstanceManager aig;
    GameSettings settings = new GameSettings();

    public GameSettings Settings { get { return settings; } }

    private void OnEnable()
    {
        settings.LoadSettings(settingsPath);
    }

    private void OnDisable()
    {
        settings.SaveSettings();
    }

    void Start () {

        DontDestroyOnLoad(this);

        if ( aig == null )
        {
            aig = FindObjectOfType<AliceInstanceManager>();
        }
        if( aig != null )
        {
            DontDestroyOnLoad(aig);
            aig.StreamIP = settings.Tracking.AliceServerAddress;
        }
    }
}
