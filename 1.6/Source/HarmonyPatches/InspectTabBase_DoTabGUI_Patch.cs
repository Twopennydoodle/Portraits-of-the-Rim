using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace PortraitsOfTheRim
{
    [HarmonyPatch(typeof(InspectTabBase), "DoTabGUI")]
    public static class InspectTabBase_DoTabGUI_Patch
    {
        public static bool awesomeInventoryLoaded = ModsConfig.IsActive("NotooShabby.AwesomeInventoryForked");
        public static bool regularRPGInventoryLoaded = ModsConfig.IsActive("Sandy.RPGStyleInventory");
        public static bool revampedRPGInventoryLoaded = ModsConfig.IsActive("Sandy.RPGStyleInventory.avilmask.Revamped");
        public static bool Prepare() => awesomeInventoryLoaded || regularRPGInventoryLoaded || revampedRPGInventoryLoaded;
        public static void Postfix(InspectTabBase __instance)
        {
            if (__instance is ITab_Pawn_Gear tab)
            {
                ITab_Pawn_Gear_FillTab_Patch.DrawPortraitArea(tab.SelPawnForGear, __instance.TabRect.x, 
                    __instance.TabRect.y - ITab_Pawn_Gear_FillTab_Patch.portraitSize, ITab_Pawn_Gear_FillTab_Patch.portraitSize, putShowPortraitToLeft: true);
            }
        } 
    }
}
