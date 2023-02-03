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
            new Harmony("PortraitsOfTheRimMod").PatchAll();
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
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref showBandagesInsteadOfInjuries, "showBandagesInsteadOfInjuries", false);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            var ls = new Listing_Standard();
            ls.Begin(inRect);
            ls.CheckboxLabeled("PR.ShowBandagesInsteadOfInjuries".Translate(), ref showBandagesInsteadOfInjuries);
            ls.End();
        }
    }
}
