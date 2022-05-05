using LLBML.Players;
using LLBML.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using LLScreen;

namespace GentleSwap {
    public static class MessagingHandler {

        public static Msg customVariantMsg = (Msg)204;


        public static void RegisterCustomMessages() {
            MessageApi.RegisterCustomMessage(GentleSwap.Instance.Info, (ushort)customVariantMsg, "CUSTOM_VARIANT_SELECTED", RemoteCustomVariantSelected);
        }
        

        public static void RemoteCustomVariantSelected(Message message) {
            if (message.msg == customVariantMsg) {
                int remotePlayerNr = message.playerNr;
                int remoteVariantNr = message.index;
                string remoteBundleName = Encoding.Default.GetString((byte[])message.ob);

                Player player = Player.GetPlayer(remotePlayerNr);
                Debug.Log(remoteBundleName);
                foreach (BundleHandler.CustomBundle bundle in BundleHandler.bundles) {
                    if (remoteBundleName == bundle.bundleName) {
                        if (GentleSwap.screenPlayers != null) {
                            PlayersSelection playerSelection = GentleSwap.screenPlayers.playerSelections.First(selection => selection.playerNr == remotePlayerNr);
                            if (playerSelection != null) {
                                CharacterVariant variant = bundle.variantIdentifiers.First(identifier => identifier.variantNr == remoteVariantNr).variant;
                                player.variant = variant;
                                playerSelection.SetCharacter(player.Character, variant, true, GentleSwap.screenPlayers.playerSelections.Length);
                            }
                        }
                    }
                }             
            }
        }

    }
}
