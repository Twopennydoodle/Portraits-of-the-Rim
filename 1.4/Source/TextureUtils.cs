using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace PortraitsOfTheRim
{
    public class RenderCamera : MonoBehaviour
    {
        private PortraitElementDef portraitElement;
        private Pawn pawn;
        public void RenderPortraitElement(PortraitElementDef portraitElement, Pawn pawn, RenderTexture renderTexture, 
            Vector3 cameraOffset, float cameraZoom)
        {
            Camera renderCamera = TextureUtils.renderCamera;
            this.portraitElement = portraitElement;
            this.pawn = pawn;
            renderCamera.backgroundColor = Color.white;
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
            var recolor = portraitElement.GetRecolor(pawn);
            if (recolor != null)
            {
                portraitElement.graphic.MatSingle.color = recolor.Value;
            }
            Matrix4x4 matrix = default;
            matrix.SetTRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(1, 0, 1));
            GenDraw.DrawMeshNowOrLater(MeshPool.plane10, Matrix4x4.identity, portraitElement.graphic.MatSingle, drawNow: true);
        }
    }

    [HotSwappable]
    [StaticConstructorOnStartup]
    public static class TextureUtils
    {
        public static Camera renderCamera;

        static TextureUtils()
        {
            GameObject gameObject = new GameObject("RenderCamera", typeof(Camera));
            gameObject.SetActive(value: false);
            gameObject.AddComponent<RenderCamera>();
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            Camera component = gameObject.GetComponent<Camera>();
            component.transform.position = new Vector3(0f, 15f, 0f);
            component.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            component.orthographic = true;
            component.cullingMask = 0;
            component.clearFlags = CameraClearFlags.Color;
            component.backgroundColor = Color.clear;
            component.useOcclusionCulling = false;
            component.renderingPath = RenderingPath.Forward;
            Camera camera = Current.Camera;
            component.nearClipPlane = camera.nearClipPlane;
            component.farClipPlane = camera.farClipPlane;
            renderCamera = component;
        }
        public static Texture2D GetReadableTexture(this Texture texture)
        {
            RenderTexture previous = RenderTexture.active;
            RenderTexture temporary = RenderTexture.GetTemporary(
                    texture.width,
                    texture.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

            Graphics.Blit(texture, temporary);
            RenderTexture.active = temporary;
            Texture2D texture2D = new Texture2D(texture.width, texture.height);
            texture2D.ReadPixels(new Rect(0f, 0f, (float)temporary.width, (float)temporary.height), 0, 0);
            texture2D.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(temporary);
            return texture2D;
        }

        public static Texture2D CombineTextures(Texture2D background, Texture2D overlay, int startX, int startY)
        {
            Texture2D newTex = new Texture2D(background.width, background.height, background.format, false);
            for (int x = 0; x < background.width; x++)
            {
                for (int y = 0; y < background.height; y++)
                {
                    if (x >= startX && y >= startY && x < overlay.width && y < overlay.height)
                    {
                        Color bgColor = background.GetPixel(x, y);
                        Color wmColor = overlay.GetPixel(x - startX, y - startY);

                        Color final_color = Color.Lerp(bgColor, wmColor, wmColor.a / 1.0f);

                        newTex.SetPixel(x, y, final_color);
                    }
                    else
                        newTex.SetPixel(x, y, background.GetPixel(x, y));
                }
            }

            newTex.Apply();
            return newTex;
        }

        public static Texture2D RecolorTexture(Texture2D texture, Color newColor)
        {
            texture = GetReadableTexture(texture);
            Texture2D newTex = new Texture2D(texture.width, texture.height, texture.format, false);
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    Color curColor = texture.GetPixel(x, y);
                    if (IndistinguishableFrom(Greyscale, curColor))
                    {
                        Log.Message(curColor.ToString());
                        newTex.SetPixel(x, y, curColor * newColor);
                    }
                    else
                    {
                        newTex.SetPixel(x, y, curColor);
                    }
                }
            }
            newTex.Apply();
            return newTex;
        }

        public static Color Greyscale = Color.grey;
        public static bool IndistinguishableFrom(this Color colA, Color colB)
        {
            if (GenColor.Colors32Equal(colA, colB))
            {
                return true;
            }
            Color color = colA - colB;
            return Mathf.Abs(color.r) + Mathf.Abs(color.g) + Mathf.Abs(color.b) + Mathf.Abs(color.a) < 2f;
        }
        public static Texture2D GetCombinedTexture(List<Texture2D> textures)
        {
            var texture = GetReadableTexture(textures[0]);
            for (int i = 1; i < textures.Count; i++)
            {
                var tex = GetReadableTexture(textures[i]);
                texture = CombineTextures(texture, tex, 0, 0);
            }
            return texture;
        }
    }
}
