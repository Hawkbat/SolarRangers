using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolarRangers.Controllers;
using SolarRangers.Interfaces;
using UnityEngine;

namespace SolarRangers.Managers
{
    public class LaserManager : AbstractManager<LaserManager>
    {
        Shader colorShader;
        readonly Dictionary<Color, Material> colorMaterials = [];
        readonly Queue<LaserController> laserPool = new();

        void Awake()
        {

        }

        public static void Fire(ICombatant attacker, float damage, Vector3 start, Vector3 end, float speed, Vector3 size, Color color)
        {
            var outerMat = Instance.GetMaterial(color);
            var innerMat = Instance.GetMaterial(Color.white);
            if (!Instance.laserPool.TryDequeue(out var laser))
            {
                laser = new GameObject("Laser").AddComponent<LaserController>();
            }
            laser.Init(attacker, damage, start, end, speed, size, size.x * 0.5f, outerMat, innerMat);
        }

        Material GetMaterial(Color color)
        {
            if (!colorShader) colorShader = Shader.Find("Unlit/Color");
            if (!colorMaterials.TryGetValue(color, out var mat))
            {
                mat = new Material(colorShader)
                {
                    color = color
                };
                colorMaterials[color] = mat;
            }
            return mat;
        }

        public static void Recycle(LaserController laser)
        {
            Instance.laserPool.Enqueue(laser);
        }
    }
}
