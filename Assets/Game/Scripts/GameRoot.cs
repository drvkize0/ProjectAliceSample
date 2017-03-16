using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectAlice;

public class GameRoot : Singleton<GameRoot> {

    public string settingsPath;

    GameSettings settings = new GameSettings();
    AliceInstanceManager aim;
    AliceGameNetwork agn;
    CameraManager cm;

    List<AliceNetworkPlayer> players = new List<AliceNetworkPlayer>();
    List<NetworkRigidbody> physicalObjects = new List<NetworkRigidbody>();

    public GameSettings Settings { get { return settings; } }
    public AliceInstanceManager AIM { get { return aim; } }
    public AliceGameNetwork AGN { get { return agn; } }
    public CameraManager CM {  get { return cm; } }
    public List<AliceNetworkPlayer> Players { get { return players; } }
    public List<NetworkRigidbody> PhysicalObjects { get { return physicalObjects; } }
    public bool useAlice = false;

    private void OnEnable()
    {
        settings.LoadSettings(settingsPath);
    }

    private void OnDisable()
    {
        //settings.SaveSettings();
    }

    void Start () {

        DontDestroyOnLoad(this);

        InitAIM();

        InitAGN();

        InitCM();
    }

    private void Update()
    {
        if( useAlice )
        {
            StartAIMTracking();
        }

        if( Input.GetKeyDown( KeyCode.Space ) )
        {
            AGN.SpawnPhysicalObject();
        }

        if( Input.GetKeyDown( KeyCode.Minus ) )
        {
            if( physicalObjects.Count > 0 )
            {
                AGN.DestroyPhyscialObject(physicalObjects[0].gameObject);
            }
        }
    }

    private void OnApplicationQuit()
    {
        StopAIMTracking();
    }

    #region handle AIM

    private void InitAIM()
    {
        if (aim == null)
        {
            aim = FindObjectOfType<AliceInstanceManager>();
        }

        if (aim != null)
        {
            aim.trackingOnStart = false;
            DontDestroyOnLoad(aim);
        }
    }

    private void StartAIMTracking()
    {
        if (aim != null && Settings.Tracking.AliceServerAddress != string.Empty && aim.StreamIP == string.Empty)
        {
            aim.StreamIP = settings.Tracking.AliceServerAddress;
            aim.StartTrack(Settings.Tracking.AliceServerAddress);
            Debug.Log("Tracking started", gameObject );
        }
    }

    private void StopAIMTracking()
    {
        if (aim != null && aim.StreamIP != string.Empty)
        {
            aim.StopTrack();
            Debug.Log("Tracking stopped", gameObject );
        }
    }

    #endregion

    #region handle AGN

    private void InitAGN()
    {
        if (agn == null)
        {
            agn = FindObjectOfType<AliceGameNetwork>();
        }
    }

    #endregion

    #region handle CM
    private void InitCM()
    {
        if (cm == null)
        {
            cm = FindObjectOfType<CameraManager>();
        }

        if (cm != null)
        {
            DontDestroyOnLoad(cm);
        }
    }
    #endregion

    #region spawn & destroy players
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
    #endregion

    #region spawn & destroy physical objects
    public void AddPhysicalObject( GameObject obj )
    {
        physicalObjects.Add(obj.GetComponent<NetworkRigidbody>());
        Debug.Log("Spawn physical object");
    }

    public void RemovePhysicalObject( GameObject obj )
    {
        physicalObjects.Remove(obj.GetComponent<NetworkRigidbody>());
        Debug.Log("Destroy physical object");
    }
    #endregion
}
