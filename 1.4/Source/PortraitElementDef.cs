﻿using RimWorld;
using UnityEngine;
using Verse;

namespace PortraitsOfTheRim
{
    [HotSwappable]
    public class PortraitElementDef : Def
    {
        public GraphicData graphicData;
        public Graphic graphic;
        public Requirements requirements;
        public PortraitLayerDef portraitLayer;
        public PortraitElementDef connectedElement;
        public bool inheritsColor = true;
        public bool Matches(Pawn pawn)
        {
            var req = requirements ?? connectedElement?.requirements;
            if (req != null)
            {
                return req.Matches(pawn);
            }
            return true;
        }

        public Color? GetRecolor(Pawn pawn)
        {
            if (inheritsColor)
            {
                var newColor = requirements.GetColor(pawn);
                if (newColor != null)
                {
                    return newColor.Value;
                }
            }
            return null;
        }
        public override void PostLoad()
        {
            base.PostLoad();
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                if (graphicData.shaderType == null)
                {
                    graphicData.shaderType = ShaderTypeDefOf.Cutout;
                }
                if (graphicData.graphicClass is null)
                {
                    graphicData.graphicClass = typeof(Graphic_Single);
                };

                graphic = graphicData.Graphic;
                if (graphic == BaseContent.BadGraphic)
                {
                    graphic = null;
                    Log.Error("Error couldn't create graphic for " + this.defName);
                }
            });
        }
    }
}
