using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolarRangers.Controllers
{
    public class ReactorCombatantController : AbstractCombatantController
    {
        bool coreExposed;
        bool coreClosing;
        bool isKillSequence;

        ReactorCoreDestructibleController reactorCore;
        OWAudioSource coreCasingAudio;
        TransformAnimator[] panelAnimators;
        Coroutine activeRoutine;

        public override string GetNameKey() => "CombatantReactor";
        public override bool CanTarget() => !reactorCore.IsDestroyed();
        public bool IsCoreExposed() => coreExposed;
        public bool IsCoreDestroyed() => isKillSequence;

        public static ReactorCombatantController Spawn()
        {
            var reactor = new GameObject("Reactor").AddComponent<ReactorCombatantController>();
            reactor.Init();
            return reactor;
        }

        public void Init()
        {

        }

        IEnumerator DoBossSequence()
        {
            while (!reactorCore.IsDestroyed())
            {
                yield return new WaitForSeconds(10f);
                yield return DoCloseCore();
                yield return new WaitForSeconds(20f);
                yield return DoOpenCore();
            }
        }

        IEnumerator DoKillSequence()
        {
            if (!coreExposed || coreClosing) yield return DoOpenCore();
        }

        IEnumerator DoOpenCore()
        {
            coreClosing = false;
            coreExposed = true;
            coreCasingAudio.PlayOneShot(AudioType.NomaiTimeLoopOpen);
            var duration = 3f;
            var openAngle = 90f;
            for (int i = 0; i < panelAnimators.Length; i++)
            {
                var sign = (i < 4) ? -1f : 1f;
                panelAnimators[i].RotateToLocalEulerAngles(new Vector3(panelAnimators[i].transform.localEulerAngles.x, panelAnimators[i].transform.localEulerAngles.y, openAngle * sign), duration);
            }
            yield return new WaitForSeconds(duration);
        }

        IEnumerator DoCloseCore()
        {
            coreClosing = true;
            coreCasingAudio.PlayOneShot(AudioType.NomaiTimeLoopClose);
            var duration = 3f;
            for (int i = 0; i < panelAnimators.Length; i++)
            {
                panelAnimators[i].RotateToOriginalLocalRotation(duration);
            }
            yield return new WaitForSeconds(duration);
            coreExposed = false;
            coreClosing = false;
        }

        void Awake()
        {
            reactorCore = new GameObject("Reactor Core").AddComponent<ReactorCoreDestructibleController>();
            reactorCore.transform.SetParent(transform, false);
            reactorCore.Init(this);

            /*
            var airlockPath = "OrbitalProbeCannon_Body/Sector_OrbitalProbeCannon/Interactables_OrbitalProbeCannon/Interactables_VisibleOrbitalProbeCannon/Prefab_NOM_Airlock (1)";
            var airlock = ObjectUtils.Spawn(transform, airlockPath);
            airlock.transform.localScale = Vector3.one * 80f;
            */

            var timeLoopDevicePath = "TowerTwin_Body/Sector_TowerTwin/Sector_TimeLoopInterior/Geometry_TimeLoopInterior/ControlledByProxy_TimeLoopInterior/Structure_NOM_TimeLoopDevice_Int";
            var timeLoopDevice = ObjectUtils.Spawn(transform, timeLoopDevicePath);
            timeLoopDevice.transform.localPosition = Vector3.zero;
            timeLoopDevice.transform.localEulerAngles = new Vector3(0f, 0f, 270f);
            timeLoopDevice.transform.localScale = Vector3.one * 20f;

            var coreCasingPath = "TowerTwin_Body/Sector_TowerTwin/Sector_TimeLoopInterior/Interactables_TimeLoopInterior/CoreCasingController";
            var coreCasing = ObjectUtils.Spawn(transform, coreCasingPath);
            coreCasing.transform.localPosition = Vector3.zero;
            coreCasing.transform.localEulerAngles = Vector3.zero;
            coreCasing.transform.localScale = Vector3.one * 20f;

            var timeLoopCoreController = coreCasing.GetComponent<TimeLoopCoreController>();
            coreCasingAudio = timeLoopCoreController._coreCasingAudio;
            panelAnimators = timeLoopCoreController._panelAnimators;
            Destroy(coreCasing.transform.Find("BlackHoleAttractVolume").gameObject);
            Destroy(coreCasing.transform.Find("BlackHoleVanishVolume").gameObject);
            Destroy(timeLoopCoreController);

            activeRoutine = StartCoroutine(DoBossSequence());
        }

        void Update()
        {
            if (reactorCore.IsDestroyed() && !isKillSequence)
            {
                StopCoroutine(activeRoutine);
                isKillSequence = true;
                activeRoutine = StartCoroutine(DoKillSequence());
            }
        }
    }
}
