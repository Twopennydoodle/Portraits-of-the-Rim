using HarmonyLib;
using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace PortraitsOfTheRim
{
    public class PortraitsOfTheRimMod : Mod
    {
        public static PortraitsOfTheRimSettings settings;
        public PortraitsOfTheRimMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<PortraitsOfTheRimSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return Content.Name;
        }
    }

    public class PortraitsOfTheRimSettings : ModSettings
    {
        public static bool showBandagesInsteadOfInjuries = false;
        public static bool randomizeFaceAndHairAssetsInPlaceOfMissingAssets = true;
        public static bool fallbackBaselinerHead = false;
        public static bool showPortraitByDefault = true;
        public static bool showHeadgearByDefault = true;
        public static bool alwaysShowHeadgearWhenDrafted = false;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref showBandagesInsteadOfInjuries, "showBandagesInsteadOfInjuries", false);
            Scribe_Values.Look(ref randomizeFaceAndHairAssetsInPlaceOfMissingAssets, "randomizeFaceAndHairAssetsInPlaceOfMissingAssets", true);
            Scribe_Values.Look(ref fallbackBaselinerHead, "fallbackBaselinerHead", false);
            Scribe_Values.Look(ref showPortraitByDefault, "showPortraitsByDefault", true);
            Scribe_Values.Look(ref showHeadgearByDefault, "showHeadgearByDefault", true);
            Scribe_Values.Look(ref alwaysShowHeadgearWhenDrafted, "alwaysShowHeadgearWhenDrafted", false);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            var ls = new Listing_Standard();
            ls.Begin(inRect);
            ls.CheckboxLabeled("PR.ShowBandagesInsteadOfInjuries".Translate(), ref showBandagesInsteadOfInjuries);
            ls.CheckboxLabeled("PR.RandomizeFaceAndHairAssetsInPlaceOfMissingAssets".Translate(), ref randomizeFaceAndHairAssetsInPlaceOfMissingAssets);
            ls.CheckboxLabeled("PR.FallbackBaselinerHead".Translate(), ref fallbackBaselinerHead);
            ls.CheckboxLabeled("PR.ShowPortraitsByDefault".Translate(), ref showPortraitByDefault);
            ls.CheckboxLabeled("PR.ShowHeadgearByDefault".Translate(), ref showHeadgearByDefault);
            ls.CheckboxLabeled("PR.ShowHeadgearWhenDrafted".Translate(), ref alwaysShowHeadgearWhenDrafted);
            ls.End();
        }
    }
}
