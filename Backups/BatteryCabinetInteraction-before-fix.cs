using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;

public sealed class BatteryCabinetInteraction : MonoBehaviour
{
 public Transform door;
 public Transform[] docks;
 public XRGrabInteractable[] batteries;
 public TMP_Text status;
 public InverterSimulation inverter;
 public bool IsOpen { get; private set; }
 public bool[] Installed { get; private set; } = { true, true };
 public bool trace;
 void Awake() {
  for(int i=0;i<batteries.Length;i++) { int index=i; batteries[i].selectEntered.AddListener(_=>Removed(index)); batteries[i].selectExited.AddListener(_=>Released(index)); }
  Apply();
 }
 public void ToggleDoor() {
  if(!IsOpen && System.Array.Exists(batteries,b=>b.isSelected))return;
  if(IsOpen && (!Installed[0]||!Installed[1])) { status.text="Pasang kedua baterai sebelum menutup."; return; }
  IsOpen=!IsOpen; Apply(); Log("door="+IsOpen);
 }
 public void Removed(int i) { Installed[i]=false;Apply();Log("removed="+i); }
 void Released(int i) { if(Vector3.Distance(batteries[i].transform.position,docks[i].position)<.35f) Install(i); else Apply(); }
 public void Install(int i) {
  if(!IsOpen||batteries[i].isSelected)return;
  var t=batteries[i].transform;t.SetPositionAndRotation(docks[i].position,docks[i].rotation);
  var rb=batteries[i].GetComponent<Rigidbody>();rb.isKinematic=true;Installed[i]=true;Apply();Log("installed="+i);
 }
 void Update() {door.localRotation=Quaternion.RotateTowards(door.localRotation,Quaternion.Euler(0,IsOpen?105:0,0),120*Time.deltaTime);for(int i=0;i<batteries.Length;i++)batteries[i].enabled=IsOpen&&Quaternion.Angle(door.localRotation,Quaternion.identity)>75;}
 void Apply() {
  if(inverter)inverter.batteryAvailable=Installed[0]&&Installed[1];
  if(status)status.text=IsOpen?"Ambil baterai dengan Grab. Lepas dekat dudukan untuk memasang.\nTombol PASANG mengembalikan baterai ke dudukannya.":"Klik pegangan untuk membuka kabinet baterai.";
 }
 void Log(string s){if(trace)Debug.Log("[BatteryCabinet] "+s+" source="+(Installed[0]&&Installed[1]));}
}
