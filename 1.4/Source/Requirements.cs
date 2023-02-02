using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PortraitsOfTheRim
{
    public class Requirements
    {
        public Gender? gender;
        public List<BodyPartType> bodyParts;
        public List<HediffDef> hediffs;
        public HeadTypeDef head;
        public List<GeneDef> genes;
        public TattooDef faceTattoo;
        public HairDef hair;
        public BeardDef beard;
        public List<BackstoryTrait> traits;
        public BodyTypeDef body;
        public List<ThingDef> apparels;
        public LifeStageDef lifeStage;
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
            if (hair != null && pawn.story.hairDef != hair)
                return false;
            if (beard != null && pawn.style.beardDef != beard)
                return false;
            if (traits != null && pawn.story.traits.allTraits.Any(x => traits.Any(y => y.def == x.def && y.degree == x.degree)) is false)
                return false;
            if (body != null && pawn.story.bodyType != body)
                return false;
            if (apparels != null && pawn.apparel.WornApparel.Any(x => apparels.Any(y => x.def == y) is false))
                return false;
            if (lifeStage != null && pawn.ageTracker.CurLifeStage != lifeStage)
                return false;
            return true;
        }
    }
}
