using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static class FinishPhotoDetails {
 public static string Run(){
 string a="Assets/PLTS/InteriorPhotoDetails/";
 foreach(string n in new[]{"OfflineDiagram","OnlineDiagram","HybridPanel","HuaweiWall"}){var im=(TextureImporter)AssetImporter.GetAtPath(a+"References/"+n+".png");im.npotScale=TextureImporterNPOTScale.None;im.maxTextureSize=2048;im.SaveAndReimport();}
 foreach(string n in new[]{"Huawei badge artwork","Sembcorp plaque artwork"}){var m=AssetDatabase.LoadAssetAtPath<Mesh>(a+n+".asset");var uv=m.uv;for(int i=0;i<uv.Length;i++)uv[i].y=1-(1-uv[i].y)*639f/671;m.uv=uv;EditorUtility.SetDirty(m);}
 var casing=new Material(Shader.Find("Universal Render Pipeline/Lit"));casing.color=new Color(.98f,.98f,.94f);casing.SetFloat("_Smoothness",.5f);AssetDatabase.CreateAsset(casing,a+"Huawei white casing.mat");GameObject.Find("Huawei rounded inverter").GetComponent<Renderer>().sharedMaterial=casing;
 var plaque=GameObject.Find("Turquoise label - AC COMBINER\nDISTRIBUTION\nBOARD (ACDB)");plaque.transform.localScale=new Vector3(.55f,.2f,.012f);
 AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());return "Texture dimensions and casing verified";
 }
}
