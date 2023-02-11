using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PortraitsOfTheRim
{
    [HotSwappable]
    [StaticConstructorOnStartup]
    public class Portrait : IExposable
    {
        private static readonly Texture2D OutlineTex = SolidColorMaterials.NewSolidColorTexture(new ColorInt(77, 77, 77).ToColor);
        public Pawn pawn;
        private List<(PortraitElementDef, Texture)> portraitTextures;
        private int lastCreatingTime;

        public bool hidePortrait;
        public bool hideHeadgear;
        public string currentStyle;

        private List<(PortraitElementDef, Texture)> PortraitTextures
        {
            get
            {
                if (portraitTextures is null || Time.frameCount % 60 == 0 && lastCreatingTime != Time.frameCount)
                {
                    lastCreatingTime = Time.frameCount;
                    portraitTextures = GetPortraitTextures();
                    var missingLayers = PortraitUtils.layers.Where(x => portraitTextures.Any(y => y.Item1.portraitLayer == x) is false);
                    var existingLayers = PortraitUtils.layers.Where(x => portraitTextures.Any(y => y.Item1.portraitLayer == x));
                    if (!missingLayers.Contains(PR_DefOf.PR_InnerFace))
                    {
                        Log.Message("MissingLayers: " + string.Join(", ", missingLayers));
                    }
                    Log.Message("Existing layers: " + string.Join(", ", existingLayers));
                    Log.Message("Drawn textures: " + string.Join(", ", portraitTextures.Select(x => x.Item2)));
                }
                return portraitTextures;
            }
        }

        public bool ShouldShow => hidePortrait is false;
        public bool HasImportantLayers
        {
            get
            {
                var textures = PortraitTextures;
                var missingLayers = PortraitUtils.layers.Where(x => textures.Any(y => y.Item1.portraitLayer == x) is false);
                if (!missingLayers.Contains(PR_DefOf.PR_InnerFace))
                {
                    return true;
                }
                return false;
            }
        }
        public void RenderPortrait(float x, float y, float width, float height)
        {
            var textures = PortraitTextures;
            var renderRect = new Rect(x, y, width, height);
            foreach (var texture in textures)
            {
                if (this.hideHeadgear && PortraitUtils.HeadgearLayers.Contains(texture.Item1.portraitLayer))
                    continue;
                GUI.DrawTexture(renderRect, texture.Item2);
            }
            Widgets.DrawBox(renderRect.ExpandedBy(1), 1, OutlineTex);
        }

        public void DrawButtons(float x, float y)
        {
            var hidePortraitRect = new Rect(x, y, 24, 24);
            TooltipHandler.TipRegion(hidePortraitRect, this.hidePortrait ? "PR.ShowPortrait".Translate() : "PR.HidePortrait".Translate());
            if (Widgets.ButtonImage(hidePortraitRect, this.hidePortrait ? TexButton.OpenInspector : TexButton.CloseXBig))
            {
                this.hidePortrait = !this.hidePortrait;
            }
            var hideHeadgear = new Rect(hidePortraitRect.x, hidePortraitRect.yMax + 5, 24, 24);
            TooltipHandler.TipRegion(hideHeadgear, this.hideHeadgear ? "PR.ShowHeadgear".Translate() : "PR.HideHeadgear".Translate());
            if (Widgets.ButtonImage(hideHeadgear, this.hideHeadgear ? TexButton.Add : TexButton.Minus))
            {
                this.hideHeadgear = !this.hideHeadgear;
            }

            var selectStyle = new Rect(hidePortraitRect.x, hideHeadgear.yMax + 5, 24, 24);
            TooltipHandler.TipRegion(selectStyle, "PR.SelectStyle".Translate());
            if (Widgets.ButtonImage(selectStyle, TexButton.Banish))
            {
                var floatList = new List<FloatMenuOption>();
                foreach (var style in PortraitUtils.allStyles)
                {
                    floatList.Add(new FloatMenuOption(style.CapitalizeFirst(), delegate
                    {
                        this.currentStyle = style;
                    }));
                }
                floatList.Add(new FloatMenuOption("None".Translate(), delegate
                {
                    this.currentStyle = null;
                }));
                Find.WindowStack.Add(new FloatMenu(floatList));
            }
        }

        public static Dictionary<Pawn, Dictionary<PortraitElementDef, RenderTexture>> cachedRenderTextures = new();

        public static float zoomValue = 2.402f;
        public static float zOffset = -0.086f;
        public List<(PortraitElementDef, Texture)> GetPortraitTextures()
        {
            List<(PortraitElementDef, Texture)> allTextures = new ();    
            foreach (var layer in PortraitUtils.layers)
            {
                if (PortraitUtils.portraitElements.TryGetValue(layer, out var elements))
                {
                    var matchingElements = elements.Where(x => x.Matches(this)).ToList();
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
                        allTextures.Add((matchingElement, renderTexture));
                    }
                }
            }
            return allTextures;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref hidePortrait, "hidePortrait");
            Scribe_Values.Look(ref hideHeadgear, "hideHeadgear");
            Scribe_Values.Look(ref currentStyle, "currentStyle");
        }
    }
}
