using ColossalFramework;
using HarmonyLib;

namespace PrisonHelicopter.HarmonyPatches
{
    [HarmonyPatch]
    public static class PoliceCarAIPatch
    {
        [HarmonyPatch(typeof(PoliceCarAI), "GetLocalizedStatus")]
        [HarmonyPrefix]
        public static bool GetLocalizedStatus(PoliceCarAI __instance, ushort vehicleID, ref Vehicle data, out InstanceID target, ref string __result)
        {
            target = InstanceID.Empty;
            if (__instance.m_info.m_class.m_level >= ItemClass.Level.Level4)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                BuildingInfo police_source = instance.m_buildings.m_buffer[data.m_sourceBuilding].Info;
                if ((data.m_flags & Vehicle.Flags.GoingBack) != 0)
                {
                    if (police_source.m_class.m_level < ItemClass.Level.Level4)
                    {
                        target.Building = data.m_sourceBuilding;
                        __result = "Transporting criminals to ";
                        return false;
                    }
                }
            }
            return true;
        }

    }
}
