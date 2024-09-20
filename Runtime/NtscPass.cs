using System.Collections;
using System.Collections.Generic;
using PixelDough.NtscVolume;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PixelDough.NtscVolume
{
    [System.Serializable]
    public class NtscPass : ScriptableRenderPass
    {
        // A variable to hold a reference to the corresponding volume component
        private NtscVolume m_VolumeComponent;

        // The postprocessing material
        private Material m_Material;

        // The ids of the shader variables
        static class ShaderIDs
        {
            internal static readonly int Input = Shader.PropertyToID("_MainTex");

            internal static readonly int HorizontalCarrierFrequency =
                Shader.PropertyToID("_NtscHorizontalCarrierFrequency");

            internal static readonly int KernelRadius = Shader.PropertyToID("_NtscKernelRadius");
            internal static readonly int KernelWidthRatio = Shader.PropertyToID("_NtscKernelWidthRatio");
            internal static readonly int Sharpness = Shader.PropertyToID("_NtscSharpness");
            internal static readonly int LinePhaseShift = Shader.PropertyToID("_NtscLinePhaseShift");
            internal static readonly int FlickerPercent = Shader.PropertyToID("_NtscFlickerPercent");
            internal static readonly int FlickerScaleX = Shader.PropertyToID("_NtscFlickerScaleX");
            internal static readonly int FlickerScaleY = Shader.PropertyToID("_NtscFlickerScaleY");
            internal static readonly int FlickerUseTimeScale = Shader.PropertyToID("_NtscFlickerUseTimeScale");
        }

        private RenderTargetIdentifier source;
        private RenderTargetIdentifier destinationA;
        private RenderTargetIdentifier destinationB;
        private RenderTargetIdentifier latestDest;

        readonly int temporaryRTIdA = Shader.PropertyToID("_TempRT");
        readonly int temporaryRTIdB = Shader.PropertyToID("_TempRTB");

        public NtscPass()
        {
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);

            if (m_Material == null)
            {
                Debug.Log("Creating NTSC Material");
                m_Material = CoreUtils.CreateEngineMaterial("PostProcessing/Ntsc");
            }
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {

        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // if (renderingData.cameraData.isSceneViewCamera)
            //     return;

            if (m_Material == null)
            {
                Debug.Log("Creating NTSC Material");
                m_Material = CoreUtils.CreateEngineMaterial("PostProcessing/Ntsc");
            }

            CommandBuffer cmd = CommandBufferPool.Get("NTSC Volume");
            cmd.Clear();

            #region From Setup

            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;

            var renderer = renderingData.cameraData.renderer;
            source = renderer.cameraColorTarget;

            cmd.GetTemporaryRT(temporaryRTIdA, descriptor, FilterMode.Bilinear);
            destinationA = new RenderTargetIdentifier(temporaryRTIdA);
            cmd.GetTemporaryRT(temporaryRTIdB, descriptor, FilterMode.Bilinear);
            destinationB = new RenderTargetIdentifier(temporaryRTIdB);

            #endregion


            var stack = VolumeManager.instance.stack;

            void BlitTo(Material mat, int pass = 0)
            {
                var first = latestDest;
                var last = first == destinationA ? destinationB : destinationA;
                Blit(cmd, first, last, mat, pass);

                latestDest = last;
            }

            latestDest = source;

            m_VolumeComponent = stack.GetComponent<NtscVolume>();
            if (m_VolumeComponent.IsActive())
            {
                // set material properties
                if (m_Material != null)
                {
                    m_Material.SetFloat(ShaderIDs.HorizontalCarrierFrequency,
                        m_VolumeComponent.horizontalCarrierFrequency.value);
                    m_Material.SetInteger(ShaderIDs.KernelRadius, m_VolumeComponent.kernelRadius.value);
                    m_Material.SetFloat(ShaderIDs.KernelWidthRatio, m_VolumeComponent.kernelWidthRatio.value);
                    m_Material.SetFloat(ShaderIDs.Sharpness, m_VolumeComponent.sharpness.value);
                    m_Material.SetFloat(ShaderIDs.LinePhaseShift, m_VolumeComponent.linePhaseShift.value);
                    m_Material.SetFloat(ShaderIDs.FlickerPercent, m_VolumeComponent.flickerPercent.value);
                    m_Material.SetFloat(ShaderIDs.FlickerScaleX, m_VolumeComponent.flickerScaleX.value);
                    m_Material.SetFloat(ShaderIDs.FlickerScaleY, m_VolumeComponent.flickerScaleY.value);
                    m_Material.SetInteger(ShaderIDs.FlickerUseTimeScale,
                        m_VolumeComponent.flickerUseTimeScale.value ? 1 : 0);
                }

                Vector2 size = new Vector2(512, 288);
                cmd.SetGlobalFloat("_NtscTimeUnscaled", Time.unscaledTime);
                cmd.SetGlobalVector("_ScreenSize",
                    new Vector4(Screen.width, Screen.height, 1.0f / (float) Screen.width,
                        1.0f / (float) Screen.height));
                cmd.SetGlobalVector("_ScreenSizeRasterizationRTScaled",
                    new Vector4(size.x, size.y, 1.0f / (float) size.x, 1.0f / (float) size.y));
                // set source texture
                cmd.SetGlobalTexture(ShaderIDs.Input, source);

                //cmd.Blit(source, destination, m_Material, 0);
                //CoreUtils.DrawFullScreen(cmd, m_Material, destination);

                BlitTo(m_Material);
            }

            Blit(cmd, latestDest, source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    //
    // // Define the renderer for the custom post processing effect
    // [CustomPostProcess("NTSC", CustomPostProcessInjectionPoint.AfterPostProcess)]
    // public class NtscRenderer : CustomPostProcessRenderer
    // {
    //     // A variable to hold a reference to the corresponding volume component
    //     private NtscVolume m_VolumeComponent;
    //
    //     // The postprocessing material
    //     private Material m_Material;
    //
    //     // The ids of the shader variables
    //     static class ShaderIDs {
    //         internal static readonly int Input = Shader.PropertyToID("_MainTex");
    //         internal static readonly int HorizontalCarrierFrequency =
    //             Shader.PropertyToID("_NtscHorizontalCarrierFrequency");
    //         internal static readonly int KernelRadius = Shader.PropertyToID("_NtscKernelRadius");
    //         internal static readonly int KernelWidthRatio = Shader.PropertyToID("_NtscKernelWidthRatio");
    //         internal static readonly int Sharpness = Shader.PropertyToID("_NtscSharpness");
    //         internal static readonly int LinePhaseShift = Shader.PropertyToID("_NtscLinePhaseShift");
    //         internal static readonly int FlickerPercent = Shader.PropertyToID("_NtscFlickerPercent");
    //         internal static readonly int FlickerScaleX = Shader.PropertyToID("_NtscFlickerScaleX");
    //         internal static readonly int FlickerScaleY = Shader.PropertyToID("_NtscFlickerScaleY");
    //         internal static readonly int FlickerUseTimeScale = Shader.PropertyToID("_NtscFlickerUseTimeScale");
    //     }
    //
    //     // By default, the effect is visible in the scene view, but we can change that here.
    //     public override bool visibleInSceneView => true;
    //
    //     /// Specifies the input needed by this custom post process. Default is Color only.
    //     public override ScriptableRenderPassInput input => ScriptableRenderPassInput.Color;
    //
    //     // Initialized is called only once before the first render call
    //     // so we use it to create our material
    //     public override void Initialize()
    //     {
    //         m_Material = CoreUtils.CreateEngineMaterial("PostProcessing/Ntsc");
    //     }
    //
    //     // Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
    //     public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
    //     {
    //         // Get the current volume stack
    //         var stack = VolumeManager.instance.stack;
    //         // Get the corresponding volume component
    //         m_VolumeComponent = stack.GetComponent<NtscVolume>();
    //
    //         return m_VolumeComponent.isEnabled.value;
    //     }
    //
    //     // The actual rendering execution is done here
    //     public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
    //     {
    //         // set material properties
    //         if(m_Material != null){
    //             m_Material.SetFloat(ShaderIDs.HorizontalCarrierFrequency, m_VolumeComponent.horizontalCarrierFrequency.value);
    //             m_Material.SetInteger(ShaderIDs.KernelRadius, m_VolumeComponent.kernelRadius.value);
    //             m_Material.SetFloat(ShaderIDs.KernelWidthRatio, m_VolumeComponent.kernelWidthRatio.value);
    //             m_Material.SetFloat(ShaderIDs.Sharpness, m_VolumeComponent.sharpness.value);
    //             m_Material.SetFloat(ShaderIDs.LinePhaseShift, m_VolumeComponent.linePhaseShift.value);
    //             m_Material.SetFloat(ShaderIDs.FlickerPercent, m_VolumeComponent.flickerPercent.value);
    //             m_Material.SetFloat(ShaderIDs.FlickerScaleX, m_VolumeComponent.flickerScaleX.value);
    //             m_Material.SetFloat(ShaderIDs.FlickerScaleY, m_VolumeComponent.flickerScaleY.value);
    //             m_Material.SetInteger(ShaderIDs.FlickerUseTimeScale, m_VolumeComponent.flickerUseTimeScale.value ? 1 : 0);
    //         }
    //
    //         Vector2 size = new Vector2(512, 288);
    //         cmd.SetGlobalVector("_ScreenSize", new Vector4(Screen.width, Screen.height, 1.0f / (float)Screen.width, 1.0f / (float)Screen.height));
    //         cmd.SetGlobalVector("_ScreenSizeRasterizationRTScaled", new Vector4(size.x, size.y, 1.0f / (float)size.x, 1.0f / (float)size.y));
    //         // set source texture
    //         cmd.SetGlobalTexture(ShaderIDs.Input, source);
    //
    //         cmd.Blit(source, destination, m_Material, 0);
    //         //CoreUtils.DrawFullScreen(cmd, m_Material, destination);
    //     }
    // }
}