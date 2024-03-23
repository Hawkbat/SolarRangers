using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolarRangers.Controllers
{
    public class GenericNoiseMaker : NoiseMaker
    {
        bool active;
        float noiseRadius;

        public static GenericNoiseMaker Merge(Component c, bool active, float noiseRadius)
        {
            var noiseMaker = c.gameObject.AddComponent<GenericNoiseMaker>();
            noiseMaker.Init(active, noiseRadius);
            return noiseMaker;
        }

        public void Init(bool active, float noiseRadius)
        {
            this.active = active;
            this.noiseRadius = noiseRadius;
            UpdateRadius();
        }

        public void SetActivation(bool active)
        {
            this.active = active;
            UpdateRadius();
        }

        public void SetNoiseRadius(float noiseRadius)
        {
            this.noiseRadius = noiseRadius;
            UpdateRadius();
        }

        void UpdateRadius()
        {
            _noiseRadius = active ? noiseRadius : 0;
        }
    }
}
