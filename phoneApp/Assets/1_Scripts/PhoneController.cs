using UnityEngine;
using UnityEngine.EventSystems;

public class PhoneController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private TaskManager taskManager;
    private const float DELAY_BEFORE_RELEASE = 0.05f;

    public void OnPointerDown(PointerEventData eventData)
    {
        taskManager.SendRayHoldState(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        taskManager.SendRaySelectionRequest();
        Invoke(nameof(DelayedRayRelease), DELAY_BEFORE_RELEASE);
    }

    private void DelayedRayRelease()
    {
        taskManager.SendRayHoldState(false);
    }
}
