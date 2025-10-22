using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

public class PhoneSimulator : MonoBehaviour
{
    //phone movement
    [SerializeField] private float rotateSpeed = 5f;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField, Range(0.3f, 1.0f)] private float zPosition = 0.75f;
    private float currentPitch = -60f;
    private float currentYaw = 0f;

    void Start()
    {

    }

    void Update()
    {
        HandleZMovement();
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            HandleMovement();
        if (Input.GetMouseButton(0))
            HandleRotation();
    }

    private void HandleMovement()
    {
        Plane groundPlane = new Plane(Vector3.forward, new Vector3(0, 0, zPosition)); // Horizontal plane at Y = 0
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        float enter;
        if (groundPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            transform.position = hitPoint;
        }
    }

    private void HandleZMovement()
    {
        float zDelta = 0f;

        if (Input.GetKey(KeyCode.Keypad8))
            zDelta = moveSpeed * Time.deltaTime;
        else if (Input.GetKey(KeyCode.Keypad2))
            zDelta = -moveSpeed * Time.deltaTime;

        if (zDelta != 0f)
        {
            transform.position += new Vector3(0f, 0f, zDelta);
            zPosition = transform.position.z;
        }
    }

    private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        currentYaw += mouseX * rotateSpeed;
        currentPitch -= mouseY * rotateSpeed;
        currentYaw = Mathf.Clamp(currentYaw, -90f, 90f);
        currentPitch = Mathf.Clamp(currentPitch, -90f, 0f);

        transform.rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
    }
}
