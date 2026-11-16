using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public sealed class CabinetDoor : MonoBehaviour
{
    [SerializeField] private float openAngle = -105f;
    [SerializeField] private float degreesPerSecond = 100f;
    private bool isOpen;
    private Quaternion targetRotation;

    private void Awake()
    {
        isOpen = Quaternion.Angle(transform.localRotation, Quaternion.identity) > 1f;
        targetRotation = transform.localRotation;
    }

    public void Toggle(SelectEnterEventArgs args)
    {
        isOpen = !isOpen;
        targetRotation = isOpen
            ? Quaternion.Euler(0f, openAngle, 0f)
            : Quaternion.identity;
    }

    private void Update()
    {
        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation, targetRotation, degreesPerSecond * Time.deltaTime);
    }
}
