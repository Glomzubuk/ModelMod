using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HarmonyLib;
using LLHandlers;
using LLBML;
using LLBML.States;
using LLBML.Players;
using Multiplayer;
using LLScreen;

namespace GentleSwap {
    public static class Patches {

        public static class EPCDKLCABNC_Progress_Patches { //EPCDKLCABNC
            [HarmonyPatch(typeof(EPCDKLCABNC), "KFFJOEAJLEH", typeof(Character), typeof(CharacterVariant), typeof(int))] //IsUnlocked
            [HarmonyPostfix]

            static bool ReturnTrueIfReferenced(bool incoming, Character __0, CharacterVariant __1) {
                Character character_ref = __0;
                CharacterVariant variant_ref = __1;

                foreach (BundleHandler.CustomBundle bundle in BundleHandler.bundles) {
                    if (character_ref == bundle.character) {
                        foreach (BundleHandler.VariantIdentifier variantIdentifier in bundle.variantIdentifiers) {
                            if (variant_ref == variantIdentifier.variant) return true;
                        }
                    }
                }

                return incoming;
            }

            [HarmonyPatch(typeof(EPCDKLCABNC), "LEMKFOAAMKA")]
            [HarmonyPostfix]

            static DLC LEMKFOAAMKA(DLC incoming, Character __0, CharacterVariant __1) {
                Character character_ref = __0;
                CharacterVariant variant_ref = __1;

                foreach (BundleHandler.CustomBundle bundle in BundleHandler.bundles) {
                    if (character_ref == bundle.character) {
                        foreach (BundleHandler.VariantIdentifier variantIdentifier in bundle.variantIdentifiers) {
                            if (variant_ref == variantIdentifier.variant) return bundle.dlc;
                        }
                    }
                }

                return incoming;
            }


            [HarmonyPatch(typeof(EPCDKLCABNC), "CPNGJKMEOOM")] //SkinName
            [HarmonyPostfix]

            static string GetSkinName(string incoming, Character __0, int __1) {
                Character character_ref = __0;
                int skinNum = __1;

                if (skinNum > 12) {
                    int customSkinID = skinNum - 13;
                    foreach (BundleHandler.CustomBundle bundle in BundleHandler.bundles) {
                        if (character_ref == bundle.character) {
                            foreach (BundleHandler.VariantIdentifier variantIdentifier in bundle.variantIdentifiers) {
                                if (variantIdentifier.variantNr == customSkinID) return $"{bundle.showcaseName}: {variantIdentifier.skinName}";
                            }
                        }
                    }
                }

                return incoming;
            }

            [HarmonyPatch(typeof(EPCDKLCABNC), "LJBIMAPKPME")] //GetUnlockedVariants
            [HarmonyPostfix]

            static List<CharacterVariant> GetUnlockedVariantsPatch(List<CharacterVariant> incoming, Character __0, int __1) {
                Character character_ref = __0;
                int peer = __1;

                foreach (BundleHandler.CustomBundle bundle in BundleHandler.bundles) {
                    if (character_ref == bundle.character) {
                        foreach (BundleHandler.VariantIdentifier variantIdentifier in bundle.variantIdentifiers) {
                            incoming.Add(variantIdentifier.variant);
                        }
                    }
                }

                return incoming;
            }

            [HarmonyPatch(typeof(EPCDKLCABNC), "GPBCPJNCEPF")] //Get number of skins to assign to skins screen
            [HarmonyPostfix]

            static int GPBCPJNCEPF_Patch(int __result, Character __0) {
                Character character_ref = __0;
                int numOfCustomVariants = 0;

                foreach (BundleHandler.CustomBundle bundle in BundleHandler.bundles) {
                    if (character_ref == bundle.character) {
                        numOfCustomVariants += bundle.variantIdentifiers.Count();
                    }
                }

                return __result + numOfCustomVariants;
            }
        }

        public static class Bundle_Patches {
            [HarmonyPatch(typeof(Bundle), "GetBundleName")]
            [HarmonyPostfix]

            static void GetBundleString(ref Bundle __instance, ref string __result) {
                if (__instance.bundleType == BundleType.DLC) {
                    Debug.Log(__instance.dlc);
                    foreach (BundleHandler.CustomBundle bundle in BundleHandler.bundles) {
                        if (__instance.dlc == bundle.dlc) {
                            __result = $"characters/custom/{bundle.bundleName}";
                        }
                    }
                }
            }
        }

        

    }
}
