using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BepInEx;
using LLScreen;
using LLHandlers;

namespace GentleSwap {
    public static class Helpers {
        public static Character getECharacterFromString(string _charName) {
            switch (_charName.ToLower()) {
                case "bag": return Character.BAG;
                case "boom": return Character.BOOM;
                case "boss": return Character.BOSS;
                case "candy": return Character.CANDY;
                case "cop": return Character.COP;
                case "croc": return Character.CROC;
                case "electro": return Character.ELECTRO;
                case "graf": return Character.GRAF;
                case "kid": return Character.KID;
                case "pong": return Character.PONG;
                case "robot": return Character.ROBOT;
                case "skate": return Character.SKATE;
            }
            return Character.NONE;
        }

        public static List<int> exhaustedVariantNumbers = new List<int>();

        public static int[] GenerateDistinctIDs(int nReservedIDs) {
            List<int> newIDs = new List<int>();
            while (true) {
                int id = UnityEngine.Random.RandomRangeInt(200000, Int32.MaxValue);
                for (var i = 0; i < nReservedIDs; i++) if (!exhaustedVariantNumbers.Contains(id)) newIDs.Add(id + i);
                if (exhaustedVariantNumbers.Intersect(newIDs).ToList().Count == 0) break;
                newIDs.Clear();
            }

            exhaustedVariantNumbers.AddRange(newIDs);
            return newIDs.ToArray();
        }


    }

    
}
