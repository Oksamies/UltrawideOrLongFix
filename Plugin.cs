namespace UltrawideOrLongFix
{
    using System;

    using BepInEx;
    using BepInEx.Logging;
    using HarmonyLib;
    using UnityEngine;

    [BepInPlugin(modGUID, modName, modVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string modGUID = "Oksamies.UltrawideOrLongFix";
        public const string modName = "UltrawideOrLongFix";
        public const string modVersion = "0.1.2";


        private readonly Harmony harmony = new Harmony(modGUID);

        public static ManualLogSource mls;

        void Awake()
        {
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            mls.LogInfo($"{modGUID} is now awake!");

            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(GraphicsManager), "Update")]
        public class AwakePatch
        {
            public static void Prefix()
            {
                if (GraphicsManager.instance.fullscreenCheckTimer <= 0f)
                {
                    var rto = GameObject.Find("Render Texture Overlay");
                    var rtm = GameObject.Find("Render Texture Main");
                    if (rto && rtm)
                    {
                        var rtort = rto.gameObject.GetComponent<RectTransform>();
                        var rtmrt = rtm.gameObject.GetComponent<RectTransform>();
                        if (rtort && rtmrt)
                        {
                            float currentAspectRatio = (float)Screen.width / (float)Screen.height;
                            if (currentAspectRatio > 1.7777778f) {
                                rtort.sizeDelta = new Vector2(428 * currentAspectRatio, 428);
                                rtmrt.sizeDelta = new Vector2(428 * currentAspectRatio, 428);
                            } else {
                                rtort.sizeDelta = new Vector2(750, 750 / currentAspectRatio);
                                rtmrt.sizeDelta = new Vector2(750, 750 / currentAspectRatio);
                            }
                        }
                    }
                }
            }
        }
    }
}
