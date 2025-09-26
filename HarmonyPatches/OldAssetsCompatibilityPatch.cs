using HarmonyLib;
using PrisonHelicopter.Utils;

namespace PrisonHelicopter.HarmonyPatches
{
    [HarmonyPatch]
    public static class OldAssetsCompatibilityPatch
    {
        // 'PrisonHelicopter.AI.PrisonCopterPoliceStationAI.PrisonCopterPoliceStationAI, PrisonHelicopter, Version=1.0.7890.49, Culture=neutral, PublicKeyToken=null'
        [HarmonyPatch(typeof(PackageHelper), "ResolveLegacyTypeHandler")]
        [HarmonyPostfix]
        public static void Postfix(ref string __result)
        {
            string[] temp = __result.Split(',');
            if (temp.Length >= 2 && temp[1] == " PrisonHelicopter")
            {
                if (temp[0] == "PrisonHelicopter.AI.PrisonCopterPoliceStationAI.PrisonCopterPoliceStationAI")
                {
                    __result = "PrisonHelicopter.AI.PrisonCopterPoliceStationAI, PrisonHelicopter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
                }
                else if (temp[0] == "PrisonHelicopter.AI.NewPoliceStationAI")
                {
                    __result = "PrisonHelicopter.AI.PrisonCopterPoliceStationAI, PrisonHelicopter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
                }

                LogHelper.Information(__result);
            }
        }
    }
}
