using HarmonyLib;
using System;

namespace PrisonHelicopter.HarmonyPatches {

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
            bool isValueDefined2 = Enum.IsDefined(typeof(TransferManager.TransferReason), 125);
            if(!isValueDefined2)
            {
                TransferManager.TransferReason CriminalMove3 = (TransferManager.TransferReason)125;
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
                else if(frameIndex == 101)
                {
                    __result = (TransferManager.TransferReason)125;
                }
            }
	}

        [HarmonyPatch(typeof(TransferManager), "GetDistanceMultiplier")]
        [HarmonyPostfix]
        public static void GetDistanceMultiplier(TransferManager.TransferReason material, ref float __result)
        {
            if(material == (TransferManager.TransferReason)126 || material == (TransferManager.TransferReason)125)
            {
                __result = 5E-07f;
            }
        } 
    }
}
