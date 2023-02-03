using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PortraitsOfTheRim
{
    public enum PawnBodyType { Small, Medium, Large, ExtraLarge };
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

        public bool Matches(Pawn pawn)
        {
            if (gender != null && pawn.gender != gender.Value)
                return false;
            if (bodyParts != null && bodyParts.Exists(x => x.Matches(pawn)) is false)
                return false;
            if (hediffs != null && hediffs.Exists(x => pawn.health.hediffSet.hediffs.Exists(hediff => hediff.def == x)) is false)
                return false;
            if (head != null && pawn.story.headType != head)
                return false;
            if (genes != null && (pawn.genes is null || genes.Any(x => pawn.genes.GenesListForReading.Any(y => y.def == x && y.Active)) is false))
                return false;
            if (faceTattoo != null && pawn.style.FaceTattoo != faceTattoo)
                return false;
            if (bodyTattoo != null && pawn.style.BodyTattoo != bodyTattoo)
                return false;
            if (hair != null && pawn.story.hairDef != hair)
                return false;
            if (beard != null && pawn.style.beardDef != beard)
                return false;
            if (traits != null && pawn.story.traits.allTraits.Any(x => traits.Any(y => y.def == x.def && y.degree == x.degree)) is false)
                return false;
            if (body != null && pawn.story.bodyType != body.Value.ToBodyType(pawn))
                return false;
            if (apparels != null && pawn.apparel.WornApparel.Any(x => apparels.Any(y => x.def == y) is false))
                return false;
            if (ageRange != null && ageRange.Value.Includes(pawn.ageTracker.AgeBiologicalYearsFloat) is false)
                return false;
            if (bodyType != null && Matches(pawn, bodyType.Value) is false)
                return false;
            if (headType.NullOrEmpty() is false && pawn.story.headType.defName.ToLower().Contains(headType.ToLower()) is false)
                return false;
            return true;
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
                    if (pawn.story.bodyType == BodyTypeDefOf.Thin && Core.teenAge.Includes(pawn.ageTracker.AgeBiologicalYearsFloat))
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
                else if (pawn.gender == Gender.Male && Core.teenAge.Includes(pawn.ageTracker.AgeBiologicalYearsFloat))
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
