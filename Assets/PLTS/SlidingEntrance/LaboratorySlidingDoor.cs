using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public sealed class LaboratorySlidingDoor : MonoBehaviour
{
    public float slideDistance = 1.05f;
    public float speed = 0.8f;
    public bool IsOpen { get; private set; }
    private Vector3 closedPosition;
    private XRSimpleInteractable interaction;

    private void Awake()
    {
        closedPosition = transform.localPosition;
        interaction = GetComponent<XRSimpleInteractable>();
        interaction.selectEntered.AddListener(Toggle);
    }

    public void Toggle(SelectEnterEventArgs args)
    {
        IsOpen = !IsOpen;
    }

    private void Update()
    {
        Vector3 target = closedPosition + (IsOpen ? Vector3.right * slideDistance : Vector3.zero);
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, target, speed * Time.deltaTime);
    }

    private void OnDestroy()
    {
        if (interaction) interaction.selectEntered.RemoveListener(Toggle);
    }
}
