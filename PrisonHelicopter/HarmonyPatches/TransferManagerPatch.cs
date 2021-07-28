using HarmonyLib;
using System;

namespace PrisonHelicopter.HarmonyPatches.TransferManagerPatch {

    [HarmonyPatch(typeof(TransferManager))]
    public static class TransferManagerPatch {

        [HarmonyPatch(typeof(TransferManager), "Awake")]
        [HarmonyPostfix]
        public static void Awake()
	{
            bool isValueDefined1 = Enum.IsDefined(typeof(TransferManager.TransferReason), 126);
            if(!isValueDefined1)
            {
                TransferManager.TransferReason CriminalMove2 = (TransferManager.TransferReason)126;
            }
	}

        [HarmonyPatch(typeof(TransferManager), "GetFrameReason")]
        [HarmonyPostfix]
        public static void GetFrameReason(int frameIndex, ref TransferManager.TransferReason __result)
	{
	    if(__result == TransferManager.TransferReason.None)
            {
                if(frameIndex == 109)
                {
                    __result = (TransferManager.TransferReason)126;
                }
            }
	}

        [HarmonyPatch(typeof(TransferManager), "GetDistanceMultiplier")]
        [HarmonyPostfix]
        public static void GetDistanceMultiplier(TransferManager.TransferReason material, ref float __result)
        {
            if(material == (TransferManager.TransferReason)126)
            {
                __result = 5E-07f;
            }
        } 
    }
}
