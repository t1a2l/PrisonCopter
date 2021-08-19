using HarmonyLib;

namespace PrisonHelicopter.HarmonyPatches
{
    [HarmonyPatch(typeof(PackageHelper), "ResolveLegacyTypeHandler")]
    static class OldAssetsCompatibilityPatch
    {
        // 'PrisonHelicopter.AI.NewPoliceStationAI.NewPoliceStationAI, PrisonHelicopter, Version=1.0.7890.49, Culture=neutral, PublicKeyToken=null'
        [HarmonyPostfix]
        public static void Postfix(ref string __result)
	{
            string[] temp = __result.Split(',');
            if(temp.Length >= 2 && temp[1] == " PrisonHelicopter")
            {
                if(temp[0] == "PrisonHelicopter.AI.NewPoliceStationAI.NewPoliceStationAI")
                {
                    __result = "PrisonHelicopter.AI.NewPoliceStationAI, PrisonHelicopter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
                }
            }
	    LogHelper.Information(__result);
	}
    }
}
