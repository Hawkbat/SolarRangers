using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolarRangers.Controllers
{
    public class GroundForcefieldController : MonoBehaviour
    {
        bool active;
        BeaconDestructibleController beacon;
        GameObject fieldObj;

        static GameObject fieldPrefab;

        public static GroundForcefieldController Spawn(BeaconDestructibleController beacon)
        {
            var field = new GameObject("GroundForcefield").AddComponent<GroundForcefieldController>();
            field.Init(beacon);
            return field;
        }

        public void Init(BeaconDestructibleController beacon)
        {
            this.beacon = beacon;

            if (!fieldPrefab)
            {
                fieldPrefab = SolarRangers.NewHorizons.GetPlanet("Egg Star").transform.Find("Sector/PREFAB_GroundForcefield").gameObject;
                fieldPrefab.SetActive(false);
            }

            fieldObj = Instantiate(fieldPrefab, transform);
            fieldObj.transform.localPosition = Vector3.zero;
            fieldObj.transform.localEulerAngles = Vector3.zero;
            fieldObj.transform.localScale = Vector3.one;
            fieldObj.SetActive(true);
        }

        void Update()
        {
            if (active && beacon.IsDestroyed())
            {
                active = false;
                fieldObj.SetActive(false);
            }
        }
    }
}
