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
        OWAudioSource fanfareAudioSource;
        VillageMusicVolume villageMusic;
        int combatMusicClipIndex;
        bool fanfarePlaying = false;

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
                victoryMusicClip = SolarRangers.Instance.ModHelper.Assets.GetAudio("assets/music/Steven McDonald - Ascend.mp3");

            audioSource = ObjectUtils.Create2DAudioSource(OWAudioMixer.TrackName.Music, combatMusicClips[0]);
            fanfareAudioSource = ObjectUtils.Create2DAudioSource(OWAudioMixer.TrackName.Music, AudioType.EYE_EndOfGame);
        }

        // Music:
        // Steven McDonald: Awakening (edit for escape music?)
        // Steven McDonald: Tempest (intense combat music)
        // Steven McDonald: To the Death (moderate combat music)
        // Steven McDonald: Spearhead (moderate combat music)
        // Steven McDonald: Ascend (victory music)
        // Steven McDonald: Legend (victory music)

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

            if (controlsMusic && Locator.GetGlobalMusicController()._travelSource.isPlaying)
            {
                Locator.GetGlobalMusicController()._travelSource.Stop();
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
                    case JamScenarioManager.State.Epilogue:
                        if (SolarRangers.PersistentAudioSource != audioSource && Locator.GetDeathManager()._isDead)
                        {
                            audioSource.SetTrack(OWAudioMixer.TrackName.Death);
                            audioSource._audioSource.outputAudioMixerGroup = Locator.GetAudioMixer().GetAudioMixerGroup(OWAudioMixer.TrackName.Death);
                            DontDestroyOnLoad(audioSource);
                            SolarRangers.PersistentAudioSource = audioSource;
                        }
                        TransitionTo(victoryMusicClip);
                        break;
                }
                if (JamScenarioManager.GetState() == JamScenarioManager.State.Ending && !fanfarePlaying)
                {
                    fanfarePlaying = true;
                    fanfareAudioSource.PlayDelayed(2f);
                }
            }
        }

        void TransitionTo(AudioClip clip)
        {
            audioSource._audioLibraryClip = AudioType.None;
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
