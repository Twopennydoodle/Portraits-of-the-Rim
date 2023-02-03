using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace PortraitsOfTheRim
{
    [HotSwappable]
    [HarmonyPatch(typeof(UIRoot_Play), "UIRootOnGUI")]
    public static class UIRoot_Play_UIRootOnGUI_Patch
    {
        public static void Prefix()
        {
            Pawn pawn = Find.Selector.SelectedPawns.FirstOrDefault();
            if (pawn != null && pawn.IsColonist)
            {
                MainTabWindow_Inspect window = Find.WindowStack.WindowOfType<MainTabWindow_Inspect>();
                if (window != null)
                {
                    var portrait = pawn.GetPortrait();
                    portrait.RenderPortrait(0, window.PaneTopY - 30f - 400, 400, 400);
                }
            }
        }
    }
}
