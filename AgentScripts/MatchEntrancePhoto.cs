using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static class MatchEntrancePhoto
{
    static Transform root;
    static Material print,metal,white,teal;
    static string folder="Assets/PLTS/EntrancePhoto/";
    static Vector2 UV(float x,float y)=>new Vector2(x/966f,1-y/722f);
    static void Face(Transform parent,string name,Vector3 pos,float width,float height,Vector2[] uv){
        var mesh=new Mesh();mesh.name=name;mesh.vertices=new[]{new Vector3(-width/2,-height/2,0),new Vector3(width/2,-height/2,0),new Vector3(width/2,height/2,0),new Vector3(-width/2,height/2,0)};mesh.uv=uv;mesh.triangles=new[]{0,2,1,0,3,2};mesh.RecalculateNormals();AssetDatabase.CreateAsset(mesh,folder+name+".asset");
        var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=pos;g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=print;
    }
    static GameObject Box(Transform p,string n,Vector3 pos,Vector3 scale,Material m){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=m;return g;}
    public static string Build(){
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(scene.name!="ITPLNCampusPLTS"||EditorApplication.isPlaying)throw new Exception("Wrong scene/mode");
        AssetDatabase.Refresh();var importer=(TextureImporter)AssetImporter.GetAtPath(folder+"EntranceFront.png");importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
        print=new Material(Shader.Find("Universal Render Pipeline/Unlit"));print.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"EntranceFront.png"));print.SetFloat("_Cull",0);AssetDatabase.CreateAsset(print,folder+"Photographic signage and motifs.mat");
        metal=AssetDatabase.LoadAssetAtPath<Material>("Assets/PLTS/SlidingEntrance/Brushed aluminium.mat");white=AssetDatabase.LoadAssetAtPath<Material>("Assets/PLTS/SlidingEntrance/Sign backing.mat");teal=AssetDatabase.LoadAssetAtPath<Material>("Assets/PLTS/ExteriorPrecision/Blue turquoise plinth.mat");
        root=GameObject.Find("Laboratory sliding entrance").transform;root.position=new Vector3(2.27f,.47f,6.2f);
        foreach(var t in root.GetComponentsInChildren<Transform>(true).Where(t=>t.name.Contains("Energy motif")||t.name.Contains("energy motif")||t.name=="Laboratory sign"||t.name=="ITPLN logo"||t.name=="Laboratorium Renewable Energy and Hybrid System"))t.gameObject.SetActive(false);
        var left=root.Find("Left sliding door");var right=root.Find("Right sliding door");
        Face(left,"Left door photographed motif",new Vector3(0,1.3f,-.026f),.92f,2.44f,new[]{UV(359,603),UV(482,605),UV(481,350),UV(373,347)});
        Face(right,"Right door photographed motif",new Vector3(0,1.3f,-.026f),.92f,2.44f,new[]{UV(504,605),UV(625,606),UV(614,352),UV(505,352)});
        Face(left,"Left inner door motif",new Vector3(0,1.3f,.026f),.92f,2.44f,new[]{UV(359,603),UV(482,605),UV(481,350),UV(373,347)});
        Face(right,"Right inner door motif",new Vector3(0,1.3f,.026f),.92f,2.44f,new[]{UV(504,605),UV(625,606),UV(614,352),UV(505,352)});
        Vector2[][] fixedUV={new[]{UV(55,568),UV(194,572),UV(230,339),UV(94,336)},new[]{UV(213,572),UV(341,576),UV(361,344),UV(239,340)}};
        for(int i=0;i<2;i++){
            float x=-2.64f+i*1.05f;
            Box(root,"Fixed left window frame",new Vector3(x,1.4f,.035f),new Vector3(1.055f,2.4f,.08f),metal);
            Face(root,"Fixed left energy artwork "+i,new Vector3(x,1.4f,-.01f),.97f,2.3f,fixedUV[i]);
            Box(root,"Fixed window white sill",new Vector3(x,.13f,0),new Vector3(1.055f,.26f,.12f),white);
        }
        Box(root,"Right fixed glazing frame",new Vector3(2.12f,1.4f,.035f),new Vector3(2.05f,2.4f,.08f),metal);
        Face(root,"Right glazing photograph",new Vector3(2.12f,1.4f,-.012f),1.96f,2.3f,new[]{UV(651,571),UV(927,580),UV(884,352),UV(648,349)});
        Box(root,"Right window white sill",new Vector3(2.12f,.13f,0),new Vector3(2.05f,.26f,.12f),white);
        Box(root,"Full laboratory sign backing",new Vector3(0,3.16f,-.055f),new Vector3(6.35f,1.02f,.12f),white);
        Face(root,"ITPLN Sembcorp laboratory sign",new Vector3(0,3.16f,-.12f),6.27f,.96f,new[]{UV(86,312),UV(897,334),UV(874,187),UV(112,166)});
        var all=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
        foreach(var t in all.Where(t=>t.name=="Low Building Window"&&t.position.z<9.5f||t.name=="Door aluminium rail"||t.name=="Door aluminium stile"||t.name=="Annex Window Mullion"&&t.position.z<9.5f||t.name=="Annex Horizontal Glazing Bar"&&t.position.z<9.5f))t.gameObject.SetActive(false);
        var before=GameObject.Find("East wall before doorway").transform;before.position=new Vector3(2.04f,2.52f,4.1f);before.localScale=new Vector3(.15f,4.14f,2.08f);
        var after=GameObject.Find("East wall after doorway").transform;after.position=new Vector3(2.04f,2.52f,14.1f);after.localScale=new Vector3(.15f,4.14f,13.68f);
        var header=GameObject.Find("Door header").transform;header.position=new Vector3(2.04f,3.85f,6.2f);
        for(int i=1;i<=3;i++){
            var step=GameObject.Find("Turquoise entrance step "+i).transform;step.position=new Vector3(3.52f,.075f*i,1.41f+.36f*i);step.localScale=new Vector3(2.35f,.15f*i,.36f);step.GetComponent<Renderer>().sharedMaterial=teal;
        }
        var mat=GameObject.Find("Entrance dark mat").transform;mat.position=new Vector3(2.85f,.49f,6.2f);
        // Extend the landing continuously from the stairhead to the doorway.
        Box(root,"Door threshold",new Vector3(0,.005f,0),new Vector3(2.06f,.02f,.3f),metal);
        EditorSceneManager.MarkSceneDirty(scene);AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene);
        return "Moved entrance forward, rebuilt full photo facade, aligned opening and stairs.";
    }
}
