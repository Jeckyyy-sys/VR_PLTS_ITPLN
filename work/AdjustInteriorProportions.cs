var s=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if(s.path!="Assets/Scenes/ITPLNCampusPLTS.unity"||UnityEditor.EditorApplication.isPlaying)throw new Exception("Campus edit mode required");
if(GameObject.Find("Interior proportion correction"))throw new Exception("Already adjusted");
var backup="Backups/LayoutReview-20261123/BeforeRoomProportions-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".unity";
if(!UnityEditor.SceneManagement.EditorSceneManager.SaveScene(s,backup,true))throw new Exception("Backup failed");
var interior=GameObject.Find("PLTS Laboratory Interior").transform;
var marker=new GameObject("Interior proportion correction");marker.transform.SetParent(interior,false);UnityEditor.Undo.RegisterCreatedObjectUndo(marker,"Room proportions");
void Set(Transform t,Vector3 pos,Vector3 size){UnityEditor.Undo.RecordObject(t,"Room proportions");t.position=pos;t.localScale=size;}
void Place(string name,Vector3 pos,float scale=1){var t=GameObject.Find(name).transform;Set(t,pos,Vector3.one*scale);}
foreach(Transform t in interior){
 if(t.name=="Interior floor"||t.name=="Ceiling")Set(t,new Vector3(-2.4f,t.position.y,9.3f),new Vector3(9,t.localScale.y,12.6f));
 if(t.name=="West wall")Set(t,new Vector3(t.position.x,t.position.y,9.3f),new Vector3(.15f,4.14f,12.6f));
 if(t.name=="North wall")Set(t,new Vector3(-2.4f,t.position.y,15.54f),t.localScale);
 if(t.name=="East wall after doorway")Set(t,new Vector3(2.04f,2.52f,11.4f),new Vector3(.15f,4.14f,8.28f));
 if(t.name=="Ceramic tile"&&t.position.z>15.3f){UnityEditor.Undo.RecordObject(t.gameObject,"Retain unused tiles");t.gameObject.SetActive(false);}
 if(t.name=="Office frosted partition"||t.name.StartsWith("Partition ")){var q=t.position;Set(t,new Vector3(-4.65f,q.y,7.8f+(q.z-9.23f)*.6f),new Vector3(t.localScale.x,t.localScale.y,t.localScale.z*.6f));}
 if(t.name=="Ceiling light fixture"||t.name=="Interior soft light"){var q=t.position;Set(t,new Vector3(q.x,q.y,3+(q.z-3)*.7f),t.localScale);}
}
Place("Blue training panel and motor",new Vector3(1.15f,.47f,4.25f),.9f);
Place("Trainer projecting lamp holders",new Vector3(1.15f,.47f,4.25f),.9f);
Place("Right bank - fuel save and multicluster",new Vector3(1.32f,.47f,7.1f),.8f);
Place("Controller slotted ducts",new Vector3(1.32f,.47f,7.1f),.8f);
Place("Right bank - six Sunny Island inverters",new Vector3(1.32f,.47f,10.65f),.9f);
Place("Distribution switches behind covers",new Vector3(1.32f,.47f,10.65f),.9f);
Place("Rear battery racks and test bench",new Vector3(-1.5f,.47f,14.05f));
Place("Rear doorway window and reference notices",new Vector3(0,0,-5.4f));
Place("Huawei inverter wall assembly",new Vector3(-6.63f,.68f,5.5f),.8f);
Place("Wall demonstration mounting and accessories",new Vector3(-6.63f,.68f,5.5f),.8f);
Place("Wood desk",new Vector3(-5.65f,.47f,9.6f),.85f);
Place("Leaning demonstration PV samples",new Vector3(-5.9f,.48f,7.42f),.9f);
// Remodel partition spans, not the room root: equipment keeps its original aspect ratio.
var office=GameObject.Find("Office glazing detail").transform;
foreach(Transform t in office){var q=t.position;var l=t.GetComponent<LineRenderer>();
 if(l){UnityEditor.Undo.RecordObject(l,"Office cable position");var pts=new Vector3[l.positionCount];l.GetPositions(pts);for(int i=0;i<pts.Length;i++)pts[i].z=7.8f+(pts[i].z-9.23f)*.6f;l.SetPositions(pts);}
 else {float x=Mathf.Abs(q.x+4.15f)<.1f?-4.65f:q.x;bool pane=t.name.Contains("pane")||t.name.Contains("rail");Set(t,new Vector3(x,q.y,7.8f+(q.z-9.23f)*.6f),new Vector3(t.localScale.x,t.localScale.y,t.localScale.z*(pane?.6f:1)));}
}
var finish=GameObject.Find("Interior reference finishing").transform;
foreach(Transform t in finish){if(!t.name.StartsWith("Office extended"))continue;var q=t.position;Set(t,new Vector3(-4.65f,q.y,7.8f+(q.z-9.23f)*.6f),new Vector3(t.localScale.x,t.localScale.y,t.localScale.z*.6f));}
var front=GameObject.Find("Office end partition and access").transform;
foreach(Transform t in front){var q=t.position;float x=-6.75f+(q.x+6.75f)*.8076923f;var scale=t.localScale;scale.x*=.8076923f;if(t.name=="Office timber floor"){q.z=9.23f+(q.z-9.23f)*.6f;scale.z*=.6f;}Set(t,new Vector3(x,q.y,q.z-1.43f),scale);}
var curtains=GameObject.Find("Right curtain extension").transform;foreach(Transform t in curtains)if(t.position.z>14.7f){UnityEditor.Undo.RecordObject(t.gameObject,"Curtain extent");t.gameObject.SetActive(false);}
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(s);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(s);
return "Room changed from 9 x 18 to 9 x 12.6 Unity metres; aisle widened by moving office edge 0.5 m left and right equipment 0.2 m right. "+backup;
