using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PortraitsOfTheRim
{
    public enum PawnBodyType { Small, Medium, Large, ExtraLarge };
    [HotSwappable]
    public class Requirements
    {
        public Gender? gender;
        public List<BodyPartType> bodyParts;
        public List<HediffDef> hediffs;
        public HeadTypeDef head;
        public List<GeneDef> genes;
        public TattooDef faceTattoo;
        public TattooDef bodyTattoo;
        public HairDef hair;
        public BeardDef beard;
        public List<BackstoryTrait> traits;
        public GeneticBodyType? body;
        public List<ThingDef> apparels;
        public FloatRange? ageRange;
        public PawnBodyType? bodyType;
        public string headType;
        public XenotypeDef xenotype;
        public string style;
        public bool Matches(Portrait portrait, PortraitElementDef portraitElementDef, bool logging = false)
        {
            var pawn = portrait.pawn;
            if (bodyParts.NullOrEmpty() is false && bodyParts.Exists(x => x.Matches(pawn)) is false)
                return false;
            if (hediffs.NullOrEmpty() is false && hediffs.Exists(x => pawn.health.hediffSet.hediffs.Exists(hediff => hediff.def == x)) is false)
                return false;
            if (head != null && pawn.story.headType != head)
                return false;
            if (genes.NullOrEmpty() is false && (pawn.genes is null 
                || genes.Exists(x => pawn.genes.GenesListForReading.Exists(y => y.def == x && y.Active)) is false))
                return false;
            if (faceTattoo != null && pawn.style.FaceTattoo != faceTattoo)
                return false;
            if (bodyTattoo != null && pawn.style.BodyTattoo != bodyTattoo)
                return false;
            if (hair != null && pawn.story.hairDef != hair)
                return false;
            if (beard != null && pawn.style.beardDef != beard)
                return false;
            if (traits.NullOrEmpty() is false && pawn.story.traits.allTraits
                .Exists(x => traits.Exists(y => y.def == x.def && y.degree == x.degree)) is false)
                return false;
            if (body != null && pawn.story.bodyType != body.Value.ToBodyType(pawn))
                return false;
            if (ageRange != null && ageRange.Value.Includes(pawn.ageTracker.AgeBiologicalYearsFloat) is false)
                return false;
            if (bodyType != null && Matches(pawn, bodyType.Value) is false)
            {
                return false;
            }
            if (headType.NullOrEmpty() is false && pawn.story.headType.defName.ToLower().Contains(headType.ToLower()) is false)
                return false;
            if (xenotype != null && xenotype != pawn.genes.xenotype)
                return false;
            if (gender != null && pawn.gender != gender.Value)
            {
                return false;
            }

            if (apparels.NullOrEmpty() is false)
            {
                if (pawn.apparel.WornApparel.Exists(x => apparels.Exists(y => x.def == y)) is false)
                {
                    return false;
                }
            }

            if (style.NullOrEmpty() is false && portrait.currentStyle.NullOrEmpty() is false && style != portrait.currentStyle)
                return false;
            return true;
        }

        public Color? GetColor(Pawn pawn, PortraitElementDef elementDef)
        {
            if (this.apparels != null)
            {
                foreach (var def in apparels)
                {
                    var apparel = pawn.apparel.WornApparel.FirstOrDefault(x => x.def == def);
                    if (apparel != null)
                    {
                        return apparel.DrawColor;
                    }
                }
            }
            if (headType.NullOrEmpty() is false || head != null)
            {
                return pawn.story.SkinColor;
            }
            if (hair != null)
            {
                return pawn.story.HairColor;
            }
            if (this.body != null || this.bodyType != null)
            {
                return pawn.story.SkinColor;
            }
            if (elementDef.portraitLayer == PR_DefOf.PR_Ear)
            {
                return pawn.story.SkinColor;
            }
            return null;
        }

        private bool Matches(Pawn pawn, PawnBodyType bodyType)
        {
            if (bodyType == PawnBodyType.Small)
            {
                if (pawn.DevelopmentalStage == DevelopmentalStage.Child)
                {
                    return true;
                }
            }
            if (bodyType == PawnBodyType.Medium)
            {
                if (pawn.gender == Gender.Female)
                {
                    if (pawn.story.bodyType == BodyTypeDefOf.Female || pawn.story.bodyType == BodyTypeDefOf.Thin)
                    {
                        return true;
                    }
                }
                else if (pawn.gender == Gender.Male)
                {
                    if (pawn.story.bodyType == BodyTypeDefOf.Thin && TextureParser.teenAge.Includes(pawn.ageTracker.AgeBiologicalYearsFloat))
                    {
                        return true;
                    }
                }
            }
            if (bodyType == PawnBodyType.Large)
            {
                if (pawn.gender == Gender.Female && pawn.story.bodyType == BodyTypeDefOf.Hulk)
                {
                    return true;
                }
                else if (pawn.gender == Gender.Male && TextureParser.teenAge.Includes(pawn.ageTracker.AgeBiologicalYearsFloat))
                {
                    if (pawn.story.bodyType == BodyTypeDefOf.Male || pawn.story.bodyType == BodyTypeDefOf.Hulk)
                    {
                        return true;
                    }
                }
            }
            if (bodyType == PawnBodyType.ExtraLarge)
            {
                if (pawn.gender == Gender.Male && pawn.story.bodyType == BodyTypeDefOf.Hulk)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
