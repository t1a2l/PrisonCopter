using ColossalFramework;
using HarmonyLib;
using System.Reflection;

namespace PrisonHelicopter.HarmonyPatches
{
    [HarmonyPatch]
    public static class CarAIPatch
    {
        private delegate bool StartPathFindPoliceCarAIDelegate(PoliceCarAI __instance, ushort vehicleID, ref Vehicle vehicleData);
        private static readonly StartPathFindPoliceCarAIDelegate StartPathFindPoliceCarAI = AccessTools.MethodDelegate<StartPathFindPoliceCarAIDelegate>(typeof(PoliceCarAI).GetMethod("StartPathFind", BindingFlags.Instance | BindingFlags.NonPublic, null, [typeof(ushort), typeof(Vehicle).MakeByRefType()], []), null, false);

        [HarmonyPatch(typeof(CarAI), "PathfindFailure")]
        [HarmonyPrefix]
        public static bool PathfindFailure(ushort vehicleID, ref Vehicle data)
        {
            Building police_building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding];
            if (data.Info.GetAI() is PoliceCarAI pcinstance)
            {
                var is_prison_van = false;
                var is_prison = false;
                var is_big_police_station = false;
                if (data.Info.GetClassLevel() >= ItemClass.Level.Level4) is_prison_van = true;
                if (police_building.Info.m_class.m_level >= ItemClass.Level.Level4) is_prison = true;
                if (police_building.Info.m_class.m_level < ItemClass.Level.Level4 && (police_building.m_flags & Building.Flags.Downgrading) != 0) is_big_police_station = true;
                if (is_prison_van && (data.m_flags & Vehicle.Flags.GoingBack) == 0 && (is_prison || is_big_police_station))
                {
                    data.m_flags |= Vehicle.Flags.GoingBack;
                    if (!StartPathFindPoliceCarAI(pcinstance, vehicleID, ref data))
                    {
                        data.Unspawn(vehicleID);
                    }
                }
                else
                {
                    data.Unspawn(vehicleID);
                }
                return false;
            }
            return true;
        }

    }
}
