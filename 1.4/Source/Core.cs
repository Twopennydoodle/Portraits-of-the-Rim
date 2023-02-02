using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PortraitsOfTheRim
{
    [StaticConstructorOnStartup]
    public static class Core
    {
        public static Dictionary<Pawn, Portrait> pawnPortraits = new Dictionary<Pawn, Portrait>();
        public static List<PortraitLayerDef> layers;
        public static Dictionary<PortraitLayerDef, List<PortraitElementDef>> portraitElements;
        static Core()
        {
            layers = DefDatabase<PortraitLayerDef>.AllDefs.OrderBy(x => x.layer).ToList();
            portraitElements = new();
            foreach (var layerDef in layers)
            {
                var list = new List<PortraitElementDef>();
                foreach (var elementDef in DefDatabase<PortraitElementDef>.AllDefs)
                {
                    list.Add(elementDef);
                }
                portraitElements[layerDef] = list;
            }
        }
        public static Texture GetPortrait(this Pawn pawn)
        {
            if (!pawnPortraits.TryGetValue(pawn, out var portrait))
            {
                pawnPortraits[pawn] = portrait = new Portrait
                {
                    pawn = pawn,
                };
            }
            return portrait.PortraitTexture;
        }
    }
}
