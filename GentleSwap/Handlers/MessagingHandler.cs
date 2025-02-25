using LLBML.Players;
using LLBML.Messages;
using LLBML.States;
using System.Linq;
using System.Text;

namespace GentleSwap {
    public static class MessagingHandler {
        public static void RegisterCustomMessages() {
            PlayerLobbyState.RegisterPayload(GentleSwap.Instance.Info, OnSendState, OnReceiveState);
        }

        public static byte[] OnSendState(PlayerLobbyState pls) {
            var bundVarTup = BundleHandler.GetBundlesAndVariantsFor(pls.character, pls.variant).FirstOrDefault();
            if (bundVarTup != null)
            {
                return Encoding.Default.GetBytes(bundVarTup.Item1.bundleName);
            }
            else
            {
                return null;
            }
        }

        public static void OnReceiveState(PlayerLobbyState pls, byte[] payload) {
            int remotePlayerNr = pls.playerNr;
            CharacterVariant remoteVariant = pls.variant;
            string remoteBundleName = Encoding.Default.GetString(payload);

            Player player = Player.GetPlayer(remotePlayerNr);
            GentleSwap.Log.LogDebug(remoteBundleName);
            foreach (CustomBundle bundle in BundleHandler.bundles) {
                if (remoteBundleName == bundle.bundleName) {
                    PlayersSelection playerSelection = GentleSwap.screenPlayers?.playerSelections.First(selection => selection.playerNr == remotePlayerNr);
                    if (playerSelection != null)
                    {
                        var vi = bundle.variantIdentifiers.FirstOrDefault(identifier => identifier.variant == remoteVariant);
                        if (vi != null)
                        {
                            player.variant = vi.variant;
                            playerSelection.SetCharacter(player.Character, vi.variant, true, GentleSwap.screenPlayers.playerSelections.Length);
                        }
                    }
                }
            }
        }
    }
}
