using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;

public static class BuildSlidingEntrance
{
    private const string Folder = "Assets/PLTS/SlidingEntrance/";
    private static Material aluminium, dark, white;
    private static GameObject Box(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = position;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = material;
        return obj;
    }
    private static Material Material(string name, Color color)
    {
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.color = color;
        material.SetFloat("_Smoothness", .55f);
        AssetDatabase.CreateAsset(material, Folder + name + ".mat");
        return material;
    }
    private static Material Print(string name, string texture)
    {
        var material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Folder + texture));
        material.SetFloat("_Cull", 0);
        AssetDatabase.CreateAsset(material, Folder + name + ".mat");
        return material;
    }
    private static void Face(Transform parent, string name, float width, float height, Vector3 position, Material material, Vector2[] uv)
    {
        var mesh = new Mesh { name = name };
        mesh.vertices = new[] {new Vector3(-width/2,-height/2,0),new Vector3(width/2,-height/2,0),new Vector3(width/2,height/2,0),new Vector3(-width/2,height/2,0)};
        mesh.uv = uv;
        mesh.triangles = new[] {0,2,1,0,3,2};
        mesh.RecalculateNormals();
        AssetDatabase.CreateAsset(mesh, Folder + name + ".asset");
        var obj = new GameObject(name);
        obj.transform.SetParent(parent,false);
        obj.transform.localPosition = position;
        obj.AddComponent<MeshFilter>().sharedMesh = mesh;
        obj.AddComponent<MeshRenderer>().sharedMaterial = material;
    }
    private static Vector2 UV(float x, float y) => new Vector2(x/407f,1-y/713f);
    public static string Build()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.name != "ITPLNCampusPLTS" || EditorApplication.isPlaying) throw new Exception("Campus edit mode required");
        if (GameObject.Find("Laboratory sliding entrance")) throw new Exception("Entrance already exists");
        AssetDatabase.Refresh();
        var old = GameObject.Find("Open interior entrance leaf");
        if(old) old.SetActive(false);
        aluminium = Material("Brushed aluminium",new Color(.63f,.66f,.65f));
        aluminium.SetFloat("_Metallic",.7f);
        dark = Material("Dark tinted door",new Color(.025f,.04f,.038f));
        white = Material("Sign backing",new Color(.94f,.94f,.9f));
        var ink = Print("Reference renewable energy artwork", "EntranceReference.png");
        var logo = Print("ITPLN official logo", "ITPLNLogo.png");
        var root = new GameObject("Laboratory sliding entrance").transform;
        root.position = new Vector3(2.27f,.47f,7.72f);
        root.rotation = Quaternion.Euler(0,-90,0);
        foreach(float x in new[]{-1.08f,1.08f})
            Box(root,"Aluminium doorway jamb",new Vector3(x,1.3f,0),new Vector3(.065f,2.65f,.14f),aluminium);
        Box(root,"Upper sliding rail",new Vector3(0,2.66f,0),new Vector3(4.32f,.12f,.16f),aluminium);
        Box(root,"Floor guide rail",new Vector3(0,.012f,0),new Vector3(4.32f,.024f,.12f),aluminium);
        for(int i=0;i<2;i++)
        {
            float direction = i==0 ? -1f : 1f;
            var leaf = new GameObject(i==0?"Left sliding door":"Right sliding door").transform;
            leaf.SetParent(root,false);
            leaf.localPosition = new Vector3(direction*.525f,0,-.08f);
            Box(leaf,"Dark glazed leaf",new Vector3(0,1.3f,0),new Vector3(1.04f,2.58f,.045f),dark);
            foreach(float x in new[]{-.505f,.505f}) Box(leaf,"Vertical aluminium stile",new Vector3(x,1.3f,-.008f),new Vector3(.035f,2.6f,.07f),aluminium);
            foreach(float y in new[]{.035f,2.565f}) Box(leaf,"Aluminium leaf rail",new Vector3(0,y,-.008f),new Vector3(1.04f,.055f,.07f),aluminium);
            Vector2[] uv = {UV(27,460),UV(109,490),UV(105,183),UV(26,203)};
            Face(leaf,"Energy motif leaf "+i,.91f,2.42f,new Vector3(0,1.3f,-.025f),ink,uv);
            Face(leaf,"Interior energy motif leaf "+i,.91f,2.42f,new Vector3(0,1.3f,.025f),ink,uv);
            float handleX = -direction*.39f;
            foreach(float z in new[]{-.1f,.1f})
            {
                Box(leaf,"Door pull handle",new Vector3(handleX,1.23f,z),new Vector3(.025f,.37f,.032f),aluminium);
                foreach(float y in new[]{1.06f,1.4f}) Box(leaf,"Handle mounting",new Vector3(handleX,y,z/2),new Vector3(.035f,.035f,.1f),aluminium);
            }
            leaf.gameObject.AddComponent<XRSimpleInteractable>();
            leaf.gameObject.AddComponent<LaboratorySlidingDoor>().slideDistance=direction*1.06f;
        }
        Box(root,"Laboratory sign",new Vector3(0,3.02f,-.07f),new Vector3(4.3f,.55f,.08f),white);
        Face(root,"ITPLN logo",.86f,.415f,new Vector3(-1.62f,3.02f,-.115f),logo,new[]{new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),new Vector2(0,1)});
        var title = new GameObject("Laboratorium Renewable Energy and Hybrid System");
        title.transform.SetParent(root,false);
        title.transform.localPosition = new Vector3(.48f,3.02f,-.119f);
        var text = title.AddComponent<TextMeshPro>();
        text.font = TMP_Settings.defaultFontAsset;
        text.text = "Laboratorium\nRenewable Energy and Hybrid System";
        text.fontSize = 1.45f;
        text.enableAutoSizing = true;
        text.fontSizeMin = .8f;
        text.fontSizeMax = 1.45f;
        text.color = new Color(.045f,.26f,.32f);
        text.alignment = TextAlignmentOptions.Center;
        text.rectTransform.sizeDelta = new Vector2(3.15f,.46f);
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return "Created two independently sliding leaves, photo artwork and official ITPLN sign.";
    }
}
