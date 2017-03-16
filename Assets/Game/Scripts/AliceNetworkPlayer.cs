using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ProjectAlice;

public class AliceNetworkPlayer : NetworkBehaviour {

    public AliceHMD hmd;

    [SyncVar]
    public string playerAddress;
    [SyncVar]
    public string aliceServerAddress;
    [SyncVar]
    public int hmdDeviceCode;

    public bool activeHMD;

    public override void OnStartServer()
    {
        Init();
    }

    public override void OnStartClient()
    {
        Init();
    }

    private void OnDestroy()
    {
        Release();
    }

    private void Update()
    {
        UpdateAliceAddress();
        UpdateHMD();
    }

    void Init()
    {
        if (hmd == null)
        {
            hmd = GetComponentInChildren<AliceHMD>();
        }

        if (hmd != null)
        {
            hmd.deviceCode = hmdDeviceCode;
            Debug.Log("Set HMD deviceCode to " + hmd.deviceCode);
        }
        else
        {
            Debug.Log("Can not find player HMD");
        }

        GameRoot.Instance.AddPlayer(this);
    }

    void Release()
    {
        GameRoot.Instance.RemovePlayer(this);
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

        if( hmd.activeHMD != activeHMD )
        {
            if( activeHMD )
            {
                if( IsLocalDevice(hmdDeviceCode) )
                {
                    hmd.SetMode(AliceHMD.Mode.FollowHMD);
                    hmd.SetActiveHMD(true);
                    hmd.ResetCamera();
                }
                else
                {
                    hmd.SetMode(AliceHMD.Mode.FollowRigidbody);
                    hmd.SetActiveHMD(true);
                }
            }
            else
            {
                hmd.SetActiveHMD(false);
            }
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
