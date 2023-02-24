using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace PortraitsOfTheRim
{
    [HotSwappable]
    [HarmonyPatch]
    public static class RPGInventory_FillTab_Patch
    {
        public static MethodInfo target;

        public static MethodInfo simplifiedViewGetter;
        public static bool Prepare()
        {
            target = AccessTools.Method("Sandy_Detailed_RPG_Inventory.Sandy_Detailed_RPG_GearTab:FillTab");
            simplifiedViewGetter = AccessTools.Method("Sandy_Detailed_RPG_Inventory.Sandy_Detailed_RPG_GearTab:get_simplifiedView");
            return target != null;
        }
        public static MethodBase TargetMethod()
        {
            return target;
        }

        public static float xPos = 495;
        public static float yPos = 293;
        public static float xPosViewList = 482;
        public static float yPosViewList = 298;
        public static float xPosSimplified = 370;
        public static float yPosSimplified = 300;
        public static float portraitSize = 180;
        public static void Postfix(ITab_Pawn_Gear __instance, bool ___viewList)
        {
            if (___viewList)
            {
                if (PortraitUtils.CELoaded)
                {
                    ITab_Pawn_Gear_FillTab_Patch.DrawPortraitArea(__instance.SelPawnForGear, xPosViewList, yPosViewList - 40, portraitSize);
                }
                else
                {
                    ITab_Pawn_Gear_FillTab_Patch.DrawPortraitArea(__instance.SelPawnForGear, xPosViewList, yPosViewList, portraitSize);
                }
            }
            else if ((bool)simplifiedViewGetter.Invoke(__instance, null) == true)
            {
                ITab_Pawn_Gear_FillTab_Patch.DrawPortraitArea(__instance.SelPawnForGear, xPosSimplified, yPosSimplified, portraitSize);
            }
            else
            {
                ITab_Pawn_Gear_FillTab_Patch.DrawPortraitArea(__instance.SelPawnForGear, xPos, yPos, portraitSize);
            }
        }
    }
}
