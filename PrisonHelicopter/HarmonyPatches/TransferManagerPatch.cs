using HarmonyLib;
using System;

namespace PrisonHelicopter.HarmonyPatches {

    [HarmonyPatch(typeof(TransferManager))]
    public static class TransferManagerPatch {

        [HarmonyPatch(typeof(TransferManager), "Awake")]
        [HarmonyPostfix]
        public static void Awake()
	{
            bool isValueDefined1 = Enum.IsDefined(typeof(TransferManager.TransferReason), 120);
            if(!isValueDefined1)
            {
                TransferManager.TransferReason CriminalMove2 = (TransferManager.TransferReason)120;
                LogHelper.Information(CriminalMove2.ToString());
            }
            bool isValueDefined2 = Enum.IsDefined(typeof(TransferManager.TransferReason), 121);
            if(!isValueDefined2)
            {
                TransferManager.TransferReason CriminalMove3 = (TransferManager.TransferReason)121;
                LogHelper.Information(CriminalMove3.ToString());
            }
            bool isValueDefined3 = Enum.IsDefined(typeof(TransferManager.TransferReason), 122);
            if(!isValueDefined3)
            {
                TransferManager.TransferReason CriminalMove4 = (TransferManager.TransferReason)122;
                LogHelper.Information(CriminalMove4.ToString());
            }
            
	}

        [HarmonyPatch(typeof(TransferManager), "GetFrameReason")]
        [HarmonyPostfix]
        public static void GetFrameReason(int frameIndex, ref TransferManager.TransferReason __result)
	{
	    if(__result == TransferManager.TransferReason.None)
            {
                if(frameIndex == 101)
                {
                    __result = (TransferManager.TransferReason)120;
                }
                else if(frameIndex == 109)
                {
                    __result = (TransferManager.TransferReason)121;
                }
                else if(frameIndex == 117)
                {
                    __result = (TransferManager.TransferReason)122;
                }
            }
	}

        [HarmonyPatch(typeof(TransferManager), "GetDistanceMultiplier")]
        [HarmonyPostfix]
        public static void GetDistanceMultiplier(TransferManager.TransferReason material, ref float __result)
        {
            if(material == (TransferManager.TransferReason)120 || material == (TransferManager.TransferReason)121 || material == (TransferManager.TransferReason)122)
            {
                __result = 5E-07f;
            }
        } 
    }
}
