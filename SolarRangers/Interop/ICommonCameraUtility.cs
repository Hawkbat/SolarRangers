using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine;

namespace SolarRangers.Interop
{
    public interface ICommonCameraUtility
    {
        // Takes an already made camera and registers it for use with CCU
        void RegisterCustomCamera(OWCamera OWCamera);

        // Creates a new camera
        (OWCamera, Camera) CreateCustomCamera(string name);

        // Creates a new camera. Optionally supply a method to run on it after it finishes initializing
        (OWCamera, Camera) CreateCustomCamera(string name, Action<OWCamera> postInitMethod);

        // Exit out of this camera while respecting which cameras were enabled previously
        void ExitCamera(OWCamera OWCamera);

        void EnterCamera(OWCamera OWCamera);

        UnityEvent<PlayerTool> EquipTool();
        UnityEvent<PlayerTool> UnequipTool();
    }
}
