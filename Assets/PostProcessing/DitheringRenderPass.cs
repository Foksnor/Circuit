using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DitheringRenderPass : ScriptableRenderPass
{
    public Material ditheringMaterial;
    private RTHandle cameraColorTargetHandle;

    public DitheringRenderPass(Material material)
    {
        this.ditheringMaterial = material;
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        // Configure the render pass (optional, depends on the effect)
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (ditheringMaterial == null)
        {
            return;
        }

        CommandBuffer cmd = CommandBufferPool.Get("Dithering Pass");

        // Apply the dithering shader to the camera's color texture using RTHandles
        Blit(cmd, cameraColorTargetHandle, cameraColorTargetHandle, ditheringMaterial);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public void SetTarget(RTHandle cameraColorTargetHandle)
    {
        this.cameraColorTargetHandle = cameraColorTargetHandle;
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        // Cleanup after the pass (optional)
    }
}