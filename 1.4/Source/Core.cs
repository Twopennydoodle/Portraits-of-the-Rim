using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace PortraitsOfTheRim
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }

    [HotSwappable]
    [StaticConstructorOnStartup]
    public static class Core
    {
        public static Dictionary<Pawn, Portrait> pawnPortraits = new Dictionary<Pawn, Portrait>();
        public static List<PortraitLayerDef> layers;
        public static Dictionary<PortraitLayerDef, List<PortraitElementDef>> portraitElements;

        [DebugAction("Portraits Of The Rim", "Create PortraitElementDefs", allowedGameStates = AllowedGameStates.Entry)]
        public static void CreatePortraitElementDefs()
        {
            List<DebugMenuOption> outerList = new List<DebugMenuOption>();
            foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading)
            {
                if (mod.textures?.contentList != null)
                {
                    outerList.Add(new DebugMenuOption(mod.Name, DebugMenuOptionMode.Action, delegate
                    {
                        var innerList = new List<DebugMenuOption>();
                        foreach (var folderName in mod.foldersToLoadDescendingOrder)
                        {
                            DirectoryInfo baseDirectory = new DirectoryInfo(Path.Combine(folderName, GenFilePaths.ContentPath<Texture2D>()));
                            if (baseDirectory.Exists)
                            {
                                innerList.Add(new DebugMenuOption(baseDirectory.FullName.Replace(mod.RootDir, ""), 
                                    DebugMenuOptionMode.Action, delegate
                                {
                                    OutputDefs(mod, baseDirectory, baseDirectory);
                                }));
                                var directories = Directory.GetDirectories(baseDirectory.FullName);
                                foreach (var directory in directories)
                                {
                                    innerList.Add(new DebugMenuOption(directory.Replace(mod.RootDir, ""), DebugMenuOptionMode.Action, delegate
                                    {
                                        OutputDefs(mod, baseDirectory, new DirectoryInfo(directory));
                                    }));
                                }
                            }
                        }
                        Find.WindowStack.Add(new Dialog_DebugOptionListLister(innerList));
                    }));
                }
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(outerList));
        }


        public const string Large = "l";
        public const string Small = "s";
        public const string Medium = "m";
        public const string ExtraLarge = "xl";
        public const string AdultAllGender = "an";
        public const string ChildAllGender = "cn";
        public const string ChildFemale = "cf";
        public const string ChildMale = "cm";
        public const string TeenFemale = "tf";
        public const string TeenMale = "tm";
        public const string YoungFemale = "yf";
        public const string YoungMale = "ym";
        public const string ElderFemale = "ef";
        public const string ElderMale = "em";
        public const string MiddleAgedFemale = "mf";
        public const string MiddleAgedMale = "mm";
        public const string AdultFemale = "af";
        public const string AdultMale = "am";

        public static FloatRange childAge = new FloatRange(7f, 13f);
        public static FloatRange teenAge = new FloatRange(13f, 19f);
        public static FloatRange youngAdultAge = new FloatRange(19f, 39f);
        public static FloatRange middleAged = new FloatRange(39f, 64f);
        public static FloatRange elderAge = new FloatRange(64, 999f);
        public static FloatRange totalChildAge = new FloatRange(7f, 19);
        public static FloatRange totalAdultAge = new FloatRange(19f, 999);

        public static Dictionary<string, int> suffices = new Dictionary<string, int>();

        private static Requirements CreateRequirements(string layer, List<string> data, out bool errored)
        {
            errored = false;
            var req = new Requirements();
            for (var index = 0; index < data.Count; index++)
            {
                var suffix = data[index];
                try
                {
                    switch (suffix)
                    {
                        case AdultAllGender: req.ageRange = totalAdultAge; continue;
                        case ChildAllGender: req.ageRange = totalChildAge; continue;
                        case Large: req.bodyType = PawnBodyType.Large; continue;
                        case Small: req.bodyType = PawnBodyType.Small; continue;
                        case Medium: req.bodyType = PawnBodyType.Medium; continue;
                        case ExtraLarge: req.bodyType = PawnBodyType.ExtraLarge; continue;
                        case ChildFemale:
                            req.gender = Gender.Female;
                            req.ageRange = childAge;
                            continue;
                        case ChildMale:
                            req.gender = Gender.Male;
                            req.ageRange = childAge;
                            continue;
                        case TeenFemale:
                            req.gender = Gender.Female;
                            req.ageRange = teenAge;
                            continue;
                        case TeenMale:
                            req.gender = Gender.Male;
                            req.ageRange = teenAge;
                            continue;
                        case YoungFemale:
                            req.gender = Gender.Female;
                            req.ageRange = youngAdultAge;
                            continue;
                        case YoungMale:
                            req.gender = Gender.Male;
                            req.ageRange = youngAdultAge;
                            continue;
                        case ElderFemale:
                            req.gender = Gender.Female;
                            req.ageRange = elderAge;
                            continue;
                        case ElderMale:
                            req.gender = Gender.Male;
                            req.ageRange = elderAge;
                            continue;
                        case MiddleAgedFemale:
                            req.gender = Gender.Female;
                            req.ageRange = middleAged;
                            continue;
                        case MiddleAgedMale:
                            req.gender = Gender.Male;
                            req.ageRange = middleAged;
                            continue;
                        case AdultFemale:
                            req.gender = Gender.Female;
                            req.ageRange = totalAdultAge;
                            continue;
                        case AdultMale:
                            req.gender = Gender.Male;
                            req.ageRange = totalAdultAge;
                            continue;
                        case "wshoulder": AddBodyPart(req, "Shoulder", Side.Left); continue;
                        case "eshoulder": AddBodyPart(req, "Shoulder", Side.Right); continue;
                        case "wclavicle": AddBodyPart(req, "Clavicle", Side.Left); continue;
                        case "eclavicle": AddBodyPart(req, "Clavicle", Side.Right); continue;
                        case "torso": AddBodyPart(req, "Torso"); continue;
                        case "weye": AddBodyPart(req, "Eye", Side.Left); continue;
                        case "eeye": AddBodyPart(req, "Eye", Side.Right); continue;
                        case "neck": AddBodyPart(req, "Neck"); continue;
                        case "jaw": AddBodyPart(req, "Jaw"); continue;
                        case "nose": AddBodyPart(req, "Nose"); continue;
                        case "wear": AddBodyPart(req, "Ear", Side.Left); continue;
                        case "eear": AddBodyPart(req, "Ear", Side.Right); continue;
                        case "scar": req.bodyParts.First().scarred = true; continue;
                        case "missing": req.bodyParts.First().destroyed = true; continue;
                        case "bandage": req.bodyParts.First().bandaged = true; continue;
                        default:
                            try
                            {
                                if (TryToResolveSuffix(layer, req, index, ref suffix) is false)
                                {
                                    errored = true;
                                    RegisterUnknownSuffix(layer, index, suffix);
                                }
                            }
                            catch (Exception e)
                            {
                                errored = true;
                                Log.Error("Failed to parse suffix: " + suffix + " - index: " + index + " - " + string.Join(", ", data) + " - " + e.ToString());
                                RegisterUnknownSuffix(layer, index, suffix);    
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    Log.Error("2 Failed to parse suffix: " + suffix + " - index: " + index + " - " + string.Join(", ", data) + " - " + e.ToString());
                    errored = true;
                }
            }
            return req;
        }

        private static bool TryToResolveSuffix(string layer, Requirements req, int index, ref string suffix)
        {
            if (layer.Contains("Clothing") || layer.Contains("Headgear"))
            {
                foreach (var def in DefDatabase<ThingDef>.AllDefs)
                {
                    if (def.IsApparel)
                    {
                        if (SuffixMatches(def.label, suffix))
                        {
                            req.apparels ??= new List<ThingDef>();
                            req.apparels.Add(def);
                            return true;
                        }
                    }
                }
            }

            if (layer.Contains("Hair"))
            {
                foreach (var def in DefDatabase<HairDef>.AllDefs)
                {
                    if (SuffixMatches(def.label, suffix))
                    {
                        req.hair = def;
                        return true;
                    }
                }
            }
            if (layer == "PR_Beard")
            {
                foreach (var def in DefDatabase<BeardDef>.AllDefs)
                {
                    if (SuffixMatches(def.label, suffix))
                    {
                        req.beard = def;
                        return true;
                    }
                }
            }

            if (layer == "PR_TattooHead")
            {
                foreach (var def in DefDatabase<TattooDef>.AllDefs)
                {
                    if (def.tattooType == TattooType.Face && SuffixMatches(def.label, suffix.ToString()))
                    {
                        req.faceTattoo = def;
                        return true;
                    }
                }
            }

            if (layer == "PR_TattooNeck")
            {
                foreach (var def in DefDatabase<TattooDef>.AllDefs)
                {
                    if (def.tattooType == TattooType.Body && SuffixMatches(def.label, suffix))
                    {
                        req.bodyTattoo = def;
                        return true;
                    }
                }
            }

            if (layer == "PR_Head" && index == 1)
            {
                req.headType = suffix;
                return true;
            }
            if (layer == "PR_OuterFace" || layer == "PR_InnerFace")
            {
                if (index == 1)
                {
                    foreach (var traitDef in DefDatabase<TraitDef>.AllDefs)
                    {
                        if (SuffixMatches(traitDef.defName, suffix))
                        {
                            req.traits ??= new List<BackstoryTrait>();
                            req.traits.Add(new BackstoryTrait { def = traitDef, degree = traitDef.degreeDatas[0].degree });
                            return true;
                        }
                    }

                    foreach (var def in DefDatabase<TraitDef>.AllDefs)
                    {
                        foreach (var degreeData in def.degreeDatas)
                        {
                            if (SuffixMatches(degreeData.label, suffix))
                            {
                                req.traits ??= new List<BackstoryTrait>();
                                req.traits.Add(new BackstoryTrait { def = def, degree = degreeData.degree });
                                return true;
                            }
                        }
                    }
                }
            }
            if (layer == "PR_OuterHediffNeck" || layer == "PR_MiddleHediffHead")
            {
                if (index == 2)
                {
                    if (suffix == "shred")
                        suffix = "shredded";
                    else if (suffix == "chemburn")
                        suffix = "chemicalburn";
                    foreach (var hediffDef in DefDatabase<HediffDef>.AllDefs)
                    {
                        if (SuffixMatches(hediffDef.defName, suffix))
                        {
                            req.bodyParts.First().hediffInjury = hediffDef;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool SuffixMatches(string label, string suffix)
        {
            return UnifyString(label) == UnifyString(suffix);
        }

        private static string UnifyString(string label)
        {
            return label.ToLower().Replace(" ", "").Replace("-", "");
        }

        private static void RegisterUnknownSuffix(string layer, int index, string suffix)
        {
            var key = layer + " - " + suffix + " (index " + index + ")";
            if (suffices.ContainsKey(key))
            {
                suffices[key] += 1;
            }
            else
            {
                suffices[key] = 1;
            }
        }

        private static void AddBodyPart(Requirements req, string partDefName, Side? side = null)
        {
            req.bodyParts ??= new List<BodyPartType>();
            var bodyPartType = new BodyPartType();
            bodyPartType.side = side;
            bodyPartType.bodyPart = DefDatabase<BodyPartDef>.GetNamed(partDefName);
            req.bodyParts.Add(bodyPartType);
        }

        public static void OutputDefs(ModContentPack mod, DirectoryInfo baseDirectory, DirectoryInfo directoryInfo)
        {
            Log.Message(baseDirectory + " - " + directoryInfo);
            var files = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);
            Dictionary<string, List<string>> erroredXML = new ();
            Dictionary<string, List<string>> resolvedXML = new ();
            foreach (var file in files)
            {
                var folders = file.FullName.Replace(baseDirectory.FullName, "").Split(Path.DirectorySeparatorChar);
                var layer = folders[0];
                var source = new Regex("(.*?)_").Match(folders[1]).Groups[1].Value.ToLower();
                if (source.NullOrEmpty())
                {
                    source = "NoSource";
                }
                var filename = folders.Last().Replace(".png", "");
                var defName = "PR_" + filename;
                if (DefDatabase<PortraitElementDef>.GetNamedSilentFail(defName) is null)
                {
                    var layerDef = DefDatabase<PortraitLayerDef>.GetNamed("PR_" + layer);
                    if (layerDef is null)
                    {
                        Log.Error("Layer def not found: " + layer);
                    }
                    else
                    {
                        var sb = new StringBuilder("\t<PortraitsOfTheRim.PortraitElementDef>\n");
                        sb.AppendLine("\t\t" + "<defName>" + defName + "</defName>");
                        sb.AppendLine("\t\t" + "<portraitLayer>" + layerDef.defName + "</portraitLayer>");
                        sb.AppendLine("\t\t" + "<graphicData>");
                        sb.AppendLine("\t\t\t" + "<texPath>" + file.FullName.Replace(baseDirectory.FullName, "").Replace("\\", "/").Replace(".png", "") + "</texPath>");
                        sb.AppendLine("\t\t" + "</graphicData>");
                        sb.AppendLine("\t\t" + "<requirements>");
                        var data = filename.Split('_').Skip(2).ToList();
                        Requirements req = CreateRequirements(layerDef.defName, data, out var errored);
                        if (req.ageRange != null)
                        {
                            sb.AppendLine("\t\t\t" + "<ageRange>" + req.ageRange.Value.min + "~" + req.ageRange.Value.max + "</ageRange>");
                        }
                        if (req.gender != null)
                        {
                            sb.AppendLine("\t\t\t" + "<gender>" + req.gender.ToString() + "</gender>");
                        }
                        if (req.bodyType != null)
                        {
                            sb.AppendLine("\t\t\t" + "<bodyType>" + req.bodyType.ToString() + "</bodyType>");
                        }
                        if (req.headType.NullOrEmpty() is false)
                        {
                            sb.AppendLine("\t\t\t" + "<headType>" + req.headType.ToString() + "</headType>");
                        }
                        if (req.traits != null)
                        {
                            sb.AppendLine("\t\t\t" + "<traits>");
                            foreach (var trait in req.traits)
                            {
                                sb.AppendLine("\t\t\t\t" + "<" + trait.def.defName + ">" + trait.degree + "</" + trait.def.defName + ">" );
                            }
                            sb.AppendLine("\t\t\t" + "</traits>");
                        }
                        if (req.bodyParts != null)
                        {
                            sb.AppendLine("\t\t\t" + "<bodyParts>");
                            foreach (var part in req.bodyParts)
                            {
                                sb.AppendLine("\t\t\t\t" + "<li>");
                                if (part.hediffInjury != null)
                                {
                                    sb.AppendLine("\t\t\t\t\t" + "<hediffInjury>" + part.hediffInjury + "</hediffInjury>");
                                }
                                sb.AppendLine("\t\t\t\t\t" + "<bodyPart>" + part.bodyPart + "</bodyPart>");
                                if (part.side != null)
                                {
                                    sb.AppendLine("\t\t\t\t\t" + "<side>" + part.side + "</side>");
                                }
                                if (part.injured)
                                {
                                    sb.AppendLine("\t\t\t\t\t" + "<injured>" + part.injured + "</injured>");
                                }
                                if (part.bandaged)
                                {
                                    sb.AppendLine("\t\t\t\t\t" + "<bandaged>" + part.bandaged + "</bandaged>");
                                }
                                if (part.scarred)
                                {
                                    sb.AppendLine("\t\t\t\t\t" + "<scarred>" + part.scarred + "</scarred>");
                                }
                                if (part.destroyed)
                                {
                                    sb.AppendLine("\t\t\t\t\t" + "<destroyed>" + part.destroyed + "</destroyed>");
                                }
                                sb.AppendLine("\t\t\t\t" + "</li>");
                            }
                            sb.AppendLine("\t\t\t" + "</bodyParts>");
                        }
                        if (req.hediffs != null)
                        {
                            sb.AppendLine("\t\t\t" + "<hediffs>");
                            foreach (var hediff in req.hediffs)
                            {
                                sb.AppendLine("\t\t\t\t" + "<li>" + hediff.defName + "</li>");
                            }
                            sb.AppendLine("\t\t\t" + "</hediffs>");
                        }
                        if (req.genes != null)
                        {
                            sb.AppendLine("\t\t\t" + "<genes>");
                            foreach (var gene in req.genes)
                            {
                                sb.AppendLine("\t\t\t\t" + "<li>" + gene.defName + "</li>");
                            }
                            sb.AppendLine("\t\t\t" + "</genes>");
                        }
                        if (req.apparels != null)
                        {
                            sb.AppendLine("\t\t\t" + "<apparels>");
                            foreach (var apparel in req.apparels)
                            {
                                sb.AppendLine("\t\t\t\t" + "<li>" + apparel.defName + "</li>");
                            }
                            sb.AppendLine("\t\t\t" + "</apparels>");
                        }
                        if (req.head != null)
                        {
                            sb.AppendLine("\t\t\t" + "<head>" + req.head.defName + "</head>");
                        }
                        if (req.faceTattoo != null)
                        {
                            sb.AppendLine("\t\t\t" + "<faceTattoo>" + req.faceTattoo.defName + "</faceTattoo>");
                        }
                        if (req.bodyTattoo != null)
                        {
                            sb.AppendLine("\t\t\t" + "<bodyTattoo>" + req.bodyTattoo.defName + "</bodyTattoo>");
                        }
                        if (req.hair != null)
                        {
                            sb.AppendLine("\t\t\t" + "<hair>" + req.hair.defName + "</hair>");
                        }
                        if (req.beard != null)
                        {
                            sb.AppendLine("\t\t\t" + "<beard>" + req.beard.defName + "</beard>");
                        }
                        if (req.body != null)
                        {
                            sb.AppendLine("\t\t\t" + "<body>" + req.body.defName + "</body>");
                        }
                        sb.AppendLine("\t\t" + "</requirements>");
                        sb.AppendLine("\t</PortraitsOfTheRim.PortraitElementDef>");
                        if (errored)
                        {
                            if (!erroredXML.TryGetValue(source, out var list))
                            {
                                erroredXML[source] = list = new List<string>();
                            }
                            list.Add(sb.ToString());
                        }
                        else
                        {
                            if (!resolvedXML.TryGetValue(source, out var list))
                            {
                                resolvedXML[source] = list = new List<string>();
                            }
                            list.Add(sb.ToString());
                        }
                    }
                }
            }

            foreach (var suff in suffices.OrderByDescending(x => x.Value))
            {
                Log.Message("Unknown: " + suff.Key + " - " + suff.Value);
                Log.ResetMessageCount();
            }

            Log.Message("Resolved xml: \n");
            foreach (var data in resolvedXML)
            {
                Log.Message(data.Key + " (" + data.Value.Count + ")\n" + string.Join("\n", data.Value));
            }
            Log.Message("=================================");
            Log.Message("Errored xml: \n");
            foreach (var data in erroredXML)
            {
                Log.Message(data.Key + " (" + data.Value.Count + ")\n" + string.Join("\n", data.Value));
            }
        }

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
