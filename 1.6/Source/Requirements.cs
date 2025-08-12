using HarmonyLib;
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
        public string xenotype;
        public string style;
        public string potrRand; // Filters out the various goat horn and such types
        public bool track; // This extra property helps with filtering out debug logs. See PortraitElementDefs.Matches
        public BoolReport Matches(Portrait portrait, PortraitElementDef portraitElementDef)
        {
            var pawn = portrait.pawn;

            // Override pawn age if the pawn has the Ageless gene
            float pawnAge = pawn.ageTracker.AgeBiologicalYearsFloat;
            if (ageRange != null
                && pawn.ageTracker.AgeBiologicalYearsFloat > 38f
                && pawn.genes.GenesListForReading.Exists(g => g.def.defName == "Ageless")
                )
            {
                pawnAge = 21f;
            }
            else if (ageRange != null 
                && pawn.ageTracker.AgeBiologicalYearsFloat > 998f
                )
            {
                pawnAge = 998f;
            }

            List<string> failReports = new List<string>();
            if (bodyParts.NullOrEmpty() is false && bodyParts.Exists(delegate (BodyPartType x)
            {
                var result = x.Matches(pawn, portraitElementDef, out var bodyPartFailReport);
                if (!result)
                {
                    failReports.Add(bodyPartFailReport);
                }
                return result;
            }) is false)
                return new BoolReport(false, "bodyParts fail: " + string.Join(", ", failReports));
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
            if (ageRange != null && ageRange.Value.Includes(pawnAge) is false)
                return new BoolReport(false, "ageRange fail");
            if (bodyType != null && Matches(pawn, bodyType.Value, out var bodyTypeReport) is false)
                return new BoolReport(false, "bodyType fail: " + bodyTypeReport + " - gender: " + pawn.gender + " - body: " + pawn.story.bodyType + " - age: " + pawn.ageTracker.AgeBiologicalYearsFloat);
            if (headType.NullOrEmpty() is false && pawn.story.headType.defName.ToLower().Contains(headType.ToLower()) is false)
                return new BoolReport(false, "headType fail");
            if (xenotype.NullOrEmpty() is false)
            {
                if (ModsConfig.BiotechActive is false && xenotype == "Baseliner")
                {
                    return new BoolReport(true, "xenotype success 1");
                }
                if (xenotype != pawn.genes.xenotype.defName)
                {
                    if (portraitElementDef.portraitLayer == PR_DefOf.PR_Ear)
                    {
                        var geneEars = pawn.genes.GenesListForReading.Where(x => x.Active && x.def.endogeneCategory == EndogeneCategory.Ears);
                        if (geneEars.Any())
                        {
                            return new BoolReport(false, "xenotype fail 2");
                        }
                        return new BoolReport(true, "xenotype success 2");
                    }
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
            if (apparels.NullOrEmpty() is false)
            {
                var pawnApparels = GetApparels(pawn);
                var exists = pawnApparels.Exists(x => apparels.Exists(y => x.def == y));
                if (!exists)
                {
                    return new BoolReport(false, "apparels fail");
                }
            }
            if (style.NullOrEmpty() is false && portrait.currentStyle.NullOrEmpty() is false && style != portrait.currentStyle)
                return new BoolReport(false, "style fail");

            // Temporarily disabling Horns/Antlers/etc for Roo's until I can figure out graphics paths and such.
            if (potrRand.NullOrEmpty() is false)
            {
                // Determine if this def is dder ears, deer antlers, goat ears, or goat horns
                if (genes.Exists(g => g.defName == "RBSF_GoatHorns"))
                {
                    Gene hornGene = pawn.genes.GenesListForReading.Find(g => g.def.defName == "RBSF_GoatHorns");
                    PawnRenderNode head = pawn.drawer.renderer.renderTree.nodesByTag[PawnRenderNodeTagDefOf.Head];
                    foreach (PawnRenderNode node in head.children)
                    {
                        if (node.gene == hornGene)
                        {
                            string hornType = node.TexPathFor(pawn);
                            if (potrRand[0] != hornType[hornType.Length - 1])
                            {
                                return new BoolReport(false, "random type fail");
                            }
                        }
                    }
                }
                if (genes.Exists(g => g.defName == "RBSF_GoatEars"))
                {
                    Gene earGene = pawn.genes.GenesListForReading.Find(g => g.def.defName == "RBSF_GoatEars");
                    PawnRenderNode head = pawn.drawer.renderer.renderTree.nodesByTag[PawnRenderNodeTagDefOf.Head];
                    foreach (PawnRenderNode node in head.children)
                    {
                        if (node.gene == earGene)
                        {
                            string earType = node.TexPathFor(pawn);
                            if (potrRand[0] != earType[earType.Length - 1])
                            {
                                return new BoolReport(false, "random type fail");
                            }
                        }
                    }
                }
                if (genes.Exists(g => g.defName == "RBSF_DeerHorns"))
                {
                    Gene hornGene = pawn.genes.GenesListForReading.Find(g => g.def.defName == "RBSF_DeerHorns");
                    PawnRenderNode head = pawn.drawer.renderer.renderTree.nodesByTag[PawnRenderNodeTagDefOf.Head];
                    foreach (PawnRenderNode node in head.children)
                    {
                        if (node.gene == hornGene)
                        {
                            string hornType = node.TexPathFor(pawn);
                            if (potrRand[0] != hornType[hornType.Length - 1])
                            {
                                return new BoolReport(false, "random type fail");
                            }
                        }
                    }
                }
                if (genes.Exists(g => g.defName == "RBSF_DeerEars"))
                {
                    Gene earGene = pawn.genes.GenesListForReading.Find(g => g.def.defName == "RBSF_DeerEars");
                    PawnRenderNode head = pawn.drawer.renderer.renderTree.nodesByTag[PawnRenderNodeTagDefOf.Head];
                    foreach (PawnRenderNode node in head.children)
                    {
                        if (node.gene == earGene)
                        {
                            string earType = node.TexPathFor(pawn);
                            if (potrRand[0] != earType[earType.Length - 1])
                            {
                                return new BoolReport(false, "random type fail");
                            }
                        }
                    }
                }
                // More checks for genes - Roo's Minotaurs
                if (genes.Exists(g => g.defName == "RBM_BovineEars"))
                {
                    Gene earGene = pawn.genes.GenesListForReading.Find(g => g.def.defName == "RBM_BovineEars");
                    PawnRenderNode head = pawn.drawer.renderer.renderTree.nodesByTag[PawnRenderNodeTagDefOf.Head];
                    foreach (PawnRenderNode node in head.children)
                    {
                        if (node.gene == earGene)
                        {
                            string earType = node.TexPathFor(pawn);
                            if (potrRand[0] != earType[earType.Length - 1])
                            {
                                return new BoolReport(false, "random type fail");
                            }
                        }
                    }
                }
                if (genes.Exists(g => g.defName == "RBM_BovineHorns"))
                {
                    Gene hornGene = pawn.genes.GenesListForReading.Find(g => g.def.defName == "RBM_BovineHorns");
                    PawnRenderNode head = pawn.drawer.renderer.renderTree.nodesByTag[PawnRenderNodeTagDefOf.Head];
                    foreach (PawnRenderNode node in head.children)
                    {
                        if (node.gene == hornGene)
                        {
                            string hornType = node.TexPathFor(pawn);
                            if (potrRand[0] != hornType[hornType.Length - 1])
                            {
                                return new BoolReport(false, "random type fail");
                            }
                        }
                    }
                }
                // Minotaur Markings
                if (genes.Exists(g => g.defName == "RBM_BovineHead"))
                {
                    Gene headGene = pawn.genes.GenesListForReading.Find(g => g.def.defName == "RBM_BovineHead");
                    PawnRenderNode head = pawn.drawer.renderer.renderTree.nodesByTag[PawnRenderNodeTagDefOf.Head];
                    foreach (PawnRenderNode node in head.children)
                    {
                        if (node.gene == headGene)
                        {
                            string headType = node.TexPathFor(pawn);
                            if (potrRand[0] != headType[headType.Length - 1])
                            {
                                return new BoolReport(false, "random type fail");
                            }
                        }
                    }
                }
                // Facial Spots
                if (genes.Exists(g => g.defName == "VRE_FacialSpots"))
                {
                    Gene spotsGene = pawn.genes.GenesListForReading.Find(g => g.def.defName == "VRE_FacialSpots");
                    PawnRenderNode head = pawn.drawer.renderer.renderTree.nodesByTag[PawnRenderNodeTagDefOf.Head];
                    foreach (PawnRenderNode node in head.children)
                    {
                        if (node.gene == spotsGene)
                        {
                            string spotsType = node.TexPathFor(pawn);
                            if (potrRand[0] != spotsType[spotsType.Length - 1])
                            {
                                return new BoolReport(false, "random type fail");
                            }
                        }
                    }
                }
            }

            return new BoolReport(true);
        }

        // Used to extract specifically heads from a chosen fallback Head type option.
        public bool MatchFallbackHead(Portrait portrait, string fallbackHead)
        {
            var pawn = portrait.pawn;

            // Override pawn age if the pawn has the Ageless gene 
            float pawnAge = pawn.ageTracker.AgeBiologicalYearsFloat;
            if (ageRange != null
                && pawn.ageTracker.AgeBiologicalYearsFloat > 38f
                && pawn.genes.GenesListForReading.Exists(g => g.def.defName == "Ageless")
                )
            {
                pawnAge = 21f;
            }
            else if (ageRange != null
                && pawn.ageTracker.AgeBiologicalYearsFloat > 998f
                )
            {
                pawnAge = 998f;
            }
            // Re-resolve gender
            if (gender != null && pawn.gender != gender.Value)
                return false;
            // Re-resolve age
            if (ageRange != null && ageRange.Value.Includes(pawnAge) is false)
                return false;
            if (headType.NullOrEmpty() is false && fallbackHead.ToLower().Contains(headType.ToLower()) is false)
                return false;
            return true;
        }

        // Used to extract specifically torsos from the "standard" body type. 
        public bool MatchFallbackBody(Portrait portrait)
        {
            var pawn = portrait.pawn;

            // Override pawn age if the pawn has the Ageless gene
            float pawnAge = pawn.ageTracker.AgeBiologicalYearsFloat;
            if (ageRange != null
                && pawn.ageTracker.AgeBiologicalYearsFloat > 38f
                && pawn.genes.GenesListForReading.Exists(g => g.def.defName == "Ageless")
                )
            {
                pawnAge = 21f;
            }
            else if (ageRange != null
                && pawn.ageTracker.AgeBiologicalYearsFloat > 998f
                )
            {
                pawnAge = 998f;
            }
            // Re-resolve gender
            if (gender != null && pawn.gender != gender.Value)
                return false;
            // Re-resolve age
            if (ageRange != null && ageRange.Value.Includes(pawnAge) is false)
                return false;
            if (body != null && GeneticBodyType.Standard != body)
                return false;
            return true;
        }

        public List<Thing> GetApparels(Pawn pawn)
        {
            if (PortraitUtils.AppearanceClothesLoaded)
            {
                var comp = pawn.AllComps.FirstOrDefault(x => x.GetType().Name == "CompAppearanceClothes");
                if (comp != null)
                {
                    var traverse = Traverse.Create(comp);
                    if ((bool)traverse.Field("showAppearanceClothes").GetValue() == true)
                    {
                        var apparels = traverse.Field("appearanceClothes").GetValue() as List<Thing>;
                        if (apparels != null)
                        {
                            return apparels;
                        }
                    }
                }
            }
            return pawn.apparel.WornApparel.Cast<Thing>().ToList();
        }
        public Color? GetColor(Pawn pawn, PortraitElementDef elementDef)
        {
            if (this.apparels != null)
            {
                foreach (var def in apparels)
                {
                    var apparel = GetApparels(pawn).FirstOrDefault(x => x.def == def);
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
                if (elementDef.portraitLayer == PR_DefOf.PR_AccessoriesHair)
                {
                    return null;
                }
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
            if (elementDef.portraitLayer.inheritsHairColor)
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
                        if (pawn.IsAdult() && (pawn.story.bodyType == BodyTypeDefOf.Female || pawn.story.bodyType == BodyTypeDefOf.Thin))
                        {
                            return true;
                        }
                        failReport = "Not Medium";
                        return false;
                    }
                case PawnBodyType.Large:
                    {           
                        if (pawn.IsAdult() && pawn.story.bodyType == BodyTypeDefOf.Male)
                        {
                            return true;
                        }
                        failReport = "Not Large";
                        return false;
                    }
                case PawnBodyType.ExtraLarge:
                    {
                        if (pawn.IsAdult() && pawn.MatchesAnyBodyType(BodyTypeDefOf.Hulk, BodyTypeDefOf.Fat))
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
