var parent=GameObject.Find("Interior corrected from IMG7468").transform;
var group=new GameObject("Interior reference finishing");group.transform.SetParent(parent,false);UnityEditor.Undo.RegisterCreatedObjectUndo(group,"Interior finish pass");
var white=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/PLTS/InteriorReferenceCorrection/Warm equipment white.mat");
var metal=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/PLTS/InteriorReferenceCorrection/Satin aluminium.mat");
var black=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/PLTS/InteriorReferenceCorrection/Graphite.mat");
var glass=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/PLTS/InteriorReferenceCorrection/Clear equipment cover.mat");
var frost=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/PLTS/InteriorReferenceCorrection/Office frosted film.mat");
GameObject Box(string n,Transform p,Vector3 pos,Vector3 scale,Material mat,bool collision=false){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=mat;if(!collision)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
var safety=GameObject.Find("Left safety storage beside demonstration").transform;UnityEditor.Undo.RecordObject(safety,"Clear accessories board");safety.position=new Vector3(-6.55f,.47f,3.95f);
var pv=GameObject.Find("Overhead demonstration PV module").transform;UnityEditor.Undo.RecordObject(pv,"Match overhead module span and pitch");pv.localScale=new Vector3(2.45f,1.4f,1);pv.localRotation=Quaternion.Euler(58,0,0);
var solar=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/PLTS/InteriorVideo7468/Demonstration solar cells.mat");
foreach(var r in GameObject.Find("PLTS Laboratory Interior").GetComponentsInChildren<Renderer>()){
 if(r.name=="Office frosted partition"){UnityEditor.Undo.RecordObject(r,"Match frosted film");r.sharedMaterial=frost;}
 if(r.name=="Sample module dark cells"){UnityEditor.Undo.RecordObject(r,"Match sample photovoltaic cells");r.sharedMaterial=solar;}
}
for(int i=0;i<4;i++){float z=14.5f+i*1.5f;Box("Office extended frosted pane",group.transform,new Vector3(-4.15f,1.15f,z),new Vector3(.05f,1.3f,1.42f),frost,true);Box("Office extended upper glazing",group.transform,new Vector3(-4.15f,2.5f,z),new Vector3(.025f,1.35f,1.42f),glass,true);Box("Office extended mullion",group.transform,new Vector3(-4.15f,1.87f,z+.74f),new Vector3(.07f,2.8f,.065f),white);foreach(float y in new[]{.53f,1.82f,3.24f})Box("Office extended rail",group.transform,new Vector3(-4.15f,y,z),new Vector3(.09f,.06f,1.5f),white);}
var bank=GameObject.Find("Right bank - fuel save and multicluster").transform;
var detail=new GameObject("Controller slotted ducts").transform;detail.SetParent(group.transform,false);detail.SetPositionAndRotation(bank.position,bank.rotation);
foreach(float x in new[]{.53f,1.57f}){Box("Controller slotted duct",detail,new Vector3(x,1.82f,.025f),new Vector3(.1f,.4f,.075f),metal);for(int i=0;i<8;i++)Box("Controller duct slot",detail,new Vector3(x,1.65f+i*.045f,-.015f),new Vector3(.068f,.013f,.004f),black);}
var inverter=GameObject.Find("Right bank - six Sunny Island inverters").transform;var equipment=new GameObject("Distribution switches behind covers").transform;equipment.SetParent(group.transform,false);equipment.SetPositionAndRotation(inverter.position,inverter.rotation);
for(int col=0;col<3;col++){float x=-1.05f+col*1.05f;for(int i=0;i<3;i++){float xx=x-.25f+i*.25f;Box("DC disconnect body",equipment,new Vector3(xx,.48f,-.175f),new Vector3(.19f,.29f,.04f),metal);Box("DC disconnect switch",equipment,new Vector3(xx,.52f,-.202f),new Vector3(.04f,.12f,.025f),black);Box("Terminal lower port",equipment,new Vector3(xx,.37f,-.2f),new Vector3(.1f,.045f,.02f),black);}}
foreach(var pair in new[]{(bank,4.2f,3.05f),(inverter,3.25f,3.25f)}){var col=UnityEditor.Undo.AddComponent<BoxCollider>(pair.Item1.gameObject);col.center=new Vector3(0,pair.Item3/2,-.03f);col.size=new Vector3(pair.Item2,pair.Item3,.58f);}
var rack=GameObject.Find("Rear battery racks and test bench").transform;var hit=UnityEditor.Undo.AddComponent<BoxCollider>(rack.gameObject);hit.center=new Vector3(0,.6f,0);hit.size=new Vector3(1.9f,1.2f,.8f);
var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
return "Finished module pitch, office continuation, distribution details and equipment colliders";
