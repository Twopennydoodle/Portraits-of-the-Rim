using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;
using RimWorld;

namespace PortraitsOfTheRim
{
    [StaticConstructorOnStartup]
    public class Portrait : IExposable
    {
        public static readonly uint buttonSize = 24;
        public static readonly uint buttonSpacing = 5;
        public static readonly uint buttonCount = 4;

        private static readonly Texture2D ChangeStyles = ContentFinder<Texture2D>.Get("UI/ChangeStyles");
        private static readonly Texture2D ShowHideHatOff = ContentFinder<Texture2D>.Get("UI/ShowHideHat");
        private static readonly Texture2D ShowHidePortraitOff = ContentFinder<Texture2D>.Get("UI/ShowHidePortrait");
        private static readonly Texture2D ShowHideHatOn = ContentFinder<Texture2D>.Get("UI/ShowHideHaton");
        private static readonly Texture2D ShowHidePortraitOn = ContentFinder<Texture2D>.Get("UI/ShowHidePortraiton");
        private static readonly Texture2D SelectExpressedTrait = ContentFinder<Texture2D>.Get("UI/SelectExpressedTrait");
        private static readonly Texture2D OutlineTex = SolidColorMaterials.NewSolidColorTexture(new ColorInt(77, 77, 77).ToColor);

        // Keep it as a one time read for the "Default" no masking gradient mask.
        public static readonly Texture2D DefaultNoMask = ContentFinder<Texture2D>.Get("PotRHairMasks/potr_MaskNone");

        // Non-static fields
        private bool fullHeadgearOn;
        public Pawn pawn;
        private List<(PortraitElementDef, Texture)> portraitTextures;
        private int lastCreatingTime;
        // Fields to keep track of the properties of a pawn that would affect a portrait
        public string cachedHairMaskName;
        public Color cachedHairColor2;
        private string cachedHairName;
        private Color cachedHairColor;
        private List<string> cachedApparels;
        private int cachedAgeBracket;
        private List<string> cachedFaceTraitAndDegrees;
        private bool forceRefresh = false; // If this is true when checking whether or not to refresh, will refresh regardless.
        private string cachedGender;
        private string cachedBodyType;
        private string cachedHeadType;
        private string cachedFaceTattoo;
        private string cachedBodyTattoo;
        private Color cachedSkinColor;
        private string cachedXenotype;
        private string cachedBeard;
        private List<string> cachedActiveGenes;
        private List<string> cachedHediffs;
        private bool cachedBandageInsteadOption;
        

        public bool hidePortrait = !PortraitsOfTheRimSettings.showPortraitByDefault;
        public bool hideHeadgear = !PortraitsOfTheRimSettings.showHeadgearByDefault;
        public string currentStyle;

        public PortraitElementDef innerFaceToSave;

        // List of traits that can be expressed per pawn
        public static Dictionary<Pawn, List<PortraitElementDef>> expressableTraits = new();

        private List<(PortraitElementDef, Texture)> PortraitTextures
        {
            get
            {
                if (portraitTextures is null || Time.frameCount % 20 == 0 && lastCreatingTime != Time.frameCount)
                {
                    lastCreatingTime = Time.frameCount;
                    if (ShouldRefreshPortrait())
                    {
                        //Log.Message("Refreshing Portraits!");
                        portraitTextures = GetPortraitTextures();
                        var missingLayers = PortraitUtils.layers.Where(x => portraitTextures.Any(y => y.Item1.portraitLayer == x) is false);
                        var existingLayers = PortraitUtils.layers.Where(x => portraitTextures.Any(y => y.Item1.portraitLayer == x));
                        //Log.Message("MissingLayers: " + string.Join(", ", missingLayers));
                        //Log.Message("Existing layers: " + string.Join(", ", existingLayers));
                        //Log.Message("Drawn textures: " + string.Join(", ", portraitTextures.Select(x => x.Item2.name)));
                    }
                }
                return portraitTextures;
            }
        }

        private bool ShouldRefreshPortrait()
        {
            if (portraitTextures is null || forceRefresh == true) 
            {
                forceRefresh = false;
                // Initialize cached stuff if necessary, then return true. 
                
                cachedHairColor = pawn.story.HairColor;
                cachedHairName = pawn.story.hairDef.defName;
                cachedBodyType = pawn.story.bodyType.defName;
                cachedGender = pawn.gender.ToString();
                cachedHeadType = pawn.story.headType.defName;
                cachedBodyTattoo = pawn.style.BodyTattoo.defName;
                cachedFaceTattoo = pawn.style.FaceTattoo.defName;
                cachedSkinColor = pawn.story.SkinColor;
                if (ModsConfig.BiotechActive)
                {
                    cachedXenotype = pawn.genes.Xenotype.defName;
                }
                cachedBeard = pawn.style.beardDef.defName;
                cachedBandageInsteadOption = PortraitsOfTheRimSettings.showBandagesInsteadOfInjuries;
                ResolveAndCacheApparels(pawn);
                ResolveAndCacheAge(pawn);
                //ResolveAndCacheGradients(pawn);
                ResolveAndCacheFaces(pawn);
                ResolveAndCacheActiveGenes(pawn);
                ResolveAndCacheHediffs(pawn);
                return true; 
            }
            // Resolve apparels
            if (ResolveAndCacheApparels(pawn)) 
            {
                return true;
            }

            // Resolve age
            if (ResolveAndCacheAge(pawn))
            {
                return true;
            }

            // Resolve Normal Hairs
            if (cachedHairColor == null || cachedHairColor != pawn.story.HairColor)
            {
                cachedHairColor = pawn.story.HairColor;
                //Log.Message("Primary hair color changed, updating portrait");
                return true;
            }
            if (cachedHairName == null || cachedHairName != pawn.story.hairDef.defName)
            {
                cachedHairName = pawn.story.hairDef.defName;
                //Log.Message("Primary hair style changed, updating portrait");
                return true;
            }
            if (cachedBeard == null || cachedBeard != pawn.style.beardDef.defName)
            {
                cachedBeard = pawn.style.beardDef.defName;
                //Log.Message("Beard style changed to " + cachedBeard + ", updating portrait");
                return true;
            }
            // Resolve skin color 
            if (cachedSkinColor == null || cachedSkinColor != pawn.story.SkinColor)
            {
                cachedSkinColor = pawn.story.SkinColor;
                //Log.Message("Pawn skin color changed to " + cachedSkinColor + ", updating portrait.");
                return true;
            }

            // Resolve bandages instead of injuries 
            if (cachedBandageInsteadOption != PortraitsOfTheRimSettings.showBandagesInsteadOfInjuries)
            {
                cachedBandageInsteadOption = PortraitsOfTheRimSettings.showBandagesInsteadOfInjuries;
                //Log.Message("Show bandage instead of injuries option toggled; changing portrait.");
                return true;
            }

            // Temp disabled
            // Resolve Gradient Hairs
            // The caching here is a little special - it is done so in the render step.
            /*if (ResolveAndCacheGradients(pawn))
            {
                return true;
            }*/
            // Resolve tattoos
            if (cachedFaceTattoo != pawn.style.FaceTattoo.defName)
            {
                cachedFaceTattoo = pawn.style.FaceTattoo.defName;
                //Log.Message("Pawn face tattoo changed to " + cachedFaceTattoo + ", updating portrait");
                return true;
            }
            if (cachedBodyTattoo != pawn.style.BodyTattoo.defName)
            {
                cachedBodyTattoo = pawn.style.BodyTattoo.defName;
                //Log.Message("Pawn body tattoo changed to " + cachedFaceTattoo + ", updating portrait");
                return true;
            }

            //Resolve Faces, ignoring randomization. If the random option is set, the swap will have to be triggered manually.
            if (ResolveAndCacheFaces(pawn))
            {
                return true;
            }
            // Resolve pawn Xenotype

            if (ModsConfig.BiotechActive)
            {
                if (cachedXenotype != pawn.genes.Xenotype.defName)
                {
                    cachedXenotype = pawn.genes.Xenotype.defName;
                    //Log.Message("Pawn xenotype changed to " + cachedXenotype + ", updating portrait");
                    return true;
                }
            }

            // Resolve pawn genes
            if (ResolveAndCacheActiveGenes(pawn))
            {
                return true;
            }
            // Resolve pawn body type
            if (cachedBodyType != pawn.story.bodyType.defName)
            {
                cachedBodyType = pawn.story.bodyType.defName;
                //Log.Message("Pawn body type changed to " + cachedBodyType + ", updating portrait");
                return true;
            }
            //Resolve pawn gender
            if (cachedGender != pawn.gender.ToString())
            {
                cachedGender = pawn.gender.ToString();
                //Log.Message("Pawn gender changed to " + cachedGender + ", updating portrait");
                return true;
            }
            // Resolve pawn headType
            if (cachedHeadType != pawn.story.headType.defName)
            {
                cachedHeadType = pawn.story.headType.defName;
                //Log.Message("Pawn head type changed to " + cachedHeadType + ", updating portrait");
                return true;
            }
            // Resolve Hediffs
            if (ResolveAndCacheHediffs(pawn))
            {
                return true;
            }

            
            return false;
        }

       
        // Temporarily disabling support for Hair Gradients
        /*private bool ResolveAndCacheGradients(Pawn pawn)
        {
            if (PortraitUtils.GradientHairLoaded && pawn != null)
            {
                // Check for hair gradient changes 
                if (pawn.Drawer  != null && 
                    pawn.Drawer.renderer != null && 
                    pawn.Drawer.renderer.graphics != null &&
                    pawn.Drawer.renderer.graphics.hairGraphic != null)
                {
                    Material material = pawn.Drawer.renderer.graphics.hairGraphic.MatSouth;
                    if (material != null)
                    {
                        Texture2D maskTex = material.GetMaskTexture();
                        if (maskTex != null)
                        {
                            if (material.GetColorTwo() != cachedHairColor2)
                            {
                                cachedHairColor2 = material.GetColorTwo();
                                //Log.Message("Gradient Hair Color 2 changed, updating portrait");
                                return true;
                            }
                            if (maskTex.name != cachedHairMaskName)
                            {
                                cachedHairMaskName = maskTex.name;
                                //Log.Message("Gradient Hair mask changed. Old: " + cachedHairMaskName + " new: " + maskTex.name + " updating portrait");
                                return true;
                            }
                        }
                    }
                }
            }
            cachedHairMaskName = "MaskNone";
            cachedHairColor2 = Color.white;

            return false;
        }*/

        private bool ResolveAndCacheHediffs(Pawn pawn)
        {
            List<Hediff> currentHediffs = pawn.health.hediffSet.hediffs;
            if (cachedHediffs == null)
            {
                cachedHediffs = new List<string>();
                CacheHediffs(currentHediffs);
                //Log.Message("Hediffs Initializing, updating portrait!");
                return true;
            }
            if (cachedHediffs.Count != currentHediffs.Count)
            {
                CacheHediffs(currentHediffs);
                //Log.Message("Hediff Count changed, updating portrait!");
                return true;
            }
            bool same = true;
            string offendingkey = "";
            foreach (Hediff hediff in currentHediffs)
            {
                string key = MakeHediffKey(hediff);   
                if (!cachedHediffs.Contains(key))
                {
                    offendingkey = key;
                    same = false; break;
                }
            }
            if (!same)
            {
                CacheHediffs(currentHediffs);
                //Log.Message("Hediffs changed in content, updating portrait!");
                //Log.Message("Offending key is: " + offendingkey);
                return true;
            }
            return false;
        }

        private void CacheHediffs(List<Hediff> currentHediffs)
        {
            cachedHediffs.Clear();
            foreach (Hediff hediff in currentHediffs)
            {
                string key = MakeHediffKey(hediff);
                //Log.Message("Adding hediff key " + key + " to hediff cache");
                cachedHediffs.Add(key);
            }
        }

        private string MakeHediffKey(Hediff hediff)
        {
            string key = "";
            if (hediff != null)
            {
                // Missing body part case
                if (hediff.GetType() == typeof(Hediff_MissingPart))
                {
                    key += "Missing";
                    Hediff_MissingPart missing = (Hediff_MissingPart)hediff;
                    if (missing.Part != null && missing.Part.def != null)
                    {
                        key += missing.Part.def.defName;
                    }
                    if (missing.lastInjury != null)
                    {
                        key += missing.lastInjury.defName;
                    }
                }
                else if (hediff.GetType() == typeof(Hediff_Injury))
                {
                    key += "Injury";
                    Hediff_Injury injury = (Hediff_Injury)hediff;
                    if (injury.def != null)
                    {
                        key += injury.def.defName;
                    }
                    if (injury.IsPermanent())
                    {
                        key += "P";
                    }
                    else
                    {
                        key += "N";
                    }
                    if (injury.IsTended())
                    {
                        key += "T";
                    }
                    else
                    {
                        key += "N";
                    }
                    if (injury.Part != null && injury.Part.def != null)
                    {
                        key += injury.Part.def.defName;
                    }
                   
                }
                else  // All other hediffs
                {
                    if (hediff != null)
                    {
                        if (hediff.def != null)
                        {
                            key += hediff.def.defName;
                        }
                        if (hediff.Part != null && hediff.Part.def != null)
                        {
                            key += hediff.Part.def.defName;
                        }
                    }
                }
            }
            return key;
        }

        private bool ResolveAndCacheActiveGenes(Pawn pawn)
        {
            List<Gene> currentActiveGenes = pawn.genes.GenesListForReading.Where(g => g.Active).ToList<Gene>();
            if (cachedActiveGenes == null)
            {
                cachedActiveGenes = new List<string>();
                CacheActiveGenes(currentActiveGenes);
                //Log.Message("Genes initializing, updating portrait!");
                return true;
            }
            if (cachedActiveGenes.Count != currentActiveGenes.Count)
            {
                CacheActiveGenes(currentActiveGenes);
                //Log.Message("Genes changed, updating portrait!");
                return true;
            }
            bool same = true;
            foreach (Gene gene in currentActiveGenes) 
            {
                string key = gene.def.defName;
                if (!cachedActiveGenes.Contains(key))
                {
                    same = false; break;
                }
            }
            if (!same)
            {
                CacheActiveGenes(currentActiveGenes);
                //Log.Message("Genes changed, updating portrait!");
                return true;
            }
            return false;
        }

        private void CacheActiveGenes(List<Gene> activeGenes)
        {
            cachedActiveGenes.Clear();
            foreach (Gene gene in activeGenes)
            {
                if (gene != null && gene.def != null)
                {
                    cachedActiveGenes.Add(gene.def.defName);
                }
                
            }
        }

        private bool ResolveAndCacheFaces(Pawn pawn)
        {
            List<Trait> currentTraits = pawn.story.traits.allTraits;
            if (cachedFaceTraitAndDegrees == null)
            {
                cachedFaceTraitAndDegrees = new List<string>();
                CacheFaceTraits(currentTraits);
                //Log.Message("Traits initializing, updating portrait!");
                return true;
            }
            if (cachedFaceTraitAndDegrees.Count != currentTraits.Count)
            {
                CacheFaceTraits(currentTraits);
                //Log.Message("Traits changed, updating portrait!");
                return true;
            }
            bool same = true;
            foreach (Trait trait in currentTraits)
            {
                string key = trait.def + trait.degree.ToString();
                if (!cachedFaceTraitAndDegrees.Contains(key))
                {
                    same = false; break;
                }
            }
            if (!same)
            {
                CacheFaceTraits(currentTraits);
                //Log.Message("Traits changed, updating portrait!");
                return true;
            }
            return false;
        }

        private void CacheFaceTraits(List<Trait> traits)
        {
            cachedFaceTraitAndDegrees.Clear();
            foreach(Trait trait in traits)
            {
                cachedFaceTraitAndDegrees.Add(trait.def + trait.degree.ToString());
            }
        }
        private bool ResolveAndCacheApparels(Pawn pawn)
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
                            if (cachedApparels == null)
                            {
                                cachedApparels = new List<string>();
                                cacheApparels(apparels);
                                return true;
                            }
                            else
                            {
                                if (cachedApparels.Count != apparels.Count)
                                {
                                    cacheApparels(apparels);
                                    return true;
                                }
                                bool same = true;
                                foreach (Thing apparel in apparels)
                                {
                                    string key = apparel.def + apparel.DrawColor.ToString();
                                    if (!cachedApparels.Contains(key)) 
                                    {
                                        same = false;
                                        break;
                                    }
                                }
                                if (!same)
                                {
                                    cacheApparels(apparels);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                List<Thing> currentApparels = pawn.apparel.WornApparel.Cast<Thing>().ToList();
                if (cachedApparels == null)
                {
                    cachedApparels = new List<string>();
                    cacheApparels(currentApparels);
                    //Log.Message("Apparel initializing, updating portrait!");
                    return true;
                }
                if (cachedApparels.Count != currentApparels.Count)
                {
                    cacheApparels(currentApparels);
                    //Log.Message("Apparel changed, updating portrait!");
                    return true;
                }
                bool same = true;
                foreach (Thing apparel in currentApparels)
                {
                    string key = apparel.def + apparel.DrawColor.ToString();
                    if (!cachedApparels.Contains(key))
                    {
                        same = false;
                        break;
                    }
                }
                if (!same)
                {
                    cacheApparels(currentApparels);
                    //Log.Message("Apparel changed, updating portrait!");
                    return true;
                }
            }
            return false;
        }

        // Helper function for caching Apparels 
        private void cacheApparels(List<Thing> apparels)
        {
            cachedApparels.Clear();
            foreach (Thing apparel in apparels)
            {
                cachedApparels.Add(apparel.def + apparel.DrawColor.ToString());
            }
        }

        // Age brackets cached will just have a 
        // 1 - Child   2 - Teen  3 - Young Adult  4 - Middle Age  5 - Elder
        private bool ResolveAndCacheAge(Pawn pawn)
        {
            float currentAge = pawn.ageTracker.AgeBiologicalYearsFloat;
            int currentAgeBracket = 0;
            if (PortraitUtils.childAge.Includes(currentAge))
            {
                currentAgeBracket = 1;
            }
            else if (PortraitUtils.teenAge.Includes(currentAge))
            {
                currentAgeBracket = 2;
            }
            else if (PortraitUtils.youngAdultAge.Includes(currentAge))
            {
                currentAgeBracket = 3;
            }
            else if (PortraitUtils.middleAged.Includes(currentAge))
            {
                currentAgeBracket = 4;
            }
            else
            {
                currentAgeBracket = 5;
            }
            if (currentAgeBracket != cachedAgeBracket)
            {
                cachedAgeBracket = currentAgeBracket;
                //Log.Message("Age Bracket changed, updating portrait");
                return true;
            }
            return false;
        }

        public bool ShouldShow => hidePortrait is false;
        public void RenderPortrait(float x, float y, float width, float height)
        {
            var textures = PortraitTextures;
            var renderRect = new Rect(x, y, width, height);
            Widgets.DrawBoxSolid(renderRect, Widgets.WindowBGFillColor);
            foreach (var texture in textures)
            {
                if (ShouldHideHeadgear() && PortraitUtils.HeadgearLayers.Contains(texture.Item1.portraitLayer))
                    continue;
                if (this.fullHeadgearOn && !ShouldHideHeadgear() && PortraitUtils.HairLayers.Contains(texture.Item1.portraitLayer))
                    continue; // Disabling hairs if Full Headgear is on (and is not hidden)
                GUI.DrawTexture(renderRect, texture.Item2);
            }
            Widgets.DrawBox(renderRect.ExpandedBy(1), 1, OutlineTex);
        }

        /* Helper function for determining if headgear should be hidden or not when the "Always show when drafted" option is on. 
         * Returns true to hide, false to show.
         * If Drafted and the "Always show when drafted" is on: Show it regardless of any other option
         * If option for hide headgear is on, hide it (obviously)
         * Otherwise, show headgear (Return false) 
         */
        private bool ShouldHideHeadgear()
        {
            if (!this.hideHeadgear)
            {
                return false;
            }
            return !(this.pawn.Drafted && PortraitsOfTheRimSettings.alwaysShowHeadgearWhenDrafted);
        }

        public void DrawButtons(float x, float y)
        {
            Rect hidePortraitRect = HidePortraitButton(x, y);
            var hideHeadgear = new Rect(hidePortraitRect.x, hidePortraitRect.yMax + buttonSpacing, buttonSize, buttonSize);
            TooltipHandler.TipRegion(hideHeadgear, this.hideHeadgear ? "PR.ShowHeadgear".Translate() : "PR.HideHeadgear".Translate());
            Widgets.DrawBoxSolidWithOutline(hideHeadgear, Color.black, Color.grey);
            if (Widgets.ButtonImage(hideHeadgear, this.hideHeadgear ? ShowHideHatOff : ShowHideHatOn))
            {
                this.hideHeadgear = !this.hideHeadgear;
            }

            var selectStyle = new Rect(hidePortraitRect.x, hideHeadgear.yMax + buttonSpacing, buttonSize, buttonSize);
            TooltipHandler.TipRegion(selectStyle, "PR.SelectStyle".Translate());
            Widgets.DrawBoxSolidWithOutline(selectStyle, Color.black, Color.grey);
            if (Widgets.ButtonImage(selectStyle, ChangeStyles))
            {
                var floatList = new List<FloatMenuOption>();
                foreach (var style in PortraitUtils.allStyles)
                {
                    floatList.Add(new FloatMenuOption(style.CapitalizeFirst(), delegate
                    {
                        this.currentStyle = style;
                        forceRefresh = true;
                    }));
                }
                floatList.Add(new FloatMenuOption("Default".Translate(), delegate
                {
                    this.currentStyle = null;
                    forceRefresh = true;
                }));
                Find.WindowStack.Add(new FloatMenu(floatList));
            }

            var selectExpressedTrait = new Rect(hidePortraitRect.x, selectStyle.yMax + buttonSpacing, buttonSize, buttonSize);
            TooltipHandler.TipRegion(selectExpressedTrait, "PR.SelectExpressedTrait".Translate());
            Widgets.DrawBoxSolidWithOutline(selectExpressedTrait, Color.black, Color.grey);
            if (Widgets.ButtonImage(selectExpressedTrait, SelectExpressedTrait))
            {
                var floatList = new List<FloatMenuOption>();
                if (!expressableTraits.TryGetValue(pawn, out var traitList))
                {
                    expressableTraits[pawn] = traitList = new List<PortraitElementDef>();
                }
                foreach (PortraitElementDef trait in traitList)
                {
                    floatList.Add(new FloatMenuOption(trait.requirements.traits[0].def.defName, delegate
                    {
                        innerFaceToSave = trait;
                        forceRefresh = true;
                    }));
                }
                if (traitList.Count == 0)
                {
                    if (PortraitsOfTheRimSettings.randomizeFaceAndHairAssetsInPlaceOfMissingAssets)
                    {
                        floatList.Add(new FloatMenuOption("PR.NoExpressableTraitRandom".Translate(), delegate
                        {
                            if (PortraitUtils.portraitElements.TryGetValue(PR_DefOf.PR_InnerFace, out var elements) && elements.Any())
                            {
                                innerFaceToSave = elements.RandomElement();
                                forceRefresh = true;
                            }
                        }));
                    }
                    else
                    {
                        floatList.Add(new FloatMenuOption("PR.NoExpressableTraitNoRandom".Translate(), delegate
                        {
                            innerFaceToSave = null;
                            forceRefresh = true;
                        }));
                    }
                }
                Find.WindowStack.Add(new FloatMenu(floatList));
            }
        }

        public Rect HidePortraitButton(float x, float y)
        {
            var hidePortraitRect = new Rect(x, y, 24, 24);
            TooltipHandler.TipRegion(hidePortraitRect, this.hidePortrait ? "PR.ShowPortrait".Translate() : "PR.HidePortrait".Translate());
            Widgets.DrawBoxSolidWithOutline(hidePortraitRect, Color.black, Color.grey);
            if (Widgets.ButtonImage(hidePortraitRect, this.hidePortrait ? ShowHidePortraitOff : ShowHidePortraitOn))
            {
                this.hidePortrait = !this.hidePortrait;
            }

            return hidePortraitRect;
        }

        public static Dictionary<Pawn, Dictionary<PortraitElementDef, RenderTexture>> cachedRenderTextures = new();

        

        public static float zoomValue = 2.402f;
        public static float zOffset = -0.086f;
        public List<(PortraitElementDef, Texture)> GetPortraitTextures()
        {
            List<(PortraitElementDef, Texture)> allTextures = new ();
            List<PortraitLayerDef> resolvedLayers = new List<PortraitLayerDef>();
            fullHeadgearOn = false; // By default, full headgear is not on
            bool noMiddleHair = false;
            foreach (var layer in PortraitUtils.layers)
            {
                if (resolvedLayers.Contains(layer))
                    continue;
                if (PortraitUtils.portraitElements.TryGetValue(layer, out var elements) && elements.Any())
                {
                    var matchingElements = elements.Where(x => x.Matches(this)).ToList();
                    if (this.currentStyle.NullOrEmpty() is false && matchingElements.Any(x => x.requirements.style.NullOrEmpty() is false))
                    {
                        matchingElements = matchingElements.Where(x => x.requirements.style == this.currentStyle).ToList();
                    }
                    else if (this.currentStyle.NullOrEmpty() is true)
                    {
                        matchingElements = matchingElements.Where(x => x.requirements.style.NullOrEmpty() is true).ToList();
                    }
                    if (matchingElements.Any())
                    {
                        // If a Full Headgear, turn the full headgear flag on
                        if (layer == PR_DefOf.PR_FullHeadgear)
                        {
                            fullHeadgearOn = true;
                        }
                        if (layer.acceptAllMatchingElements)
                        {
                            foreach (var matchingElement in matchingElements)
                            {
                                GetTexture(allTextures, matchingElement);
                            }
                        }
                        else
                        {
                            if (layer == PR_DefOf.PR_InnerFace)
                            { 
                                GetFaceTexture(allTextures, matchingElements);
                            }
                            else if (layer == PR_DefOf.PR_OuterFace)
                            {
                                continue;
                            }
                            else
                            {
                                GetTextureFrom(allTextures, matchingElements);
                            }
                        }
                    }
                    else if (PortraitsOfTheRimSettings.randomizeFaceAndHairAssetsInPlaceOfMissingAssets)
                    {
                        // Temporarily disabling randominzation of head and neck. Some issues with randomization
                        // where human necks may have insectoid heads, etc.
                        /*
                        if (layer == PR_DefOf.PR_Head || layer == PR_DefOf.PR_Neck)
                        {
                            GetTextureFrom(allTextures, elements);
                        }
                        */
                        if (layer == PR_DefOf.PR_InnerFace)
                        {
                            if (!expressableTraits.TryGetValue(pawn, out var traitList))
                            {
                                expressableTraits[pawn] = traitList = new List<PortraitElementDef>();
                            }
                            else
                            {
                                if (traitList.Any())
                                {
                                    traitList.Clear();
                                }
                            }

                            if (innerFaceToSave == null)
                            {
                                var pickedElement = GetTextureFrom(allTextures, elements);
                                innerFaceToSave = pickedElement;
                            }
                            else
                            {
                                GetTexture(allTextures, innerFaceToSave);
                            }
                            var outerFace = DefDatabase<PortraitElementDef>.GetNamedSilentFail(innerFaceToSave.defName.Replace(layer.defName,
                                    PR_DefOf.PR_OuterFace.defName));
                            if (outerFace != null)
                            {
                                GetTexture(allTextures, outerFace);
                                resolvedLayers.Add(PR_DefOf.PR_OuterFace);
                            }
                        }
                        else if (layer == PR_DefOf.PR_MiddleHair)
                        {
                            noMiddleHair = true;
                        }
                        else if (layer == PR_DefOf.PR_OuterHair)
                        {
                            if (noMiddleHair)
                            {
                                var pickedElement = GetTextureFrom(allTextures, elements);
                                var middleHairs = new List<PortraitElementDef>();
                                var baseName = new Regex("(.*)-(.*)").Replace(pickedElement.defName, "$1").Replace(layer.defName, PR_DefOf.PR_MiddleHair.defName);
                                var postfix = new Regex("(.*)-(.*)").Replace(pickedElement.defName, "$2");
                                foreach (var suffix in PortraitUtils.allSuffixes)
                                {
                                    var newDefName = baseName + "-" + suffix + "-" + postfix;
                                    var def = DefDatabase<PortraitElementDef>.GetNamedSilentFail(newDefName);
                                    if (def != null)
                                    {
                                        middleHairs.Add(def);
                                    }
                                }
                                if (middleHairs.Any())
                                {
                                    var middleHair = middleHairs.FirstOrDefault(x => x.requirements.ageRange is null
                                    || x.requirements.ageRange.Value.Includes(pawn.ageTracker.AgeBiologicalYearsFloat));
                                    if (middleHair != null)
                                    {
                                        GetTexture(allTextures, middleHair);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (layer == PR_DefOf.PR_InnerFace && expressableTraits.ContainsKey(pawn)) 
                        {
                            expressableTraits[pawn].Clear();
                        }
                    }
                }
            }
            return allTextures;
        }

        private PortraitElementDef GetTextureFrom(List<(PortraitElementDef, Texture)> allTextures, List<PortraitElementDef> elements)
        {
            Rand.PushState();
            Rand.Seed = pawn.thingIDNumber;
            var element = elements.RandomElement();
            Rand.PopState();
            GetTexture(allTextures, element);
            return element;
        }

        private void GetTexture(List<(PortraitElementDef, Texture)> allTextures, PortraitElementDef matchingElement)
        {
            var mainTexture = matchingElement.graphic.MatSingle.mainTexture;
            // Adding a check and exiting if the texture is unloadable for whatever reason
            if (mainTexture == null)
            {
                Log.Error("Portraits of the Rim Error! Cannot find texture for " + matchingElement.defName);
                return;
            }
            if (!cachedRenderTextures.TryGetValue(pawn, out var dict))
            {
                cachedRenderTextures[pawn] = dict = new Dictionary<PortraitElementDef, RenderTexture>();
            }
            if (!dict.TryGetValue(matchingElement, out var renderTexture))
            {
                dict[matchingElement] = renderTexture = new RenderTexture(mainTexture.width, mainTexture.height, 0);
            }
            renderTexture.RenderElement(matchingElement, pawn, new Vector3(0, 0, zOffset), zoomValue);
            renderTexture.name = matchingElement.defName;
            allTextures.Add((matchingElement, renderTexture));
        }

        private void GetFaceTexture(List<(PortraitElementDef, Texture)> allTextures, List<PortraitElementDef> elements)
        {
            expressableTraits[pawn] = elements;
            if (innerFaceToSave == null || !elements.Contains(innerFaceToSave))
            {
                Rand.PushState();
                Rand.Seed = pawn.thingIDNumber;
                innerFaceToSave = elements.RandomElement();
                Rand.PopState();
            }
            // Inner face 
            if (innerFaceToSave != null)
            {
                GetTexture(allTextures, innerFaceToSave);
                var outerFace = DefDatabase<PortraitElementDef>.GetNamedSilentFail(innerFaceToSave.defName.Replace(PR_DefOf.PR_InnerFace.defName,
                                    PR_DefOf.PR_OuterFace.defName));
                if (outerFace != null)
                {
                    GetTexture(allTextures, outerFace);
                }
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref hidePortrait, "hidePortrait", !PortraitsOfTheRimSettings.showPortraitByDefault);
            Scribe_Values.Look(ref hideHeadgear, "hideHeadgear", !PortraitsOfTheRimSettings.showHeadgearByDefault);
            Scribe_Values.Look(ref currentStyle, "currentStyle", "");
            Scribe_Defs.Look(ref innerFaceToSave, "innerFaceToSave");
        }
    }
}
