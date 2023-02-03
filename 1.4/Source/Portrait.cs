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

        [TweakValue("0Portrait", 0, 3f)] public static float zoomValue = 1;
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
                        var output = new RenderTexture(400, 400, 0);
                        output.RenderElement(matchingElement, pawn, Vector3.zero, zoomValue);
                        allTextures.Add(output);
                        Rand.PopState();
                    }
                }
            }
            return allTextures;
        }
    }
}
