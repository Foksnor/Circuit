using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelDitherPostProcessing : ScriptableRendererFeature
{
    class PixelDitherPass : ScriptableRenderPass
    {
        private readonly Material material;
        private RTHandle _cameraColorHandle;

        public PixelDitherPass(Material mat)
        {
            material = mat;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;

            RenderingUtils.ReAllocateIfNeeded(
                ref _cameraColorHandle,
                descriptor,
                FilterMode.Point,
                TextureWrapMode.Clamp,
                name: "_CameraColorTexture"
            );

            ConfigureTarget(renderingData.cameraData.renderer.cameraColorTargetHandle);
        }


        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!Application.isPlaying || material == null)
                return;

            var cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;

            CommandBuffer cmd = CommandBufferPool.Get("Pixel Dither Effect");

            // 1. Copy camera color to a temporary handle
            Blitter.BlitCameraTexture(cmd, cameraColorTarget, _cameraColorHandle);

            // 2. Set the texture for Shader Graph
            material.SetTexture("_CameraColorTexture", _cameraColorHandle);

            // 3. Final pass with material
            Blitter.BlitCameraTexture(cmd, _cameraColorHandle, cameraColorTarget, material, 0);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    [System.Serializable]
    public class PixelDitherSettings
    {
        public Material pixelDitherMaterial;
    }

    public PixelDitherSettings settings = new PixelDitherSettings();
    private PixelDitherPass pixelDitherPass;

    public override void Create()
    {
        pixelDitherPass = new PixelDitherPass(settings.pixelDitherMaterial)
        {
            renderPassEvent = RenderPassEvent.AfterRendering
        };
        pixelDitherPass.ConfigureInput(ScriptableRenderPassInput.Color);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.pixelDitherMaterial != null)
        {
            renderer.EnqueuePass(pixelDitherPass);
        }
    }
}
