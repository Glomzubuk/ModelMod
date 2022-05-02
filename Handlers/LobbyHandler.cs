using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using LLScreen;
using LLGUI;
using HarmonyLib;
using LLBML.States;
using Multiplayer;
using LLBML.Players;
using LLHandlers;

namespace GentleSwap {
    public class LobbyHandler : MonoBehaviour {

        public static LobbyHandler instance;
        public ScreenPlayers screenPlayers;

        public static LobbyHandler Initialize() {
            GameObject gameObject = new GameObject("LobbyHandler");
            instance = gameObject.AddComponent<LobbyHandler>();
            return instance;
        }

        public static int GetNumUnlockedSkinsExcludingCustom() {
            int n = 0;
            foreach (CharacterVariant variant in Enum.GetValues(typeof(CharacterVariant))) {
                switch (variant) {
                    case CharacterVariant.CORPSE:
                    case CharacterVariant.MODEL_ALT_CORPSE:
                    case CharacterVariant.MODEL_ALT_CORPSE3_4:
                    case CharacterVariant._MAX:
                        break;
                    default:
                        n++;
                        break;
                }
            }
            return n;
        }
    }

    public static class HDLIJDBFGKN_GameStatesLobbyOnline_Patches { //GameStateLobbyOnline
        [HarmonyPatch(typeof(HDLIJDBFGKN), "OAACLLGMFLH", typeof(int[]))] //StartGame(int[])
        [HarmonyPrefix]

        static void OAACLLGMFLH_PrePatch(ref HDLIJDBFGKN __instance, ref int[] __state) { //Checks if the local player wants a custom skin and provides the wish to the postfix
            __state = new int[3] { -1, 0, -1 }; //PlayerID, WantsCustomSkin, RequestedCharacterVariant

            foreach (Player player in Player.GetPlayerList()) {
                if (player.isLocal) {
                    foreach (BundleHandler.CustomBundle bundle in BundleHandler.bundles) {
                        if (player.Character == bundle.character) {
                            foreach (BundleHandler.VariantIdentifier variantIdentifier in bundle.variantIdentifiers) {
                                if (player.CharacterVariant == variantIdentifier.variant) {
                                    GentleSwap.Log.LogDebug($"Local player wants {player.CharacterVariant}");
                                    __state[0] = player.nr;
                                    __state[1] = 1;
                                    __state[2] = (int)variantIdentifier.variant;
                                }
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(HDLIJDBFGKN), "OAACLLGMFLH", typeof(int[]))] //StartGame(int[])
        [HarmonyPostfix]
        static void OAACLLGMFLH_PostPatch(ref HDLIJDBFGKN __instance, int[] __state) { //Overrides the received variant from the host
            if (__state[1] == 1) {
                Player.GetPlayer(__state[0]).CharacterVariant = (CharacterVariant)__state[2];
                GentleSwap.Log.LogDebug($"Local player was assigned {(CharacterVariant)__state[2]}");
            }
        }

        [HarmonyPatch(typeof(HDLIJDBFGKN), "HBHMJHDDJIF")] //OnProcessMessage
        [HarmonyPrefix]
        static bool HBHMJHDDJIF_PostPatch(ref Message __1) {

            if ((__1.msg == Msg.SEL_CHAR || __1.msg == Msg.SEL_SKIN) && GameStates.GetCurrent() == LLBML.States.GameState.LOBBY_ONLINE) {
                Player player = Player.GetLocalPlayer();
                int currentVariantID = (int)player.variant;

                if (__1.index == (int)player.Character) {
                    foreach (BundleHandler.CustomBundle bundle in BundleHandler.bundles) {
                        bool isFinalBundle = BundleHandler.bundles[BundleHandler.bundles.Count - 1] == bundle;

                        try {
                            int selectedsVariantIndex = bundle.variantIdentifiers.IndexOf(bundle.variantIdentifiers.First(identifier => (int)identifier.variant == currentVariantID));
                            bool selectedVariantIsLastInBundle = selectedsVariantIndex == bundle.variantIdentifiers.Count - 1;

                            if (selectedVariantIsLastInBundle && isFinalBundle) return true;
                            else {
                                if (!selectedVariantIsLastInBundle) {
                                    var identifier = bundle.variantIdentifiers[selectedsVariantIndex + 1];
                                    HDLIJDBFGKN.instance.JPNNBHNHHJC();
                                    HDLIJDBFGKN.instance.EMFKKOJEIPN(player.nr, false);
                                    HDLIJDBFGKN.instance.IKPDLPDNHIJ(false, true);

                                    Debug.Log($"Sending message to other players: new Message({MessagingHandler.customVariantMsg}, {P2P.localPeer.playerNr}, {identifier.variantNr}, {Encoding.Default.GetBytes(bundle.bundleName)})");
                                    P2P.SendOthers(new Message(MessagingHandler.customVariantMsg, P2P.localPeer.playerNr, identifier.variantNr, Encoding.Default.GetBytes(bundle.bundleName)));
                                    player.CharacterVariant = identifier.variant;

                                    PlayersSelection playersSelection = GentleSwap.screenPlayers.playerSelections.First(selection => selection.playerNr == player.nr);
                                    playersSelection.SetCharacter(player.Character, identifier.variant, true, GentleSwap.screenPlayers.playerSelections.Length);
                                    playersSelection.characterModel.PlayCamAnim();
                                    AudioHandler.PlayMenuSfx(Sfx.MENU_SCROLL);
                                    
                                    return false;
                                }
                            }
                        }
                        catch {
                            return true;
                        }
                    }         
                }
            }

            return true;
        }
    }

    public static class DNPFJHMAIBP_GameState_Patches {
        [HarmonyPatch(typeof(DNPFJHMAIBP), "GKBNNFEAJGO", typeof(Message))] //Send(Message)
        [HarmonyPrefix]

        static void GKBNNFEAJGO_Patch(ref Message __0) {
            Debug.Log(__0.msg);
            Msg msg = __0.msg;
            int playerNr = __0.playerNr;
            int index = __0.index;
            object obj = __0.ob;
            int objSize = __0.obSize;


            if (msg == Msg.SEL_SKIN || msg == Msg.HOVER_SKIN) {
                var skinID = index - LobbyHandler.GetNumUnlockedSkinsExcludingCustom();

                Debug.Log(index);
                foreach (BundleHandler.CustomBundle bundle in BundleHandler.bundles) {
                    foreach (BundleHandler.VariantIdentifier identifier in bundle.variantIdentifiers) {
                        if (skinID == identifier.variantNr) {
                            __0.index = (int)identifier.variant;

                            if (GameStates.GetCurrent() == LLBML.States.GameState.LOBBY_ONLINE) {
                                Debug.Log($"Sending message to other players: new Message({MessagingHandler.customVariantMsg}, {P2P.localPeer.playerNr}, {identifier.variantNr}, {Encoding.Default.GetBytes(bundle.bundleName)})");
                                P2P.SendOthers(new Message(MessagingHandler.customVariantMsg, P2P.localPeer.playerNr, identifier.variantNr, Encoding.Default.GetBytes(bundle.bundleName)));
                            }
                        }
                    }
                }
            }
        }
    }

    public static class ScreenPlayers_Patches {
        [HarmonyPatch(typeof(ScreenPlayers), "OnOpen")]
        [HarmonyPostfix]

        static void OnOpen_Patch() {
            if (LLBML.States.GameStates.GetCurrent() == LLBML.States.GameState.LOBBY_TRAINING) {
                GentleSwap.controllerSimplifier = ControllerSimplifier.Initialize();
            }
        }

        [HarmonyPatch(typeof(ScreenPlayers), "OnClose")]
        [HarmonyPostfix]

        static void OnClose_Patch() {
            if (GentleSwap.controllerSimplifier != null) GentleSwap.Destroy(GentleSwap.controllerSimplifier);
        }
    }
}
