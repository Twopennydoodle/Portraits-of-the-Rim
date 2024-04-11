using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PortraitsOfTheRim
{
    /* Patch to access the Gradient Hair textures if Gradient Hair mod is loaded.*/
    [HarmonyPatch(typeof(PawnRenderNode_Hair), "GraphicFor")]
    public static class PawnRenderNodeHair_Init_Patch
    {
        public static void Postfix(Pawn pawn, ref Graphic __result)
        {
            if (PortraitUtils.GradientHairLoaded && pawn != null)
            {
                if (__result.MatSouth)
                {
                    // Set color 2 in Dictionary 
                    PortraitUtils.gradientMaskColors.Add(pawn, __result.MatSouth.GetColorTwo());
                    if (__result.MatSouth.GetMaskTexture() != null)
                    {
                        // If Mask texture exists
                        PortraitUtils.gradientMaskTextures.Add(pawn, __result.MatSouth.GetMaskTexture().name);
                    }
                    else
                    {
                        // No mask texture - fall back to none.
                        PortraitUtils.gradientMaskTextures.Add(pawn, "MaskNone");
                    }
                }
                else
                {
                    PortraitUtils.gradientMaskTextures.Add(pawn, "MaskNone");
                }
                if (PortraitUtils.pawnPortraits.ContainsKey(pawn))
                {
                    PortraitUtils.pawnPortraits[pawn].forceRefresh = true; // Set the need to refresh manually here
                }
            }
        }
    }

}
