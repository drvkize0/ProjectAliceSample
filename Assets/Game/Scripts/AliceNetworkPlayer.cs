using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ProjectAlice;

public class AliceNetworkPlayer : NetworkBehaviour {

    public AliceHMD hmd;

    [SyncVar]
    public string aliceServerAddress;
    [SyncVar]
    public int hmdDeviceCode;

    private void Start()
    {
        if( hmd == null )
        {
            hmd = GetComponentInChildren<AliceHMD>();
        }

        if( hmd != null )
        {
            hmd.deviceCode = hmdDeviceCode;
            Debug.Log("Set HMD deviceCode to " + hmd.deviceCode);
        }
        else
        {
            Debug.Log("Can not find player HMD");
        }
    }

    private void Update()
    {
        UpdateAliceAddress();
        UpdateHMD();
    }

    void UpdateAliceAddress()
    {
        if( GameRoot.Instance.Settings.Tracking.AliceServerAddress == string.Empty )
        {
            GameRoot.Instance.Settings.Tracking.AliceServerAddress = aliceServerAddress;
        }
    }

    void UpdateHMD()
    {
        if( hmd.rigidbodyName == string.Empty )
        {
            hmd.rigidbodyName = TryGetDeviceName(hmdDeviceCode);
        }

        if( IsLocalDevice( hmdDeviceCode ) )
        {
            hmd.activeHMD = true;
        }
    }

    string TryGetDeviceName( int deviceCode )
    {
        AliceDeviceCfg config = AliceDeviceConfigs.Instance.GetConfigByDeviceCode(hmd.deviceCode);
        return config != null ? config.DeviceName : string.Empty;
    }

    bool IsLocalDevice( int deviceCode )
    {
        AliceDeviceCfg config = AliceDeviceConfigs.Instance.GetConfigByDeviceCode(hmd.deviceCode);
        return config != null ? config.IsLocal : false;
    }
}
