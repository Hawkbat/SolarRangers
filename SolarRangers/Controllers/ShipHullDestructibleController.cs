using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolarRangers.Interfaces;
using UnityEngine;

namespace SolarRangers.Controllers
{
    public class ShipHullDestructibleController : AbstractDestructibleController
    {
        const float MAX_HEALTH = 300f;

        ShipHull hull;

        public override string GetNameKey() => UITextLibrary.GetString(hull.hullName);
        public override float GetHealth() => hull._integrity * MAX_HEALTH;
        public override float GetMaxHealth() => MAX_HEALTH;

        public static ShipHullDestructibleController Merge(Component c)
        {
            var shipHull = c.gameObject.AddComponent<ShipHullDestructibleController>();
            shipHull.Init();
            return shipHull;
        }

        public void Init()
        {

        }

        public override bool TakeDamage(IDamageSource source, float damage)
        {
            hull._integrity = Mathf.Max(hull._integrity - damage / MAX_HEALTH, 0f);
            
            var shipDamageController = Locator.GetShipBody().GetComponent<ShipDamageController>();
            if (!hull._damaged)
            {
                hull._damaged = true;
                shipDamageController.OnHullDamaged(hull);
                Locator.GetShipBody().GetComponentInChildren<ShipDamageDisplayV2>().OnHullUpdate(hull);
            }
            shipDamageController._audioController.PlayImpactAtPosition(AudioType.ShipImpact_HeavyDamage, 1f, UnityEngine.Random.Range(0.7f, 0.9f), source.GetDamagePosition());
            if (PlayerState.IsInsideShip() && (PlayerState.IsAttached() || Locator.GetPlayerController().IsGrounded()))
            {
                RumbleManager.PulseHeavyImpact();
            }
            if (hull._damageEffect)
            {
                hull._damageEffect.SetEffectBlend(1f - hull._integrity);
            }

            var damageType = source.GetDamageType();
            var hazardType = source.GetHazardType();
            var isIonWeapon = damageType == InstantDamageType.Electrical || hazardType == HazardVolume.HazardType.ELECTRICITY;
            var componentDamage = damage * (isIonWeapon ? 5f : 1f);
            foreach (var component in hull._components)
            {
                if (UnityEngine.Random.value < component._damageProbabilityCurve.Evaluate(componentDamage))
                {
                    component.SetDamaged(true);
                }
            }

            if (hull._integrity <= 0f && hull.shipModule is ShipDetachableModule module)
            {
                module.Detach();
            }

            return true;
        }

        void Awake()
        {
            hull = GetComponent<ShipHull>();
        }
    }
}
