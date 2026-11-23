using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class MatchExteriorVideo
{
    private static Transform root;
    private static Material steel, concrete, cable, ivory, red;
    private const string Folder="Assets/PLTS/ExteriorVideo/";
    private static Material Mat(string name,Color color)
    {
        var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));
        m.color=color;m.SetFloat("_Smoothness",.18f);
        AssetDatabase.CreateAsset(m,Folder+name+".mat");return m;
    }
    private static GameObject Box(string name,Vector3 pos,Vector3 size,Material mat,Transform parent=null)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;
        g.transform.SetParent(parent?parent:root,false);g.transform.localPosition=pos;
        g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=mat;return g;
    }
    private static void Beam(string name,Vector3 a,Vector3 b,float width,Material mat)
    {
        var g=Box(name,(a+b)*.5f,new Vector3(width,Vector3.Distance(a,b),width),mat);
        g.transform.rotation=Quaternion.FromToRotation(Vector3.up,b-a);
    }
    private static void Wire(string name,Vector3[] points,float width,Material mat)
    {
        var g=new GameObject(name);g.transform.SetParent(root,false);
        var line=g.AddComponent<LineRenderer>();line.useWorldSpace=false;line.sharedMaterial=mat;
        line.widthMultiplier=width;line.positionCount=points.Length;line.SetPositions(points);
        line.numCornerVertices=5;line.numCapVertices=4;
    }
    public static string Build()
    {
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(scene.name!="ITPLNCampusPLTS"||EditorApplication.isPlaying)throw new Exception("Campus edit mode required");
        if(GameObject.Find("Exterior video details IMG_6688"))throw new Exception("Already applied");
        root=new GameObject("Exterior video details IMG_6688").transform;
        steel=Mat("Galvanized dark steel",new Color(.27f,.29f,.27f));
        concrete=Mat("Aged concrete",new Color(.49f,.47f,.41f));
        cable=Mat("Cable rubber",new Color(.025f,.028f,.026f));
        ivory=Mat("Weathered panel backsheet",new Color(.72f,.71f,.64f));
        red=Mat("Service pipe red",new Color(.49f,.045f,.035f));
        // The existing entrance remains under its canopy; the filmed rear array is exposed.
        var veranda=GameObject.Find("Laboratory Video Details").transform;
        foreach(var t in veranda.Cast<Transform>().ToArray())
        {
            bool longRoof=t.name=="Sloping corrugated canopy"||t.name=="Longitudinal roof purlin"||t.name=="Canopy gutter";
            if(longRoof){var p=t.position;p.z=5.85f;t.position=p;var s=t.localScale;s.z=6.6f;t.localScale=s;}
            bool roofPart=t.name=="Roof corrugation"||t.name=="Steel veranda post"||t.name=="Post foot"||t.name.StartsWith("Truss ");
            if(roofPart&&t.position.z>9.1f)t.gameObject.SetActive(false);
        }
        var source=GameObject.Find("PV Rack 1").transform.Find("Inclined Modules");
        for(int i=0;i<4;i++)
        {
            float z=11.1f+i*2.35f;
            var bank=new GameObject("Building side PV rack "+(i+1)).transform;
            bank.SetParent(root,false);bank.position=new Vector3(3.8f,1.95f,z);
            var modules=UnityEngine.Object.Instantiate(source.gameObject,bank).transform;
            modules.name="Low inclined photovoltaic modules";
            modules.localPosition=Vector3.zero;modules.localRotation=Quaternion.Euler(0,0,-12);
            modules.localScale=new Vector3(.52f,1,1);
            foreach(float x in new[]{2.9f,4.65f})
            {
                float height=x<3?2.1f:1.72f;
                Box("Low array concrete footing",new Vector3(x,.53f,z),new Vector3(.35f,.12f,.4f),concrete);
                Beam("Low array upright",new Vector3(x,.59f,z),new Vector3(x,height,z),.055f,steel);
            }
            Beam("Low array transverse rail",new Vector3(2.75f,2.08f,z),new Vector3(4.85f,1.63f,z),.06f,steel);
            Beam("Low array diagonal brace",new Vector3(2.9f,.64f,z),new Vector3(4.65f,1.66f,z),.035f,steel);
            Wire("Low array cable loop",new[]{new Vector3(3.4f,1.9f,z),new Vector3(3.6f,1.48f,z+.15f),new Vector3(3.95f,1.44f,z+.25f),new Vector3(4.15f,1.75f,z+.35f)},.016f,cable);
        }
        // Keep a continuous central passage between the two arrays.
        Box("Central concrete passage",new Vector3(5.3f,.065f,11),new Vector3(.95f,.07f,18),concrete);
        for(int i=0;i<12;i++)Box("Passage expansion joint",new Vector3(5.3f,.102f,2.3f+i*1.55f),new Vector3(.94f,.002f,.009f),steel);
        for(int i=1;i<=7;i++)
        {
            var rack=GameObject.Find("PV Rack "+i);
            float z=rack.transform.position.z;
            Beam("PV rack transverse underside rail",new Vector3(5.9f,2.68f,z),new Vector3(9.5f,1.71f,z),.065f,steel);
            Beam("PV rack triangular bracing",new Vector3(5.9f,.3f,z),new Vector3(7.2f,2.3f,z),.04f,steel);
            Wire("Hanging PV interconnect",new[]{new Vector3(6.4f,2.5f,z),new Vector3(6.6f,2.12f,z+.12f),new Vector3(7.0f,1.95f,z+.22f),new Vector3(7.35f,2.25f,z+.28f)},.018f,cable);
            var modules=rack.transform.Find("Inclined Modules");
            foreach(var t in modules.Cast<Transform>().Where(t=>t.name=="Aluminium Module Frame"))
                Box("Cream photovoltaic backsheet",t.localPosition+new Vector3(0,-.032f,0),new Vector3(1.98f,.008f,2.08f),ivory,modules);
        }
        foreach(float x in new[]{5.92f,9.48f})Beam("Longitudinal array channel",new Vector3(x,x<7?2.65f:1.7f,2.6f),new Vector3(x,x<7?2.65f:1.7f,18.9f),.055f,steel);
        // Details visible along the road-facing edge.
        Beam("Red service pipe",new Vector3(10.05f,.55f,2.5f),new Vector3(10.05f,.55f,19.8f),.12f,red);
        for(int i=0;i<16;i++)
        {
            float z=3+i*1.02f;
            Wire("Fence diamond",new[]{new Vector3(10.45f,.65f,z),new Vector3(10.45f,1.45f,z+.5f),new Vector3(10.45f,.65f,z+1),new Vector3(10.45f,.3f,z+.5f),new Vector3(10.45f,.65f,z)},.009f,steel);
        }
        Beam("Utility pole",new Vector3(10.9f,0,15.8f),new Vector3(10.9f,7.5f,15.8f),.13f,steel);
        Beam("Utility crossarm",new Vector3(10.9f,7.1f,15),new Vector3(10.9f,7.1f,16.6f),.06f,steel);
        Wire("Streetlight curved arm",new[]{new Vector3(10.9f,5.1f,15.8f),new Vector3(10.7f,5.65f,15.8f),new Vector3(10.3f,5.9f,15.8f),new Vector3(9.9f,5.95f,15.8f)},.045f,steel);
        Box("Streetlight head",new Vector3(9.9f,5.94f,15.8f),new Vector3(.43f,.09f,.17f),ivory);
        Wire("Overhead service cable",new[]{new Vector3(2.2f,5,4),new Vector3(5.2f,4.5f,8),new Vector3(8.1f,5.05f,12),new Vector3(10.9f,7.15f,15.8f),new Vector3(11.7f,6.4f,22)},.017f,cable);
        Beam("Slender instrument mast",new Vector3(5.5f,0,21),new Vector3(5.5f,9.5f,21),.045f,steel);
        Wire("Mast guy wire",new[]{new Vector3(5.5f,8.3f,21),new Vector3(9.5f,.2f,19)},.007f,steel);
        Wire("Mast second guy wire",new[]{new Vector3(5.5f,8.3f,21),new Vector3(2.5f,.5f,18.5f)},.007f,steel);
        // Small equipment on top of the battery case is visible in the close-up.
        var cabinet=GameObject.Find("Battery Cabinet Case").transform;
        Box("Small terminal enclosure",new Vector3(.23f,1.57f,.02f),new Vector3(.22f,.1f,.16f),ivory,cabinet);
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        return "Added building-side PV row, exposed rear layout, walkway, braces, backsheets, cables, pipe, fence detail, utility pole and mast.";
    }
}
