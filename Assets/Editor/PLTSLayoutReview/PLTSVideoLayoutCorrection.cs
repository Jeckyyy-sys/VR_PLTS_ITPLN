using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Editor-only, explicitly invoked. Dimensions are visual estimates, not a site survey.
public static class PLTSVideoLayoutCorrection
{
    const string Marker = "Entrance layout correction IMG_7484";
    static Transform[] objects;
    static Transform additions;
    static readonly List<string> report = new List<string>();
    static Transform One(string name) {
        var found = objects.Where(t => t.name == name).ToArray();
        if (found.Length != 1) throw new InvalidOperationException(name + ": expected one object, found " + found.Length);
        return found[0];
    }
    static void Place(Transform t, Vector3 p, Vector3? scale = null) {
        Undo.RecordObject(t, "Align PLTS layout"); t.position=p;
        if(scale.HasValue) t.localScale=scale.Value;
    }
    static void Hide(Transform t) { Undo.RecordObject(t.gameObject,"Preserve superseded detail");t.gameObject.SetActive(false); }
    static GameObject Box(string name, Vector3 p, Vector3 size, Material material) {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;
        g.transform.SetParent(additions,false);g.transform.position=p;g.transform.localScale=size;
        g.GetComponent<Renderer>().sharedMaterial=material;
        Undo.RegisterCreatedObjectUndo(g,"Align PLTS layout");return g;
    }
    static void RefreshObjects(Scene scene) {
        objects=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
    }

    [MenuItem("Tools/PLTS/Apply video layout correction")]
    public static void Apply() {
        var scene=SceneManager.GetActiveScene();
        if(scene.path!="Assets/Scenes/ITPLNCampusPLTS.unity" || EditorApplication.isPlaying)
            throw new InvalidOperationException("Open ITPLNCampusPLTS in Edit Mode first.");
        if(scene.isDirty) throw new InvalidOperationException("Save or review existing scene edits before applying this correction.");
        RefreshObjects(scene);
        if(objects.Any(t=>t.name==Marker)) throw new InvalidOperationException("Layout correction already applied; use Undo or the scene backup to revise it.");
        foreach(var n in new[]{"Laboratory sliding entrance","South wall","East wall before doorway","East wall after doorway","Door header","Laboratory Video Details","Interior floor"}) One(n);
        Directory.CreateDirectory("Backups/LayoutReview-20261123");
        var backup="Backups/LayoutReview-20261123/BeforeApply-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".unity";
        File.Copy(scene.path,backup,false);
        Undo.IncrementCurrentGroup(); int group=Undo.GetCurrentGroup();Undo.SetCurrentGroupName("Align environment to IMG_7484");
        foreach(var root in scene.GetRootGameObjects()) Undo.RegisterFullObjectHierarchyUndo(root,"Align environment to IMG_7484");
        report.Clear();report.Add("Source: IMG_7484.MOV and supplied entrance photo. Positions are proportional estimates.");
        try {
            if(!objects.Any(t=>t.name=="Exterior reference finishing")) PLTSReferenceEnvironmentBuilder.Build();
            RefreshObjects(scene);
            additions=new GameObject(Marker).transform;
            Undo.RegisterCreatedObjectUndo(additions.gameObject,"Align PLTS layout");
            var door=One("Laboratory sliding entrance");
            Place(door,new Vector3(-2.4f,.47f,2.86f));door.rotation=Quaternion.identity;
            // The entrance belongs to the short end wall, around the corner from the long PV facade.
            var south=One("South wall");var plaster=south.GetComponent<Renderer>().sharedMaterial;
            Place(south,new Vector3(-5.185f,2.52f,3.06f),new Vector3(3.43f,4.14f,.15f));
            Box("Entrance wall right return",new Vector3(.385f,2.52f,3.06f),new Vector3(3.43f,4.14f,.15f),plaster);
            var header=One("Door header");
            Place(header,new Vector3(-2.4f,3.85f,3.06f),new Vector3(2.14f,1.48f,.15f));
            Place(One("East wall before doorway"),new Vector3(2.04f,2.52f,5.16f),new Vector3(.15f,4.14f,4.2f));
            // Existing east wall after doorway remains in place, meeting the extended segment.
            foreach(var t in objects.Where(t=>t.name=="Annex Entry Glazing" || t.name=="Annex Door Mullion")) Hide(t);
            report.Add("Entrance: south/end facade, x=-2.4, z=2.86, yaw=0; clear opening 2.14 m, existing sliding leaves preserved.");

            // Reuse the existing short canopy and trusses, turning the complete structural set.
            var turn=Quaternion.Euler(0,90,0);var shift=new Vector3(-8.25f,0,5.2f);
            var veranda=One("Laboratory Video Details");
            foreach(Transform t in veranda) {
                bool structural=t.name=="Sloping corrugated canopy" || t.name=="Longitudinal roof purlin" ||
                    t.name=="Canopy gutter" || t.name=="Roof corrugation" || t.name=="Steel veranda post" ||
                    t.name=="Post foot" || t.name.StartsWith("Truss ",StringComparison.Ordinal);
                if(structural && t.gameObject.activeSelf) {
                    var p=t.position; var r=t.rotation;Place(t,turn*p+shift);t.rotation=turn*r;
                    // Preserve the centre post inactive; the two end posts support this span.
                    if((t.name=="Steel veranda post" || t.name=="Post foot") && Mathf.Abs(t.position.x+2.4f)<.5f)
                        Hide(t);
                }
            }
            var tile=One("Interior floor").GetComponent<Renderer>().sharedMaterial;
            Box("Entrance continuous landing",new Vector3(-2.4f,.225f,1.65f),new Vector3(6.6f,.45f,2.9f),tile);
            for(int i=1;i<=4;i++) {
                var step=One("Turquoise entrance step "+i);
                Place(step,new Vector3(-2.4f,.06f*i,-1.32f+.36f*i),new Vector3(2.35f,.12f*i,.36f));step.rotation=Quaternion.identity;
            }
            Place(One("Entrance dark mat"),new Vector3(-2.4f,.465f,2.15f));One("Entrance dark mat").rotation=Quaternion.identity;
            report.Add("Canopy, trusses and posts moved together to cover entrance; continuous landing and four turquoise steps aligned.");

            var approach=One("Stair approach concrete");Place(approach,new Vector3(-2.4f,.025f,-3.49f));
            Place(One("Red entry threshold"),new Vector3(-2.4f,.03f,-6.06f));
            // Generated groove mesh stores vertices in world coordinates.
            Place(One("Approach anti slip grooves"),new Vector3(-5.92f,0,-2.74f));
            Place(One("Entrance garden soil"),new Vector3(-6.72f,.018f,-2.64f));
            Place(One("Garden concrete edge"),new Vector3(-3.97f,.13f,-2.64f));
            foreach(var t in objects.Where(t=>t.name.StartsWith("Entrance hedge ",StringComparison.Ordinal)))
                Place(t,t.position+new Vector3(-7.12f,0,-2.74f));
            foreach(var t in objects.Where(t=>t.name.StartsWith("Tiered garden shrub",StringComparison.Ordinal)||t.name.StartsWith("Topiary trunk",StringComparison.Ordinal)))
                Place(t,t.position+new Vector3(-4.6f,0,-1.4f));
            foreach(var t in objects.Where(t=>t.name.StartsWith("Utility house ",StringComparison.Ordinal)||t.name=="Utility screened opening"))
                Place(t,t.position+new Vector3(-5.92f,0,-2.74f));
            Place(One("White utility screen"),new Vector3(-5.92f,0,-2.74f));
            report.Add("Approach, garden, hedge and screened utility house positioned beside the entrance; PV passage retained.");
            foreach(var t in objects.Where(t=>t.name=="Continuous pleated curtains"||t.name=="Continuous aluminium glazing rail"||t.name=="Curtain wall mullion")) {
                var copy=UnityEngine.Object.Instantiate(t.gameObject,additions);
                copy.name="End facade extension - "+t.name;
                var p=t.position;p.z=p.z*.54f-1.7192f;copy.transform.position=p;
                var size=t.localScale;size.z*=.54f;copy.transform.localScale=size;
                Undo.RegisterCreatedObjectUndo(copy,"Extend lab glazing");
            }

            // Simple blockers for the finished fence, which otherwise consists only of rendered wires.
            var fence=One("Last fence post").GetComponent<Renderer>().sharedMaterial;
            var boundary=Box("Road fence collision",new Vector3(11.05f,.75f,11.2f),new Vector3(.08f,1.5f,17.6f),fence);
            boundary.GetComponent<Renderer>().enabled=false;
            var front=Box("Front fence collision",new Vector3(8.7f,.75f,2.4f),new Vector3(4.7f,1.5f,.08f),fence);
            front.GetComponent<Renderer>().enabled=false;
            report.Add("Road and front fence colliders added; access gate and central passage remain open.");
            RefreshObjects(scene);Physics.SyncTransforms();
            var xr=objects.Count(t=>t.gameObject.activeInHierarchy && t.name.StartsWith("XR Origin",StringComparison.Ordinal));
            report.Add("Active XR Origin objects: "+xr+". Layout script does not configure XR, input, packages or render pipeline. Unity may reserialize package settings during import.");
            var missing=objects.Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
            report.Add("Missing script components in scene: "+missing);
            var blockers=Physics.OverlapBox(new Vector3(-2.4f,1.7f,3.06f),new Vector3(.9f,1.1f,.07f))
                .Where(c=>!c.isTrigger && !c.transform.IsChildOf(door)).Select(c=>c.name).Distinct().ToArray();
            if(blockers.Length>0) throw new InvalidOperationException("Doorway blocked by: "+string.Join(", ",blockers));
            report.Add("Doorway clearance check passed (excluding the intended sliding leaves).");
            EditorSceneManager.MarkSceneDirty(scene);AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Docs");File.WriteAllLines("Docs/LayoutReview-20261123.txt",report);
            ReviewExterior.Capture();
            if(!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save the corrected scene.");
            Undo.CollapseUndoOperations(group);
            Selection.activeGameObject=door.gameObject;
            if(SceneView.lastActiveSceneView) SceneView.lastActiveSceneView.LookAt(new Vector3(0,2.5f,5),Quaternion.LookRotation(new Vector3(-.7f,-.25f,1)),19,false,true);
            Debug.Log("PLTS_LAYOUT_COMPLETE: "+string.Join(" | ",report));
        } catch(Exception ex) {
            Undo.RevertAllDownToGroup(group);Debug.LogException(ex);
            Directory.CreateDirectory("Docs");File.WriteAllText("Docs/LayoutReview-error.txt",ex.ToString());
            throw;
        }
    }
}
