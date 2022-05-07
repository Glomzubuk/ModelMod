using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Text;

namespace GentleSwap {
    public static class BundleHandler {

        public static List<CustomBundle> bundles = new List<CustomBundle>();
        public static KIIIINKJKNI kiiiinkjkni;


        public static void ImportCustomBundles() {
            FileInfo[] files = GentleSwap.customCharBundleDir.GetFiles();
            foreach (FileInfo file in files) {
                if (file.Extension != ".json") {
                    bundles.Add(new CustomBundle(file.Name));
                }
            }
            if (bundles.Count > 0) {
                GentleSwap.Log.LogInfo($"Imported data for {bundles.Count} {(bundles.Count > 1 ? "custombundles" : "custombundle")}");
            }

        }


        public static void AddCustomDLCToAccount() {
            foreach (CustomBundle bundle in bundles) {
                if (!kiiiinkjkni.LKGIPDAIFGA.TryGetValue(bundle.dlc, out bool added)) kiiiinkjkni.LKGIPDAIFGA.Add(bundle.dlc, true);
            }
        }


        public class CustomBundle {
            public string bundlePath;
            public string bundleName;
            public int bundleId;
            public Character character;
            public string showcaseName;
            public List<VariantIdentifier> variantIdentifiers = new List<VariantIdentifier>();
            public DLC dlc;
            public bool error = false;


            public CustomBundle(string _bundleName) {
                bundlePath = Path.Combine(GentleSwap.customCharBundleDir.FullName, _bundleName);
                bundleName = _bundleName;

                string json;
                try {
                    bundleId = Helpers.GenerateDistinctIDs(1)[0];
                    json = File.ReadAllText(GentleSwap.customCharBundleDir.GetFiles().First(item => item.Name.EndsWith(_bundleName + ".json")).FullName);
                    JObject data = JObject.Parse(json);

                    dlc = (DLC)bundleId;
                    character = Helpers.getECharacterFromString(data["character"].ToString());
                    showcaseName = data["showcaseName"].ToString();

                    int numOfVariants = (int)data["variants"];
                    int[] variantIDs = Helpers.GenerateDistinctIDs(numOfVariants);
                    List<JPLELOFJOOH.NCBHPNHFLAJ> variantInfos = JPLELOFJOOH.LKIFMPEFNGB.ToList();
                    for (var i = 0; i < numOfVariants; i++) {
                        string newName = (string)data["variantNames"][i];
                        CharacterVariant newVariant = (CharacterVariant)variantIDs[i];

                        VariantIdentifier variantIdentifier = new VariantIdentifier(newName, newVariant, i);
                        variantIdentifiers.Add(variantIdentifier);
                        if (i == 0) variantInfos.Add(new JPLELOFJOOH.NCBHPNHFLAJ(character, newVariant, (string)data["prefabName"], $"{(string)data["prefabName"]}Mat", dlc));
                        else if (i > 0 && i < 11) variantInfos.Add(new JPLELOFJOOH.NCBHPNHFLAJ(character, newVariant, (string)data["prefabName"], $"{(string)data["prefabName"]}Mat_Alt0{i - 1}", dlc));
                        else variantInfos.Add(new JPLELOFJOOH.NCBHPNHFLAJ(character, newVariant, (string)data["prefabName"], $"{(string)data["prefabName"]}Mat_Alt{i - 1}", dlc));
                    }
                    JPLELOFJOOH.LKIFMPEFNGB = variantInfos.ToArray();


                    List<JPLELOFJOOH.GHKGDLBCFPK> meshInfos = JPLELOFJOOH.OGAHHGABFPE.ToList();
                    meshInfos.Add(new JPLELOFJOOH.GHKGDLBCFPK((string)data["prefabName"], (float)data["meshInfo"]["meshScale"], (int)data["meshInfo"]["meshOffset"]));
                    JPLELOFJOOH.OGAHHGABFPE = meshInfos.ToArray();
                }
                catch (Exception ex) {
                    GentleSwap.Log.LogError($"Couldn't find a config file for {_bundleName} or it is wrongly configured");
                    GentleSwap.Log.LogError(ex);
                    error = true;
                }

                if (error == false) GentleSwap.Log.LogInfo($"Loaded bundle {_bundleName}..");
            }
        }

        public class VariantIdentifier {
            public string skinName;
            public CharacterVariant variant;
            public int variantNr; //Number of the variant for the skin EG: 0,1,2,3,4
            public int orderNr; 


            public VariantIdentifier(string _skinName, CharacterVariant _variant, int _variantNr) {
                skinName = _skinName;
                variant = _variant;
                variantNr = _variantNr;
            }
        }
    }
}
