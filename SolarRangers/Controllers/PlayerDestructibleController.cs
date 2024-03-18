using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolarRangers.Interfaces;
using UnityEngine;

namespace SolarRangers.Controllers
{
    public class PlayerDestructibleController : AbstractDestructibleController
    {
        PlayerResources playerResources;

        public override string GetNameKey() => "CombatantPlayer";
        public override float GetHealth() => playerResources.GetHealth();
        public override float GetMaxHealth() => PlayerResources._maxHealth;
        public override bool IsDestroyed()
            => Locator.GetDeathManager().IsPlayerDying() || Locator.GetDeathManager().IsPlayerDead();

        public static PlayerDestructibleController Merge(Component c)
        {
            var player = c.gameObject.AddComponent<PlayerDestructibleController>();
            player.Init();
            return player;
        }

        public void Init()
        {

        }

        public override bool TakeDamage(IDamageSource source, float damage)
        {
            return playerResources.ApplyInstantDamage(damage, source.GetDamageType());
        }

        void Awake()
        {
            playerResources = Locator.GetPlayerTransform().GetComponent<PlayerResources>();
        }
    }
}
