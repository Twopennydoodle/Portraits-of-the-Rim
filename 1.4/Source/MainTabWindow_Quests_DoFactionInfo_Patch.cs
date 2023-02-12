using HarmonyLib;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace PortraitsOfTheRim
{
    [HotSwappable]
    [HarmonyPatch(typeof(MainTabWindow_Quests), "DoFactionInfo")]
    public static class MainTabWindow_Quests_DoFactionInfo_Patch
    {
        public static void Prefix(MainTabWindow_Quests __instance, Rect rect, ref float curY)
        {
            foreach (var part in __instance.selected.PartsListForReading)
            {
                if (part is QuestPart_Hyperlinks hyperlinks)
                {
                    var pawn = hyperlinks.pawns?.FirstOrDefault();
                    if (pawn != null)
                    {
                        DrawPortrait(rect, ref curY, pawn);
                        return;
                    }
                }
                else if (part is QuestPart_PawnsArrive pawnsArrive)
                {
                    var pawn = pawnsArrive.pawns?.FirstOrDefault();
                    if (pawn != null)
                    {
                        DrawPortrait(rect, ref curY, pawn);
                        return;
                    }
                }
                else if (part is QuestPart_ExtraFaction extraFaction)
                {
                    var pawn = extraFaction.affectedPawns?.FirstOrDefault();
                    if (pawn != null)
                    {
                        DrawPortrait(rect, ref curY, pawn);
                        return;
                    }
                }
                else
                {
                    var pawn = part.QuestLookTargets.Where(x => x.Thing is Pawn).Select(x => x.Thing).Cast<Pawn>().FirstOrDefault();
                    if (pawn != null)
                    {
                        DrawPortrait(rect, ref curY, pawn);
                        return;
                    }
                }
            }
        }

        private static void DrawPortrait(Rect rect, ref float curY, Pawn pawn)
        {
            var portrait = pawn.GetPortrait();
            portrait.RenderPortrait(rect.width - ITab_Pawn_Gear_FillTab_Patch.portraitSize, curY + 15,
                ITab_Pawn_Gear_FillTab_Patch.portraitSize, ITab_Pawn_Gear_FillTab_Patch.portraitSize);
            curY += ITab_Pawn_Gear_FillTab_Patch.portraitSize;
        }
    }
}
