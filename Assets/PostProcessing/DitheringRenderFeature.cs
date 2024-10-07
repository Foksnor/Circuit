using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DitheringRenderFeature : ScriptableRendererFeature
{
    class DitheringPass : ScriptableRenderPass
    {
        public Material ditheringMaterial;
        private RTHandle temporaryRenderTargetHandle;

        // Constructor
        public DitheringPass(Material material)
        {
            ditheringMaterial = material;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            // Retrieve the camera's color target descriptor
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;

            // Log the camera descriptor to see if we are getting valid dimensions
            Debug.Log($"Camera Descriptor: {descriptor.width}x{descriptor.height}");

            // Allocate a temporary render texture (RTHandle) using the correct depth and format
            temporaryRenderTargetHandle = RTHandles.Alloc(
                descriptor.width,
                descriptor.height,
                depthBufferBits: DepthBits.Depth24,  // Choose an appropriate depth format
                dimension: descriptor.dimension,
                colorFormat: descriptor.graphicsFormat,  // GraphicsFormat, not RenderTextureFormat
                useMipMap: false,  // Disable mipmaps unless needed
                autoGenerateMips: false  // Disable automatic mipmaps unless needed
            );

            // Ensure the allocation is successful
            if (temporaryRenderTargetHandle == null)
            {
                Debug.LogError("Failed to allocate temporary RTHandle.");
            }
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (ditheringMaterial == null)
            {
                Debug.LogError("Dithering material is null.");
                return;
            }

            if (temporaryRenderTargetHandle == null)
            {
                Debug.LogError("Temporary render target is null.");
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get("Dithering Effect");

            // Get the current camera color target
            var cameraColorTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;

            // Ensure the camera color target is valid
            if (cameraColorTargetHandle == null)
            {
                Debug.LogError("Camera color target handle is null.");
                CommandBufferPool.Release(cmd);
                return;
            }

            // Log the blitting process
            Debug.Log("Blitting from camera color target to temporary target");

            // Blit from the camera's color target to the temporary render target using the dithering material
            Blitter.BlitCameraTexture(cmd, cameraColorTargetHandle, temporaryRenderTargetHandle, ditheringMaterial, 0);

            // Log the blitting process
            Debug.Log("Blitting from temporary target back to camera color target");

            // Blit back from the temporary render target to the camera's color target
            Blitter.BlitCameraTexture(cmd, temporaryRenderTargetHandle, cameraColorTargetHandle);

            // Execute the command buffer
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            // Release the temporary render target
            if (temporaryRenderTargetHandle != null)
            {
                RTHandles.Release(temporaryRenderTargetHandle);
                temporaryRenderTargetHandle = null; // Clear reference to avoid issues
            }
        }
    }

    DitheringPass ditheringPass;
    public Material ditheringMaterial;

    public override void Create()
    {
        // Create a new instance of the pass
        ditheringPass = new DitheringPass(ditheringMaterial)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (ditheringMaterial != null)
        {
            renderer.EnqueuePass(ditheringPass);
        }
    }
}