using System;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;
public static class ReviewExterior {
 public static string Capture(){
  Directory.CreateDirectory("Screenshots/ExteriorReview");
  var go=new GameObject("Temporary exterior review camera");var c=go.AddComponent<Camera>();c.fieldOfView=63;c.nearClipPlane=.05f;c.farClipPlane=220;c.allowHDR=true; c.clearFlags=CameraClearFlags.Skybox;
  var extra=go.AddComponent<UniversalAdditionalCameraData>();extra.renderPostProcessing=true;extra.antialiasing=AntialiasingMode.FastApproximateAntialiasing;
  var rt=new RenderTexture(1600,1000,24);c.targetTexture=rt;
  var positions=new[]{new Vector3(16,5,-7),new Vector3(5.6f,1.7f,19.5f),new Vector3(8.2f,2.2f,6.2f)};
  var targets=new[]{new Vector3(1.4f,3,9),new Vector3(5.6f,1.65f,5),new Vector3(2.2f,2.0f,6.2f)};
  for(int i=0;i<3;i++){go.transform.position=positions[i];go.transform.LookAt(targets[i]);c.Render();c.Render();var prev=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(1600,1000,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,1000),0,0);tex.Apply();File.WriteAllBytes("Screenshots/ExteriorReview/View"+i+".png",tex.EncodeToPNG());RenderTexture.active=prev;UnityEngine.Object.DestroyImmediate(tex);}
  c.targetTexture=null;UnityEngine.Object.DestroyImmediate(go);UnityEngine.Object.DestroyImmediate(rt);return "Rendered approach, corridor and entrance";
 }
 public static string Inspect(){
 var list=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(t=>t.gameObject.scene.IsValid() && (t.name.Contains("Window")||t.name.Contains("window")||t.name.Contains("Roof")||t.name.Contains("roof")||t.name.Contains("Curb")||t.name.Contains("Road")||t.name.Contains("Fence")||t.name.Contains("Parapet")||t.name.Contains("Downpipe"))).Select(t=>t.name+" "+t.position+" size"+t.lossyScale+" active"+t.gameObject.activeInHierarchy).ToArray(); File.WriteAllLines("Temp/ExteriorObjects.txt",list);return string.Join("\n",list.Take(18));
 }
}
