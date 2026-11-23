using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
public static class RefineExterior7484
{
    static Transform root;
    static Material teal, silver, grey, yellow, black, white;
    static Material M(string n,Color c){var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=c;m.SetFloat("_Smoothness",.2f);AssetDatabase.CreateAsset(m,"Assets/PLTS/ExteriorPrecision/"+n+".mat");return m;}
    static GameObject B(string n,Vector3 p,Vector3 s,Material m){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(root,false);g.transform.localPosition=p;g.transform.localScale=s;g.GetComponent<Renderer>().sharedMaterial=m;return g;}
    static void Line(string n,Vector3[] p,float w,Material m){var g=new GameObject(n);g.transform.SetParent(root,false);var l=g.AddComponent<LineRenderer>();l.useWorldSpace=false;l.sharedMaterial=m;l.widthMultiplier=w;l.positionCount=p.Length;l.SetPositions(p);l.numCornerVertices=4;}
    static void Text(string value,Vector3 p,Vector2 size,float font){var g=new GameObject(value);g.transform.SetParent(root,false);g.transform.localPosition=p;g.transform.localRotation=Quaternion.Euler(0,-90,0);var t=g.AddComponent<TextMeshPro>();t.text=value;t.font=TMP_Settings.defaultFontAsset;t.color=Color.black;t.fontSize=font;t.alignment=TextAlignmentOptions.Center;t.rectTransform.sizeDelta=size;}
    public static string Build(){
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(scene.name!="ITPLNCampusPLTS"||EditorApplication.isPlaying)throw new Exception("Wrong scene or mode");
        if(GameObject.Find("Exterior precision IMG_7484"))throw new Exception("Already applied");
        root=new GameObject("Exterior precision IMG_7484").transform;
        teal=M("Blue turquoise plinth",new Color(.035f,.48f,.59f));silver=M("Silver galvanized frame",new Color(.57f,.59f,.56f));grey=M("Worn concrete",new Color(.56f,.54f,.48f));yellow=M("Yellow ventilation grille",new Color(.82f,.62f,.08f));black=M("Dark joints",new Color(.13f,.14f,.12f));white=M("Warm white enamel",new Color(.87f,.87f,.82f));
        var all=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
        // Widen the passage by moving the road-side array as a unit.
        for(int i=1;i<=7;i++){
            var rack=GameObject.Find("PV Rack "+i).transform;rack.position+=Vector3.right*.6f;
            if(i>=4){
                var modules=rack.Find("Inclined Modules");modules.localPosition+=Vector3.down*.85f;
                foreach(var post in rack.Cast<Transform>().Where(t=>t.name=="Rack Post")){
                    post.localPosition+=Vector3.down*.425f;var s=post.localScale;s.y-=.85f;post.localScale=s;
                }
            }
        }
        foreach(var t in all.Where(t=>t.name=="Rack Concrete Footing"||t.name.StartsWith("PLTS Fence")||t.name=="Fence diamond"||t.name=="Red service pipe"))t.position+=Vector3.right*.6f;
        var old=GameObject.Find("Exterior video details IMG_6688").transform;
        foreach(var t in old.Cast<Transform>().Where(t=>t.name.StartsWith("PV rack ")||t.name=="Hanging PV interconnect"||t.name=="Longitudinal array channel").ToArray()){
            if(t.name=="Longitudinal array channel"){t.gameObject.SetActive(false);continue;}
            t.position+=Vector3.right*.6f;
            if(t.position.z>9.5f)t.gameObject.SetActive(false);
        }
        var cabinet=GameObject.Find("OffGridCabinet").transform;cabinet.position=new Vector3(8.3f,cabinet.position.y,7.6f);
        var battery=GameObject.Find("Battery Cabinet Case").transform;battery.position=new Vector3(8.3f,battery.position.y,5.96f);
        var path=GameObject.Find("Central concrete passage").transform;path.position=new Vector3(5.6f,.065f,11);path.localScale=new Vector3(1.55f,.07f,18);
        foreach(var t in all.Where(t=>t.name=="Passage expansion joint")){t.position+=Vector3.right*.3f;var s=t.localScale;s.x=1.54f;t.localScale=s;}
        // Blue plinth and yellow ventilation openings visible under the windows.
        B("Turquoise lower wall",new Vector3(2.14f,.52f,15.1f),new Vector3(.09f,1.04f,11.6f),teal);
        foreach(float z in new[]{10.1f,14.5f,18.8f}){
            B("Vent dark recess",new Vector3(2.2f,.55f,z),new Vector3(.03f,.43f,.68f),black);
            foreach(float y in new[]{.31f,.79f})B("Vent yellow frame",new Vector3(2.225f,y,z),new Vector3(.04f,.035f,.76f),yellow);
            foreach(float offset in new[]{-.36f,.36f})B("Vent yellow edge",new Vector3(2.225f,.55f,z+offset),new Vector3(.04f,.5f,.035f),yellow);
            for(int j=0;j<7;j++)B("Vent horizontal louvre",new Vector3(2.24f,.36f+j*.06f,z),new Vector3(.035f,.018f,.7f),yellow);
        }
        // Service pipes beneath the low array.
        foreach(float x in new[]{2.5f,2.62f,2.74f})Line("Parallel service pipe",new[]{new Vector3(x,.6f,9.7f),new Vector3(x,.6f,20.1f)},.055f,white);
        // Concrete wear stays flat so the walking collider remains continuous.
        var rng=new System.Random(7484);
        for(int i=0;i<22;i++){
            float x=5.02f+(float)rng.NextDouble()*1.13f,z=3f+(float)rng.NextDouble()*16f;
            var stain=GameObject.CreatePrimitive(PrimitiveType.Cylinder);stain.name="Concrete worn patch";stain.transform.SetParent(root,false);stain.transform.position=new Vector3(x,.102f,z);stain.transform.localScale=new Vector3(.12f+(float)rng.NextDouble()*.3f,.001f,.16f+(float)rng.NextDouble()*.38f);stain.GetComponent<Renderer>().sharedMaterial=grey;UnityEngine.Object.DestroyImmediate(stain.GetComponent<Collider>());
        }
        for(int i=0;i<7;i++){float z=4+i*2.2f;Line("Concrete hairline crack",new[]{new Vector3(4.85f,.104f,z),new Vector3(5.2f,.104f,z+.13f),new Vector3(5.45f,.104f,z-.04f),new Vector3(5.85f,.104f,z+.16f)},.007f,black);}
        // Wall-mounted handwash basin and meter beside the entrance.
        B("Handwash basin",new Vector3(2.48f,1.28f,9.3f),new Vector3(.5f,.17f,.5f),white);
        B("Basin recessed interior",new Vector3(2.5f,1.37f,9.3f),new Vector3(.33f,.006f,.34f),grey);
        Line("Basin faucet",new[]{new Vector3(2.27f,1.38f,9.3f),new Vector3(2.27f,1.57f,9.3f),new Vector3(2.4f,1.57f,9.3f)},.025f,silver);
        Line("Basin drain",new[]{new Vector3(2.48f,1.2f,9.3f),new Vector3(2.48f,.75f,9.3f),new Vector3(2.22f,.75f,9.3f)},.04f,white);
        B("Entrance meter enclosure",new Vector3(2.3f,2.1f,9.25f),new Vector3(.23f,.52f,.35f),white);
        B("Meter display",new Vector3(2.425f,2.17f,9.25f),new Vector3(.012f,.11f,.2f),black);
        // Warning text is legible in the supplied video.
        B("IPAL warning plate",new Vector3(4.95f,.65f,2.7f),new Vector3(.03f,.65f,.6f),yellow);
        Text("HATI-HATI\nADA PERALATAN\nIPAL",new Vector3(4.973f,.65f,2.7f),new Vector2(.55f,.58f),.68f);
        // Shorter rear tables have their own lower rails and braces.
        for(int i=0;i<4;i++){
            float z=10.75f+i*2.35f;
            Line("Low table crossbeam",new[]{new Vector3(6.5f,1.82f,z),new Vector3(10.1f,.86f,z)},.055f,silver);
            Line("Low table diagonal",new[]{new Vector3(6.5f,.3f,z),new Vector3(7.4f,1.5f,z)},.04f,silver);
        }
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        return "Adjusted passage, stepped array heights, equipment position, blue plinth, vents, service pipes, concrete wear, sink and entrance meter.";
    }
}
