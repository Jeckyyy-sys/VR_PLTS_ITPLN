using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PLTS
{
    // Keep the campus shadow range local to this scene; the shared VR quality asset stays unchanged.
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class CampusExteriorLighting : MonoBehaviour
    {
        [Range(10f, 60f)] public float shadowDistance = 32f;
        private RenderPipelineAsset previousQualityPipeline;
        private UniversalRenderPipelineAsset scenePipeline;

        private void OnEnable()
        {
            if (scenePipeline != null || !gameObject.scene.IsValid()) return;
            var source = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (source == null) return;
            previousQualityPipeline = QualitySettings.renderPipeline;
            scenePipeline = Instantiate(source);
            scenePipeline.name = source.name + " (campus scene)";
            scenePipeline.hideFlags = HideFlags.HideAndDontSave;
            scenePipeline.shadowDistance = shadowDistance;
            scenePipeline.shadowCascadeCount = 2;
            QualitySettings.renderPipeline = scenePipeline;
        }

        private void OnValidate()
        {
            if (scenePipeline != null) scenePipeline.shadowDistance = shadowDistance;
        }

        private void OnDisable()
        {
            if (scenePipeline == null) return;
            if (QualitySettings.renderPipeline == scenePipeline)
                QualitySettings.renderPipeline = previousQualityPipeline;
            if (Application.isPlaying) Destroy(scenePipeline);
            else DestroyImmediate(scenePipeline);
            scenePipeline = null;
        }
    }
}
