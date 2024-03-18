using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolarRangers.Managers
{
    public class AbstractManager<T> : MonoBehaviour where T : AbstractManager<T>
    {
        public static T Instance;

        public static T Spawn()
        {
            if (!Instance)
            {
                Instance = new GameObject(typeof(T).Name).AddComponent<T>();
            }
            return Instance;
        }
    }
}
