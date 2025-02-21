using System;
using System.Collections.Generic;
using System.IO;

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
