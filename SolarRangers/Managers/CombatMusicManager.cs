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
        AudioClip bossFightMusicClip;
        AudioClip escapeMusicClip;
        AudioClip victoryMusicClip;

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
            if (!bossFightMusicClip)
                bossFightMusicClip = SolarRangers.Instance.ModHelper.Assets.GetAudio("assets/music/Steven McDonald - Titan.mp3");
            if (!escapeMusicClip)
                escapeMusicClip = SolarRangers.Instance.ModHelper.Assets.GetAudio("assets/music/Steven McDonald - Awakening.mp3");
            if (!victoryMusicClip)
                victoryMusicClip = SolarRangers.Instance.ModHelper.Assets.GetAudio("assets/music/Steven McDonald - Legend.mp3");

            var audioObj = new GameObject("AudioSource");
            audioObj.transform.SetParent(transform, false);
            audioObj.SetActive(false);
            audioObj.AddComponent<AudioSource>();
            audioSource = audioObj.AddComponent<OWAudioSource>();
            audioSource.SetTrack(OWAudioMixer.TrackName.Music);
            audioObj.SetActive(true);
        }

        // Music:
        // Steven McDonald: Awakening (edit for escape music?)
        // Steven McDonald: Tempest (intense combat music)
        // Steven McDonald: To the Death (moderate combat music)
        // Steven McDonald: Spearhead (moderate combat music)
        // Steven McDonald: Ascend (victory music)
        // Steven McDonald: Legend (victory music)

        // Matthew Pablo: Tactical Pursuit (moderate combat music)

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
                switch (JamScenarioManager.GetState())
                {
                    case JamScenarioManager.State.Initial:
                    case JamScenarioManager.State.OuterDefenses:
                    case JamScenarioManager.State.InnerDefenses:
                        if (!audioSource.isPlaying)
                        {
                            audioSource.clip = combatMusicClips[combatMusicClipIndex];
                            combatMusicClipIndex = (combatMusicClipIndex + 1) % combatMusicClips.Count;
                            audioSource.SetMaxVolume(0.75f);
                            audioSource.Play();
                        }
                        break;
                    case JamScenarioManager.State.BossFight:
                        TransitionTo(bossFightMusicClip);
                        break;
                    case JamScenarioManager.State.Escape:
                        TransitionTo(escapeMusicClip);
                        break;
                    case JamScenarioManager.State.Ending:
                        TransitionTo(victoryMusicClip);
                        break;
                }

            }
        }

        void TransitionTo(AudioClip clip)
        {
            if (audioSource.clip != clip && !audioSource.IsFadingOut())
            {
                audioSource.FadeOut(0.5f);
            }
            if (!audioSource.isPlaying)
            {
                audioSource.clip = clip;
                audioSource.SetMaxVolume(1f);
                audioSource.FadeIn(0f);
                audioSource.Play();
            }
        }
    }
}
