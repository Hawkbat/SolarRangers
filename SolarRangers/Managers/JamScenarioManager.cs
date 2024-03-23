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
        readonly static List<(Vector3, Vector3)> exposedBeaconLocs = [
            (new(-556.4736f, 177.3767f, -590.6564f), new(12.8713f, 313.9004f, 76.96229f)),
            (new(386.3673f, 62.15552f, -728.5385f), new(349.1227f, 244.4996f, 84.42482f)),
            (new(-482.7887f, -488.1671f, 465.8023f), new(28.48313f, 67.90216f, 130.966f)),
            (new(219.0927f, -612.7933f, 509.4692f), new(349.8737f, 305.4434f, 221.8733f)),
            (new(371.6974f, 247.6658f, 692.604f), new(339.2303f, 292.143f, 289.0709f)),
            (new(-255.8235f, 356.976f, 702.2967f), new(355.6956f, 67.9834f, 63.86371f)),
            (new(582.8622f, 584.3083f, -11.08832f), new(30.21015f, 44.67143f, 328.7579f)),
            (new(-15.53556f, 672.9326f, -483.236f), new(320.0523f, 345.0906f, 7.339639f)),
            (new(-396.4197f, 730.2441f, 5.966614f), new(348.5993f, 165.9272f, 336.4727f)),
        ];
        readonly static List<(Vector3, Vector3)> shieldedBeaconLocs = [
            (new(-164.1224f, 218.0329f, -781.5752f), new(348.2879f, 286.2274f, 76.60799f)),
            (new(-119.0013f, -584.2071f, -577.3982f), new(46.15285f, 355.6879f, 171.7899f)),
            (new(-81.47157f, -156.3072f, 805.6272f), new(320.8534f, 72.34212f, 105.7312f)),
            (new(177.8307f, 707.4901f, 383.0002f), new(329.7046f, 236.3416f, 344.4792f)),
        ];

        /*
(o => `(new(${o.position.x}f, ${o.position.y}f, ${o.position.z}f), new(${o.rotation.x}f, ${o.rotation.y}f, ${o.rotation.z}f)),`)({
"position": {"x": -396.4197, "y": 730.2441, "z": 5.966614},
"rotation": {"x": 348.5993, "y": 165.9272, "z": 336.4727},
})
        */

        GameObject eggStar;
        GameObject entryDimension;
        GameObject reactorDimension;
        NomaiAirlock eggStarAirlock;
        GameObject rangerShip;
        List<BeaconDestructibleController> beacons;
        ReactorCombatantController reactor;
        OWCamera cutsceneCam;
        float escapeTimer;
        State state;

        public static State GetState() => Instance.state;

        void Awake()
        {
            eggStar = SolarRangers.NewHorizons.GetPlanet("Egg Star");
            entryDimension = SolarRangers.NewHorizons.GetPlanet("RangerEntryOuter");
            reactorDimension = SolarRangers.NewHorizons.GetPlanet("RangerReactorOuter");

            eggStarAirlock = eggStar.transform.Find("Sector/Prefab_NOM_Airlock (1)").GetComponentInChildren<NomaiAirlock>();
            eggStarAirlock._stopRotationQuats = eggStarAirlock._rotationObjectList.Select(r => Quaternion.Euler(r.closedRotation)).ToArray();
            eggStarAirlock._cycleLength--;
            eggStarAirlock._totalCycleTime--;
            eggStarAirlock._cycleTimer = eggStarAirlock._totalCycleTime / 2f;
            eggStarAirlock.FixedUpdate();
            eggStarAirlock._currentRotationState = NomaiMultiPartDoor.RotationState.CLOSED;

            eggStar.transform.Find("Sector/EggStarInner/Terrain_DB_BrambleSphere_Inner_v2/BrambleSphere 1").gameObject.SetActive(false);
            eggStar.transform.Find("Sector/EggStarInner/Terrain_DB_BrambleSphere_Inner_v2/bramblesphere_gateways").gameObject.SetActive(false);
            eggStar.transform.Find("Sector/EggStarInner/Terrain_DB_BrambleSphere_Inner_v2/COLLIDER_BrambleSphere").gameObject.SetActive(false);
            eggStar.transform.Find("Sector/EggStarInner/Effects/DB_BrambleLightShafts").gameObject.SetActive(false);

            rangerShip = eggStar.transform.Find("Sector/Structure_TH_HEA_MiningRig").gameObject;
            ObjectUtils.AddReferenceFrame(rangerShip, "LocationRangerShip", 300f, 100f, 100000f);

            ObjectUtils.PlaceOnPlanet(LightController.Spawn(Color.red, 1f, 500f, 0.5f), eggStar, new Vector3(0f, 2000f, 0f), Vector3.zero);

            (cutsceneCam, _) = SolarRangers.CommonCameraUtility.CreateCustomCamera($"{nameof(SolarRangers)}.Cutscene");
        }

        void Start()
        {
            ObjectUtils.ReplaceModMaterials(eggStar);
            ObjectUtils.ReplaceModMaterials(SolarRangers.NewHorizons.GetPlanet("RangerEntryOuter"));
            ObjectUtils.ReplaceModMaterials(SolarRangers.NewHorizons.GetPlanet("RangerReactorOuter"));
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
                    if (SolarRangers.DEBUG && Keyboard.current.numpad2Key.wasPressedThisFrame) shouldOpenEggStar = true;
                    if (shouldOpenEggStar)
                    {
                        state = State.InnerDefenses;
                        StartCoroutine(DoStartInnerDefensesPhase());
                    }
                    break;
                case State.InnerDefenses:
                    var bossDistance = Vector3.Distance(Locator.GetPlayerTransform().position, reactorDimension.transform.position);
                    var shouldStartBossFight = bossDistance < 1500f;
                    if (SolarRangers.DEBUG && Keyboard.current.numpad3Key.wasPressedThisFrame) shouldStartBossFight = true;
                    if (shouldStartBossFight)
                    {
                        state = State.BossFight;
                        StartCoroutine(DoStartBossFightPhase());
                    }
                    break;
                case State.BossFight:
                    var shouldStartEscape = reactor.IsCoreDestroyed();
                    if (SolarRangers.DEBUG && Keyboard.current.numpad4Key.wasPressedThisFrame) shouldStartEscape = true;
                    if (shouldStartEscape)
                    {
                        state = State.Escape;
                        StartCoroutine(DoStartEscapePhase());
                    }
                    break;
                case State.Escape:
                    var shouldPlayEnding = escapeTimer <= 0f;
                    if (SolarRangers.DEBUG && Keyboard.current.numpad5Key.wasPressedThisFrame) shouldPlayEnding = true;
                    if (shouldPlayEnding)
                    {
                        state = State.Ending;
                        StartCoroutine(DoEnding());
                    }
                    break;
                case State.Ending:

                    break;
            }
        }

        IEnumerator DoStartOuterDefensesPhase()
        {
            SolarRangers.CombatModeActive = true;

            ObjectUtils.PlaceOnPlanet(EggTowerCombatantController.Spawn(), eggStar, new Vector3(-745f, -385f, 154f), new Vector3(55f, 44f, 131f));
            ObjectUtils.PlaceOnPlanet(EggTowerCombatantController.Spawn(), eggStar, new Vector3(-712f, -187f, -431f), new Vector3(64f, 135f, 251f));
            ObjectUtils.PlaceOnPlanet(EggTowerCombatantController.Spawn(), eggStar, new Vector3(-610f, 419f, -423f), new Vector3(62f, 205f, 331f));
            ObjectUtils.PlaceOnPlanet(EggTowerCombatantController.Spawn(), eggStar, new Vector3(-581f, 598f, 164f), new Vector3(40f, 242f, 333f));
            ObjectUtils.PlaceOnPlanet(EggTowerCombatantController.Spawn(), eggStar, new Vector3(-651f, 120f, 532f), new Vector3(71f, 351f, 39f));

            beacons = [];

            foreach (var (pos, rot) in exposedBeaconLocs)
            {
                beacons.Add(ObjectUtils.PlaceOnPlanet(BeaconDestructibleController.Spawn(), eggStar, pos, rot));
            }

            foreach (var (pos, rot) in shieldedBeaconLocs)
            {
                var beacon = ObjectUtils.PlaceOnPlanet(BeaconDestructibleController.Spawn(), eggStar, pos, rot);
                beacons.Add(beacon);
                ObjectUtils.PlaceOnPlanet(GroundForcefieldController.Spawn(beacon), eggStar, pos, rot);
            }

            for (int i = 0; i < 6; i++)
            {
                var angle = i / 6f * Mathf.PI * 2f;
                var radius = 828f + 100f;
                var pos = radius * new Vector3(0f, Mathf.Cos(angle), Mathf.Sin(angle));
                var rot = Quaternion.LookRotation(pos.normalized, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
                ObjectUtils.PlaceOnPlanet(EggDroneCombatantController.Spawn(eggStar), eggStar, pos, rot.eulerAngles);
            }

            var targetPos = eggStar.transform.TransformPoint(-2000f, 0f, -100f);
            var targetRot = eggStar.transform.rotation * Quaternion.Euler(0f, 90f, 0f);
            eggStarAirlock.ResetToOpenState();
            yield return DoCutscene(CutsceneBody, targetPos, targetRot);
            eggStarAirlock.Close(eggStarAirlock._closeSwitches[0]);

            var notificationTemplateText = SolarRangers.NewHorizons.GetTranslationForUI("NotificationBeacons");
            var notificationText = string.Format(notificationTemplateText, beacons.Count(b => !b.IsDestroyed()));
            var notification = new NotificationData(NotificationTarget.All, notificationText);
            NotificationManager.SharedInstance.PostNotification(notification, true);

            while (state == State.OuterDefenses)
            {
                yield return null;
                notificationText = string.Format(notificationTemplateText, beacons.Count(b => !b.IsDestroyed()));
                if (notificationText != notification.displayMessage)
                {
                    notification.displayMessage = notificationText;
                    NotificationManager.SharedInstance.RepostNotifcation(notification);
                }
            }

            NotificationManager.SharedInstance.UnpinNotification(notification);

            IEnumerator CutsceneBody()
            {
                yield return new WaitForSeconds(2f);

                ObjectUtils.PlaceOnPlanet(EggDroneCombatantController.Spawn(eggStar), eggStar, new Vector3(-1000f, 25f, 0f), new Vector3(0f, 90f, 0f));
                ObjectUtils.PlaceOnPlanet(EggDroneCombatantController.Spawn(eggStar), eggStar, new Vector3(-1000f, -25f, -25f), new Vector3(0f, 90f, 0f));
                ObjectUtils.PlaceOnPlanet(EggDroneCombatantController.Spawn(eggStar), eggStar, new Vector3(-1000f, -25f, 25f), new Vector3(0f, 90f, 0f));
                // Spawn flying enemies
                yield return new WaitForSeconds(3f);
            }
        }

        IEnumerator DoStartInnerDefensesPhase()
        {
            var notificationText = SolarRangers.NewHorizons.GetTranslationForUI("NotificationInterior");
            var notification = new NotificationData(NotificationTarget.All, notificationText);
            NotificationManager.SharedInstance.PostNotification(notification, true);

            var targetPos = eggStar.transform.TransformPoint(-2000f, 0f, -100f);
            var targetRot = eggStar.transform.rotation * Quaternion.Euler(0f, 90f, 0f);
            eggStarAirlock.ResetToOpenState();
            yield return DoCutscene(CutsceneBody, targetPos, targetRot);

            var entryDimension = SolarRangers.NewHorizons.GetPlanet("RangerEntryOuter");
            ObjectUtils.PlaceOnPlanet(AnglerCombatantController.Spawn(entryDimension.transform.Find("Sector"), 1f, true), "RangerEntryOuter", new Vector3(0f, 0f, -200f), new Vector3(0f, 0f, 0f));
            ObjectUtils.PlaceOnPlanet(AnglerCombatantController.Spawn(entryDimension.transform.Find("Sector"), 1f, true), "RangerEntryOuter", new Vector3(0f, 200f, -100f), new Vector3(0f, 60f, 60f));
            ObjectUtils.PlaceOnPlanet(AnglerCombatantController.Spawn(entryDimension.transform.Find("Sector"), 1f, true), "RangerEntryOuter", new Vector3(50f, -200f, -100f), new Vector3(90f, 120f, 0f));

            while (state == State.InnerDefenses)
            {
                yield return null;
            }

            NotificationManager.SharedInstance.UnpinNotification(notification);

            IEnumerator CutsceneBody()
            {
                yield return new WaitForSeconds(2f);
            }
        }

        IEnumerator DoStartBossFightPhase()
        {
            var notificationText = SolarRangers.NewHorizons.GetTranslationForUI("NotificationReactor");
            var notification = new NotificationData(NotificationTarget.All, notificationText);
            NotificationManager.SharedInstance.PostNotification(notification, true);

            reactor = ObjectUtils.PlaceOnPlanet(ReactorCombatantController.Spawn(), "RangerReactorOuter", Vector3.zero, new Vector3(90f, 0f, 0f));
            
            while (state == State.BossFight)
            {
                yield return null;
            }

            NotificationManager.SharedInstance.UnpinNotification(notification);
        }

        IEnumerator DoStartEscapePhase()
        {
            escapeTimer = 90f;

            var shipNotificationDisplay = Locator.GetShipTransform().GetComponentInChildren<ShipNotificationDisplay>();

            var evacuateText = SolarRangers.NewHorizons.GetTranslationForUI("NotificationEvacuate");
            var evacuateNotif = new NotificationData(NotificationTarget.All, evacuateText);
            NotificationManager.SharedInstance.PostNotification(evacuateNotif, true);

            var countdownNotif = new NotificationData(NotificationTarget.All, string.Empty);
            UpdateNotificationText();
            NotificationManager.SharedInstance.PostNotification(countdownNotif, true);

            var explosionTime = 0f;

            while (escapeTimer > 0f)
            {
                yield return null;
                escapeTimer = Mathf.Max(escapeTimer - Time.deltaTime, 0f);
                UpdateNotificationText();
                explosionTime -= Time.deltaTime;
                if (explosionTime <= 0f)
                {
                    ExplosionManager.HugeExplosion(reactor, 10f, eggStar.transform, eggStar.transform.position + UnityEngine.Random.onUnitSphere * 828f);
                    ExplosionManager.HugeExplosion(reactor, 10f, entryDimension.transform, entryDimension.transform.position + UnityEngine.Random.onUnitSphere * 725f);
                    ExplosionManager.HugeExplosion(reactor, 10f, reactorDimension.transform, reactorDimension.transform.position + UnityEngine.Random.onUnitSphere * 725f);

                    explosionTime = UnityEngine.Random.Range(0.5f, 1f);
                }
            }

            NotificationManager.SharedInstance.UnpinNotification(countdownNotif);
            NotificationManager.SharedInstance.UnpinNotification(evacuateNotif);

            void UpdateNotificationText()
            {
                var countdownTimeSpan = TimeSpan.FromSeconds(escapeTimer);
                var countdownTimeText = countdownTimeSpan.ToString(@"mm\:ss\.ff");
                countdownNotif.displayMessage = countdownTimeText;

                var index = shipNotificationDisplay.FindDuplicateIndex(countdownNotif);
                if (index >= 0)
                {
                    var scrollEffect = shipNotificationDisplay._listDisplayData[index].TextScrollEffect;
                    scrollEffect.SetDisplayString(countdownTimeText);
                    scrollEffect.SetTextWithNoEffect();
                }
            }
        }

        IEnumerator DoEnding()
        {
            var targetPos = eggStar.transform.TransformPoint(-2000f, 0f, -300f);
            var targetRot = eggStar.transform.rotation * Quaternion.Euler(0f, 90f, 0f);
            yield return DoCutscene(CutsceneBody, targetPos, targetRot);
            var creditsVolume = eggStar.transform.Find("Sector/VOLUME_Victory");
            creditsVolume.position = Locator.GetPlayerTransform().position;

            IEnumerator CutsceneBody()
            {
                yield return new WaitForSeconds(2f);
                ExplosionManager.MassiveExplosion(reactor, 200f, eggStar.transform, eggStar.transform.position);
                yield return new WaitForSeconds(2.2f);
                eggStar.transform.Find("Sector/Prefab_NOM_Airlock (1)").gameObject.SetActive(false);
                eggStar.transform.Find("Sector/Airlock_Cap_Empty").gameObject.SetActive(false);
                eggStar.transform.Find("Sector/EggStarInner").gameObject.SetActive(false);
                yield return new WaitForSeconds(8f);
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
            Ending,
        }
    }
}
