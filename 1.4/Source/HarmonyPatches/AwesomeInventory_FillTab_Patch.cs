using HarmonyLib;
using RimWorld;
using System.Reflection;
using UnityEngine;
using Verse;

namespace PortraitsOfTheRim
{
    [HotSwappable]
    [HarmonyPatch]
    public static class AwesomeInventory_FillTab_Patch
    {
        public static MethodInfo target;
        public static bool Prepare()
        {
            target = AccessTools.Method("AwesomeInventory.UI.AwesomeInventoryTabBase:FillTab");
            return target != null;
        }
        public static MethodBase TargetMethod()
        {
            return target;
        }

        public static float xPos = 500;
        public static float yPos = 133;
        public static float portraitSize = 180;

        public static bool isGreedy = false;
        public static void Prefix(ITab_Pawn_Gear __instance, bool ____isGreedy)
        {
            isGreedy = ____isGreedy;
        }
    }

    [HotSwappable]
    [HarmonyPatch]
    public static class AwesomeInventory_DrawStatPanel_Patch
    {
        public static MethodInfo target;
        public static bool Prepare()
        {
            target = AccessTools.Method("AwesomeInventory.UI.DrawGearTabWorker:DrawStatPanel");
            return target != null;
        }
        public static MethodBase TargetMethod()
        {
            return target;
        }

        public static float xPos = 480;
        public static float yPos = 113;
        public static float portraitSize = 180;
        public static void Postfix(Rect rect, Pawn pawn)
        {
            ITab_Pawn_Gear_FillTab_Patch.DrawPortraitArea(pawn, xPos, yPos, portraitSize);
        }
    }
}
