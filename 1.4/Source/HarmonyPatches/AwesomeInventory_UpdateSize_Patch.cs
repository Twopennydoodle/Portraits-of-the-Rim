using HarmonyLib;
using RimWorld;
using System.Reflection;

namespace PortraitsOfTheRim
{
    [HotSwappable]
    [HarmonyPatch]
    public static class AwesomeInventory_UpdateSize_Patch
    {
        public static MethodInfo target;
        public static bool Prepare()
        {
            target = AccessTools.Method("AwesomeInventory.UI.AwesomeInventoryTabBase:UpdateSize");
            return target != null;
        }
        public static MethodBase TargetMethod()
        {
            return target;
        }

        public static void Postfix(ITab_Pawn_Gear __instance)
        {
           __instance.size.x += 150;
        }
    }
}
