﻿using HarmonyLib;
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
            Scribe_Values.Look(ref portrait.hideHeadgear, "PR_hideHeadgear", !PortraitsOfTheRimSettings.showHeadgearByDefault);
            Scribe_Values.Look(ref portrait.currentStyle, "PR_currentStyle", "");
            Scribe_Values.Look(ref portrait.hairSeed, "hairSeed", 0);
            Scribe_Values.Look(ref portrait.isHairRandomized, "isHairRandomized", false);
            Scribe_Defs.Look(ref portrait.innerFaceToSave, "innerFaceToSave");
        }
    }
}
