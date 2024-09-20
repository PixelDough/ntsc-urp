using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PixelDough.NtscVolume
{
    [System.Serializable]
    public class NtscRenderer : ScriptableRendererFeature
    {
        private NtscPass pass;

        public override void Create()
        {
            pass = new NtscPass();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(pass);
        }
    }
}
