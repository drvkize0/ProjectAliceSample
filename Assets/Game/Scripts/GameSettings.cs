using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectAlice.Utilities;

// use easy json to serialize settings to json file
public class PlayerSettings
{
    public PlayerSettings()
    {
        this.HMDName = string.Empty;
    }

    public PlayerSettings( string HMDName )
    {
        this.HMDName = HMDName;
    }

    public string HMDName;
}

public class TrackingList : Profile
{
    [ProfileKey]
    public string profileName = "DefaultSettings";

    public string AliceServerAddress = "192.168.0.100";
    public Dictionary<string, PlayerSettings> Players = new Dictionary<string, PlayerSettings>();
    public List<string> Controllers = new List<string>();
    public List<string> Properties = new List<string>();

    #region access methods

    public string ServerAddress { get { return AliceServerAddress; } set { AliceServerAddress = value; } }

    public void AddPlayer( string address, string HMDName )
    {
        Players.Add(address, new PlayerSettings( HMDName ) );
    }

    public void RemovePlayer( string address )
    {
        Players.Remove(address);
    }

    public void AddController(string name)
    {
        Controllers.Add(name);
    }

    public void RemoveController(string name)
    {
        Controllers.Remove(name);
    }

    public void AddProperty(string name)
    {
        Controllers.Add(name);
    }

    public void RemoveProperty(string name)
    {
        Properties.Remove(name);
    }

    public int PlayerCount { get { return Players.Count; } }

    public PlayerSettings GetPlayer(int index)
    {
        PlayerSettings[] playerArray = Players.Values.ToArray();
        return index < playerArray.Length ? playerArray[index] : null;
    }

    public int ControllerCount { get { return Controllers.Count; } }    

    public string GetControllerName(int index)
    {
        return index < Controllers.Count ? Controllers[index] : string.Empty;
    }

    public int PropertyCount { get { return Properties.Count; } }

    public string GetPropertyName(int index)
    {
        return index < Properties.Count ? Properties[index] : string.Empty;
    }

    #endregion
}

public class GameSettings
{
    ProfileManager<TrackingList> manager;
    TrackingList list;

    public string path;
    public TrackingList Tracking { get { return list; } }

    public void LoadSettings(string path)
    {
        manager = ProfileManager<TrackingList>.LoadProfile(path);
        list = manager.GetProfile("DefaultSettings");
        if( list != null )
        {
            Debug.Log( "Default settings loaded" );
        }
        else
        {
            Debug.LogWarning("Game settings file not found, path = " + path + " rebuild with default value");
            list = manager.GetOrCreateProfile("DefaultSettings", new TrackingList() );
            Tracking.AddPlayer("192.168.0.100", "N/A");
            Debug.Log(Tracking.Players.Count + " players saved");
        }
    }

    public void SaveSettings()
    {
        ProfileManager<TrackingList>.SaveProfile(manager);
        Debug.Log("Game settings saved");
    }
}
