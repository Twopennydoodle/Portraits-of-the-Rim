using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace PortraitsOfTheRim
{
    public class PortraitCamera : MonoBehaviour

    {
        // Maps name to the actual masking texture
        private static Dictionary<string, Texture2D> maskTextureDict = new();

        public PortraitElementDef portraitElementDef;
        public Pawn pawn;

        public void RenderElement(PortraitElementDef portraitElementDef, Pawn pawn, RenderTexture renderTexture, Vector3 cameraOffset, float cameraZoom)
        {
            Camera renderCamera = PortraitUtils.portraitCamera;
            this.portraitElementDef = portraitElementDef;
            this.pawn = pawn;
            renderCamera.backgroundColor = Color.clear;
            renderCamera.targetTexture = renderTexture;
            Vector3 position = renderCamera.transform.position;
            float orthographicSize = renderCamera.orthographicSize;
            renderCamera.transform.position += cameraOffset;
            renderCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            renderCamera.orthographicSize = 1f / cameraZoom;
            renderCamera.Render();
            renderCamera.transform.position = position;
            renderCamera.orthographicSize = orthographicSize;
            renderCamera.targetTexture = null;
        }

        /* Handles recoloring and gradienting. Huge thanks and credit to bolphen and the Avatar
         * mod (https://github.com/bolphen/rimworld-avatar/) for insight on how the masking and
         * the hair gradient mod works. */
        public void OnPostRender()
        {
            var recolor = portraitElementDef.GetRecolor(pawn);
            if (recolor != null)
            {
                // Set primary color for all
                portraitElementDef.graphic.MatSingle.color = recolor.Value;

                if (PortraitUtils.GradientHairLoaded && portraitElementDef.portraitLayer.canGradient)
                {
                    // Set color of gradiented hair
                    if (PortraitUtils.gradientMaskColors.TryGetValue(pawn, out Color color))
                    {
                        portraitElementDef.graphic.MatSingle.SetColor(ShaderPropertyIDs.ColorTwo, color);
                    }
                    if (PortraitUtils.gradientMaskTextures.TryGetValue(pawn, out string value))
                    {
                        string fullMaskPath = "PotRHairMasks/potr_" + value;
                        if (!maskTextureDict.TryGetValue(fullMaskPath, out Texture2D maskTex))
                        {
                            maskTextureDict[fullMaskPath] = maskTex = ContentFinder<Texture2D>.Get(fullMaskPath);
                        }
                        if (maskTex != null)
                        {
                            portraitElementDef.graphic.MatSingle.SetTexture(ShaderPropertyIDs.MaskTex, maskTex);
                        }
                        else // Case where the mask texture, for whatever reason, comes back as null, even if it's in the dictionary
                        {
                            portraitElementDef.graphic.MatSingle.SetTexture(ShaderPropertyIDs.MaskTex, Portrait.DefaultNoMask);
                        }

                    }
                    else
                    {
                        // Fall back to no mask if no mask texture exists in dictionary
                        portraitElementDef.graphic.MatSingle.SetTexture(ShaderPropertyIDs.MaskTex, Portrait.DefaultNoMask);
                    }
                }
                else
                {
                    // This is an element that is not able to be gradiented, or Gradient Hair is not loaded
                    portraitElementDef.graphic.MatSingle.SetTexture(ShaderPropertyIDs.MaskTex, Portrait.DefaultNoMask);
                }
            }
            Matrix4x4 matrix = default;
            matrix.SetTRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(1, 0, 1));
            GenDraw.DrawMeshNowOrLater(MeshPool.plane10, matrix, portraitElementDef.graphic.MatSingle, drawNow: true);
        }
    }
}
