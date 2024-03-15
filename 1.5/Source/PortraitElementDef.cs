using RimWorld;
using UnityEngine;
using Verse;

namespace PortraitsOfTheRim
{
    public class PortraitElementDef : Def
    {
        public GraphicData graphicData;
        public Graphic_Single graphic;
        public Requirements requirements;
        public PortraitLayerDef portraitLayer;
        public PortraitElementDef connectedElement;
        public bool Matches(Portrait portrait)
        {
            var req = requirements ?? connectedElement?.requirements;
            if (req != null)
            {
                var boolReport = req.Matches(portrait, this);
                /*if (boolReport.result is false)
                {
                    Log.Message("Cannot get " + this + " for " + portrait.pawn + " because of " + boolReport.report);
                }
                else
                {
                    Log.Message("Can get " + this + " for " + portrait.pawn);
                }*/
                /*if (boolReport.result is true)
                {
                    Log.Message("Can get " + this + " for " + portrait.pawn);
                }*/
                //Log.ResetMessageCount();
                return boolReport.result;
            }
            return true;
        }

        public Color? GetRecolor(Pawn pawn)
        {
            var newColor = requirements.GetColor(pawn, this);
            if (newColor != null)
            {
                return newColor.Value;
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
                    graphicData.shaderType = ShaderTypeDefOf.CutoutComplex;
                }
                if (graphicData.graphicClass is null)
                {
                    graphicData.graphicClass = typeof(Graphic_Single);
                };

                graphic = (Graphic_Single) graphicData.Graphic;
                if (graphic == BaseContent.BadGraphic)
                {
                    graphic = null;
                    Log.Error("Error couldn't create graphic for " + this.defName);
                }
            });
        }
    }
}
