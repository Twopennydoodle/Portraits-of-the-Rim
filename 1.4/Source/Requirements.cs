using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PortraitsOfTheRim
{
    public class BoolReport
    {
        public bool result;
        public string report;
        public BoolReport(bool result, string report = null)
        {
            this.result = result;
            this.report = report;
        }
    }
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
        public BoolReport Matches(Portrait portrait, PortraitElementDef portraitElementDef)
        {
            var pawn = portrait.pawn;
            if (bodyParts.NullOrEmpty() is false && bodyParts.Exists(x => x.Matches(pawn, portraitElementDef)) is false)
                return new BoolReport(false, "bodyParts fail");
            if (hediffs.NullOrEmpty() is false && hediffs.Exists(x => pawn.health.hediffSet.hediffs.Exists(hediff => hediff.def == x)) is false)
                return new BoolReport(false, "hediffs fail");
            if (head != null && pawn.story.headType != head)
                return new BoolReport(false, "head fail");
            if (genes.NullOrEmpty() is false && (pawn.genes is null 
                || genes.Exists(x => pawn.genes.GenesListForReading.Exists(y => y.def == x && y.Active)) is false))
                return new BoolReport(false, "genes fail");
            if (faceTattoo != null && pawn.style.FaceTattoo != faceTattoo)
                return new BoolReport(false, "faceTattoo fail");
            if (bodyTattoo != null && pawn.style.BodyTattoo != bodyTattoo)
                return new BoolReport(false, "bodyTattoo fail");
            if (hair != null && pawn.story.hairDef != hair)
                return new BoolReport(false, "hair fail");
            if (beard != null && pawn.style.beardDef != beard)
                return new BoolReport(false, "beard fail");
            if (traits.NullOrEmpty() is false && pawn.story.traits.allTraits
                .Exists(x => traits.Exists(y => y.def == x.def && y.degree == x.degree)) is false)
                return new BoolReport(false, "traits fail");
            if (body != null && pawn.story.bodyType != body.Value.ToBodyType(pawn))
                return new BoolReport(false, "body fail");
            if (ageRange != null && ageRange.Value.Includes(pawn.ageTracker.AgeBiologicalYearsFloat) is false)
                return new BoolReport(false, "ageRange fail");
            if (bodyType != null && Matches(pawn, bodyType.Value, out var bodyTypeReport) is false)
                return new BoolReport(false, "bodyType fail: " + bodyTypeReport + " - gender: " + pawn.gender + " - body: " + pawn.story.bodyType + " - age: " + pawn.ageTracker.AgeBiologicalYearsFloat);
            if (headType.NullOrEmpty() is false && pawn.story.headType.defName.ToLower().Contains(headType.ToLower()) is false)
                return new BoolReport(false, "headType fail");
            if (xenotype != null)
            {
                if (xenotype != pawn.genes.xenotype)
                {
                    return new BoolReport(false, "xenotype fail");
                }
                else
                {
                    if (portraitElementDef.portraitLayer == PR_DefOf.PR_Ear)
                    {
                        var geneEars = pawn.genes.GenesListForReading.Where(x => x.Active && x.def.endogeneCategory == EndogeneCategory.Ears);
                        if (geneEars.Any())
                        {
                            return new BoolReport(false, "xenotype fail 2");
                        }
                    }
                }
            }
            if (gender != null && pawn.gender != gender.Value)
                return new BoolReport(false, "gender fail");
            if (apparels.NullOrEmpty() is false && pawn.apparel.WornApparel.Exists(x => apparels.Exists(y => x.def == y)) is false)
                return new BoolReport(false, "apparels fail");
            if (style.NullOrEmpty() is false && portrait.currentStyle.NullOrEmpty() is false && style != portrait.currentStyle)
                return new BoolReport(false, "style fail");
            return new BoolReport(true);
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
            if (elementDef.portraitLayer.inheritsSkinColor)
            {
                return pawn.story.SkinColor;
            }
            else if (elementDef.portraitLayer.inheritsHairColor)
            {
                return pawn.story.HairColor;
            }
            return null;
        }

        private bool Matches(Pawn pawn, PawnBodyType bodyType, out string failReport)
        {
            failReport = "";
            switch (bodyType)
            {
                case PawnBodyType.Small:
                    if (pawn.IsChild()) 
                        return true;
                    else
                    {
                        failReport = "Not Small";
                        return false;
                    }
                case PawnBodyType.Medium:
                    {
                        if (pawn.gender == Gender.Female && (pawn.story.bodyType == BodyTypeDefOf.Female || pawn.story.bodyType == BodyTypeDefOf.Thin))
                        {
                            return true;
                        }
                        else if (pawn.IsTeen() && pawn.gender == Gender.Male && pawn.MatchesAnyBodyType(BodyTypeDefOf.Thin))
                        {
                            return true;
                        }
                        failReport = "Not Medium";
                        return false;
                    }
                case PawnBodyType.Large:
                    {           
                        if (pawn.IsTeen())
                        {
                            if (pawn.gender == Gender.Female && pawn.story.bodyType == BodyTypeDefOf.Fat)
                            {
                                return true;
                            }
                            else if (pawn.gender == Gender.Male && pawn.story.bodyType == BodyTypeDefOf.Fat)
                            {
                                return true;
                            }
                        }
                        else if (pawn.IsAdult())
                        {
                            if (pawn.gender == Gender.Female && pawn.MatchesAnyBodyType(BodyTypeDefOf.Hulk, BodyTypeDefOf.Fat))
                            {
                                return true;
                            }
                            else if (pawn.gender == Gender.Male && pawn.MatchesAnyBodyType(BodyTypeDefOf.Male, BodyTypeDefOf.Hulk, BodyTypeDefOf.Thin))
                            {
                                return true;
                            }
                        }
                        failReport = "Not Large";
                        return false;
                    }
                case PawnBodyType.ExtraLarge:
                    {
                        if (pawn.IsAdult() && pawn.gender == Gender.Male && pawn.MatchesAnyBodyType(BodyTypeDefOf.Hulk, BodyTypeDefOf.Fat))
                        {
                            return true;
                        }
                        failReport = "Not ExtraLarge";
                        return false;
                    }
            }
            failReport = "Unhandled";
            return false;
        }
    }
}
