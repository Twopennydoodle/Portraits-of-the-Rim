using System;
using UnityEngine;
using Verse;

namespace PortraitsOfTheRim
{
    [HotSwappable]
    public class PortraitCamera : MonoBehaviour
    {
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
                portraitElementDef.graphic.MatSingle.color = recolor.Value;
            }
            Matrix4x4 matrix = default;
            matrix.SetTRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(1, 0, 1));
            GenDraw.DrawMeshNowOrLater(MeshPool.plane10, matrix, portraitElementDef.graphic.MatSingle, drawNow: true);
        }
    }
}
