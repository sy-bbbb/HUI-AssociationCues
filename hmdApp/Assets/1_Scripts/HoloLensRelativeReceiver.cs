using UnityEngine;
using Photon.Pun;

public class HoloLensRelativeReceiver : MonoBehaviourPun
{
    [Header("Phone Representation")]
    //private Transform phoneRepresentation;
    public Transform phoneLogicalPosition;
    //public GameObject phoneModel;

    [Header("Relative Transform")]
    public bool useRelativeTransform = true;
    public float maxDisplayDistance = 3f;

    [Header("Smoothing")]
    public bool useSmoothing = true;
    public float positionSmoothing = 20f;
    public float rotationSmoothing = 20f;
    public float snapDistance = 0.5f;


    private Vector3 currentRelativePosition;
    private Quaternion currentRelativeRotation;
    private bool phoneTracked = false;
    private bool hololensTracked = false;
    private float lastUpdateTime;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool hasValidTarget = false;

    void Start()
    {
        //if (phoneModel != null)
        //{
        //    phoneLogicalPosition = phoneModel.transform;
        //    //GameObject logicalPhone = new GameObject("PhoneLogicalPosition");
        //    //phoneLogicalPosition = logicalPhone.transform;
        //}

    }

    [PunRPC]
    void ReceiveRelativeTransform(string jsonData)
    {
        try
        {
            RelativeTransformData data = JsonUtility.FromJson<RelativeTransformData>(jsonData);

            phoneTracked = data.phoneTracked;
            hololensTracked = data.hololensTracked;
            lastUpdateTime = Time.time;

            if (phoneTracked && hololensTracked)
            {
                currentRelativePosition = new Vector3(
                    data.relativePosition[0],
                    data.relativePosition[1],
                    data.relativePosition[2]
                );

                currentRelativeRotation = new Quaternion(
                    data.relativeRotation[0],
                    data.relativeRotation[1],
                    data.relativeRotation[2],
                    data.relativeRotation[3]
                );

                hasValidTarget = true;
                UpdatePhoneVisualization();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to parse relative transform data: {e.Message}");
        }
    }

    void Update()
    {
        if (Time.time - lastUpdateTime > 2f)
        {
            phoneTracked = false;
            hololensTracked = false;
        }

        if (hasValidTarget && phoneTracked && hololensTracked)
            UpdatePhonePosition();
    }

    void UpdatePhoneVisualization()
    {
        //if (phoneRepresentation == null) return;

        float distance = currentRelativePosition.magnitude;
        bool withinRange = distance <= maxDisplayDistance;

        if (!withinRange) return;

        Transform cameraTransform = Camera.main.transform;
        targetPosition = cameraTransform.TransformPoint(currentRelativePosition);
        targetRotation = cameraTransform.rotation * currentRelativeRotation;
    }

    void UpdatePhonePosition()
    {
        if (!hasValidTarget) return;

        // Update logical position immediately (no smoothing)
        //if (phoneLogicalPosition != null)
        //{
        //    phoneLogicalPosition.position = targetPosition;
        //    phoneLogicalPosition.rotation = targetRotation;
        //}

        //// Update visual representation with smoothing
        float distance = Vector3.Distance(phoneLogicalPosition.position, targetPosition);

        if (useSmoothing && distance < snapDistance)
        {
            phoneLogicalPosition.position = Vector3.Lerp(
                phoneLogicalPosition.position,
                targetPosition,
                positionSmoothing * Time.deltaTime
            );

            phoneLogicalPosition.rotation = Quaternion.Lerp(
                phoneLogicalPosition.rotation,
                targetRotation,
                rotationSmoothing * Time.deltaTime
            );
        }
        else
        {
            phoneLogicalPosition.position = targetPosition;
            phoneLogicalPosition.rotation = targetRotation;
        }
    }

    //public Transform GetLogicalTransform()
    //{
    //    return phoneLogicalPosition;
    //}

    public Vector3 GetRelativePosition()
    {
        return currentRelativePosition;
    }

    public Quaternion GetRelativeRotation()
    {
        return currentRelativeRotation;
    }

    public bool IsPhoneTracked()
    {
        return phoneTracked;
    }

    public bool IsHoloLensTracked()
    {
        return hololensTracked;
    }

    public float GetDistanceToPhone()
    {
        return currentRelativePosition.magnitude;
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

