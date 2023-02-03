using UnityEngine;
using Verse;

namespace PortraitsOfTheRim
{
    [HotSwappable]
    [StaticConstructorOnStartup]
    public static class PortraitUtils
    {
        public static Camera portraitCamera;
        static PortraitUtils()
        {
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
        public static void RenderElement(this RenderTexture renderTexture, PortraitElementDef def, Pawn pawn, Vector3 offset, float zoom = 1f)
        {
            portraitCamera.GetComponent<PortraitCamera>().RenderElement(def, pawn, renderTexture, offset, zoom);
        }
    }
}
