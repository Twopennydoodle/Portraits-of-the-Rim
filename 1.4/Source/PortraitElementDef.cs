using RimWorld;
using Verse;

namespace PortraitsOfTheRim
{
    public class PortraitElementDef : Def
    {
        public GraphicData graphicData;
        public Graphic graphic;
        public Requirements requirements;
        public PortraitLayerDef portraitLayer;
        public PortraitElementDef connectedElement;
        public bool inheritsColor;

        public bool Matches(Pawn pawn)
        {
            var req = requirements ?? connectedElement?.requirements;
            if (req != null)
            {
                return req.Matches(pawn);
            }
            return true;
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
                graphic = graphicData.Graphic;
                if (graphic == BaseContent.BadGraphic)
                {
                    graphic = null;
                }
            });
        }
    }
}
