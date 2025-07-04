
using HarmonyLib;
using Verse;
using UnityEngine;

namespace PortraitsOfTheRim
{
    /* Patch to access the Gradient Hair textures if Gradient Hair mod is loaded.*/
    [HarmonyPatch(typeof(PawnRenderNode_Hair), "GraphicFor")]
    public static class PawnRenderNodeHair_Init_Patch
    {
        public static void Postfix(Pawn pawn, ref Graphic __result)
        {
            if (PortraitUtils.GradientHairLoaded && pawn != null && pawn.ageTracker.AgeBiologicalYearsFloat >= 7f)
            {
                if (__result.MatSouth && __result.MatSouth.GetMaskTexture() != null)
                {
                    string maskName = __result.MatSouth.GetMaskTexture().name;
                    // If Mask texture exists AND verify that the mask is a PoTR approved mask
                    if (PortraitUtils.validMasks.Contains(maskName))
                    {
                        PortraitUtils.gradientMaskTextures[pawn] = maskName;
                        PortraitUtils.gradientMaskColors[pawn] = __result.MatSouth.GetColorTwo();
                    }
                    else
                    {
                        // No mask texture - fall back to none.
                        PortraitUtils.gradientMaskTextures[pawn] =  "MaskNone";
                        PortraitUtils.gradientMaskColors[pawn] = Color.white;
                    }
                }
                else
                {
                    PortraitUtils.gradientMaskTextures[pawn] = "MaskNone";
                    PortraitUtils.gradientMaskColors[pawn] = Color.white;
                }
                if (PortraitUtils.pawnPortraits.ContainsKey(pawn))
                {
                    PortraitUtils.pawnPortraits[pawn].forceRefresh = true; // Set the need to refresh manually here
                }
            }
        }
    }

}
