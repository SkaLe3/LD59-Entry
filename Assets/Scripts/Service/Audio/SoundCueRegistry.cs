using System.Collections.Generic;
using UnityEngine;

namespace Service.Audio
{
    [CreateAssetMenu(menuName = "Scriptable/Audio/Sound Cue Registry")]
    public class SoundCueRegistry : ScriptableObject
    {
        public List<SoundCueArrayEntry> Entries = new List<SoundCueArrayEntry>();

        private Dictionary<string, SoundCue> _lookup;

        private void OnEnable()
        {
            BuildLookup();
        }

        public SoundCue GetSoundCue(string key)
        {
            if (_lookup == null)
                BuildLookup();

            return _lookup[key];
        }

        private void BuildLookup()
        {
            _lookup = new Dictionary<string, SoundCue>();

            foreach (SoundCueArrayEntry entry in Entries)
            {
                if (!_lookup.ContainsKey(entry.Key))
                    _lookup.Add(entry.Key, entry.so);
            }
        }
    }
}