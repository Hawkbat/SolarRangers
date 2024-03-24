using HarmonyLib;
using SolarRangers.Controllers;
using SolarRangers.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolarRangers
{
    public static class Patches
    {
        [HarmonyPostfix, HarmonyPatch(typeof(ShipCockpitController), nameof(ShipCockpitController.IsMatchVelocityAvailable))]
        public static void ShipCockpitController_IsMatchVelocityAvailable(ref bool __result)
        {
            if (!SolarRangers.CombatModeActive) return;
            if (SolarRangers.ShipCombatant && SolarRangers.ShipCombatant.IsInAttackMode)
            {
                __result = false;
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ShipCockpitUI), nameof(ShipCockpitUI.OnShipComponentDamaged))]
        public static bool ShipCockpitUI_OnShipComponentDamaged(ShipComponent shipComponent)
        {
            if (!SolarRangers.CombatModeActive) return true;
            if (shipComponent.repairFraction > 0.75f) return false;
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ShipCockpitUI), nameof(ShipCockpitUI.OnShipHullDamaged))]
        public static bool ShipCockpitUI_OnShipHullDamaged(ShipHull shipHull)
        {
            if (!SolarRangers.CombatModeActive) return true;
            if (shipHull.integrity > 0.75f) return false;
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ProbeLauncher), nameof(ProbeLauncher.TakeSnapshotWithCamera))]
        public static bool ProbeLauncher_TakeSnapshotWithCamera(ProbeCamera camera)
        {
            if (!SolarRangers.CombatModeActive) return true;
            var probe = camera.GetComponentInParent<SurveyorProbe>();
            var sector = probe._sectorDetector ? probe._sectorDetector.GetLastEnteredSector() : null;

            if (PlayerState.AtFlightConsole())
            {
                ExplosionManager.MediumExplosion(SolarRangers.ShipCombatant, 200f, sector ? sector.transform : probe.transform.parent, probe.transform.position);
            }
            else
            {
                ExplosionManager.SmallExplosion(SolarRangers.PlayerCombatant, 50f, sector ? sector.transform : probe.transform.parent, probe.transform.position);
            }

            probe.Retrieve(0f);

            SolarRangers.PlayerCombatant.StartProbeReload(5f);

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ProbeLauncher), nameof(ProbeLauncher.LaunchProbe))]
        public static bool ProbeLauncher_LaunchProbe()
        {
            if (!SolarRangers.CombatModeActive) return true;
            if (SolarRangers.PlayerCombatant && SolarRangers.PlayerCombatant.IsReloadingProbe())
                return false;
            return true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(MeteorController), nameof(MeteorController.Initialize))]
        public static void MeteorController_Initialize(MeteorController __instance)
        {
            var projectile = __instance.GetComponent<MeteorProjectileController>();
            if (!projectile)
            {
                projectile = __instance.gameObject.AddComponent<MeteorProjectileController>();
                projectile.Init(SolarRangers.WorldCombatant, UnityEngine.Random.Range(__instance._minDamage, __instance._maxDamage));
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(MeteorController), nameof(MeteorController.Impact))]
        public static void MeteorController_Impact(MeteorController __instance)
        {
            var projectile = __instance.GetComponent<MeteorProjectileController>();
            if (projectile) projectile.OnImpact();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(MeteorController), nameof(MeteorController.Suspend), [])]
        public static void MeteorController_Suspend(MeteorController __instance)
        {
            var projectile = __instance.GetComponent<MeteorProjectileController>();
            if (projectile) projectile.OnSuspend();
        }


        [HarmonyPrefix, HarmonyPatch(typeof(AnglerfishController), nameof(AnglerfishController.OnCaughtObject))]
        public static bool AnglerfishController_OnCaughtObject(AnglerfishController __instance)
        {
            if (!SolarRangers.CombatModeActive) return true;
            var combatant = __instance.gameObject.GetComponent<AnglerCombatantController>();
            if (!combatant) return true;
            if (combatant.CanEatPlayer()) return true;
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ExplosionController), nameof(ExplosionController.Update))]
        public static bool ExplosionController_Update(ExplosionController __instance)
        {
            if (!SolarRangers.CombatModeActive) return true;
            var newTimerValue = Mathf.Clamp01((__instance._timer + Time.deltaTime) / __instance._length);
            if (newTimerValue == 1f)
            {
                __instance._playing = false;
                ExplosionManager.Recycle(__instance);
                return false;
            }
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(FogLight), nameof(FogLight.UpdateFogLight))]
        public static void FogLight_UpdateFogLight(FogLight __instance)
        {
            if (!SolarRangers.CombatModeActive) return;
            while (__instance._linkedLightData.Count < __instance._linkedFogLights.Count)
            {
                var nextFogLight = __instance._linkedFogLights[__instance._linkedLightData.Count - 1];
                var lightData = new FogLight.LightData()
                {
                    color = nextFogLight.GetTint(),
                    maxAlpha = __instance._maxAlpha,
                };
                __instance._linkedLightData.Add(lightData);
                Locator.GetFogLightManager().RegisterLightData(lightData);
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ReferenceFrame), nameof(ReferenceFrame.GetHUDDisplayName))]
        public static void ReferenceFrame_GetHUDDisplayName(ReferenceFrame __instance, ref string __result)
        {
            var customName = ReferenceFrameManager.GetCustomName(__instance);
            if (customName != null) __result = customName;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ConstantFluidDetector), nameof(ConstantFluidDetector.AccumulateFluidAcceleration))]
        public static bool ConstantFluidDetector_AccumulateFluidAcceleration(ConstantFluidDetector __instance)
        {
            return !!__instance._onlyDetectableFluid;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(NoiseMaker), nameof(NoiseMaker.GetNoiseOrigin))]
        public static bool NoiseMaker_GetNoiseOrigin(NoiseMaker __instance, ref Vector3 __result)
        {
            if (!__instance._attachedBody)
            {
                __result = __instance.transform.position;
                return false;
            }
            return true;
        }
    }
}
