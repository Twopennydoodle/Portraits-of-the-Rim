using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace PortraitsOfTheRim
{
    [HotSwappable]
    [HarmonyPatch]
    public static class AwesomeInventory_DrawJealous_Patch
    {
        public static MethodInfo target;
        public static bool Prepare()
        {
            target = AccessTools.Method("AwesomeInventory.UI.DrawGearTabWorker:DrawJealous");
            return target != null;
        }
        public static MethodBase TargetMethod()
        {
            return target;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            foreach (var code in codes)
            {
                yield return code;
                if (code.opcode == OpCodes.Stloc_2)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AwesomeInventory_DrawJealous_Patch), "AdjustedRect"));
                    yield return new CodeInstruction(OpCodes.Stloc_2);
                }
            }
        }
        public static Rect AdjustedRect(Rect rect)
        {
            return new Rect(rect.x - (230f / 2f), rect.y, rect.width, rect.height);
        }
    }
}
