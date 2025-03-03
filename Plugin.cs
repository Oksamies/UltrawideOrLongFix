namespace UltrawideOrLongFix
{
    using BepInEx;
    using BepInEx.Configuration;

    using BepInEx.Logging;
    using HarmonyLib;
    using UnityEngine;

    [BepInPlugin(modGUID, modName, modVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string modGUID = "Oksamies.UltrawideOrLongFix";
        public const string modName = "UltrawideOrLongFix";
        public const string modVersion = "0.1.3";
        public static ConfigEntry<bool> configAspectRatioFix;
        public static ConfigEntry<bool> configFieldOfViewFix;
        public static ConfigEntry<float> configFieldOfView;
        public static ConfigEntry<bool> configAutoFieldOfView;
        public static float previousAspectRatio;
        public static float currentAspectRatio;
        public static readonly float defaultAspectRatio = 1.7777778f;
        public static bool fovSetOnce = false;
        public static float sceneFoV = 33f;
        public static bool menuFoVSet = false;

        private readonly Harmony harmony = new Harmony(modGUID);

        public static ManualLogSource mls;

        void Awake()
        {
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            configAspectRatioFix = Config.Bind
            (
                "Gameplay", 
                "Aspect-ratio fix on/off", 
                true, 
                "In case aspect-ratio fix is broken or you want to disable aspect-ratio fixing, turn this to false"
            );
            configFieldOfViewFix = Config.Bind
            (
                "Gameplay", 
                "Field of View Fix on/off", 
                true, 
                "In case field of view fix is broken or you want to disable FOV fixing, turn this to false"
            );
            configFieldOfView = Config.Bind
            (
                "Gameplay", 
                "Field of View", 
                70f, 
                "Set this and turn Auto Field of View off to control it manually."
            );
            configAutoFieldOfView = Config.Bind
            (
                "Gameplay", 
                "Auto Field of View", 
                true, 
                "Automatically try to figure out a proper field of view for the window"
            );
            mls.LogInfo($"{modGUID} is now awake!");

            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(GraphicsManager), "Update")]
        public class GraphicsManagerUpdatePatch
        {
            public static void Prefix()
            {
                if (GraphicsManager.instance.fullscreenCheckTimer <= 0f)
                {
                    currentAspectRatio = (float)Screen.width / (float)Screen.height;
                    if (configAspectRatioFix.Value) {
                    var rto = GameObject.Find("Render Texture Overlay");
                    var rtm = GameObject.Find("Render Texture Main");
                    if (rto && rtm)
                    {
                        var rtort = rto.gameObject.GetComponent<RectTransform>();
                        var rtmrt = rtm.gameObject.GetComponent<RectTransform>();
                        if (rtort && rtmrt)
                        {
                            if (currentAspectRatio > defaultAspectRatio) {
                                rtort.sizeDelta = new Vector2(428 * currentAspectRatio, 428);
                                rtmrt.sizeDelta = new Vector2(428 * currentAspectRatio, 428);
                            } else {
                                rtort.sizeDelta = new Vector2(750, 750 / currentAspectRatio);
                                rtmrt.sizeDelta = new Vector2(750, 750 / currentAspectRatio);
                            }
                            previousAspectRatio = currentAspectRatio;
                            if (configFieldOfViewFix.Value) {

                                // mls.LogInfo("AAAAAAAAAA");
                                // mls.LogInfo($"GameDirector.instance.MainCamera.fieldOfView: {GameDirector.instance.MainCamera.fieldOfView}");
                                // mls.LogInfo($"configFieldOfViewFix.Value: {configFieldOfViewFix.Value}");
                                // mls.LogInfo($"configAutoFieldOfView.Value: {configAutoFieldOfView.Value}");
                                // mls.LogInfo($"configFieldOfView.Value: {configFieldOfView.Value}");
                                // mls.LogInfo($"CameraZoom.Instance.playerZoomDefault: {CameraZoom.Instance.playerZoomDefault}");
                                // mls.LogInfo($"CameraZoom.Instance.zoomPrev: {CameraZoom.Instance.zoomPrev}");
                                // mls.LogInfo($"CameraZoom.Instance.zoomCurrent: {CameraZoom.Instance.zoomCurrent}");
                                // mls.LogInfo($"CameraZoom.Instance.SprintZoom: {CameraZoom.Instance.SprintZoom}");
                                // mls.LogInfo($"StatsManager.instance.playerUpgradeSpeed[PlayerController.instance.playerAvatarScript.steamID]: {StatsManager.instance.playerUpgradeSpeed[PlayerController.instance.playerAvatarScript.steamID]}");
                                // mls.LogInfo("##########");

                            }
                        }
                    }
                    previousAspectRatio = currentAspectRatio;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CameraZoom), "Update")]
        public class CameraZoomAwakePatch
        {
            public static void Prefix(CameraZoom __instance)
            {
                if (SemiFunc.MenuLevel()) {
                    sceneFoV = 33f;
                } else {
                    sceneFoV = 70f;
                    menuFoVSet = false;
                }
                if (configFieldOfViewFix.Value && !fovSetOnce || configFieldOfViewFix.Value && currentAspectRatio != defaultAspectRatio || SemiFunc.MenuLevel() && !menuFoVSet) {

                    if (configAutoFieldOfView.Value) {
                        if (currentAspectRatio != defaultAspectRatio) {
                            if (previousAspectRatio != currentAspectRatio || !fovSetOnce) {
                                if (SemiFunc.MenuLevel()) {
                                    CameraNoPlayerTarget.instance.cam.fieldOfView = 33f / (defaultAspectRatio / currentAspectRatio);
                                } else {
                                    __instance.playerZoomDefault = 70f / (defaultAspectRatio / currentAspectRatio);
                                    __instance.zoomPrev = 70f / (defaultAspectRatio / currentAspectRatio);
                                    __instance.zoomCurrent = __instance.zoomPrev;
                                }
                            }
                        }
                    } else {
                        if (SemiFunc.MenuLevel()) {
                            CameraNoPlayerTarget.instance.cam.fieldOfView = 33f / (33f / configFieldOfView.Value);
                            menuFoVSet = true;
                        } else {
                            __instance.playerZoomDefault = configFieldOfView.Value;
                            __instance.zoomPrev = configFieldOfView.Value;
                            __instance.zoomCurrent = __instance.zoomPrev;
                        }
                    }
                    previousAspectRatio = currentAspectRatio;
                }
            }
        }
    }
}
