using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
public sealed class BatteryCabinetButton : MonoBehaviour {
 public BatteryCabinetInteraction cabinet;
 public int batteryIndex=-1;
 void Awake(){GetComponent<XRSimpleInteractable>().selectEntered.AddListener(_=>{if(batteryIndex<0)cabinet.ToggleDoor();else cabinet.Install(batteryIndex);});}
}
