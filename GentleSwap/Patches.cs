using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
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
                    GentleSwap.Log.LogDebug(__instance.dlc);
                    foreach (BundleHandler.CustomBundle bundle in BundleHandler.bundles) {
                        if (__instance.dlc == bundle.dlc) {
                            __result = "custom;" + Path.Combine(GentleSwap.customCharBundleDir.FullName,bundle.bundleName);
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(LLHandlers.BundleHandler), nameof(LLHandlers.BundleHandler.CLoadBundle))]
            [HarmonyPatch(MethodType.Enumerator, new Type[] { typeof(Bundle), typeof(BundleLoadType), typeof(string[]), typeof(Action), typeof(bool)})]
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> CLoadBundle_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iL)
            {
                CodeMatcher cm = new CodeMatcher(instructions, iL);

                cm.MatchForward(false, // false = move at the start of the match, true = move at the end of the match
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldsfld),
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldfld),
                        new CodeMatch(OpCodes.Call),
                        new CodeMatch(OpCodes.Stfld)
                        );
                cm.Advance(3);
                GentleSwap.Log.LogDebug(cm.Instruction);
                var bundleName_fld = cm.Operand;
                cm.Advance(2);
                GentleSwap.Log.LogDebug(cm.Instruction);
                var bundleFile_fld = cm.Operand;
                cm.Advance(1);
                try
                {
                    cm.Insert(
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, bundleName_fld),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, bundleFile_fld),
                        Transpilers.EmitDelegate<Func<string, string, string>>((string bundleName, string bundleFile) =>
                        {
                            GentleSwap.Log.LogDebug("Name: " + bundleName);
                            GentleSwap.Log.LogDebug("File: " + bundleFile);
                            if (bundleName.StartsWith("custom;"))
                            {
                                return bundleName.Split(';')[1];
                            }
                            else
                            {
                                return LLHandlers.BundleHandler.bundlePath + bundleName;
                            }
                        }),
                        new CodeInstruction(OpCodes.Stfld, bundleFile_fld)
                    );
                }
                catch (Exception e)
                {
                    GentleSwap.Log.LogError(e);
                }
                return cm.InstructionEnumeration();

            }


            [HarmonyPatch(typeof(LLHandlers.BundleHandler), nameof(LLHandlers.BundleHandler.GetAssetBundle))]
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> GetAssetBundle_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iL)
            {
                CodeMatcher cm = new CodeMatcher(instructions, iL);

                cm.MatchForward(false, // false = move at the start of the match, true = move at the end of the match
                        new CodeMatch(OpCodes.Ldsfld),
                        new CodeMatch(OpCodes.Ldarga_S),
                        new CodeMatch(OpCodes.Call),
                        new CodeMatch(OpCodes.Call),
                        new CodeMatch(OpCodes.Stloc_2)
                        );
                cm.Advance(1);
                GentleSwap.Log.LogDebug(cm.Instruction);
                var bundle_arga_s = cm.Operand;
                cm.Advance(1);
                GentleSwap.Log.LogDebug(cm.Instruction);
                var getBundleName_call = cm.Instruction;
                cm.Advance(3);
                GentleSwap.Log.LogDebug(cm.Instruction);
                try
                {
                    cm.Insert(
                        new CodeInstruction(OpCodes.Ldarga_S, bundle_arga_s),
                        new CodeInstruction(getBundleName_call),
                        new CodeInstruction(OpCodes.Ldloc_2),
                        Transpilers.EmitDelegate<Func<string, string, string>>((string bundleName, string bundleFile) =>
                        {
                            GentleSwap.Log.LogDebug("Name: " + bundleName);
                            GentleSwap.Log.LogDebug("File: " + bundleFile);
                            if (bundleName.StartsWith("custom;"))
                            {
                                return bundleName.Split(';')[1];
                            }
                            else
                            {
                                return LLHandlers.BundleHandler.bundlePath + bundleName;
                            }
                        }),
                        new CodeInstruction(OpCodes.Stloc_2)
                    );
                }
                catch (Exception e)
                {
                    GentleSwap.Log.LogError(e);
                }
                return cm.InstructionEnumeration();

            }
        }
    }
}
