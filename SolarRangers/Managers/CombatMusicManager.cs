using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolarRangers.Managers
{
    public class CombatMusicManager : AbstractManager<CombatMusicManager>
    {
        static readonly List<AudioClip> combatMusicClips = [];

        OWAudioSource audioSource;
        VillageMusicVolume villageMusic;
        int combatMusicClipIndex;

        void Awake()
        {
            villageMusic = FindObjectOfType<VillageMusicVolume>();
            if (!combatMusicClips.Any())
            {
                combatMusicClips.Add(SolarRangers.Instance.ModHelper.Assets.GetAudio("assets/music/Steven McDonald - To the Death.mp3"));
                combatMusicClips.Add(SolarRangers.Instance.ModHelper.Assets.GetAudio("assets/music/Steven McDonald - Tempest.mp3"));
                combatMusicClips.Add(SolarRangers.Instance.ModHelper.Assets.GetAudio("assets/music/Steven McDonald - Spearhead.mp3"));
            }

            var audioObj = new GameObject("AudioSource");
            audioObj.transform.SetParent(transform, false);
            audioObj.SetActive(false);
            audioObj.AddComponent<AudioSource>();
            audioSource = audioObj.AddComponent<OWAudioSource>();
            audioSource.SetTrack(OWAudioMixer.TrackName.Music);
            audioObj.SetActive(true);
        }

        void Update()
        {
            var controlsMusic = SolarRangers.CombatModeActive && SolarRangers.MUSIC_ENABLED;

            if (villageMusic)
            {
                villageMusic.SetVolumeActivation(!controlsMusic);
                if (controlsMusic) villageMusic._owAudioSrc.Stop();
            }

            if (controlsMusic && Locator.GetGlobalMusicController().enabled)
            {
                Locator.GetGlobalMusicController().OnTriggerSupernova();
            }
            else if (!controlsMusic && !Locator.GetGlobalMusicController().enabled)
            {
                Locator.GetGlobalMusicController().enabled = true;
            }

            if (controlsMusic)
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.clip = combatMusicClips[combatMusicClipIndex];
                    combatMusicClipIndex = (combatMusicClipIndex + 1) % combatMusicClips.Count;
                    audioSource.SetMaxVolume(0.75f);
                    audioSource.Play();
                }
            }
        }
    }
}
