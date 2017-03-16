using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[NetworkSettings(channel = 1, sendInterval = 0.1f)]
public class NetworkRigidbody : NetworkBehaviour {

    public float lifeTime = -1;
    public float distanceTolerance = 0.1f;
    public float angleTolerance = 1.0f;

    float spawnTime;
    float syncTime;
    float syncDelay;
    float lastSyncTime;
    float lastSendTime;

    Vector3 syncPosition;
    Quaternion syncRotation;
    Vector3 syncVelocity;
    Vector3 syncAngularVelocity;
    Vector3 syncEndPosition;
    Quaternion syncEndRotation;

    private void Start()
    {
        if( isServer )
        {
            InitLeftTime();
        }
    }

    void Update () {

        if (isServer)
        {
            CheckLifeTime();
            UpdateSync();
        }
    }

    [Server]
    void InitLeftTime()
    {
        spawnTime = Time.time;
    }

    [Server]
    void CheckLifeTime()
    {
        if ( lifeTime > 0 && Time.time > spawnTime + lifeTime )
        {
            GameRoot.Instance.AGN.DestroyPhyscialObject(gameObject);
        }
    }

    [Server]
    void UpdateSync()
    {
        if( Time.time > lastSendTime + GetNetworkSendInterval() )
        {
            SetDirtyBit(1);
        }
    }

    [Client]
    void ApplySync()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        if( rb != null )
        {
            syncTime += Time.deltaTime;

            // calculate target position and rotation
            float targetLerpRatio = syncTime / syncDelay;
            Vector3 targetPosition = Vector3.Lerp(transform.position, syncEndPosition, targetLerpRatio);
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, syncEndRotation, targetLerpRatio);

            // smooth apply based on error and tolerance
            transform.position = Vector3.Lerp(transform.position, targetPosition, Mathf.Clamp01(Vector3.Distance(transform.position, targetPosition) / distanceTolerance));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Mathf.Clamp01(Quaternion.Angle(transform.rotation, targetRotation) / angleTolerance));

            // apply velocity and angular velocity
            rb.velocity = syncVelocity;
            rb.angularVelocity = syncAngularVelocity;
        }
    }

    public override bool OnSerialize(NetworkWriter writer, bool initialState)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        syncPosition = transform.position;
        syncRotation = transform.rotation;
        syncVelocity = rb.velocity;
        syncAngularVelocity = rb.angularVelocity;

        writer.Write(syncPosition);
        writer.Write(syncRotation);
        writer.Write(syncVelocity);
        writer.Write(syncAngularVelocity);

        return true;
    }

    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
        //Debug.Log("Deserialize");
        syncPosition = reader.ReadVector3();
        syncRotation = reader.ReadQuaternion();
        syncVelocity = reader.ReadVector3();
        syncAngularVelocity = reader.ReadVector3();

        syncTime = 0.0f;
        float now = Time.time;
        syncDelay = now - lastSyncTime;
        lastSyncTime = now;

        syncEndPosition = syncPosition + syncVelocity * syncDelay;
        syncEndRotation = syncRotation * Quaternion.AngleAxis( syncDelay, syncAngularVelocity );

        // apply server sync data once received it, use local physical calculation instead of update transform manually to get a much smoother result
        ApplySync();
    }
}
