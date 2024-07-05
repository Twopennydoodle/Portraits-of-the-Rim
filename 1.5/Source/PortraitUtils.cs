using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace PortraitsOfTheRim
{
    [StaticConstructorOnStartup]
    public static class PortraitUtils
    {
        public static Camera portraitCamera;
        public static Dictionary<Pawn, Portrait> pawnPortraits = new Dictionary<Pawn, Portrait>();
        public static List<PortraitLayerDef> layers;
        public static Dictionary<PortraitLayerDef, List<PortraitElementDef>> portraitElements;
        public static Dictionary<Pawn, string> gradientMaskTextures = new Dictionary<Pawn, string>();
        public static Dictionary<Pawn, Color> gradientMaskColors = new Dictionary<Pawn, Color>();

        // Flags to support further mod inclusion
        public static bool CELoaded = ModsConfig.IsActive("CETeam.CombatExtended");
        public static bool AppearanceClothesLoaded = ModsConfig.IsActive("tammybee.appearanceclothes");
        public static bool GradientHairLoaded = ModsConfig.IsActive("automatic.gradienthair");
        public static Type gradientCompType = null;
        public static MethodInfo getGradientHairComp = null;
        public static MethodInfo getGradientHairSettings = null;

        public static HashSet<PortraitLayerDef> HeadgearLayers = new HashSet<PortraitLayerDef>
        {
            PR_DefOf.PR_FullHeadgear, PR_DefOf.PR_InnerHeadgear, PR_DefOf.PR_OuterHeadgear, PR_DefOf.PR_UnderHeadgear, PR_DefOf.PR_OverHeadgear
        };
        public static HashSet<PortraitLayerDef> HairLayers = new HashSet<PortraitLayerDef>
        {
            PR_DefOf.PR_UnderHair, PR_DefOf.PR_InnerHair, PR_DefOf.PR_OuterHair, PR_DefOf.PR_MiddleHair, PR_DefOf.PR_AccessoriesHair
        };
        [NoTranslate]
        public static HashSet<string> validMasks = new HashSet<string>
        {
            "G01", "G02", "G03", "G04", "G05", "G06", "G07", "G08", "G09", "G10", "G11", "G12", "MaskAHigh",
            "MaskAHigh2", "MaskAHigh3", "MaskALow", "MaskALow2", "MaskALow3", "MaskAMidHigh", "MaskAMidLow",
            "MaskBHigh", "MaskBHigh2", "MaskBHigh3", "MaskBLow", "MaskBLow2", "MaskBLow3", "MaskBMidHigh", "MaskBMidLow",
            "MaskCHigh", "MaskCHigh2", "MaskCHigh3", "MaskCLow", "MaskCLow2", "MaskCLow3", "MaskCMidHigh", "MaskCMidLow"
        };
        [NoTranslate]
        public static HashSet<string> fallbackHeads = new HashSet<string>
        {
            "averagenormal", "averagepointy", "averagewide", "narrownormal", "narrowpointy", "narrowwide"
        };

        // Suffixes used in filenames. Brought over from TextureParser until that can be figured out again.
        [NoTranslate]
        public const string Large = "l";
        [NoTranslate]
        public const string Small = "s";
        [NoTranslate] 
        public const string Medium = "m";
        [NoTranslate]
        public const string ExtraLarge = "xl";
        [NoTranslate]
        public const string AdultAllGender = "an"; // 13-999 for general use
        [NoTranslate] 
        public const string ChildAllGender = "cn";
        [NoTranslate] 
        public const string ChildFemale = "cf";
        [NoTranslate] 
        public const string ChildMale = "cm";
        [NoTranslate] 
        public const string TeenFemale = "tf";
        [NoTranslate] 
        public const string TeenMale = "tm";
        [NoTranslate] 
        public const string YoungFemale = "yf";
        [NoTranslate] 
        public const string YoungMale = "ym";
        [NoTranslate] 
        public const string ElderFemale = "ef";
        [NoTranslate] 
        public const string ElderMale = "em";
        [NoTranslate] 
        public const string MiddleAgedFemale = "mf";
        [NoTranslate] 
        public const string MiddleAgedMale = "mm";
        [NoTranslate] 
        public const string AdultFemale = "af"; // Just used for faces, 19-999 (Different from AdultAllGender above) 
        [NoTranslate] 
        public const string AdultMale = "am"; // Just used for faces, 19-999 (Different from AdultAllGender above)
        [NoTranslate] 
        public const string TeenAdultMale = "tam"; // Just used for necks, 13-999 male
        [NoTranslate]
        public const string TeenAdultFemale = "taf"; // Just used for necks, 13-999 female

        public static List<string> allSuffixes = new List<string>
        {
            Large, Small, Medium, ExtraLarge, AdultAllGender, ChildAllGender, ChildFemale, ChildMale, TeenFemale, TeenMale, YoungFemale,
            YoungMale, ElderFemale, ElderMale, MiddleAgedFemale, MiddleAgedMale, AdultFemale, AdultMale,TeenAdultMale, TeenAdultFemale,
        };

        public static HashSet<string> allStyles;
        public static FloatRange childAge = new FloatRange(7f, 13f);
        public static FloatRange teenAge = new FloatRange(13f, 19f);
        public static FloatRange youngAdultAge = new FloatRange(19f, 39f);
        public static FloatRange middleAged = new FloatRange(39f, 64f);
        public static FloatRange elderAge = new FloatRange(64, 999f);
        public static FloatRange totalChildAge = new FloatRange(7f, 13f);
        public static FloatRange totalAdultAge = new FloatRange(13f, 999);
        public static FloatRange teenAdultAge = new FloatRange(19f, 999);
        static PortraitUtils()
        {
            new Harmony("PortraitsOfTheRimMod").PatchAll();
            layers = DefDatabase<PortraitLayerDef>.AllDefs.OrderBy(x => x.layer).ToList();
            portraitElements = new();
            foreach (var layerDef in layers)
            {
                var list = new List<PortraitElementDef>();
                foreach (var elementDef in DefDatabase<PortraitElementDef>.AllDefs)
                {
                    if (elementDef.portraitLayer == layerDef)
                    {
                        list.Add(elementDef);
                    }
                }
                portraitElements[layerDef] = list;
            }
            allStyles = new HashSet<string>();
            foreach (var elementDef in DefDatabase<PortraitElementDef>.AllDefs.ToList())
            {
                if (elementDef.requirements.style.NullOrEmpty() is false)
                {
                    allStyles.Add(elementDef.requirements.style);
                }
                if (elementDef.requirements.apparels.NullOrEmpty() is false)
                {
                    if (elementDef.requirements.apparels.Any(x => x is null))
                    {
                        foreach (var kvp in portraitElements.ToList())
                        {
                            kvp.Value.Remove(elementDef);
                            Log.Error("Wrong PortraitElementDef: " + elementDef + ", removed it");
                        }
                    }
                }
            }

            GameObject gameObject = new GameObject("PortraitCamera", typeof(Camera));
            gameObject.SetActive(value: false);
            gameObject.AddComponent<PortraitCamera>();
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            Camera component = gameObject.GetComponent<Camera>();
            component.transform.position = new Vector3(0f, 15f, 0f);
            component.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            component.orthographic = true;
            component.cullingMask = 0;
            component.clearFlags = CameraClearFlags.Color;
            component.backgroundColor = Color.clear;
            component.useOcclusionCulling = false;
            component.renderingPath = RenderingPath.Forward;
            Camera camera = Current.Camera;
            component.nearClipPlane = camera.nearClipPlane;
            component.farClipPlane = camera.farClipPlane;
            portraitCamera = component;

            /* Setup area for Gradient Hairs mod. This will find the settings accessors needed to resolve textures later. */
            if (GradientHairLoaded)
            { 
                gradientCompType = Type.GetType("GradientHair.CompGradientHair, GradientHair");
                if (gradientCompType == null) { 
                    Log.Error("Portraits of the Rim Error: Gradient Hair mod is installed, but can't access Gradient Hair type. Please let us know on Steam!");
                    GradientHairLoaded = false;
                    return;
                }
                getGradientHairComp = AccessTools.Method(typeof(Pawn), "GetComp", null, new Type[]{gradientCompType});
                if (getGradientHairComp == null) 
                {
                    Log.Error("Portraits of the Rim Error: Gradient Hair mod is installed, but can't access Gradient Hair Comp. Please let us know on Steam!");
                    GradientHairLoaded = false;
                    return;
                }
                PropertyInfo pinfo = gradientCompType.GetProperty("Settings");
                if (pinfo == null)
                {
                    Log.Error("Portraits of the Rim Error: Gradient Hair mod is installed, but can't access Gradient Hair Comp's Settings. Please let us know on Steam!");
                    GradientHairLoaded = false;
                    return;
                }
                getGradientHairSettings = pinfo.GetAccessors()[0];
                if (getGradientHairSettings == null)
                {
                    Log.Error("Portraits of the Rim Error: Gradient Hair mod is installed, but can't access Gradient Hair Setting Accessors. Please let us know on Steam!");
                    GradientHairLoaded = false;
                }
            }
        }
        public static bool ShouldShowPortrait(this Pawn pawn)
        {
            return pawn != null && pawn.RaceProps.Humanlike && pawn.ageTracker.AgeBiologicalYearsFloat >= 7;
        }
        public static bool IsAdult(this Pawn pawn)
        {
            return totalAdultAge.Includes(pawn.ageTracker.AgeBiologicalYearsFloat);
        }
        public static bool IsTeen(this Pawn pawn)
        {
            return teenAge.Includes(pawn.ageTracker.AgeBiologicalYearsFloat);
        }
        public static bool IsChild(this Pawn pawn)
        {
            return childAge.Includes(pawn.ageTracker.AgeBiologicalYearsFloat);
        }

        public static bool MatchesAnyBodyType(this Pawn pawn, params BodyTypeDef[] bodyTypes)
        {
            return bodyTypes.Contains(pawn.story.bodyType);
        }
        public static Portrait GetPortrait(this Pawn pawn)
        {
            if (!pawnPortraits.TryGetValue(pawn, out var portrait))
            {
                pawnPortraits[pawn] = portrait = new Portrait
                {
                    pawn = pawn,
                };
            }
            return portrait;
        }
        public static void RenderElement(this RenderTexture renderTexture, PortraitElementDef def, Pawn pawn, Vector3 offset, float zoom = 1f)
        {
            portraitCamera.GetComponent<PortraitCamera>().RenderElement(def, pawn, renderTexture, offset, zoom);
        }
    }
}
