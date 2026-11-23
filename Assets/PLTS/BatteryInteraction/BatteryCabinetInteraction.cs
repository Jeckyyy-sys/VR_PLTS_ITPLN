using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public sealed class BatteryCabinetInteraction : MonoBehaviour
{
    public Transform door;
    public Transform[] docks;
    public XRGrabInteractable[] batteries;
    public TMP_Text status;
    public InverterSimulation inverter;
    public bool trace;
    public bool IsOpen { get; private set; }
    public bool[] Installed { get; private set; }
    private Quaternion closedRotation;

    private void Awake()
    {
        closedRotation = door.localRotation;
        Installed = new bool[batteries.Length];
        for (int i = 0; i < batteries.Length; i++)
        {
            int index = i;
            Installed[i] = Vector3.Distance(batteries[i].transform.position, docks[i].position) < 0.02f;
            batteries[i].selectEntered.AddListener(_ => Removed(index));
            batteries[i].selectExited.AddListener(_ => StartCoroutine(Released(index)));
            batteries[i].enabled = false;
        }
        Apply();
    }

    public void ToggleDoor()
    {
        if (System.Array.Exists(batteries, battery => battery.isSelected)) return;
        IsOpen = !IsOpen;
        Apply();
        Log("door=" + IsOpen);
    }

    public void Removed(int index)
    {
        Installed[index] = false;
        Apply();
        Log("removed=" + index);
    }

    private IEnumerator Released(int index)
    {
        // XR completes detach at the end of the frame before we restore a docked pose.
        yield return new WaitForEndOfFrame();
        if (Vector3.Distance(batteries[index].transform.position, docks[index].position) < 0.22f)
            Install(index);
    }

    public void Install(int index)
    {
        if (!IsOpen || batteries[index].isSelected) return;
        var battery = batteries[index];
        var body = battery.GetComponent<Rigidbody>();
        body.isKinematic = true;
        battery.transform.SetPositionAndRotation(docks[index].position, docks[index].rotation);
        Installed[index] = true;
        Apply();
        Log("installed=" + index);
    }

    private void Update()
    {
        var target = closedRotation * Quaternion.Euler(0, IsOpen ? 105f : 0f, 0);
        door.localRotation = Quaternion.RotateTowards(door.localRotation, target, 120f * Time.deltaTime);
        bool accessible = IsOpen && Quaternion.Angle(door.localRotation, closedRotation) > 75f;
        for (int i = 0; i < batteries.Length; i++)
            batteries[i].enabled = accessible || !Installed[i] || batteries[i].isSelected;
    }

    private void Apply()
    {
        bool allInstalled = System.Array.TrueForAll(Installed, installed => installed);
        if (inverter) inverter.batteryAvailable = allInstalled;
        if (status)
            status.text = IsOpen
                ? "Grab baterai untuk melepas. Lepaskan dekat dudukan untuk memasang.\nPASANG mengembalikan baterai ke dudukannya."
                : "Pilih pegangan untuk membuka kabinet baterai.";
    }

    private void Log(string message)
    {
        if (trace) Debug.Log("[BatteryCabinet] " + message);
    }
}
