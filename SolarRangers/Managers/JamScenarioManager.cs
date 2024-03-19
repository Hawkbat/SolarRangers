using SolarRangers.Controllers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SolarRangers.Managers
{
    public class JamScenarioManager : AbstractManager<JamScenarioManager>
    {
        GameObject eggStar;
        NomaiAirlock eggStarAirlock;
        GameObject rangerShip;
        List<BeaconDestructibleController> beacons;
        ReactorCombatantController reactor;
        OWCamera cutsceneCam;
        State state;

        void Awake()
        {
            eggStar = SolarRangers.NewHorizons.GetPlanet("Egg Star");
            eggStarAirlock = eggStar.transform.Find("Sector/Prefab_NOM_Airlock (1)").GetComponentInChildren<NomaiAirlock>();
            eggStarAirlock._stopRotationQuats = eggStarAirlock._rotationObjectList.Select(r => Quaternion.Euler(r.closedRotation)).ToArray();
            eggStarAirlock._cycleTimer = eggStarAirlock._totalCycleTime / 2f;
            eggStarAirlock.FixedUpdate();
            eggStarAirlock._currentRotationState = NomaiMultiPartDoor.RotationState.CLOSED;

            eggStar.transform.Find("Sector/EggStarInner/Terrain_DB_BrambleSphere_Inner_v2/BrambleSphere 1").gameObject.SetActive(false);
            eggStar.transform.Find("Sector/EggStarInner/Terrain_DB_BrambleSphere_Inner_v2/bramblesphere_gateways").gameObject.SetActive(false);
            eggStar.transform.Find("Sector/EggStarInner/Terrain_DB_BrambleSphere_Inner_v2/COLLIDER_BrambleSphere").gameObject.SetActive(false);
            eggStar.transform.Find("Sector/EggStarInner/Effects/DB_BrambleLightShafts").gameObject.SetActive(false);

            rangerShip = eggStar.transform.Find("Sector/Structure_TH_HEA_MiningRig").gameObject;
            ObjectUtils.AddReferenceFrame(rangerShip, "LocationRangerShip", 300f, 100f, 100000f);

            ObjectUtils.PlaceOnPlanet(LightController.Spawn(Color.red, 1f, 500f, 0.5f), "Egg Star", new Vector3(0f, 2000f, 0f), Vector3.zero);

            (cutsceneCam, _) = SolarRangers.CommonCameraUtility.CreateCustomCamera($"{nameof(SolarRangers)}.Cutscene");

            var materialMapping = new Dictionary<string, string> {
                { "Effects_NOM_VolumetricLight", "Effects_EGG_VolumetricLight" },
                { "Structure_NOM_Porcelain_mat", "Structure_EGG_Porcelain_mat" },
                { "Structure_NOM_BlueGlow_mat", "Structure_EGG_RedGlow_mat" },
                { "Structure_NOM_Silver_mat", "Structure_EGG_RedMetal_mat" },
                { "Structure_NOM_Airlock_mat", "Structure_EGG_Porcelain_mat" },
            };
            ObjectUtils.ReplaceMaterials(eggStar, materialMapping);
        }

        void Update()
        {
            switch (state)
            {
                case State.Initial:
                    var shouldStartInvasion = DialogueConditionManager.SharedInstance.GetConditionState("RANGER_INVASION");
                    if (SolarRangers.DEBUG && Keyboard.current.numpad1Key.wasPressedThisFrame) shouldStartInvasion = true;
                    if (shouldStartInvasion)
                    {
                        state = State.OuterDefenses;
                        StartCoroutine(DoStartOuterDefensesPhase());
                    }
                    break;
                case State.OuterDefenses:
                    var shouldOpenEggStar = beacons != null && beacons.All(b => b.IsDestroyed());
                    if (shouldOpenEggStar)
                    {
                        state = State.InnerDefenses;
                        StartCoroutine(DoStartInnerDefensesPhase());
                    }
                    break;
            }
        }

        IEnumerator DoStartOuterDefensesPhase()
        {
            SolarRangers.CombatModeActive = true;

            ObjectUtils.PlaceOnPlanet(EggTowerCombatantController.Spawn(), "Egg Star", new Vector3(-745f, -385f, 154f), new Vector3(55f, 44f, 131f));
            ObjectUtils.PlaceOnPlanet(EggTowerCombatantController.Spawn(), "Egg Star", new Vector3(-712f, -187f, -431f), new Vector3(64f, 135f, 251f));
            ObjectUtils.PlaceOnPlanet(EggTowerCombatantController.Spawn(), "Egg Star", new Vector3(-610f, 419f, -423f), new Vector3(62f, 205f, 331f));
            ObjectUtils.PlaceOnPlanet(EggTowerCombatantController.Spawn(), "Egg Star", new Vector3(-581f, 598f, 164f), new Vector3(40f, 242f, 333f));
            ObjectUtils.PlaceOnPlanet(EggTowerCombatantController.Spawn(), "Egg Star", new Vector3(-651f, 120f, 532f), new Vector3(71f, 351f, 39f));

            var entryDimension = SolarRangers.NewHorizons.GetPlanet("RangerEntryOuter");
            ObjectUtils.PlaceOnPlanet(AnglerCombatantController.Spawn(entryDimension.transform.Find("Sector"), 0.8f, true), "RangerEntryOuter", new Vector3(0f, 0f, -200f), new Vector3(0f, 0f, 0f));

            ObjectUtils.PlaceOnPlanet(EggDroneCombatantController.Spawn(), "Egg Star", new Vector3(-1000f, 0f, 0f), new Vector3(270f, 0f, 0f));

            reactor = ObjectUtils.PlaceOnPlanet(ReactorCombatantController.Spawn(), "RangerReactorOuter", Vector3.zero, new Vector3(90f, 0f, 0f));

            beacons = [];
            for (int i = 0; i < 8; i++)
            {
                var angle = i / 8f * Mathf.PI * 2f;
                var radius = 828f;
                var pos = radius * new Vector3(0f, Mathf.Cos(angle), Mathf.Sin(angle));
                var rot = Quaternion.LookRotation(pos.normalized, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
                beacons.Add(ObjectUtils.PlaceOnPlanet(BeaconDestructibleController.Spawn(), "Egg Star", pos, rot.eulerAngles));
            }

            var targetPos = eggStar.transform.TransformPoint(-2000f, 0f, -100f);
            var targetRot = eggStar.transform.rotation * Quaternion.Euler(0f, 90f, 0f);
            eggStarAirlock.ResetToOpenState();
            yield return DoCutscene(CutsceneBody, targetPos, targetRot);
            eggStarAirlock.Close(eggStarAirlock._closeSwitches[0]);

            IEnumerator CutsceneBody()
            {
                yield return new WaitForSeconds(2f);
                // Spawn flying enemies
                yield return new WaitForSeconds(2f);
            }
        }

        IEnumerator DoStartInnerDefensesPhase()
        {
            var targetPos = eggStar.transform.TransformPoint(-2000f, 0f, -100f);
            var targetRot = eggStar.transform.rotation * Quaternion.Euler(0f, 90f, 0f);
            eggStarAirlock.ResetToOpenState();
            yield return DoCutscene(CutsceneBody, targetPos, targetRot);

            IEnumerator CutsceneBody()
            {
                yield return new WaitForSeconds(2f);
            }
        }

        IEnumerator DoCutscene(Func<IEnumerator> action, Vector3 targetPos, Quaternion targetRot)
        {
            var previousMode = OWInput.GetInputMode();
            OWInput.ChangeInputMode(InputMode.None);
            Locator.GetPlayerBody().GetComponent<PlayerResources>().ToggleInvincibility();
            SolarRangers.CommonCameraUtility.EnterCamera(cutsceneCam);
            cutsceneCam.transform.position = Locator.GetPlayerCamera().transform.position;
            cutsceneCam.transform.rotation = Locator.GetPlayerCamera().transform.rotation;
            yield return AsyncUtils.DoObjectLerp(cutsceneCam.transform, targetPos, targetRot, 1f);
            yield return action();
            targetPos = Locator.GetPlayerCamera().transform.position;
            targetRot = Locator.GetPlayerCamera().transform.rotation;
            yield return AsyncUtils.DoObjectLerp(cutsceneCam.transform, targetPos, targetRot, 1f);
            SolarRangers.CommonCameraUtility.ExitCamera(cutsceneCam);
            OWInput.ChangeInputMode(previousMode);
            Locator.GetPlayerBody().GetComponent<PlayerResources>().ToggleInvincibility();
        }

        public enum State
        {
            Initial,
            OuterDefenses,
            InnerDefenses,
            BossFight,
            Escape,
        }
    }
}
