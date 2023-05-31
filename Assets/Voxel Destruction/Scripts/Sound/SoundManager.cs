using UnityEngine;
using System;
using Random = UnityEngine.Random;

namespace VoxelDestruction
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager instance;

        public Sound[] sounds;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
        }

        public void Play (string name, string auth)
        {
            Sound s = Array.Find(sounds, sound => sound.name == name);

            if (s == null)
            {
                Debug.LogWarning("Sound whit name " + name + " not found!");
            }
            else
            {
                for (int i = s.source.Count - 1; i > -1; i--)
                {
                    if (s.source[i] == null)
                        s.source.RemoveAt(i);
                }

                GameObject g = new GameObject();
                g.transform.parent = transform;
                g.transform.localPosition = Vector3.zero;
                g.transform.localRotation = Quaternion.identity;
                g.transform.name = s.name + " source";

                AudioSource localSource = g.AddComponent<AudioSource>();
                s.source.Add(localSource);
                localSource.clip = s.clip;
                localSource.volume = s.volume;
                localSource.pitch = s.pitch;
                localSource.loop = s.loop;
                s.auth = auth;

                localSource.outputAudioMixerGroup = s.group;
                localSource.rolloffMode = AudioRolloffMode.Linear;
                localSource.maxDistance = s.maxDis;

                localSource.Play();

                if (!s.loop)
                    Destroy(g, s.clip.length + 0.1f);
            }
        }

        public void Play(string name, string auth, bool ignoreExisting)
        {
            Sound s = Array.Find(sounds, sound => sound.name == name);

            if (s == null)
            {
                Debug.LogWarning("Sound whit name " + name + " not found!");
            }
            else
            {
                for (int i = s.source.Count - 1; i > -1; i--)
                {
                    if (s.source[i] == null)
                        s.source.RemoveAt(i);
                }

                if (!ignoreExisting && s.source.Count > 0)
                    return;

                GameObject g = new GameObject();
                g.transform.parent = transform;
                g.transform.localPosition = Vector3.zero;
                g.transform.localRotation = Quaternion.identity;
                g.transform.name = s.name + " source";

                AudioSource localSource = g.AddComponent<AudioSource>();
                s.source.Add(localSource);
                localSource.clip = s.clip;
                localSource.volume = s.volume;
                localSource.pitch = s.pitch;
                localSource.loop = s.loop;
                s.auth = auth;

                localSource.outputAudioMixerGroup = s.group;
                localSource.rolloffMode = AudioRolloffMode.Linear;
                localSource.maxDistance = s.maxDis;

                localSource.Play();

                if (!s.loop)
                    Destroy(g, s.clip.length + 0.1f);
            }
        }

        public void Play(string name, string auth, bool ignoreExisting, Vector3 pos, bool pitchRandomize = false, float volumescale = 1f)
        {
            Sound s = Array.Find(sounds, sound => sound.name == name);

            if (s == null)
            {
                Debug.LogWarning("Sound whit name " + name + " not found!");
            }
            else
            {
                for (int i = s.source.Count - 1; i > -1; i--)
                {
                    if (s.source[i] == null)
                        s.source.RemoveAt(i);
                }

                if (!ignoreExisting && s.source.Count > 0)
                    return;

                GameObject g = new GameObject();
                g.transform.parent = transform;
                g.transform.position = pos;
                g.transform.localRotation = Quaternion.identity;
                g.transform.name = s.name + " source";

                AudioSource localSource = g.AddComponent<AudioSource>();
                s.source.Add(localSource);
                localSource.clip = s.clip;
                localSource.volume = s.volume * volumescale;
                localSource.pitch = pitchRandomize ? Random.Range(s.pitch - s.pitchRandomize, s.pitch + s.pitchRandomize) : s.pitch;
                localSource.loop = s.loop;
                s.auth = auth;

                localSource.outputAudioMixerGroup = s.group;
                localSource.rolloffMode = AudioRolloffMode.Linear;
                localSource.maxDistance = s.maxDis;
                if (s.use3Daudio)
                    localSource.spatialBlend = 1;

                localSource.Play();

                if (!s.loop)
                    Destroy(g, s.clip.length + 0.1f);
            }
        }

        public void Stop (string name)
        {
            Sound s = Array.Find(sounds, sound => sound.name == name);

            if (s == null)
            {
                Debug.LogWarning("Sound whit name " + name + " not found!");
            }
            else
            {
                if (s.source.Count < 1)
                    return;

                foreach (AudioSource sources in s.source)
                {
                    if (sources == null)
                        continue;

                    sources.Stop();
                    Destroy(sources.gameObject);
                }

                s.source.Clear();
            }
        }

        public void Stop(string name, string auth)
        {
            Sound s = Array.Find(sounds, sound => sound.name == name);

            if (s == null)
            {
                Debug.LogWarning("Sound whit name " + name + " not found!");
            }
            else
            {
                if (s.auth != auth)
                    return;

                if (s.source.Count < 1)
                    return;

                foreach (AudioSource sources in s.source)
                {
                    if (sources == null)
                        continue;

                    sources.Stop();
                    Destroy(sources.gameObject);
                }

                s.source.Clear();
            }
        }

        public bool IsPlaying (string name)
        {
            Sound s = Array.Find(sounds, sound => sound.name == name);

            if (s == null)
            {
                Debug.LogWarning("Sound whit name " + name + " not found!");
                return false;
            }
            else
            {
                for (int i = s.source.Count - 1; i > -1; i--)
                {
                    if (s.source[i] == null)
                        s.source.RemoveAt(i);
                }

                if (s.source.Count > 0)
                    return true;
                else
                    return false;
            }
        }

        public bool IsPlayingArray(string[] names)
        {
            for (int f = 0; f < names.Length; f++)
            {
                Sound s = Array.Find(sounds, sound => sound.name == names[f]);

                if (s == null)
                {
                    Debug.LogWarning("Sound whit name " + names[f] + " not found!");
                    return false;
                }
                else
                {
                    for (int i = s.source.Count - 1; i > -1; i--)
                    {
                        if (s.source[i] == null)
                            s.source.RemoveAt(i);
                    }

                    if (s.source.Count > 0)
                        return true;
                }
            }

            return false;
        }
    }   
}
