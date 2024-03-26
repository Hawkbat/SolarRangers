using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolarRangers.Controllers
{
    internal class MaterialPanController : MonoBehaviour
    {
        Material materialRef;
        public int materialIndex;
        public Vector2 scrollSpeed;
        void Awake()
        {
            try
            {
                materialRef = GetComponent<MeshRenderer>().sharedMaterials[materialIndex];
            }
            catch (Exception e)
            {
                SolarRangers.Log(e.ToString(), OWML.Common.MessageType.Error);
            }
        }
        public void Update()
        {
            if (!materialRef) return;
            materialRef.mainTextureOffset += scrollSpeed * Time.deltaTime;
        }
    }
}
