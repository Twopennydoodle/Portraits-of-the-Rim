using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PortraitsOfTheRim
{
    [HotSwappable]
    [StaticConstructorOnStartup]
    public class Portrait
    {
        private static readonly Texture2D OutlineTex = SolidColorMaterials.NewSolidColorTexture(new ColorInt(77, 77, 77).ToColor);
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
                GUI.DrawTexture(renderRect, texture);
            }
            Widgets.DrawBox(renderRect.ExpandedBy(1), 1, OutlineTex);
        }

        public static Dictionary<Pawn, Dictionary<PortraitElementDef, RenderTexture>> cachedRenderTextures = new();

        [TweakValue("0Portrait", 2f, 3f)] public static float zoomValue = 2.402f;
        [TweakValue("0Portrait", -0.2f, 0f)] public static float zOffset = -0.086f;
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
                        Rand.PopState();
                        var mainTexture = matchingElement.graphic.MatSingle.mainTexture;
                        if (!cachedRenderTextures.TryGetValue(pawn, out var dict))
                        {
                            cachedRenderTextures[pawn] = dict = new Dictionary<PortraitElementDef, RenderTexture>();
                        }
                        if (!dict.TryGetValue(matchingElement, out var renderTexture))
                        {
                            dict[matchingElement] = renderTexture = new RenderTexture(mainTexture.width, mainTexture.height, 0);
                        }
                        renderTexture.RenderElement(matchingElement, pawn, new Vector3(0, 0, zOffset), zoomValue);
                        allTextures.Add(renderTexture);
                    }
                }
            }
            return allTextures;
        }
    }
}
