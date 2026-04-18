using UnityEngine;

namespace Service.Audio
{
    [CreateAssetMenu(menuName = "Scriptable/Audio/Sound Cue")]
    public class SoundCue : ScriptableObject
    {
        [Tooltip("Array of sound clips, one of which will be chosen randomly.")]
        public AudioClip[] clips;
    }
}