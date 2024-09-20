using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PixelDough.NtscVolume
{
// Define the Volume Component for the custom post processing effect 
    [Serializable, VolumeComponentMenu("NTSC/NTSC Volume")]
    public class NtscVolume : VolumeComponent, IPostProcessComponent
    {

        [Tooltip("Controls whether the NTSC effect is active, which creates color bleeding and a natural blurriness." +
                 "\n\nIMPORTANT: This effect is not compatible with the Cathode Ray Tube Volume, and using them together can yield strange results.")]
        public BoolParameter isEnabled = new BoolParameter(false);

        [Tooltip(
            "The carrier wave is driven by a very fast oscillator at a fixed frequency. Since the beam is travelling, " +
            "the phase of the carrier is linear both in time but also in horizontal distance over a scanline. This value " +
            "determines the frequency of the wave of the horizontal carrier. " +
            "\n\nIdeally, this should be set to a value which " +
            "makes the scanlines as hidden as possible. Doing it this way will create a \"rainbowing\" effect along edges, " +
            "directly related to the scanline frequency produced by this value.")]
        public ClampedFloatParameter horizontalCarrierFrequency = new ClampedFloatParameter(0.44f, 0.1f, 3f);

        [Tooltip("Controls how many steps the Gaussian blur should take (default 3).")]
        public ClampedIntParameter kernelRadius = new ClampedIntParameter(2, 1, 5);

        [Tooltip("Controls the scale of the horizontal blur. " +
                 "\n\nTo achieve the intended effect, this should be used to blur out the vertical lines produced by the Horizontal Carrier Frequency parameter.")]
        public ClampedFloatParameter kernelWidthRatio = new ClampedFloatParameter(0.203f, 0.1f, 2f);

        [Tooltip("How much to apply sharpening after blurring.")]
        public ClampedFloatParameter sharpness = new ClampedFloatParameter(0.25f, 0, 1);

        [Tooltip(
            "Offsets the wave produced by the Horizontal Carrier Frequency. In most cases this value is unnoticable, and is best left at the default of 3.14.")]
        public ClampedFloatParameter linePhaseShift = new ClampedFloatParameter(3.14f, 0, 6.28f);

        [Tooltip("Represents how fast the flicker effect animates relative to the current FPS.")]
        public ClampedFloatParameter flickerPercent = new ClampedFloatParameter(1f, 0f, 1f);

        [Tooltip("How much to scale the flicker effect horizontally (default 0.1).")]
        public ClampedFloatParameter flickerScaleX = new ClampedFloatParameter(0.36f, 0f, 5f);

        [Tooltip("How much to scale the flicker effect vertically (default 4).")]
        public ClampedFloatParameter flickerScaleY = new ClampedFloatParameter(1.62f, 1f, 5f);

        [Tooltip("Controls whether the flicker uses scaled time.")]
        public BoolParameter flickerUseTimeScale = new BoolParameter(false);

        public bool IsActive() => isEnabled.value;

        public bool IsTileCompatible() => true;
    }
}
