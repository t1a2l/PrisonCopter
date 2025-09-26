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
            if(frameIndex == 146 || frameIndex == 148 || frameIndex == 150)
            {
                if(frameIndex == 146)
                {
                    __result = PrisonHelicopterMod.PoliceVanCriminalMove;
                }
                else if(frameIndex == 148)
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
