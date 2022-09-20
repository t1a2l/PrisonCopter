using HarmonyLib;

namespace PrisonHelicopter.HarmonyPatches {

    [HarmonyPatch(typeof(TransferManager))]
    public static class TransferManagerPatch {

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
