using BepInEx;
using LLBML;
using HarmonyLib;
using System.IO;
using BepInEx.Logging;
using LLScreen;

namespace GentleSwap {
    //Meta
    [BepInPlugin("com.gentle.plugin.gentleswap", "GentleSwap", "0.0.1")]
    [BepInDependency(LLBML.PluginInfos.PLUGIN_ID)]
    [BepInProcess("LLBlaze.exe")]


    //Plugin
    public class GentleSwap : BaseUnityPlugin {
        internal static GentleSwap Instance { get; private set; } = null;
        internal static ManualLogSource Log { get; private set; } = null;
        internal static ControllerSimplifier controllerSimplifier { get; set; } = null;
        public static ScreenPlayers screenPlayers;

        public string gameDirectory = BepInEx.Paths.GameRootPath;
        public static Harmony harmony = new Harmony(PluginInfos.PLUGIN_ID);
        internal static DirectoryInfo moddingFolder;
        internal static DirectoryInfo customCharBundleDir;


        private void Awake() {
            Instance = this;
            Log = Logger;

            InitDirectories();

            harmony.PatchAll(typeof(Patches.EPCDKLCABNC_Progress_Patches));
            harmony.PatchAll(typeof(Patches.Bundle_Patches));
            harmony.PatchAll(typeof(HDLIJDBFGKN_GameStatesLobbyOnline_Patches));
            harmony.PatchAll(typeof(DNPFJHMAIBP_GameState_Patches));
            harmony.PatchAll(typeof(UnlocksSkinsHandler.ScreenUnlocksSkins_Patches));
            harmony.PatchAll(typeof(ScreenPlayers_Patches));


            MessagingHandler.RegisterCustomMessages();
            BundleHandler.ImportCustomBundles();
        }

        void InitDirectories ()
        {
            moddingFolder = LLBML.Utils.ModdingFolder.GetModSubFolder(this.Info);
            customCharBundleDir = moddingFolder.CreateSubdirectory("bundle").CreateSubdirectory("characters").CreateSubdirectory("custom");
        }

        void FixedUpdate() {
            if (screenPlayers == null) screenPlayers = FindObjectOfType<ScreenPlayers>();

            if (BundleHandler.kiiiinkjkni != null) BundleHandler.AddCustomDLCToAccount();
            else BundleHandler.kiiiinkjkni = FindObjectOfType<KIIIINKJKNI>();

            /*foreach (Message message in GameStates.Messages) {
                Logger.LogInfo("Message: " + message.msg.ToString() + "|" + message.playerNr + "|" + message.index + "|" + (message.ob ?? "null") + "|" + message.obSize);
            }*/
        }
    }
}
