using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using SolarRangers.Controllers;
using SolarRangers.Interop;
using SolarRangers.Managers;
using SolarRangers.Objects;
using System.Collections;
using UnityEngine;

namespace SolarRangers
{
    public class SolarRangers : ModBehaviour
    {
        public const bool MUSIC_ENABLED = true;
        public const bool DEBUG = true;

        public static SolarRangers Instance;

        public static INewHorizons NewHorizons;
        public static ICommonCameraUtility CommonCameraUtility;

        public static PlayerCombatantController PlayerCombatant;
        public static PlayerDestructibleController PlayerDestructible;
        public static ShipCombatantController ShipCombatant;
        public static TransientCombatant WorldCombatant;

        public static bool CombatModeActive = false;

        void Start()
        {
            Instance = this;

            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"{nameof(SolarRangers)} is loaded!", MessageType.Success);

            Harmony.CreateAndPatchAll(typeof(Patches));

            // Get the New Horizons API and load configs
            NewHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            NewHorizons.LoadConfigs(this);

            CommonCameraUtility = ModHelper.Interaction.TryGetModApi<ICommonCameraUtility>("xen.CommonCameraUtility");

            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (loadScene != OWScene.SolarSystem) return;

                CombatModeActive = false;

                ModHelper.Events.Unity.FireOnNextUpdate(() =>
                {
                    ReferenceFrameManager.Spawn();
                    CombatantManager.Spawn();
                    CombatMusicManager.Spawn();
                    ExplosionManager.Spawn();
                    LaserManager.Spawn();
                    MeteorManager.Spawn();
                    JamScenarioManager.Spawn();

                    WorldCombatant ??= new TransientCombatant("CombatantWorld");

                    PlayerCombatant = PlayerCombatantController.Merge(Locator.GetPlayerTransform());
                    PlayerDestructible = PlayerDestructibleController.Merge(Locator.GetPlayerTransform());
                    ShipCombatant = ShipCombatantController.Merge(Locator.GetShipTransform());

                    foreach (var hull in Locator.GetShipTransform().GetComponentsInChildren<ShipHull>())
                        ShipHullDestructibleController.Merge(hull);

                    foreach (var fragment in FindObjectsOfType<FragmentIntegrity>())
                        FragmentDestructibleController.Merge(fragment);

                    foreach (var angler in Resources.FindObjectsOfTypeAll<AnglerfishController>())
                        AnglerCombatantController.Merge(angler);
                });
            };
        }

        public static void Log(string msg, MessageType type = MessageType.Info)
        {
            Instance.ModHelper.Console.WriteLine(msg, type);
        }
    }

}
