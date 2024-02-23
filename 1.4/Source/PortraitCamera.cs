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

        public void OnPostRender()
        {
            var recolor = portraitElementDef.GetRecolor(pawn);
            if (recolor != null)
            {
                // Set the two colors; second color will be white (and not really mean anything) if no gradient mask is needed.
                portraitElementDef.graphic.MatSingle.color = recolor.Value;
                portraitElementDef.graphic.MatSingle.SetColor(ShaderPropertyIDs.ColorTwo, portraitElementDef.gradientColor);
                if (portraitElementDef.maskPath != "")
                {
                    if (!maskTextureDict.TryGetValue(portraitElementDef.maskPath, out Texture2D maskTex))
                    {
                        maskTextureDict[portraitElementDef.maskPath] = maskTex = ContentFinder<Texture2D>.Get("Masks/potr_" + portraitElementDef.maskPath);
                    }
                    if (maskTex != null)
                    {
                        portraitElementDef.graphic.MatSingle.SetTexture(ShaderPropertyIDs.MaskTex, maskTex);
                    }
                    else
                    {
                        portraitElementDef.graphic.MatSingle.SetTexture(ShaderPropertyIDs.MaskTex, Portrait.DefaultNoMask);
                    }
                    
                }
                else
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
