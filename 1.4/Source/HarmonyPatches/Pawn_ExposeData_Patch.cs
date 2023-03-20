using HarmonyLib;
using RimWorld;
using Verse;

namespace PortraitsOfTheRim
{
    [HarmonyPatch(typeof(Pawn), "ExposeData")]
    public static class Pawn_ExposeData_Patch
    {
        public static void Postfix(Pawn __instance)
        {
            var portrait = __instance.GetPortrait();
            Scribe_Values.Look(ref portrait.hidePortrait, "PR_hidePortrait", !PortraitsOfTheRimSettings.showPortraitByDefault);
            Scribe_Values.Look(ref portrait.hideHeadgear, "PR_hideHeadgear");
            Scribe_Values.Look(ref portrait.currentStyle, "PR_currentStyle", "");
        }
    }
}
