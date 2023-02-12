using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace PortraitsOfTheRim
{
    [HotSwappable]
    [HarmonyPatch(typeof(ITab_Pawn_Gear), "TryDrawComfyTemperatureRange")]
    public static class ITab_Pawn_Gear_TryDrawComfyTemperatureRange_Patch
    {
        public static bool Prefix(ITab_Pawn_Gear __instance, ref float curY, float width)
        {
            var pawn = __instance.SelPawnForGear;
            if (!pawn.Dead)
            {
                if (pawn.RaceProps.Humanlike)
                {
                    Rect rect = new Rect(0f, curY, width - ITab_Pawn_Gear_FillTab_Patch.portraitSize - 20, 44);
                    float statValue = pawn.GetStatValue(StatDefOf.ComfyTemperatureMin);
                    float statValue2 = pawn.GetStatValue(StatDefOf.ComfyTemperatureMax);
                    Widgets.Label(rect, "ComfyTemperatureRange".Translate() + ": " + statValue.ToStringTemperature("F0") + " ~ " + statValue2.ToStringTemperature("F0"));
                    curY += 44f;
                    return false;
                }
            }
            return true;
        }
    }

    [HotSwappable]
    [HarmonyPatch(typeof(ITab_Pawn_Gear), "FillTab")]
    public static class ITab_Pawn_Gear_FillTab_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var separator = AccessTools.Method(typeof(Widgets), nameof(Widgets.ListSeparator), 
                new Type[] { typeof(float), typeof(float), typeof(string) });
            var width = AccessTools.PropertyGetter(typeof(Rect), nameof(Rect.width));
            var codes = codeInstructions.ToList();
            for (var i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if (code.Calls(width) && codes[i + 1].OperandIs("OverallArmor"))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ITab_Pawn_Gear_FillTab_Patch), nameof(FixedWidth)));
                }
                else
                {
                    yield return code;
                }
            }
        }

        public static float FixedWidth(ref Rect rect, ITab_Pawn_Gear tab)
        {
            if (tab.SelPawnForGear.RaceProps.Humanlike)
            {
                return rect.width - portraitSize - 7;
            }
            return rect.width;
        }

        [TweakValue("0Portrait", 0f, 300f)] public static float xPos = 247;
        [TweakValue("0Portrait", 0f, 30f)] public static float yPos = 30;
        [TweakValue("0Portrait", 0f, 30f)] public static float portraitSize = 180;
        public static void Postfix(ITab_Pawn_Gear __instance)
        {
            Pawn pawn = __instance.SelPawnForGear;
            if (pawn != null && pawn.RaceProps.Humanlike)
            {
                var portrait = pawn.GetPortrait();
                if (portrait.ShouldShow)
                {
                    portrait.RenderPortrait(xPos, yPos, portraitSize, portraitSize);
                }
                portrait.DrawButtons(xPos + portraitSize + 5, yPos + portraitSize - 85);
            }
        }
    }
}
