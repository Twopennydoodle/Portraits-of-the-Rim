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
        public static bool regularRPGInventoryLoaded = ModsConfig.IsActive("Sandy.RPGStyleInventory");
        public static bool revampedRPGInventoryLoaded = ModsConfig.IsActive("Sandy.RPGStyleInventory.avilmask.Revamped");
        public static bool Prepare()
        {
            if (regularRPGInventoryLoaded || revampedRPGInventoryLoaded)
            {
                target = AccessTools.Method("Sandy_Detailed_RPG_Inventory.Sandy_Detailed_RPG_GearTab:FillTab");
                simplifiedViewGetter = AccessTools.Method("Sandy_Detailed_RPG_Inventory.Sandy_Detailed_RPG_GearTab:get_simplifiedView");
                return target != null;
            }
            return false;
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

        public static float xPosRegular = 368;
        public static float yPosRegular = 306;
        public static float xPosViewListRegular = 507;
        public static float yPosViewListRegular = 6;

        public static float portraitSize = 180;
        public static void Postfix(ITab_Pawn_Gear __instance, bool ___viewList)
        {
            if (regularRPGInventoryLoaded)
            {
                if (___viewList)
                {
                    ITab_Pawn_Gear_FillTab_Patch.DrawPortraitArea(__instance.SelPawnForGear, xPosViewListRegular, yPosViewListRegular, portraitSize);
                }
                else
                {
                    ITab_Pawn_Gear_FillTab_Patch.DrawPortraitArea(__instance.SelPawnForGear, xPosRegular, yPosRegular, portraitSize);
                }
            }
            else if (revampedRPGInventoryLoaded)
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
                else if (simplifiedViewGetter != null && (bool)simplifiedViewGetter.Invoke(__instance, null) == true)
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
}
