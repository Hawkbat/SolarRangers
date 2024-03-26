using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolarRangers.Interfaces;
using UnityEngine;

namespace SolarRangers.Controllers
{
    public class PlayerDestructibleController : MonoBehaviour, IDestructible
    {
        PlayerResources playerResources;

        public string GetNameKey() => "CombatantPlayer";
        public float GetHealth() => playerResources.GetHealth();
        public float GetMaxHealth() => PlayerResources._maxHealth;
        public bool IsDestroyed()
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

        public bool OnTakeDamage(IDamageSource source)
        {
            return playerResources.ApplyInstantDamage(source.GetDamage(), source.GetDamageType());
        }

        void Awake()
        {
            playerResources = Locator.GetPlayerTransform().GetComponent<PlayerResources>();
        }
    }
}
