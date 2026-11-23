using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;
public static class BuildBatteryInteraction {
 static Transform p;static Material body,lid,metal;
 static GameObject B(string n,Vector3 pos,Vector3 size,Material m){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=m;return g;}
 static Material M(string n,Color c){var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=c;AssetDatabase.CreateAsset(m,"Assets/PLTS/BatteryInteraction/"+n+".mat");return m;}
 static TMP_Text Label(string txt,Vector3 pos,float w,float h,float size){var g=new GameObject(txt);g.transform.SetParent(p,false);g.transform.localPosition=pos;var t=g.AddComponent<TextMeshPro>();t.font=TMP_Settings.defaultFontAsset;t.fontSize=size;t.text=txt;t.alignment=TextAlignmentOptions.Center;t.rectTransform.sizeDelta=new Vector2(w,h);return t;}
 static void Button(GameObject g,BatteryCabinetInteraction c,int index){g.AddComponent<XRSimpleInteractable>();var action=g.AddComponent<BatteryCabinetButton>();action.cabinet=c;action.batteryIndex=index;}
 public static string Build(){var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(scene.name!="ITPLNCampusPLTS"||EditorApplication.isPlaying)throw new Exception("Campus edit mode required");var cabinet=GameObject.Find("Battery Cabinet Case");if(cabinet.GetComponent<BatteryCabinetInteraction>())throw new Exception("Already installed");if(scene.isDirty)EditorSceneManager.SaveScene(scene);System.IO.File.Copy(scene.path,"Backups/ITPLNCampusPLTS-before-batteries.unity",true);
 var ctrl=cabinet.AddComponent<BatteryCabinetInteraction>();ctrl.inverter=UnityEngine.Object.FindFirstObjectByType<InverterSimulation>();ctrl.docks=new Transform[2];ctrl.batteries=new XRGrabInteractable[2];
 body=M("Battery black case",new Color(.055f,.06f,.055f));lid=M("Battery lid",new Color(.085f,.09f,.08f));metal=M("Terminal metal",new Color(.53f,.54f,.5f));
 cabinet.transform.Find("Shadow Behind Vents").gameObject.SetActive(false);
 var pivot=new GameObject("Battery front cover hinge");pivot.transform.SetParent(cabinet.transform,false);pivot.transform.localPosition=new Vector3(-.55f,.94f,-.415f);ctrl.door=pivot.transform;
 string[] panels={"Panel Upper Field","Panel Lower Field","Vent Left Margin","Vent Right Margin","Vent Bridge","Pressed Horizontal Rib","Panel Folded Lower Edge","Panel Joint Fold","Mounting Screw"};
 foreach(var t in cabinet.GetComponentsInChildren<Transform>(true).Where(t=>t.parent==cabinet.transform&&t.localPosition.z<-.37f&&panels.Contains(t.name)).ToArray())Undo.SetTransformParent(t,pivot.transform,"Make front panels openable");
 p=pivot.transform;var handle=B("Battery cover handle",new Vector3(.98f,0,-.055f),new Vector3(.055f,.22f,.065f),body);Button(handle,ctrl,-1);
 var coverBox=pivot.AddComponent<BoxCollider>();coverBox.center=new Vector3(.55f,0,0);coverBox.size=new Vector3(1.07f,1.06f,.035f);pivot.AddComponent<XRSimpleInteractable>();var cb=pivot.AddComponent<BatteryCabinetButton>();cb.cabinet=ctrl;cb.batteryIndex=-1;
 p=cabinet.transform;B("Battery shelf",new Vector3(0,.94f,0),new Vector3(1.05f,.025f,.66f),metal);
 AssetDatabase.ImportAsset("Assets/PLTS/BatteryInteraction/BatteryReference.png");var im=(TextureImporter)AssetImporter.GetAtPath("Assets/PLTS/BatteryInteraction/BatteryReference.png");im.npotScale=TextureImporterNPOTScale.None;im.textureCompression=TextureImporterCompression.Uncompressed;im.SaveAndReimport();var print=new Material(Shader.Find("Universal Render Pipeline/Unlit"));print.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/PLTS/BatteryInteraction/BatteryReference.png"));AssetDatabase.CreateAsset(print,"Assets/PLTS/BatteryInteraction/SMT12200 label.mat");
 var mesh=new Mesh();mesh.name="SMT12200 front label";mesh.vertices=new[]{new Vector3(-.46f,-.175f,0),new Vector3(.46f,-.175f,0),new Vector3(.46f,.175f,0),new Vector3(-.46f,.175f,0)};mesh.uv=new[]{new Vector2(77f/535,1-352f/626),new Vector2(439f/535,1-343f/626),new Vector2(449f/535,1-215f/626),new Vector2(77f/535,1-206f/626)};mesh.triangles=new[]{0,2,1,0,3,2};mesh.RecalculateNormals();AssetDatabase.CreateAsset(mesh,"Assets/PLTS/BatteryInteraction/BatteryLabel.asset");
 for(int i=0;i<2;i++){float y=i==0?1.19f:.65f;var dock=new GameObject("Battery dock "+(i+1));dock.transform.SetParent(cabinet.transform,false);dock.transform.localPosition=new Vector3(0,y,-.02f);ctrl.docks[i]=dock.transform;var battery=new GameObject("SMT12200 Battery "+(i+1));battery.transform.SetParent(cabinet.transform,false);battery.transform.localPosition=dock.transform.localPosition;p=battery.transform;
 B("Deep cycle gel battery case",Vector3.zero,new Vector3(.98f,.4f,.59f),body);B("Raised lid",new Vector3(0,.22f,0),new Vector3(1.01f,.04f,.61f),lid);B("Lid sealing rim",new Vector3(0,.19f,0),new Vector3(1.02f,.018f,.62f),body);
 for(int k=0;k<17;k++)B("Case reinforcing rib",new Vector3(-.45f+k*.056f,.15f,-.3f),new Vector3(.014f,.065f,.014f),lid);
 for(int k=0;k<2;k++){float x=k==0?-.4f:.4f;B("Terminal pedestal",new Vector3(x,.255f,.18f),new Vector3(.08f,.03f,.09f),body);B(k==0?"Negative terminal":"Positive terminal",new Vector3(x,.28f,.18f),new Vector3(.045f,.025f,.05f),metal);}
 var label=new GameObject("SMT-POWER SMT12200 12V 200AH 10HR");label.transform.SetParent(p,false);label.transform.localPosition=new Vector3(0,-.015f,-.303f);label.AddComponent<MeshFilter>().sharedMesh=mesh;label.AddComponent<MeshRenderer>().sharedMaterial=print;
 var rb=battery.AddComponent<Rigidbody>();rb.isKinematic=true;rb.useGravity=false;rb.mass=1;var grab=battery.AddComponent<XRGrabInteractable>();grab.throwOnDetach=false;grab.useDynamicAttach=true;ctrl.batteries[i]=grab;
 p=cabinet.transform;var b=B("Install battery "+(i+1),new Vector3(i==0?-.26f:.26f,1.56f,-.43f),new Vector3(.4f,.09f,.045f),lid);Button(b,ctrl,i);Label("PASANG "+(i+1),new Vector3(i==0?-.26f:.26f,1.56f,-.457f),.39f,.08f,.65f);
 }
 p=cabinet.transform;ctrl.status=Label("Klik pegangan untuk membuka kabinet baterai.",new Vector3(0,1.72f,-.42f),1.5f,.22f,.6f);
 AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);return "Created two removable batteries, hinged front cover, and installation controls";
 }
}
