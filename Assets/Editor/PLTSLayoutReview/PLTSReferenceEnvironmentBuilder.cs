using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using TMPro;

public static class PLTSReferenceEnvironmentBuilder
{
    const string Folder = "Assets/PLTS/ExteriorFinish";
    static Transform root;
    static Material steel, concrete, dark, ivory, blue, red, yellow, curtain;
    static Material Mat(string n, Color c, float smooth = .2f)
    {
        var path=Folder+"/"+n+".mat";
        var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!m) { m=new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m,path); }
        m.color=c;m.SetFloat("_Smoothness",smooth);m.enableInstancing=true;return m;
    }
    static GameObject Box(string n, Vector3 p, Vector3 s, Material m, bool collision=true, Transform parent=null)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(parent?parent:root,false);
        g.transform.localPosition=p;g.transform.localScale=s;g.GetComponent<Renderer>().sharedMaterial=m;
        if(!collision) UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;
    }
    static GameObject Rod(string n,Vector3 a,Vector3 b,float d,Material m, bool collision=false)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name=n;g.transform.SetParent(root,false);
        g.transform.position=(a+b)*.5f;g.transform.rotation=Quaternion.FromToRotation(Vector3.up,b-a);
        g.transform.localScale=new Vector3(d,(b-a).magnitude*.5f,d);g.GetComponent<Renderer>().sharedMaterial=m;
        if(!collision)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;
    }
    static void Text(string n, string value,Vector3 p,Vector3 euler,Vector2 size,float fs,Color color)
    {
        var g=new GameObject(n);g.transform.SetParent(root,false);g.transform.position=p;g.transform.eulerAngles=euler;
        var t=g.AddComponent<TextMeshPro>();t.text=value;t.font=TMP_Settings.defaultFontAsset;t.fontSize=fs;
        t.color=color;t.alignment=TextAlignmentOptions.Center;t.rectTransform.sizeDelta=size;
    }
    // A single mesh per fence bay avoids hundreds of individual wire renderers.
    sealed class WireMesh
    {
        public List<Vector3> v=new List<Vector3>();public List<int> t=new List<int>();
        public void Segment(Vector3 a,Vector3 b,float radius)
        {
            var d=(b-a).normalized;var s=Vector3.Cross(d,Mathf.Abs(d.y)<.9f?Vector3.up:Vector3.right).normalized*radius;
            var u=Vector3.Cross(d,s).normalized*radius;int k=v.Count;
            v.AddRange(new[]{a-s-u,a+s-u,a+s+u,a-s+u,b-s-u,b+s-u,b+s+u,b-s+u});
            int[] tri={0,1,5,0,5,4,1,2,6,1,6,5,2,3,7,2,7,6,3,0,4,3,4,7};foreach(int i in tri)t.Add(k+i);
        }
        public void Save(string n, Material material)
        {
            var mesh=new Mesh{name=n};mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh,Folder+"/"+n+".asset");var g=new GameObject(n);g.transform.SetParent(root,false);
            g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=material;
        }
    }
    static void Fence(string name,Vector3 a,Vector3 b)
    {
        Vector3 along=(b-a).normalized;float length=Vector3.Distance(a,b);var mesh=new WireMesh();
        var normal=Vector3.Cross(along,Vector3.up);
        for(float x=0;x<length+.001f;x+=.105f)
        {
            Vector3 p=a+along*Mathf.Min(x,length);mesh.Segment(p+Vector3.up*.17f,p+Vector3.up*1.27f,.0025f);
            mesh.Segment(p+Vector3.up*1.27f,p+Vector3.up*1.37f+normal*.085f,.0025f);
            mesh.Segment(p+Vector3.up*1.37f+normal*.085f,p+Vector3.up*1.47f,.0025f);
        }
        for(float y=.17f;y<=1.28f;y+=.12f)mesh.Segment(a+Vector3.up*y,b+Vector3.up*y,.0025f);
        mesh.Segment(a+Vector3.up*1.37f+normal*.085f,b+Vector3.up*1.37f+normal*.085f,.0025f);
        mesh.Segment(a+Vector3.up*1.47f,b+Vector3.up*1.47f,.003f);mesh.Save(name,steel);
        foreach(float y in new[]{.13f,1.5f})Rod(name+" frame rail",a+Vector3.up*y,b+Vector3.up*y,.026f,steel);
    }
    public static string Build()
    {
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(scene.name!="ITPLNCampusPLTS"||EditorApplication.isPlaying)throw new InvalidOperationException("Campus Edit Mode required");
        if(GameObject.Find("Exterior reference finishing"))throw new InvalidOperationException("Already applied");
        if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/PLTS","ExteriorFinish");
        root=new GameObject("Exterior reference finishing").transform;Undo.RegisterCreatedObjectUndo(root.gameObject,"Finish exterior from video");
        steel=Mat("Weathered galvanized silver",new Color(.57f,.59f,.57f),.34f);steel.SetFloat("_Metallic",.35f);
        concrete=Mat("Textured exposed concrete",new Color(.77f,.75f,.7f));
        var albedo=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/FencesPack/Textures/Albedo/TexturesCom_Concrete_Base8_2x2_1K_albedo.tif");
        concrete.SetTexture("_BaseMap",albedo);concrete.SetTextureScale("_BaseMap",new Vector2(3,7));
        var bump=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/FencesPack/Textures/normal maps/TexturesCom_Concrete_Base8_2x2_1K_normal.tif");
        if(bump){concrete.SetTexture("_BumpMap",bump);concrete.SetFloat("_BumpScale",.35f);concrete.EnableKeyword("_NORMALMAP");}
        dark=Mat("Charcoal rubber",new Color(.055f,.06f,.058f));ivory=Mat("Aged warm white",new Color(.83f,.84f,.81f));
        blue=Mat("Turquoise site wall",new Color(.045f,.43f,.53f));red=Mat("Oxide red pipe",new Color(.44f,.045f,.025f),.26f);
        yellow=Mat("Curb safety yellow",new Color(.83f,.61f,.045f));curtain=Mat("Grey green curtain",new Color(.58f,.63f,.6f));
        var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
        foreach(var t in all)
        {
            bool hide=t.name=="Low Flat Roof"||t.name=="Annex Parapet"||t.name=="Rooftop Service Unit"||
                t.name=="Concrete worn patch"||t.name=="Concrete hairline crack"||t.name=="Fence diamond"||
                t.name.StartsWith("PLTS Fence")||t.name.StartsWith("Rear green roller")||t.name=="Rear shade bottom rail"||
                t.name=="Entrance meter enclosure"||t.name=="Meter display"||t.name=="Downpipe"&&t.position.z>15||
                t.name=="Low Building Window"&&t.position.z>9.4f;
            if(hide){Undo.RecordObject(t.gameObject,"Remove superseded exterior detail");t.gameObject.SetActive(false);}
            if(t.name=="Hanging PV interconnect")
            {
                var l=t.GetComponent<LineRenderer>();if(l&&l.positionCount>0&&l.GetPosition(0).z>9.5f)
                    for(int j=0;j<l.positionCount;j++)l.SetPosition(j,l.GetPosition(j)-Vector3.up*.85f);
            }
            if(t.name=="PLTS Concrete Pad") {t.position=new Vector3(8.3f,t.position.y,t.position.z);var s=t.localScale;s.x=5.5f;t.localScale=s;}
            if(t.name=="Central concrete passage"||t.name=="PLTS Concrete Pad"||t.name=="Campus Ground")
            {
                var m=new Material(concrete);m.name=t.name+" surface";
                m.SetTextureScale("_BaseMap",new Vector2(t.lossyScale.x*.6f,t.lossyScale.z*.6f));
                AssetDatabase.CreateAsset(m,Folder+"/"+m.name+".mat");t.GetComponent<Renderer>().sharedMaterial=m;
            }
        }
        var curbs=all.Where(t=>t.name=="Curb").OrderBy(t=>t.position.z).ToArray();
        for(int i=0;i<curbs.Length;i++)curbs[i].GetComponent<Renderer>().sharedMaterial=i%2==0?yellow:dark;
        // Rectangular welded wire with its characteristic upper fold.
        for(int i=0;i<8;i++)
        {
            float z=2.4f+i*2.2f;Fence("Road fence bay "+i,new Vector3(11.05f,0,z),new Vector3(11.05f,0,z+2.2f));
            Box("Galvanized fence post",new Vector3(11.05f,.78f,z),new Vector3(.045f,1.56f,.045f),steel);
        }
        Box("Last fence post",new Vector3(11.05f,.78f,20),new Vector3(.045f,1.56f,.045f),steel);
        for(int i=0;i<2;i++)
        {
            float x=6.35f+i*2.35f;Fence("Front fence bay "+i,new Vector3(x,0,2.4f),new Vector3(x+2.35f,0,2.4f));
            Box("Front fence post",new Vector3(x,.78f,2.4f),new Vector3(.045f,1.56f,.045f),steel);
        }
        Fence("Open access gate",new Vector3(6.35f,0,1.1f),new Vector3(6.35f,0,2.4f));
        Rod("Gate pull handle",new Vector3(6.3f,.72f,1.22f),new Vector3(6.3f,.94f,1.22f),.025f,steel);
        // Replace the old square-section pipe with a round service main and flange joints.
        foreach(var t in all.Where(t=>t.name=="Red service pipe"))t.gameObject.SetActive(false);
        Rod("Main red service pipe",new Vector3(10.65f,.5f,2.4f),new Vector3(10.65f,.5f,20.2f),.18f,red);
        foreach(float z in new[]{3.2f,8.5f,13.8f,19.5f})
        {
            Rod("Pipe flange",new Vector3(10.65f,.5f,z-.035f),new Vector3(10.65f,.5f,z+.035f),.26f,red);
            for(int i=0;i<8;i++) {float a=i*Mathf.PI/4;Rod("Flange bolt",new Vector3(10.65f+Mathf.Cos(a)*.108f,.5f+Mathf.Sin(a)*.108f,z-.045f),new Vector3(10.65f+Mathf.Cos(a)*.108f,.5f+Mathf.Sin(a)*.108f,z+.045f),.016f,dark);}
            Box("Pipe concrete saddle",new Vector3(10.65f,.16f,z),new Vector3(.34f,.32f,.24f),concrete);
        }
        Rod("Valve riser",new Vector3(10.65f,.5f,3.6f),new Vector3(10.65f,1.07f,3.6f),.055f,red);
        var wheel=new WireMesh();for(int i=0;i<24;i++){float a=i*Mathf.PI/12,b=(i+1)*Mathf.PI/12;wheel.Segment(new Vector3(10.65f+Mathf.Cos(a)*.12f,1.07f,3.6f+Mathf.Sin(a)*.12f),new Vector3(10.65f+Mathf.Cos(b)*.12f,1.07f,3.6f+Mathf.Sin(b)*.12f),.012f);}wheel.Save("Valve wheel",red);
        // Continuous curtained glazing, tall lower panes and narrow top lights.
        var verts=new List<Vector3>();var tris=new List<int>();
        for(int i=0;i<400;i++)
        {
            float z=9.48f+i*(11.25f/400),zz=z+11.25f/400;
            float x=2.17f+.014f*Mathf.Sin(i*.9f),xx=2.17f+.014f*Mathf.Sin((i+1)*.9f);int n=verts.Count;
            verts.AddRange(new[]{new Vector3(x,1.05f,z),new Vector3(xx,1.05f,zz),new Vector3(xx,3.9f,zz),new Vector3(x,3.9f,z)});
            tris.AddRange(new[]{n,n+2,n+1,n,n+3,n+2});
        }
        var curtainMesh=new Mesh{name="Curtain folds exterior"};curtainMesh.SetVertices(verts);curtainMesh.SetTriangles(tris,0);curtainMesh.RecalculateNormals();curtainMesh.RecalculateBounds();
        AssetDatabase.CreateAsset(curtainMesh,Folder+"/Curtain folds.asset");var fabric=new GameObject("Continuous pleated curtains");fabric.transform.SetParent(root,false);fabric.AddComponent<MeshFilter>().sharedMesh=curtainMesh;fabric.AddComponent<MeshRenderer>().sharedMaterial=curtain;curtain.SetFloat("_Cull",0);
        foreach(float y in new[]{1.05f,2.92f,3.9f})Box("Continuous aluminium glazing rail",new Vector3(2.205f,y,15.105f),new Vector3(.065f,.045f,11.3f),steel,false);
        for(int i=0;i<=9;i++)Box("Curtain wall mullion",new Vector3(2.205f,2.47f,9.48f+i*1.25f),new Vector3(.065f,2.86f,.045f),steel,false);
        // Concrete approach at the stair foot, bordered garden and red road threshold.
        Box("Stair approach concrete",new Vector3(3.52f,.025f,-.75f),new Vector3(2.35f,.05f,4.7f),concrete);
        Box("Red entry threshold",new Vector3(3.52f,.03f,-3.32f),new Vector3(2.35f,.06f,.45f),red);
        var joints=new WireMesh();for(int i=0;i<29;i++){float z=-3.04f+i*.16f;joints.Segment(new Vector3(2.35f,.053f,z),new Vector3(4.69f,.053f,z),.002f);}joints.Save("Approach anti slip grooves",dark);
        Box("Entrance garden soil",new Vector3(-.8f,.018f,.1f),new Vector3(5.4f,.035f,2.1f),Mat("Garden soil",new Color(.20f,.18f,.12f)));
        Box("Garden concrete edge",new Vector3(1.95f,.13f,.1f),new Vector3(.12f,.26f,2.2f),concrete);
        // Small screened service house, seen to the left of the access path in the video.
        Box("Utility house back",new Vector3(-2.3f,1.3f,-1.0f),new Vector3(2.25f,2.6f,.12f),ivory);
        Box("Utility house left",new Vector3(-3.4f,1.3f,-.25f),new Vector3(.12f,2.6f,1.6f),ivory);
        Box("Utility house right",new Vector3(-1.2f,1.3f,-.25f),new Vector3(.12f,2.6f,1.6f),ivory);
        Box("Utility house roof",new Vector3(-2.3f,2.65f,-.25f),new Vector3(2.5f,.12f,1.85f),ivory);
        Box("Utility screened opening",new Vector3(-2.3f,1.5f,.55f),new Vector3(2.05f,2.1f,.03f),dark);
        var grille=new WireMesh();for(int i=0;i<=16;i++)grille.Segment(new Vector3(-3.3f,.45f+i*.125f,.58f),new Vector3(-1.3f,.45f+i*.125f,.58f),.011f);
        for(int i=0;i<=16;i++)grille.Segment(new Vector3(-3.3f+i*.125f,.45f,.58f),new Vector3(-3.3f+i*.125f,2.5f,.58f),.011f);grille.Save("White utility screen",ivory);
        // Small photovoltaic powered instrument at the back of the yard.
        Rod("Rear sensor upright",new Vector3(4.85f,.48f,20.2f),new Vector3(4.85f,4.15f,20.2f),.045f,steel);
        var solar=Box("Instrument PV frame",new Vector3(4.85f,3.45f,20.2f),new Vector3(.72f,.045f,.55f),steel,false);solar.transform.rotation=Quaternion.Euler(20,0,0);
        var cell=Box("Instrument PV glass",new Vector3(4.85f,3.478f,20.21f),new Vector3(.67f,.005f,.5f),Mat("Instrument dark glass",new Color(.035f,.07f,.095f),.5f),false);cell.transform.rotation=solar.transform.rotation;
        Box("Instrument logger box",new Vector3(4.85f,1.5f,20.2f),new Vector3(.3f,.22f,.15f),ivory);
        Box("Rear turquoise boundary",new Vector3(5.7f,.43f,21.2f),new Vector3(7f,.86f,.16f),blue);
        Rod("Rear rising pipe",new Vector3(10.65f,.5f,20.2f),new Vector3(10.65f,1.4f,20.2f),.18f,red);
        Rod("Rear cross site pipe",new Vector3(10.65f,1.4f,20.2f),new Vector3(2.8f,1.4f,20.2f),.18f,red);
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);
        return "Finished curtain wall, folded wire fences and access gate, round pipe flanges and valve, concrete approach, utility enclosure, texture surfaces, roof cleanup and scene-scoped lighting.";
    }
}

