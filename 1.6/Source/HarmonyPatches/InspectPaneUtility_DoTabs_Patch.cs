using HarmonyLib;
using RimWorld;
using Verse;

namespace PortraitsOfTheRim
{
    [HarmonyPatch(typeof(InspectPaneUtility), "DoTabs")]
    public static class InspectPaneUtility_DoTabs_Patch
    {
        public static void Postfix(IInspectPane pane)
        {
            if (pane is MainTabWindow_Inspect inspectPanel && inspectPanel.SelThing is Pawn pawn && inspectPanel.OpenTabType is null)
            {
                ITab_Pawn_Gear_FillTab_Patch.DrawPortraitArea(pawn, inspectPanel.windowRect.x, inspectPanel.PaneTopY - 
                    30f - ITab_Pawn_Gear_FillTab_Patch.portraitSize, ITab_Pawn_Gear_FillTab_Patch.portraitSize, putShowPortraitToLeft: true);
            }
        }
    }
}
