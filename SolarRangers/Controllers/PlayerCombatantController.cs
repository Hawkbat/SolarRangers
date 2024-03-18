using SolarRangers.Interfaces;
using SolarRangers.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolarRangers.Controllers
{
    public class PlayerCombatantController : AbstractCombatantController
    {

        NotificationData probeReloadNotification;
        bool probeReloading;
        float probeReloadTimer;

        ScreenPrompt fireLasersPrompt;
        LaserTurretController scopeTurret;

        public override string GetNameKey() => "CombatantPlayer";
        public override bool CanTarget() => false;
        public override bool IsPlayer() => true;

        public bool IsReloadingProbe() => probeReloading;
        public bool IsSignalscopeGun()
            => SolarRangers.CombatModeActive && Locator.GetToolModeSwapper().IsInToolMode(ToolMode.SignalScope, ToolGroup.Suit);

        public static PlayerCombatantController Merge(Component c)
        {
            var player = c.gameObject.AddComponent<PlayerCombatantController>();
            player.Init();
            return player;
        }

        public void Init()
        {

        }

        public void StartProbeReload(float reloadTime)
        {
            probeReloading = true;
            probeReloadTimer = reloadTime;
            if (probeReloadNotification == null)
            {
                var probeReloadText = SolarRangers.NewHorizons.GetTranslationForUI("NotificationProbeReload");
                probeReloadNotification = new NotificationData(NotificationTarget.All, probeReloadText, reloadTime, false);
            }
            probeReloadNotification.minDuration = reloadTime;
            NotificationManager.SharedInstance.PostNotification(probeReloadNotification);
        }

        void Awake()
        {
            var fireLasersPromptText = SolarRangers.NewHorizons.GetTranslationForUI("PromptSuitFireLaser");
            fireLasersPrompt = new ScreenPrompt(InputLibrary.lockOn, fireLasersPromptText);
            Locator.GetPromptManager().AddScreenPrompt(fireLasersPrompt, PromptPosition.BottomCenter);

            var scope = Locator.GetToolModeSwapper().GetSignalScope();

            var scopeTurretObj = new GameObject("ScopeTurret");
            scopeTurretObj.transform.SetParent(scope._scopeGameObject.transform, false);
            scopeTurret = scopeTurretObj.AddComponent<LaserTurretController>();
            scopeTurret.Init(this, 1f, 0f, 20f, 0f, 200f, 1000f, new Vector3(0.2f, 1f, 0.2f), Color.red);
        }

        void LateUpdate()
        {
            if (IsSignalscopeGun())
            {
                var scope = Locator.GetToolModeSwapper().GetSignalScope();
                if (scope.GetFrequencyFilter() != CombatantManager.GetCombatFrequency())
                {
                    scope.SelectFrequency(CombatantManager.GetCombatFrequency());
                }

                var source = scope.InZoomMode() ? Locator.GetPlayerCamera().transform.position : scope._scopeGameObject.transform.position;
                var target = Locator.GetPlayerCamera().transform.position + Locator.GetPlayerCamera().transform.forward * 50f;
                var dir = (target - source).normalized;

                scopeTurret.transform.position = source + dir;
                scopeTurret.transform.forward = dir;
            }

            var firing = IsSignalscopeGun() && OWInput.IsPressed(InputLibrary.lockOn);
            scopeTurret.SetFiringState(firing);

            fireLasersPrompt.SetVisibility(OWInput.IsInputMode(InputMode.Character | InputMode.ScopeZoom) && IsSignalscopeGun());

            if (probeReloading)
            {
                probeReloadTimer = Mathf.Max(0f, probeReloadTimer - Time.deltaTime);
                if (probeReloadTimer <= 0f)
                {
                    probeReloading = false;
                }
            }
        }

    }
}
