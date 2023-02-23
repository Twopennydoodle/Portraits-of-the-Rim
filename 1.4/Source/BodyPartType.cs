using System.Linq;
using Verse;

namespace PortraitsOfTheRim
{
    public enum Side { Left, Right }
    [HotSwappable]
    public class BodyPartType
    {
        public HediffDef hediffInjury;
        public BodyPartDef bodyPart;
        public Side? side;
        public bool injured;
        public bool scarred;
        public bool destroyed;
        public bool bandaged;
        public bool Matches(Pawn pawn, PortraitElementDef def, out string failReport)
        {
            failReport = bodyPart + " - ";
            var bodyParts = pawn.def.race.body.AllParts.Where(x => Matches(x, def)).ToList();
            if (bodyParts.Any() is false)
            {
                failReport = "No pawn matching parts";
                return false;
            }
            var nonMissingBodyParts = pawn.health.hediffSet.GetNotMissingParts().Where(x => Matches(x, def)).ToList();
            if (destroyed && nonMissingBodyParts.Any())
            {
                failReport = "No pawn matching destroyed parts";
                return false;
            }
            else if (!destroyed && nonMissingBodyParts.Any() is false)
            {
                failReport = "No pawn matching injured parts";
                return false;
            }
            var allHediffsWithPart = pawn.health.hediffSet.hediffs.Where(x => x.Part != null && Matches(x.Part, def)).ToList();
            if (hediffInjury != null && allHediffsWithPart.Exists(x => x.def == hediffInjury) is false)
            {
                failReport = "No pawn matching injured parts to hediff: " + hediffInjury;
                return false;
            }
            if (destroyed || scarred || injured || bandaged)
            {
                if (!PortraitsOfTheRimSettings.showBandagesInsteadOfInjuries)
                {
                    if (scarred && allHediffsWithPart.Exists(x => x.IsPermanent()) is false)
                    {
                        return false;
                    }
                    if (injured && allHediffsWithPart.OfType<Hediff_Injury>().Any() is false)
                    {
                        return false;
                    }
                    if (bandaged && allHediffsWithPart.OfType<Hediff_Injury>().Any(x => x.IsTended()) is false)
                    {
                        return false;
                    }
                    return true;
                }
                else
                {
                    if (!bandaged)
                    {
                        failReport = "No bandage";
                        return false;
                    }
                    if (allHediffsWithPart.Exists(x => x.IsPermanent()) is false)
                    {
                        failReport = "No permanent hediff injuries: " + hediffInjury;
                        return false;
                    }
                    if (allHediffsWithPart.OfType<Hediff_Injury>().Any() is false)
                    {
                        failReport = "No hediff injuries: " + hediffInjury;
                        return false;
                    }
                    return true;
                }
            }

            return true;
        }

        public bool Matches(BodyPartRecord bodyPartRecord, PortraitElementDef portraitElementDef)
        {
            if (bodyPartRecord.def != bodyPart)
                return false;
            if (side != null)
            {
                if (side.Value == Side.Left && BodyPartHasTag(bodyPartRecord, "Left"))
                {
                    return true;
                }
                if (side.Value == Side.Right && BodyPartHasTag(bodyPartRecord, "Right"))
                {
                    return true;
                }
                if (bodyPartRecord.parent.parts != null)
                {
                    var allSameParts = bodyPartRecord.parent.parts.Where(x => x.def == bodyPartRecord.def);
                    if (allSameParts.Count() == 2)
                    {
                        if (side.Value == Side.Left && allSameParts.First() == bodyPartRecord)
                        {
                            return true;
                        }
                        else if (side.Value == Side.Right && allSameParts.Last() == bodyPartRecord)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            return true;
        }

        public static bool BodyPartHasTag(BodyPartRecord bodyPartRecord, string tag)
        {
            return bodyPartRecord.woundAnchorTag != null && bodyPartRecord.woundAnchorTag.ToLower().Contains(tag.ToLower()) 
                || bodyPartRecord.parent != null && BodyPartHasTag(bodyPartRecord.parent, tag);
        }
    }
}
