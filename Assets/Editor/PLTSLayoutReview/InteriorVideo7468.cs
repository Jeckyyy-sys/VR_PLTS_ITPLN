using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Manual reference pass: IMG_7468.MOV. Dimensions are estimated, not surveyed.
public static class InteriorVideo7468 {
 const string Folder="Assets/PLTS/InteriorVideo7468";
 static Transform root,p; static Material white,metal,black,red,yellow,glass;
 static Material Mat(string n,Color c,float metallic=0) {
  var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=c;m.SetFloat("_Metallic",metallic);m.SetFloat("_Smoothness",.4f);AssetDatabase.CreateAsset(m,Folder+"/"+n+".mat");return m;
 }
 static GameObject Box(string n,Vector3 v,Vector3 size,Material m) {
  var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(p,false);g.transform.localPosition=v;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=m;
  UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;
 }
 static void B(string n,float x,float y,float z,float w,float h,float d,Material m){Box(n,new Vector3(x,y,z),new Vector3(w,h,d),m);}
 static void Disc(string n,float x,float y,float z,float r,float depth,Material m){var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name=n;g.transform.SetParent(p,false);g.transform.localPosition=new Vector3(x,y,z);g.transform.localRotation=Quaternion.Euler(90,0,0);g.transform.localScale=new Vector3(r*2,depth/2,r*2);g.GetComponent<Renderer>().sharedMaterial=m;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());}
 static void Wire(string n,Material m,float width,params Vector3[] pts){var g=new GameObject(n);g.transform.SetParent(p,false);var l=g.AddComponent<LineRenderer>();l.useWorldSpace=false;l.sharedMaterial=m;l.widthMultiplier=width;l.numCornerVertices=4;l.numCapVertices=3;l.positionCount=pts.Length;l.SetPositions(pts);}
 static void Group(string n,Transform reference){p=new GameObject(n).transform;p.SetParent(root,false);p.position=reference.position;p.rotation=reference.rotation;}
 static void Change(Transform t,Vector3 pos,Vector3 scale){Undo.RecordObject(t,"Interior video correction");t.localPosition=pos;t.localScale=scale;}
 public static string Apply(){
  var s=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(s.path!="Assets/Scenes/ITPLNCampusPLTS.unity"||EditorApplication.isPlaying)throw new Exception("Campus scene in edit mode required");
  if(GameObject.Find("Interior video refinement IMG7468"))throw new Exception("Already applied");
  Directory.CreateDirectory("Backups/LayoutReview-20261123");var backup="Backups/LayoutReview-20261123/BeforeInterior7468-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".unity";if(!EditorSceneManager.SaveScene(s,backup,true))throw new Exception("Backup failed");
  if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/PLTS","InteriorVideo7468");
  white=Mat("Ivory enclosure",new Color(.87f,.87f,.83f));metal=Mat("Galvanized steel",new Color(.5f,.53f,.54f),.7f);black=Mat("Cable rubber",new Color(.025f,.028f,.03f));red=Mat("Red cable",new Color(.48f,.045f,.025f));yellow=Mat("Warning yellow",new Color(.95f,.7f,.025f));glass=Mat("Clear partition",new Color(.7f,.82f,.8f,.12f));
  glass.SetFloat("_Surface",1);glass.SetFloat("_SrcBlend",5);glass.SetFloat("_DstBlend",10);glass.SetFloat("_ZWrite",0);glass.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");glass.renderQueue=3000;glass.SetOverrideTag("RenderType","Transparent");
  root=new GameObject("Interior video refinement IMG7468").transform;root.SetParent(GameObject.Find("PLTS Laboratory Interior").transform,false);Undo.RegisterCreatedObjectUndo(root.gameObject,"Interior video detail pass");
  Group("Office glazing detail",root.parent);
  foreach(var t in root.parent.GetComponentsInChildren<Transform>().Where(t=>t.name=="Office frosted partition").ToArray()){
   var pos=t.localPosition;Change(t,new Vector3(pos.x,1.15f,pos.z),new Vector3(.05f,1.3f,1.42f));
   B("Clear upper office pane",pos.x,2.5f,pos.z,.025f,1.35f,1.42f,glass);B("Office mid rail",pos.x,1.82f,pos.z,.08f,.055f,1.48f,white);
  }
  B("Office end mullion",-.65f,1.85f,13.74f,.09f,2.8f,.065f,white);
  B("Office TV rear housing",1.85f,2.65f,11.5f,.12f,.74f,1.3f,black);B("Office TV screen",1.78f,2.65f,11.5f,.015f,.66f,1.21f,black);
  Wire("TV power cable",black,.012f,new Vector3(1.86f,2.3f,11.5f),new Vector3(1.86f,1.35f,11.5f),new Vector3(1.86f,1.35f,11.85f));
  Group("Cabinet cable management",GameObject.Find("Grey electrical cabinet bank").transform);
  B("Lower cable tray",0,.19f,.09f,3.65f,.08f,.25f,metal);
  for(int col=0;col<4;col++){
   float x=-1.38f+col*.92f;
   B("Slotted cable duct",x+.34f,1.36f,-.06f,.095f,1.36f,.09f,white);
   for(int j=0;j<26;j++)B("Duct ventilation slot",x+.34f,.74f+j*.047f,-.108f,.065f,.016f,.004f,black);
   for(int j=0;j<3;j++){
    float xx=x-.13f+j*.12f;Wire("Cabinet supply cable",j==0?red:black,.022f,new Vector3(xx,1.88f,-.12f),new Vector3(xx,1.5f,-.14f),new Vector3(xx+.04f,.32f,-.12f),new Vector3(xx+.2f,.23f,-.05f));
    B("Cable identification sleeve",xx,1.78f,-.12f,.03f,.09f,.03f,yellow);
   }
   B("Rotary switch face",x,.61f,-.244f,.16f,.17f,.02f,white);Disc("Rotary switch bezel",x,.61f,-.265f,.051f,.02f,metal);
   var handle=Box("Rotary disconnect handle",new Vector3(x,.61f,-.284f),new Vector3(.026f,.11f,.025f),black);handle.transform.localRotation=Quaternion.Euler(0,0,35);
   for(int j=0;j<7;j++){float xx=x-.18f+j*.06f;B("Terminal block",xx,1.52f,-.19f,.045f,.09f,.07f,white);Disc("Terminal screw",xx,1.54f,-.231f,.009f,.008f,metal);}
   for(int j=0;j<5;j++)B("Cable tie",x+.13f,.37f+j*.25f,-.153f,.16f,.013f,.014f,white);
  }
  Group("Trainer projecting lamp holders",GameObject.Find("Blue training panel and motor").transform);
  // Existing rings already follow the approved photographic fascia coordinates.
  var trainer=GameObject.Find("Blue training panel and motor").transform;
  foreach(var t in trainer.GetComponentsInChildren<Transform>().Where(t=>t.name=="Dummy load socket ring")){
   var q=t.localPosition;Disc("Ceramic dummy lamp socket",q.x,q.y,-.207f,.023f,.045f,white);Disc("Threaded lamp socket rim",q.x,q.y,-.235f,.019f,.014f,metal);Disc("Lamp socket cavity",q.x,q.y,-.244f,.013f,.006f,black);
  }
  Group("Wall demonstration mounting and accessories",GameObject.Find("Huawei inverter wall assembly").transform);
  for(int i=0;i<2;i++){float x=-1.95f+i*.42f;B("Aluminium accessory rail",x,.76f,-.13f,.3f,.07f,.065f,metal);B("Rail hollow channel",x,.775f,-.166f,.27f,.024f,.004f,black);B("Rail clamp",x+.06f,.84f,-.13f,.055f,.09f,.08f,metal);Disc("Clamp bolt",x+.06f,.85f,-.18f,.013f,.014f,metal);}
  for(int i=0;i<3;i++){float x=-1.96f+i*.16f;B("MC4 branch stem",x,1.55f,-.14f,.038f,.16f,.04f,black);B("MC4 branch crosspiece",x,1.58f,-.14f,.11f,.035f,.04f,black);foreach(float dx in new[]{-.045f,.045f}){B("MC4 branch socket",x+dx,1.63f,-.14f,.03f,.09f,.04f,black);Disc("Connector collar",x+dx,1.65f,-.165f,.018f,.014f,metal);}}
  // Demonstration module visible above the inverter wall at approximately 06:20.
  var panel=new GameObject("Overhead demonstration PV module").transform;panel.SetParent(p,false);panel.localPosition=new Vector3(-.45f,2.9f,-.3f);panel.localRotation=Quaternion.Euler(18,0,0);var prev=p;p=panel;
  B("PV aluminium backing",0,0,0,1.68f,.86f,.055f,metal);
  var cell=Mat("Demonstration solar cells",new Color(.055f,.09f,.13f),.15f);
  for(int row=0;row<4;row++)for(int col=0;col<8;col++){float x=-.715f+col*.204f,y=-.305f+row*.204f;B("Demo photovoltaic cell",x,y,-.033f,.195f,.195f,.012f,cell);for(int k=0;k<2;k++)B("Cell busbar",x-.043f+k*.086f,y,-.041f,.002f,.19f,.002f,metal);}
  p=prev;foreach(float x in new[]{-1.08f,.18f})B("PV wall bracket",x,2.67f,-.12f,.035f,.42f,.28f,metal);
  AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);return "Interior refinements saved. Backup: "+backup;
 }
 public static string Capture(string stage){
  string dir="Screenshots/InteriorReview/"+stage;Directory.CreateDirectory(dir);var go=new GameObject("Temporary interior review camera");var c=go.AddComponent<Camera>();c.fieldOfView=67;c.nearClipPlane=.05f;c.farClipPlane=80;var rt=new RenderTexture(1440,1000,24);var old=RenderTexture.active;
  try{c.targetTexture=rt;var positions=new[]{new Vector3(-2.6f,2.1f,4),new Vector3(-2.8f,2.1f,8.5f),new Vector3(-3.8f,2.1f,17.1f),new Vector3(-3.8f,2.05f,11.5f)};var targets=new[]{new Vector3(-5.85f,1.8f,5.8f),new Vector3(-5.98f,1.7f,8.3f),new Vector3(1.8f,2.1f,17.1f),new Vector3(.2f,2,11.5f)};for(int i=0;i<4;i++){go.transform.position=positions[i];go.transform.LookAt(targets[i]);c.Render();c.Render();RenderTexture.active=rt;var t=new Texture2D(1440,1000,TextureFormat.RGB24,false);try{t.ReadPixels(new Rect(0,0,1440,1000),0,0);t.Apply();File.WriteAllBytes(dir+"/View"+i+".png",t.EncodeToPNG());}finally{UnityEngine.Object.DestroyImmediate(t);}}}
  finally{RenderTexture.active=old;c.targetTexture=null;UnityEngine.Object.DestroyImmediate(go);UnityEngine.Object.DestroyImmediate(rt);}return dir;
 }
}
