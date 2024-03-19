using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolarRangers.Controllers
{
    public class LightController : MonoBehaviour
    {
        float intensity;
        float strobeRate;

        Light light;

        public static LightController Spawn(Color color, float intensity, float range, float strobeRate)
        {
            var light = new GameObject("Light").AddComponent<LightController>();
            light.Init(color, intensity, range, strobeRate);
            return light;
        }

        public void Init(Color color, float intensity, float range, float strobeRate)
        {
            this.intensity = intensity;
            this.strobeRate = strobeRate;

            light.color = color;
            light.intensity = intensity;
            light.range = range;
        }

        void Awake()
        {
            light = gameObject.GetAddComponent<Light>();
        }

        void Update()
        {
            if (strobeRate != 0f)
            {
                light.intensity = intensity * Mathf.Abs(Mathf.Sin(Time.time * Mathf.PI * strobeRate));
            }
        }
    }
}
