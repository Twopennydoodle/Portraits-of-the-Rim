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
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ITab_Pawn_Gear_FillTab_Patch), nameof(FixedWidth)));
                }
                else
                {
                    yield return code;
                }
            }
        }

        public static float FixedWidth(ref Rect rect)
        {
            return rect.width - 180;
        }

        public static void Postfix(ITab_Pawn_Gear __instance)
        {
            if (PortraitsOfTheRimSettings.showPortraitsInGearTab)
            {
                Pawn pawn = __instance.SelPawnForGear;
                if (pawn != null && pawn.IsColonist)
                {
                    var portrait = pawn.GetPortrait();
                    portrait.RenderPortrait(254, 8, 180, 180);
                }
            }
        }
    }
}
