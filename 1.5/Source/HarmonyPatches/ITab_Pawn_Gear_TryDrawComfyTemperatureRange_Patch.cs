using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace PortraitsOfTheRim
{
    [HarmonyPatch(typeof(ITab_Pawn_Gear), "TryDrawComfyTemperatureRange")]
    public static class ITab_Pawn_Gear_TryDrawComfyTemperatureRange_Patch
    {
        public static bool Prefix(ITab_Pawn_Gear __instance, ref float curY, float width)
        {
            var pawn = __instance.SelPawnForGear;
            if (!pawn.Dead)
            {
                var portrait = pawn.GetPortrait();
                if (pawn.ShouldShowPortrait() && portrait.ShouldShow)
                {
                    Rect rect = new Rect(0f, curY, width - ITab_Pawn_Gear_FillTab_Patch.portraitSize - 20, 44);
                    float statValue = pawn.GetStatValue(StatDefOf.ComfyTemperatureMin);
                    float statValue2 = pawn.GetStatValue(StatDefOf.ComfyTemperatureMax);
                    Widgets.Label(rect, "ComfyTemperatureRange".Translate() + ": " + statValue.ToStringTemperature("F0") + " ~ " + statValue2.ToStringTemperature("F0"));
                    curY += 44f;
                    if (PortraitUtils.CELoaded)
                    {
                        curY += 110f;
                    }
                    return false;
                }
            }

            return true;
        }
    }
}
