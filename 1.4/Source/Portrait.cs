using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PortraitsOfTheRim
{
    [HotSwappable]
    public class Portrait
    {
        public Pawn pawn;
        private List<Texture> portraitTextures;
        private int lastCreatingTime;
        private List<Texture> PortraitTextures
        {
            get
            {
                if (portraitTextures is null || Time.frameCount % 60 == 0 && lastCreatingTime != Time.frameCount)
                {
                    lastCreatingTime = Time.frameCount;
                    portraitTextures = GetPortraitTextures();
                }
                return portraitTextures;
            }
        }

        public void RenderPortrait(float x, float y, float width, float height)
        {
            var textures = PortraitTextures;
            var renderRect = new Rect(x, y, width, height);
            foreach (var texture in textures)
            {
                GUI.DrawTexture(renderRect, texture, ScaleMode.StretchToFill);
            }
            Widgets.DrawBox(renderRect);
        }

        [TweakValue("0Portraits", 0, 5f)] public static float zoomValue = 1f;
        public List<Texture> GetPortraitTextures()
        {
            List<Texture> allTextures = new List<Texture>();    
            foreach (var layer in Core.layers)
            {
                if (Core.portraitElements.TryGetValue(layer, out var elements))
                {
                    var matchingElements = elements.Where(x => x.Matches(pawn)).ToList();
                    if (matchingElements.Any())
                    {
                        Rand.PushState();
                        Rand.Seed = pawn.thingIDNumber;
                        var matchingElement = matchingElements.RandomElement();
                        var texture = matchingElement.graphic.MatSingle.mainTexture;
                        var renderTexture = new RenderTexture(400, 400, 0);
                        TextureUtils.renderCamera.GetComponent<RenderCamera>().RenderPortraitElement(matchingElement, pawn, renderTexture, 
                            Vector3.zero, zoomValue);
                        allTextures.Add(renderTexture);
                        Rand.PopState();
                    }
                }
            }
            return allTextures;
        }
    }

    public class PortraitCamera : MonoBehaviour
    {
        private Pawn pawn;
        public void RenderPawn(Pawn pawn, RenderTexture renderTexture)
        {
            Camera camera = new Camera();
            Vector3 position = camera.transform.position;
            float orthographicSize = camera.orthographicSize;
            this.pawn = pawn;
            camera.SetTargetBuffers(renderTexture.colorBuffer, renderTexture.depthBuffer);
            camera.Render();
            this.pawn = null;
            camera.transform.position = position;
            camera.orthographicSize = orthographicSize;
            camera.targetTexture = null;
        }

        public void OnPostRender()
        {

        }
    }
}
