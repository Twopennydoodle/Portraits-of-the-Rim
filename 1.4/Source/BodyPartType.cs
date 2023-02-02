using System.Linq;
using Verse;

namespace PortraitsOfTheRim
{
    public enum Side { Left, Right }
    public class BodyPartType
    {
        public BodyPartDef bodyPart;
        public Side? side;
        public bool injured;
        public bool scarred;
        public bool destroyed;
        public bool Matches(Pawn pawn)
        {
            var bodyParts = pawn.def.race.body.AllParts.Where(x => Matches(x)).ToList();
            if (bodyParts.Any() is false)
                return false;
            var nonMissingBodyParts = pawn.health.hediffSet.GetNotMissingParts().Where(x => Matches(x)).ToList();
            if (destroyed && nonMissingBodyParts.Any())
                return false;
            var allHediffsWithPart = pawn.health.hediffSet.hediffs.Where(x => x.Part != null && Matches(x.Part)).ToList();
            if (scarred && allHediffsWithPart.Exists(x => x.IsPermanent()) is false)
                return false;
            if (injured && allHediffsWithPart.OfType<Hediff_Injury>().Any() is false)
                return false;
            return true;
        }

        public bool Matches(BodyPartRecord bodyPartRecord)
        {
            if (bodyPartRecord.def != bodyPart)
                return false;
            if (side != null)
            {
                if (side.Value == Side.Left && BodyPartHasTag(bodyPartRecord, "Left"))
                    return true;

                if (side.Value == Side.Right && BodyPartHasTag(bodyPartRecord, "Right"))
                    return true;

                return false;
            }
            return true;
        }

        private bool BodyPartHasTag(BodyPartRecord bodyPartRecord, string tag)
        {
            return bodyPartRecord.woundAnchorTag.Contains(tag) || ParentsHaveTag(bodyPartRecord, tag);
        }

        public bool ParentsHaveTag(BodyPartRecord bodyPartRecord, string tag)
        {
            if (bodyPartRecord.parent is null)
            {
                return false;
            }
            if (bodyPartRecord.parent.woundAnchorTag.Contains(tag))
            {
                return true;
            }
            return ParentsHaveTag(bodyPartRecord.parent, tag);
        }
    }
}
