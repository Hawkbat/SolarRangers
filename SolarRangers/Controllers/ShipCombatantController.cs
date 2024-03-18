using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolarRangers.Controllers
{
    public class ShipCombatantController : AbstractCombatantController
    {
        OWCamera shipCam;
        Coroutine shipTransformAnimationCoroutine;
        bool wingsOpen;

        ShipWingController wingLL;
        ShipWingController wingUL;
        ShipWingController wingLR;
        ShipWingController wingUR;

        ScreenPrompt fireLasersPrompt;

        public override string GetNameKey() => "CombatantShip";
        public override bool CanTarget() => false;
        public override bool IsPlayer() => true;

        public bool IsInAttackMode =>
            SolarRangers.CombatModeActive && wingsOpen && Locator.GetToolModeSwapper().IsInToolMode(ToolMode.Probe, ToolGroup.Ship);

        public static ShipCombatantController Merge(Component c)
        {
            var ship = c.gameObject.AddComponent<ShipCombatantController>();
            ship.Init();
            return ship;
        }

        public void Init()
        {

        }

        void Awake()
        {
            (shipCam, _) = SolarRangers.CommonCameraUtility.CreateCustomCamera($"{nameof(SolarRangers)}.ThirdPersonShip");

            var fireLasersPromptText = SolarRangers.NewHorizons.GetTranslationForUI("PromptShipFireLasers");
            fireLasersPrompt = new ScreenPrompt(InputLibrary.matchVelocity, "<CMD>" + UITextLibrary.GetString(UITextType.HoldPrompt) + "   " + fireLasersPromptText);
            Locator.GetPromptManager().AddScreenPrompt(fireLasersPrompt, PromptPosition.BottomCenter);

            wingLL = new GameObject().AddComponent<ShipWingController>().Init(this, true, true);
            wingUL = new GameObject().AddComponent<ShipWingController>().Init(this, true, false);
            wingLR = new GameObject().AddComponent<ShipWingController>().Init(this, false, true);
            wingUR = new GameObject().AddComponent<ShipWingController>().Init(this, false, false);

            GlobalMessenger.AddListener("StartShipIgnition", StartShipIgnition);
            GlobalMessenger.AddListener("CancelShipIgnition", CancelShipIgnition);
            GlobalMessenger.AddListener("CompleteShipIgnition", CompleteShipIgnition);
        }

        void OnDestroy()
        {
            GlobalMessenger.RemoveListener("StartShipIgnition", StartShipIgnition);
            GlobalMessenger.RemoveListener("CancelShipIgnition", CancelShipIgnition);
            GlobalMessenger.RemoveListener("CompleteShipIgnition", CompleteShipIgnition);
        }

        void Update()
        {
            fireLasersPrompt.SetVisibility(OWInput.IsInputMode(InputMode.ShipCockpit) && IsInAttackMode);
            var firing = IsInAttackMode && OWInput.IsPressed(InputLibrary.matchVelocity);
            SetFiringState(firing);
        }

        void StartShipIgnition()
        {
            wingsOpen = false;
            shipTransformAnimationCoroutine = StartCoroutine(DoShipTransformAnimation(true));
        }

        void CancelShipIgnition()
        {
            wingsOpen = false;
            CancelShipTransformAnimation();
        }

        void CompleteShipIgnition()
        {
            wingsOpen = true;
        }

        IEnumerator DoShipTransformAnimation(bool opening)
        {
            if (shipTransformAnimationCoroutine != null)
            {
                CancelShipTransformAnimation();
            }

            var shipT = Locator.GetShipTransform();
            shipCam.transform.parent = shipT;

            var camT = Locator.GetActiveCamera().transform;

            var initialPosition = shipT.InverseTransformPoint(camT.position);
            var initialRotation = shipT.InverseTransformRotation(camT.rotation);
            var targetPosition = new Vector3(0f, 5f, -25f);
            var targetRotation = Quaternion.Euler(0f, 0f, 0f);

            var inTime = 0.5f;
            var delay = 1.5f;
            var outTime = 0.5f;

            var totalWingTime = 1.5f;

            UpdateWingRotations(0f, opening);

            SolarRangers.CommonCameraUtility.EnterCamera(shipCam);

            for (var t = 0f; t < 1f; t = Mathf.Clamp01(t + Time.deltaTime / inTime))
            {
                shipCam.transform.localPosition = Vector3.Lerp(initialPosition, targetPosition, t);
                shipCam.transform.localRotation = Quaternion.Slerp(initialRotation, targetRotation, t);
                UpdateWingRotations(Mathf.InverseLerp(0f, totalWingTime, t * inTime), opening);
                yield return null;
            }
            shipCam.transform.localPosition = targetPosition;
            shipCam.transform.localRotation = targetRotation;

            for (var t = 0f; t < 1f; t = Mathf.Clamp01(t + Time.deltaTime / delay))
            {
                UpdateWingRotations(Mathf.InverseLerp(0f, totalWingTime, inTime + t * delay), opening);
                yield return null;
            }
            UpdateWingRotations(1f, opening);

            Locator.GetToolModeSwapper().EquipToolMode(ToolMode.Probe);

            for (var t = 0f; t < 1f; t = Mathf.Clamp01(t + Time.deltaTime / outTime))
            {
                shipCam.transform.localPosition = Vector3.Lerp(targetPosition, initialPosition, t);
                shipCam.transform.localRotation = Quaternion.Slerp(targetRotation, initialRotation, t);
                UpdateWingRotations(Mathf.InverseLerp(0f, totalWingTime, inTime + delay + t * outTime), opening);
                yield return null;
            }
            shipCam.transform.localPosition = initialPosition;
            shipCam.transform.localRotation = initialRotation;

            SolarRangers.CommonCameraUtility.ExitCamera(shipCam);
        }

        void CancelShipTransformAnimation()
        {
            if (shipTransformAnimationCoroutine != null)
            {
                StopCoroutine(shipTransformAnimationCoroutine);
                shipTransformAnimationCoroutine = null;
            }
            SolarRangers.CommonCameraUtility.ExitCamera(shipCam);
        }

        void SetFiringState(bool firing)
        {
            wingLL.SetFiringState(firing);
            wingUL.SetFiringState(firing);
            wingLR.SetFiringState(firing);
            wingUR.SetFiringState(firing);
        }

        void UpdateWingRotations(float t, bool opening)
        {
            t = Mathf.Clamp01(t);
            if (!opening) t = 1f - t;
            wingLL.UpdateRotation(t);
            wingUL.UpdateRotation(t);
            wingLR.UpdateRotation(t);
            wingUR.UpdateRotation(t);
        }
    }
}
