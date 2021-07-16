using HarmonyLib;
using System;
using System.Reflection;
using ColossalFramework;
using UnityEngine;

namespace PrisonHelicopter.HarmonyPatches.TransferManagerPatch {

    [HarmonyPatch(typeof(TransferManager))]
    public static class TransferManagerPatch {

        [HarmonyPatch(typeof(TransferManager), "Awake")]
        [HarmonyPostfix]
        public static void Awake()
	{
            bool isValueDefined = Enum.IsDefined(typeof(TransferManager.TransferReason), 127);
            if(!isValueDefined)
            {
                TransferManager.TransferReason CriminalMove2 = (TransferManager.TransferReason)127;
            }
	}

        [HarmonyPatch(typeof(TransferManager), "GetFrameReason")]
        [HarmonyPostfix]
        public static void GetFrameReason(int frameIndex, ref TransferManager.TransferReason __result)
	{
	    if(__result == TransferManager.TransferReason.None)
            {
                if(frameIndex == 117)
                {
                    __result = (TransferManager.TransferReason)127;
                }
            }
	}

        [HarmonyPatch(typeof(TransferManager), "GetDistanceMultiplier")]
        [HarmonyPostfix]
        public static void GetDistanceMultiplier(TransferManager.TransferReason material, ref float __result)
        {
            if(material == (TransferManager.TransferReason)127)
            {
                __result = 5E-07f;
            }
        } 
    }
}
