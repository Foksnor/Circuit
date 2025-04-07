using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

public class DitherEffectRenderFeature : ScriptableRendererFeature
{
    class DitherEffectPass : ScriptableRenderPass
    {
        const string m_PassName = "DitherEffectPass";
        public LayerMask affectedLayers = ~0;
        Material m_BlitMaterial;

        public void Setup(Material mat, LayerMask layerMask)
        {
            m_BlitMaterial = mat;
            affectedLayers = layerMask;
            requiresIntermediateTexture = true;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var cameraData = frameData.Get<UniversalCameraData>();
            var camera = cameraData.camera;

            // Skip if the camera's culling mask doesn't match affected layers
            if ((camera.cullingMask & affectedLayers.value) == 0)
                return;

            var stack = VolumeManager.instance.stack;
            var customEffect = stack.GetComponent<SphereVolumeComponent>();

            if (!customEffect.IsActive())
                return;

            var resourceData = frameData.Get<UniversalResourceData>();

            if (resourceData.isActiveTargetBackBuffer)
            {
                Debug.LogError($"Skipping render pass. DitherEffectRenderFeature requires an intermediate ColorTexture. We can't use the BackBuffer as a texture input.");
                return;
            }

            var source = resourceData.activeColorTexture;

            var destinationDesc = renderGraph.GetTextureDesc(source);
            destinationDesc.name = $"CameraColor-{m_PassName}";
            destinationDesc.clearBuffer = false;

            TextureHandle destination = renderGraph.CreateTexture(destinationDesc);

            RenderGraphUtils.BlitMaterialParameters para = new(source, destination, m_BlitMaterial, 0);
            renderGraph.AddBlitPass(para, passName: m_PassName);

            resourceData.cameraColor = destination;
        }
    }

    public RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingTransparents;
    public Material material;
    public LayerMask affectedLayers = ~0;

    DitherEffectPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new DitherEffectPass
        {
            renderPassEvent = injectionPoint
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (material == null)
        {
            Debug.LogWarning("DitherEffectRenderFeature material is null and will be skipped.");
            return;
        }

        m_ScriptablePass.Setup(material, affectedLayers);
        renderer.EnqueuePass(m_ScriptablePass);
    }
}
