using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolarRangers.Managers
{
    public class ReferenceFrameManager : AbstractManager<ReferenceFrameManager>
    {
        readonly List<(ReferenceFrame, string)> nameKeys = [];

        public static void Register(ReferenceFrame frame, string nameKey)
        {
            Instance.nameKeys.Add((frame, nameKey));
        }

        public static string GetCustomName(ReferenceFrame frame)
        {
            if (!Instance) return null;
            var nameKey = Instance.nameKeys.Find(f => f.Item1 == frame);
            if (nameKey.Item1 != null) return SolarRangers.NewHorizons.GetTranslationForUI(nameKey.Item2);
            return null;
        }
    }
}
