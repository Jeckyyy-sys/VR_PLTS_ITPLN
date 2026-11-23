using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static class BuildLabInterior {
 static Transform root, p;
 static Material wall,tile,metal,black,blue,yellow,gold,paper,wood,glass,green;
 static Material M(string n,Color c,float metallic=0){string path="Assets/PLTS/LabInterior/"+n+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.color=c;m.SetFloat("_Metallic",metallic);m.SetFloat("_Smoothness",.32f);return m;}
 static GameObject B(string n,float x,float y,float z,float sx,float sy,float sz,Material m){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(p,false);g.transform.localPosition=new Vector3(x,y,z);g.transform.localScale=new Vector3(sx,sy,sz);g.GetComponent<Renderer>().sharedMaterial=m;return g;}
 static GameObject C(string n,Vector3 pos,Vector3 scale,Material m){var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name=n;g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=m;return g;}
 static void Wire(string n,Vector3[] points,Material m,float w=.012f){var go=new GameObject(n);go.transform.SetParent(p,false);var l=go.AddComponent<LineRenderer>();l.useWorldSpace=false;l.sharedMaterial=m;l.widthMultiplier=w;l.positionCount=points.Length;l.SetPositions(points);l.numCornerVertices=4;l.numCapVertices=3;}
 static void Group(string name,Vector3 pos,float yaw=0){var g=new GameObject(name);g.transform.SetParent(root,false);g.transform.localPosition=pos;g.transform.localRotation=Quaternion.Euler(0,yaw,0);p=g.transform;}
 static void Frame(float width,float height){foreach(float x in new[]{-width/2,width/2}){B("Steel upright",x,height/2,0,.045f,height,.06f,metal);B("Rack foot",x,.04f,0,.09f,.08f,.8f,metal);}foreach(float y in new[]{.4f,1.5f,height})B("DIN support cross rail",0,y,0,width,.05f,.07f,metal);}
 static void Dial(float x,float y,float z,Material m){var c=C("Control socket",new Vector3(x,y,z),new Vector3(.045f,.012f,.045f),m);c.transform.localRotation=Quaternion.Euler(90,0,0);}
 static void Cabinet(float x,float y,float width,Material m){B("Equipment enclosure",x,y,-.14f,width,.72f,.3f,m);B("Door seam",x,y,-.296f,width-.04f,.67f,.008f,metal);B("Cabinet front",x,y,-.304f,width-.06f,.65f,.012f,m);B("Small manufacturer plate",x-width*.32f,y+.26f,-.318f,.11f,.035f,.008f,blue);B("Door latch",x+width*.35f,y-.1f,-.325f,.02f,.08f,.02f,black);}
 public static string Build(){
  var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(scene.path!="Assets/Scenes/ITPLNCampusPLTS.unity"||EditorApplication.isPlaying)throw new Exception("Campus Edit Mode required");
  if(GameObject.Find("PLTS Laboratory Interior"))throw new Exception("Interior already exists");
  if(scene.isDirty)EditorSceneManager.SaveScene(scene);
  System.IO.Directory.CreateDirectory("Backups");System.IO.File.Copy(scene.path,"Backups/ITPLNCampusPLTS-before-interior-20261122.unity",true);
  AssetDatabase.CreateFolder("Assets/PLTS","LabInterior");
  wall=M("Warm white plaster",new Color(.86f,.85f,.81f));tile=M("Grey ceramic",new Color(.52f,.55f,.54f));metal=M("Aluminium",new Color(.57f,.59f,.59f),.65f);black=M("Dark equipment",new Color(.035f,.04f,.045f));blue=M("Trainer blue",new Color(.025f,.32f,.59f));yellow=M("Inverter yellow",new Color(.95f,.65f,.025f));gold=M("Mustard curtain",new Color(.65f,.47f,.17f));paper=M("Printed panel",new Color(.88f,.89f,.84f));wood=M("Notice board wood",new Color(.43f,.23f,.095f));glass=M("Frosted glazing",new Color(.55f,.67f,.66f));green=M("Motor green",new Color(.13f,.33f,.25f));
  root=new GameObject("PLTS Laboratory Interior").transform;p=root;Undo.RegisterCreatedObjectUndo(root.gameObject,"Build video reference interior");
  var solid=GameObject.Find("Low Building");Undo.RecordObject(solid,"Replace solid building with hollow room");solid.SetActive(false);
  B("Interior floor",-2.4f,.35f,12,9,.2f,18,tile);
  for(int x=0;x<15;x++)for(int z=0;z<30;z++)B("Ceramic tile",-6.6f+x*.6f,.46f,3.3f+z*.6f,.592f,.018f,.592f,tile);
  B("West wall",-6.84f,2.52f,12,.15f,4.14f,18,wall);B("South wall",-2.4f,2.52f,3.06f,9,4.14f,.15f,wall);B("North wall",-2.4f,2.52f,20.94f,9,4.14f,.15f,wall);
  B("East wall before doorway",2.04f,2.52f,4.86f,.15f,4.14f,3.6f,wall);B("East wall after doorway",2.04f,2.52f,14.84f,.15f,4.14f,12.15f,wall);B("Door header",2.04f,3.85f,7.72f,.15f,1.48f,2.12f,wall);
  B("Ceiling",-2.4f,4.55f,12,9,.12f,18,wall);
  // Open the existing glass leaf and its motifs together, retaining the existing framing.
  var leaf=GameObject.Find("Laboratory side entrance glass");if(leaf){var hinge=new GameObject("Open interior entrance leaf");hinge.transform.SetParent(root);hinge.transform.position=new Vector3(2.21f,.47f,6.675f);foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t=>t.gameObject==leaf||t.name.StartsWith("Door motif")).ToArray()){Undo.SetTransformParent(t,hinge.transform,"Open laboratory door");}hinge.transform.rotation=Quaternion.Euler(0,85,0);}
  // Windows and pleated yellow curtains behind the electrical equipment.
  for(int i=0;i<5;i++){float z=5+i*1.65f;B("Interior window",-6.745f,2.6f,z,.025f,2.7f,1.5f,glass);for(int j=0;j<15;j++){var pleat=B("Curtain vertical fold",-6.67f+(j%2)*.035f,2.55f,z-.7f+j*.1f,.055f,2.65f,.105f,gold);pleat.transform.localRotation=Quaternion.Euler(0,j%2==0?15:-15,0);}B("Dark curtain heading",-6.61f,3.92f,z,.08f,.18f,1.52f,wood);}
  // Six yellow enclosures, two rows of three, on the left end of the equipment bank.
  Group("Yellow inverter bank",new Vector3(-5.98f,.47f,11.7f),-90);Frame(2.15f,2.6f);
  for(int r=0;r<2;r++)for(int c=0;c<3;c++){float x=-.7f+c*.7f,y=1.02f+r*.98f;B("Yellow inverter body",x,y,-.2f,.55f,.82f,.34f,yellow);B("Inverter dark top",x,y+.3f,-.378f,.46f,.16f,.025f,black);B("Small LCD",x,y+.28f,-.395f,.13f,.055f,.012f,glass);for(int k=0;k<5;k++)B("Cooling vent",x-.18f+k*.09f,y-.3f,-.38f,.035f,.09f,.014f,black);Wire("Cable drop",new[]{new Vector3(x,y-.4f,-.17f),new Vector3(x,.2f,-.05f),new Vector3(x+.15f,.12f,-.05f)},black);}
  Group("Grey electrical cabinet bank",new Vector3(-5.98f,.47f,8.3f),-90);Frame(3.7f,2.85f);
  for(int c=0;c<4;c++){float x=-1.38f+c*.92f;Cabinet(x,2.25f,.84f,wall);B("Vertical cable trunking",x,1.55f,.025f,.1f,.65f,.08f,metal);B("Meter base",x,1.04f,-.17f,.28f,.45f,.24f,metal);B("Meter transparent face",x,1.08f,-.301f,.21f,.3f,.02f,glass);B("Meter display",x,1.13f,-.317f,.11f,.06f,.01f,black);B("Switch box",x,.61f,-.16f,.2f,.21f,.15f,wall);Wire("Wiring loop",new[]{new Vector3(x,1.7f,-.08f),new Vector3(x+.13f,1.5f,-.18f),new Vector3(x+.14f,.3f,-.18f),new Vector3(x-.25f,.15f,-.14f)},black);}
  Cabinet(-.4f,1.08f,.65f,blue);
  Group("Blue training panel and motor",new Vector3(-5.8f,.47f,4.6f),-90);Frame(1.35f,1.05f);B("Lower equipment shelf",0,.15f,0,1.4f,.06f,.75f,blue);B("Upper equipment shelf",0,1.05f,0,1.4f,.06f,.75f,blue);B("Trainer blue cabinet",0,1.75f,.05f,.95f,1.4f,.24f,blue);B("Printed control panel",0,2.03f,-.079f,.84f,.68f,.015f,paper);
  for(int row=0;row<3;row++)for(int col=0;col<4;col++){float x=-.3f+col*.2f,y=1.85f+row*.18f;B("Instrument outline",x,y,-.091f,.15f,.13f,.008f,metal);B("Instrument face",x,y+.02f,-.098f,.12f,.065f,.006f,paper);Dial(x,y-.035f,-.11f,row==0?black:green);}
  var motor=C("Green motor housing",new Vector3(-.23f,.42f,-.05f),new Vector3(.38f,.3f,.38f),green);motor.transform.localRotation=Quaternion.Euler(0,0,90);for(int i=0;i<10;i++){var fin=C("Motor cooling rib",new Vector3(-.49f+i*.055f,.42f,-.05f),new Vector3(.4f,.009f,.4f),green);fin.transform.localRotation=Quaternion.Euler(0,0,90);}B("Motor terminal box",-.23f,.65f,-.05f,.16f,.1f,.16f,green);B("Black auxiliary equipment",.43f,.4f,.04f,.3f,.45f,.38f,black);for(int i=0;i<5;i++)Wire("Trainer cable",new[]{new Vector3(-.3f+i*.13f,1.1f,.1f),new Vector3(-.4f+i*.19f,.65f,.2f),new Vector3(.35f,.25f,.12f)},black);
  p=root;
  // Glazed partition and visible office area opposite the equipment.
  for(int i=0;i<3;i++){float z=10+i*1.5f;B("Office frosted partition",-.65f,1.85f,z,.05f,2.7f,1.42f,glass);B("Partition aluminium mullion",-.65f,1.85f,z-.74f,.09f,2.8f,.065f,wall);}B("Partition top rail",-.65f,3.24f,11.5f,.1f,.12f,4.6f,wall);B("Partition lower rail",-.65f,.53f,11.5f,.1f,.12f,4.6f,wall);
  Group("Wood desk",new Vector3(-3,.47f,14.2f));B("Desk top",0,.77f,0,2,.07f,.8f,wood);foreach(float x in new[]{-.92f,.92f})B("Desk side",x,.38f,0,.06f,.76f,.75f,wood);B("Desk modesty panel",0,.43f,.32f,1.85f,.6f,.04f,wood);B("Chair seat",0,.45f,.9f,.45f,.08f,.45f,black);B("Chair back",0,.78f,1.1f,.46f,.6f,.07f,black);foreach(float x in new[]{-.18f,.18f})foreach(float z in new[]{.73f,1.07f})B("Chair leg",x,.22f,z,.025f,.44f,.025f,metal);
  Group("Wood information board",new Vector3(.95f,.47f,5.3f),90);foreach(float x in new[]{-.8f,.8f})B("Board support",x,.85f,0,.07f,1.7f,.07f,wood);B("Notice board backing",0,1.6f,0,1.85f,1.15f,.08f,wood);foreach(float x in new[]{-.47f,.47f}){B("White information sheet",x,1.65f,-.048f,.76f,.78f,.012f,paper);for(int i=0;i<12;i++)B("Printed diagram line",x,1.95f-i*.052f,-.056f,.61f-(i%3)*.08f,.004f,.003f,metal);}
  B("Small tilted PV demonstration",0,2.64f,.1f,1.1f,.06f,.67f,black).transform.localRotation=Quaternion.Euler(-28,0,0);
  for(int i=0;i<6;i++)B("PV demonstration cell grid",-.45f+i*.18f,2.69f,.08f,.008f,.008f,.55f,metal).transform.localRotation=Quaternion.Euler(-28,0,0);
  var exting=C("Red fire extinguisher",new Vector3(-.65f,.38f,.1f),new Vector3(.19f,.32f,.19f),M("Extinguisher red",new Color(.55f,.025f,.02f)));B("Extinguisher handle",-.65f,.74f,.1f,.12f,.05f,.06f,black);
  p=root;B("Wall split AC",1.82f,3.55f,5.3f,.28f,.43f,1.1f,wall);B("AC outlet",1.665f,3.4f,5.3f,.015f,.045f,.95f,black);
  // Plain framed graphics preserve layout without inventing unreadable text.
  for(int i=0;i<4;i++){B("Framed laboratory notice",-5.4f+i*1.25f,3.25f,20.84f,1,.62f,.04f,black);B("Blue notice face",-5.4f+i*1.25f,3.25f,20.81f,.91f,.53f,.02f,glass);}
  for(int i=0;i<3;i++){float z=6+i*5;B("Ceiling light fixture",-2.8f,4.43f,z,.35f,.06f,1.2f,paper);var g=new GameObject("Interior soft light");g.transform.SetParent(root);g.transform.position=new Vector3(-2.8f,3.8f,z);var l=g.AddComponent<Light>();l.type=LightType.Point;l.range=9;l.intensity=2.2f;l.shadows=LightShadows.None;l.color=new Color(1,.96f,.88f);}
  AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
  return "Saved interior. Components="+root.GetComponentsInChildren<Renderer>().Length;
 }
 public static string View(){var v=SceneView.lastActiveSceneView;v.LookAt(new Vector3(-4.3f,1.9f,8),Quaternion.LookRotation(new Vector3(-1,-.05f,.2f)),2.2f,false,true);v.Repaint();return "Interior view";}
}
