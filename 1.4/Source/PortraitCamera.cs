using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace PortraitsOfTheRim
{
    [HotSwappable]
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
                    if (pawn.Drawer != null &&
                    pawn.Drawer.renderer != null &&
                    pawn.Drawer.renderer.graphics != null &&
                    pawn.Drawer.renderer.graphics.hairGraphic != null)
                    {
                        // Gradient Hair code will set the appropriate mask on all of the hairGraphics
                        Material material = pawn.Drawer.renderer.graphics.hairGraphic.MatSouth;
                        if (material != null)
                        {
                            // Set secondary color on our graphic's material based off of the above material's secondary color
                            portraitElementDef.graphic.MatSingle.SetColor(ShaderPropertyIDs.ColorTwo, material.GetColorTwo());
                            // Very nice that the MatSouth's mask texture name field is just the filename
                            // If that ever changes, this technique will no longer work 
                            Texture2D hairMaskTex = material.GetMaskTexture();
                            if (hairMaskTex != null && PortraitUtils.validMasks.Contains(hairMaskTex.name))
                            {
                                string testMaskPath = "PotRHairMasks/potr_" + hairMaskTex.name; // Use the mask texture name field to make our path
                                if (!maskTextureDict.TryGetValue(testMaskPath, out Texture2D maskTex))
                                {
                                    maskTextureDict[testMaskPath] = maskTex = ContentFinder<Texture2D>.Get(testMaskPath);
                                }
                                if (maskTex != null)
                                {
                                    portraitElementDef.graphic.MatSingle.SetTexture(ShaderPropertyIDs.MaskTex, maskTex);
                                }
                                else // Case where the mask texture could not be obtained. Fall back to no masking instead of leaving that field undefined.
                                {
                                    portraitElementDef.graphic.MatSingle.SetTexture(ShaderPropertyIDs.MaskTex, Portrait.DefaultNoMask);
                                }
                            }
                            else //Case where the material's mask texture is null 
                            {
                                portraitElementDef.graphic.MatSingle.SetTexture(ShaderPropertyIDs.MaskTex, Portrait.DefaultNoMask);
                            }
                        }
                        else // Material is not set
                        {
                            portraitElementDef.graphic.MatSingle.SetTexture(ShaderPropertyIDs.MaskTex, Portrait.DefaultNoMask);
                        }
                    }
                    else
                    {
                        // Pawn's mask tex is not set, default to no mask 
                        portraitElementDef.graphic.MatSingle.SetTexture(ShaderPropertyIDs.MaskTex, Portrait.DefaultNoMask);
                    }
                    
                }
                else // Gradient Hair is not enabled
                {
                    portraitElementDef.graphic.MatSingle.SetTexture(ShaderPropertyIDs.MaskTex, Portrait.DefaultNoMask);
                }
            }
            Matrix4x4 matrix = default;
            matrix.SetTRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(1, 0, 1));
            GenDraw.DrawMeshNowOrLater(MeshPool.plane10, matrix, portraitElementDef.graphic.MatSingle, drawNow: true);
        }
    }
}
