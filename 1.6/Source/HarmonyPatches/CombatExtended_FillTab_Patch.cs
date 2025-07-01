using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace PortraitsOfTheRim
{
    [HarmonyPatch]
    public static class CombatExtended_FillTab_Patch
    {
        public static MethodInfo target;
        public static bool Prepare()
        {
            if (PortraitUtils.CELoaded)
            {
                target = AccessTools.Method("CombatExtended.ITab_Inventory:FillTab");
                return target != null;
            }
            return false;
        }
        public static MethodBase TargetMethod()
        {
            return target;
        }

        public static float xPos = 265;
        public static float yPos = 9;
        public static float portraitSize = 180;
        public static void Postfix(ITab_Pawn_Gear __instance)
        {
            ITab_Pawn_Gear_FillTab_Patch.DrawPortraitArea(__instance.SelPawnForGear, xPos, yPos, portraitSize);
        }
    }
}
