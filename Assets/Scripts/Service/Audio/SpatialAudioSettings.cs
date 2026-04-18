using UnityEngine;

namespace Service.Audio
{
    [CreateAssetMenu(fileName = "NewSpatialAudioSettings", menuName = "Scriptable/Audio/Spatial Audio Settings")]
    public class SpatialAudioSettings : ScriptableObject
    {
        [Header("3D Sound Settings")]
        [Tooltip("0 = 2D sound, 1 = 3D sound")]
        [Range(0f, 1f)]
        public float spatialBlend = 1f;

        [Tooltip("Doppler effect intensity")]
        [Range(0f, 5f)]
        public float dopplerLevel = 1f;

        [Tooltip("3D stereo spread angle")]
        [Range(0f, 360f)]
        public float spread = 0f;

        [Header("Distance Attenuation")]
        [Tooltip("Distance where sound starts to attenuate")]
        public float minDistance = 1f;

        [Tooltip("Distance where sound is completely inaudible")]
        public float maxDistance = 500f;

        [Tooltip("How sound attenuates over distance")]
        public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

        [Header("Custom Rolloff (if using Custom mode)")]
        [Tooltip("Custom volume rolloff curve")]
        public AnimationCurve customRolloffCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    }
}