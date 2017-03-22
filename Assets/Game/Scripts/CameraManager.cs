using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectAlice;

public class CameraManager : MonoBehaviour
{
    public enum CameraMode
    {
        Unknown,
        ThirdPerson,
        FirstPerson
    }

    public Camera[] cameras;
    public CameraMode currentCameraMode = CameraMode.ThirdPerson;
    public int currentIndex = 0;
    public int lastCameraIndex = 0;
    public int lastHMDIndex = 0;
    public KeyCode toggleCameraModeKey = KeyCode.C;
    public KeyCode resetAllCameraKey;

    private void Update()
    { 
        RefreshCamera();
    }

    void RefreshCamera()
    {
        if (Input.GetKeyDown(toggleCameraModeKey))
        {
            CameraMode toMode = GetOppositeMode(currentCameraMode);
            int toIndex = toMode == CameraMode.ThirdPerson ? lastCameraIndex : lastHMDIndex;
            ToggleCamera(currentCameraMode, currentIndex, toMode, toIndex);
        }
    }

    private void OnGUI()
    {
        if (Event.current != null && Event.current.isKey && Event.current.keyCode >= KeyCode.Alpha1 && Event.current.keyCode <= KeyCode.Alpha9 )
        {
            // start count from 1, so camera 0 will should press 1
            int toIndex = Event.current.keyCode - KeyCode.Alpha1;
            ToggleCamera(currentCameraMode, currentIndex, currentCameraMode, toIndex);
        }
    }

    CameraMode GetOppositeMode( CameraMode mode )
    {
        return mode == CameraMode.FirstPerson ? CameraMode.ThirdPerson : CameraMode.FirstPerson;
    }

    void ToggleCamera( CameraMode fromMode, int fromIndex, CameraMode toMode, int toIndex )
    {
        if( fromMode == toMode && fromIndex == toIndex )
        {
            return;
        }

        // disable from camera
        if( fromMode == CameraMode.ThirdPerson )
        {
            SetCameraEnabled(fromIndex, false);
        }
        else if( fromMode == CameraMode.FirstPerson )
        {
            SetHMDEnabled(fromIndex, false);
        }

        // enable to camera
        if( toMode == CameraMode.ThirdPerson )
        {
            SetCameraEnabled(toIndex, true);
        }
        else if( toMode == CameraMode.FirstPerson )
        {
            SetHMDEnabled(toIndex, true);
        }
    }

    void SetCameraEnabled( int index, bool value )
    {
        if( index < cameras.Length )
        {
            cameras[index].gameObject.SetActive(value);
            if( value )
            {
                lastCameraIndex = index;
                currentIndex = index;
                currentCameraMode = CameraMode.ThirdPerson;
            }

            Debug.Log( ( value ? "Enable " : "Disable " ) + cameras[index].gameObject.name );
        }
        else
        {
            Debug.LogWarning("Can not find camera[" + index + "]" );
        }
    }

    void SetHMDEnabled( int index, bool value )
    {
        if( index < GameRoot.Instance.Players.Count )
        {
            GameRoot.Instance.Players[index].activeHMD = value;
            if( value )
            {
                lastHMDIndex = index;
                currentIndex = index;
                currentCameraMode = CameraMode.FirstPerson;
            }

            Debug.Log( ( value ? "Enable " : "Disable " ) + " HMD on " + GameRoot.Instance.Players[index].gameObject.name );
        }
        else
        {
            Debug.LogWarning("Can not find HMD on player[" + index + "]");
        }
    }
}
