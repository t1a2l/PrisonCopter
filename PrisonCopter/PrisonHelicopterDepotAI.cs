using HarmonyLib;

namespace PrisonCopter {

    [HarmonyPatch(typeof(HelicopterDepotAI))]
    public static class PrisonHelicopterDepotAI
    {

        [HarmonyPatch(typeof(HelicopterDepotAI), "GetTransferReason1")]
        [HarmonyPrefix]
        public static bool GetTransferReason1(HelicopterDepotAI __instance, ref TransferManager.TransferReason __result) {
            if (__instance.m_info.m_class.m_service == ItemClass.Service.HealthCare) {
                __result = TransferManager.TransferReason.Sick2;
            } else if (__instance.m_info.m_class.m_service == ItemClass.Service.PoliceDepartment && __instance.m_info.m_class.m_level >= ItemClass.Level.Level4) {
                __result = TransferManager.TransferReason.CriminalMove;
            } else if (__instance.m_info.m_class.m_service == ItemClass.Service.PoliceDepartment) {
                __result = TransferManager.TransferReason.Crime;
            } else if (__instance.m_info.m_class.m_service == ItemClass.Service.FireDepartment) {
                __result = TransferManager.TransferReason.ForestFire;
            } else {
                __result = TransferManager.TransferReason.None;
            }
            return false;
        }
    }
}
