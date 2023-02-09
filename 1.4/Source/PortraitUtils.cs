using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PortraitsOfTheRim
{
    [HotSwappable]
    [StaticConstructorOnStartup]
    public static class PortraitUtils
    {
        public static Camera portraitCamera;
        public static Dictionary<Pawn, Portrait> pawnPortraits = new Dictionary<Pawn, Portrait>();
        public static List<PortraitLayerDef> layers;
        public static Dictionary<PortraitLayerDef, List<PortraitElementDef>> portraitElements;

        public static HashSet<PortraitLayerDef> HeadgearLayers = new HashSet<PortraitLayerDef>
        {
            PR_DefOf.PR_FullHeadgear, PR_DefOf.PR_InnerHeadgear, PR_DefOf.PR_OuterHeadgear, PR_DefOf.PR_UnderHeadgear
        };

        public static HashSet<string> allStyles;
        static PortraitUtils()
        {
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
            foreach (var elementDef in DefDatabase<PortraitElementDef>.AllDefs)
            {
                if (elementDef.requirements.style.NullOrEmpty() is false)
                {
                    allStyles.Add(elementDef.requirements.style);
                }
            }

            GameObject gameObject = new GameObject("PortraitCamera", typeof(Camera));
            gameObject.SetActive(value: false);
            gameObject.AddComponent<PortraitCamera>();
            Object.DontDestroyOnLoad(gameObject);
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
