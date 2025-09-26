using HarmonyLib;

namespace PrisonHelicopter.HarmonyPatches
{
    [HarmonyPatch]
    public static class TransferManagerPatch
    {
        [HarmonyPatch(typeof(TransferManager), "GetFrameReason")]
        [HarmonyPrefix]
        public static bool GetFrameReason(TransferManager __instance, int frameIndex, ref TransferManager.TransferReason __result)
        {
            if(frameIndex == 0 || frameIndex == 2 || frameIndex == 4)
            {
                if(frameIndex == 0)
                {
                    __result = PrisonHelicopterMod.PoliceVanCriminalMove;
                }
                else if(frameIndex == 2)
                {
                    __result = PrisonHelicopterMod.PrisonHelicopterCriminalPickup;
                }
                else
                {
                    __result = PrisonHelicopterMod.PrisonHelicopterCriminalMove;
                }
                return false;
            }
            return true;
        }
    }
}
