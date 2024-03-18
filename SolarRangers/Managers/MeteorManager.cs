using SolarRangers.Controllers;
using SolarRangers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolarRangers.Managers
{
    public class MeteorManager : AbstractManager<MeteorManager>
    {
        GameObject meteorPrefab;
        readonly Queue<MeteorProjectileController> meteorPool = new();

        void Awake()
        {
            meteorPrefab = GameObject.Find("VolcanicMoon_Body/Sector_VM/Effects_VM/VolcanoPivot/MeteorLauncher").GetComponent<MeteorLauncher>()._meteorPrefab;
        }

        public static void Launch(ICombatant attacker, float damage, float size, float speed, Transform launcher)
        {
            if (!Instance.meteorPool.TryDequeue(out var meteor))
            {
                meteor = Instantiate(Instance.meteorPrefab, Vector3.zero, Quaternion.identity).AddComponent<MeteorProjectileController>();
            }
            meteor.Init(attacker, damage, size, speed, launcher);
        }

        public static void Recycle(MeteorProjectileController meteor)
        {
            Instance.meteorPool.Enqueue(meteor);
        }
    }
}
