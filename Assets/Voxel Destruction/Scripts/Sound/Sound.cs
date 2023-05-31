using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace VoxelDestruction
{
    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [HideInInspector]
        public List<AudioSource> source = new List<AudioSource>();
        [HideInInspector]
        public string auth;

        [Range(0f, 1f)]
        public float volume;
        [Range(-3f, 3f)]
        public float pitch;

        public bool loop;
        public bool use3Daudio;
        public float maxDis;

        [Space] 
    
        public float pitchRandomize;
    
        public AudioMixerGroup group;
    }
}