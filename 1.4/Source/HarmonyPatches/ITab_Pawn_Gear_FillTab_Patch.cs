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
            var portrait = tab.SelPawnForGear.GetPortrait();
            if (portrait.ShouldShow && tab.SelPawnForGear.ShouldShowPortrait())
            {
                return rect.width - portraitSize - 7;
            }
            return rect.width;
        }

        public static float xPos = 247;
        public static float yPos = 30;
        public static float portraitSize = 180;
        public static void Postfix(ITab_Pawn_Gear __instance)
        {
            DrawPortraitArea(__instance.SelPawnForGear, xPos, yPos, portraitSize);
        }
        public static void DrawPortraitArea(Pawn pawn, float xPos, float yPos, float portraitSize, bool putShowPortraitToLeft = false)
        {
            if (pawn.ShouldShowPortrait())
            {
                var portrait = pawn.GetPortrait();
                if (portrait.ShouldShow)
                {
                    portrait.RenderPortrait(xPos, yPos, portraitSize, portraitSize);
                    var buttonAreaHeight = Portrait.buttonCount * (Portrait.buttonSize + Portrait.buttonSpacing);
                    portrait.DrawButtons(xPos + portraitSize + Portrait.buttonSpacing, yPos + portraitSize - buttonAreaHeight);
                }
                else
                {
                    if (putShowPortraitToLeft)
                    {
                        portrait.HidePortraitButton(xPos, yPos + portraitSize - 24);
                    }
                    else
                    {
                        portrait.HidePortraitButton(xPos + portraitSize + 5, yPos + portraitSize - 85);
                    }
                }
            }
        }
    }
}
