using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneTracker : MonoBehaviour
{
    [SerializeField] private Transform delta;
    private void Update()
    {
        transform.localPosition = delta.position;
        transform.localRotation = delta.rotation;
    }
}
