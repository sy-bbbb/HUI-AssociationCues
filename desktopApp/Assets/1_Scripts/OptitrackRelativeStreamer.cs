using UnityEngine;
using Photon.Pun;

public class OptiTrackRelativeStreamer : MonoBehaviourPun
{
    [Header("External Managers")]
    public StudyConfigurationManager studyConfigurationManager;

    [Header("OptiTrack Rigid Bodies")]
    public OptitrackRigidBody phoneRigidBody;
    public OptitrackRigidBody hololensRigidBody;

    [Header("Streaming Settings")]
    public int sendRate = 60;
    public float positionThreshold = 0.002f;
    public float rotationThreshold = 0.3f;
    public bool sendWorldTransforms = true;

    [Header("Rotation Velocity Filter")]
    public bool enableRotationVelocityFilter = true;
    public float maxHeadAngularVelocity = 90f; // degrees per second
    //public float maxPhoneAngularVelocity = 120f; // degrees per second
    public float velocityFilterDuration = 0.1f; // how long to suppress after fast movement

    [Header("Coordinate System")]
    public bool flipYZ = false;
    public Vector3 positionScale = Vector3.one;
    public Vector3 rotationOffset = Vector3.zero;

    private Vector3 lastRelativePosition;
    private Quaternion lastRelativeRotation;
    private float sendInterval;
    private float lastSendTime;
    private PhotonView pv;

    private Quaternion lastHololensRotation;
    private Quaternion lastPhoneRotation;
    private float lastRotationCheckTime;
    private float lastFastMovementTime;
    private bool hasInitialRotations = false;


    void Start()
    {
        pv = PhotonView.Get(this);
        sendInterval = 1f / sendRate;
    }

    void Update()
    {
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom || studyConfigurationManager.HmdPlayer == null)
            return;

        if (Time.time - lastSendTime >= sendInterval)
        {
            CalculateAndSendRelativeTransform();
            lastSendTime = Time.time;
        }
    }

    void CalculateAndSendRelativeTransform()
    {
        bool phoneTracked = phoneRigidBody != null;
        bool hololensTracked = hololensRigidBody != null;

        if (!phoneTracked || !hololensTracked)
        {
            SendTrackingStatus(phoneTracked, hololensTracked);
            return;
        }

        Vector3 phoneWorldPos = ConvertPosition(phoneRigidBody.transform.position);
        Quaternion phoneWorldRot = ConvertRotation(phoneRigidBody.transform.rotation);
        Vector3 hololensWorldPos = ConvertPosition(hololensRigidBody.transform.position);
        Quaternion hololensWorldRot = ConvertRotation(hololensRigidBody.transform.rotation);

        if (enableRotationVelocityFilter && ShouldSuppressUpdateDueToFastRotation(hololensWorldRot, phoneWorldRot))
        {
            UpdateLastRotations(hololensWorldRot);
            return;
        }


        Vector3 relativePosition = CalculateRelativePosition(phoneWorldPos, hololensWorldPos, hololensWorldRot);
        Quaternion relativeRotation = CalculateRelativeRotation(phoneWorldRot, hololensWorldRot);

        if (ShouldSendUpdate(relativePosition, relativeRotation))
        {
            RelativeTransformData data = new RelativeTransformData
            {
                relativePosition = new float[] { relativePosition.x, relativePosition.y, relativePosition.z },
                relativeRotation = new float[] { relativeRotation.x, relativeRotation.y, relativeRotation.z, relativeRotation.w },
                timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                phoneTracked = phoneTracked,
                hololensTracked = hololensTracked
            };

            if (sendWorldTransforms)
            {
                data.phoneWorldPosition = new float[] { phoneWorldPos.x, phoneWorldPos.y, phoneWorldPos.z };
                data.phoneWorldRotation = new float[] { phoneWorldRot.x, phoneWorldRot.y, phoneWorldRot.z, phoneWorldRot.w };
                data.hololensWorldPosition = new float[] { hololensWorldPos.x, hololensWorldPos.y, hololensWorldPos.z };
                data.hololensWorldRotation = new float[] { hololensWorldRot.x, hololensWorldRot.y, hololensWorldRot.z, hololensWorldRot.w };
            }

            pv.RPC("ReceiveRelativeTransform", studyConfigurationManager.HmdPlayer, JsonUtility.ToJson(data));

            lastRelativePosition = relativePosition;
            lastRelativeRotation = relativeRotation;
        }

        UpdateLastRotations(hololensWorldRot);
    }

    bool ShouldSuppressUpdateDueToFastRotation(Quaternion currentHololensRot, Quaternion currentPhoneRot)
    {
        if (!hasInitialRotations)
            return false;

        float deltaTime = Time.time - lastRotationCheckTime;
        if (deltaTime <= 0f) return false;

        float hololensAngularVelocity = GetAngularVelocity(lastHololensRotation, currentHololensRot, deltaTime);

        bool headTooFast = hololensAngularVelocity > maxHeadAngularVelocity;

        if (headTooFast)
        {
            lastFastMovementTime = Time.time;
            Debug.Log($"Suppressing update - Head: {hololensAngularVelocity:F1}¡Æ/s");
            return true;
        }

        // Also suppress for a short duration after fast movement
        bool stillInSuppressionWindow = (Time.time - lastFastMovementTime) < velocityFilterDuration;
        return stillInSuppressionWindow;
    }

    float GetAngularVelocity(Quaternion from, Quaternion to, float deltaTime)
    {
        if (deltaTime <= 0f) return 0f;

        float angleDifference = Quaternion.Angle(from, to);
        return angleDifference / deltaTime;
    }

    void UpdateLastRotations(Quaternion hololensRot)
    {
        lastHololensRotation = hololensRot;
        lastRotationCheckTime = Time.time;
        hasInitialRotations = true;
    }

    Vector3 CalculateRelativePosition(Vector3 phoneWorld, Vector3 hololensWorld, Quaternion hololensRotation)
    {
        Vector3 worldOffset = phoneWorld - hololensWorld;
        Vector3 localOffset = Quaternion.Inverse(hololensRotation) * worldOffset;

        return localOffset;
    }

    Quaternion CalculateRelativeRotation(Quaternion phoneRotation, Quaternion hololensRotation)
    {
        return Quaternion.Inverse(hololensRotation) * phoneRotation;
    }

    Vector3 ConvertPosition(Vector3 optiTrackPos)
    {
        Vector3 converted = optiTrackPos;

        if (flipYZ)
            converted = new Vector3(optiTrackPos.x, optiTrackPos.z, optiTrackPos.y);

        converted.Scale(positionScale);

        return converted;
    }

    Quaternion ConvertRotation(Quaternion optiTrackRot)
    {
        Quaternion converted = optiTrackRot;

        if (flipYZ)
            converted = new Quaternion(optiTrackRot.x, optiTrackRot.z, optiTrackRot.y, -optiTrackRot.w);

        if (rotationOffset != Vector3.zero)
            converted *= Quaternion.Euler(rotationOffset);

        return converted;
    }

    bool ShouldSendUpdate(Vector3 currentRelativePos, Quaternion currentRelativeRot)
    {
        float positionDelta = Vector3.Distance(currentRelativePos, lastRelativePosition);
        float rotationDelta = Quaternion.Angle(currentRelativeRot, lastRelativeRotation);
        return positionDelta > positionThreshold || rotationDelta > rotationThreshold;
    }

    void SendTrackingStatus(bool phoneTracked, bool hololensTracked)
    {
        RelativeTransformData data = new RelativeTransformData
        {
            phoneTracked = phoneTracked,
            hololensTracked = hololensTracked,
            timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        pv.RPC("ReceiveRelativeTransform", studyConfigurationManager.HmdPlayer, JsonUtility.ToJson(data));
    }

}

[System.Serializable]
public class RelativeTransformData
{
    public float[] relativePosition = new float[3];
    public float[] relativeRotation = new float[4];
    public float[] phoneWorldPosition = new float[3];
    public float[] phoneWorldRotation = new float[4];
    public float[] hololensWorldPosition = new float[3];
    public float[] hololensWorldRotation = new float[4];
    public long timestamp;
    public bool phoneTracked;
    public bool hololensTracked;
}
