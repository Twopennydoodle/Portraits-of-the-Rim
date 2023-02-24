using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace PortraitsOfTheRim
{
    [HotSwappable]
    [HarmonyPatch]
    public static class AwesomeInventory_DrawGreedy_Patch
    {
        public static MethodInfo target;
        public static bool Prepare()
        {
            target = AccessTools.Method("AwesomeInventory.UI.DrawGearTabWorker:DrawGreedy");
            return target != null;
        }
        public static MethodBase TargetMethod()
        {
            return target;
        }

        public static float xPos = 480;
        public static float yPos = 113;
        public static float portraitSize = 120;

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstruction)
        {
            var methods = AccessTools.GetDeclaredMethods(typeof(Widgets));
            var listSeparator = methods.LastOrDefault(x => x.Name == "ListSeparator");
            var codes = codeInstruction.ToList();
            bool patched = false;
            foreach ( var code in codes )
            {
                yield return code;
                if (!patched && code.Calls(listSeparator))
                {
                    patched = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AwesomeInventory_DrawGreedy_Patch), nameof(DrawPortrait)));
                }
            }
        }
        public static void DrawPortrait(Pawn pawn)
        {
            ITab_Pawn_Gear_FillTab_Patch.DrawPortraitArea(pawn, xPos, 1, 159);
        }
    }
}
