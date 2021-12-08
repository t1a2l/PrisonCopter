using ColossalFramework;
using HarmonyLib;
using System;
using System.Reflection;

namespace PrisonHelicopter.HarmonyPatches {

    [HarmonyPatch(typeof(CarAI))]
    public static class CarAIPatch {

        private delegate bool StartPathFindDelegate(ushort vehicleID, ref Vehicle vehicleData);
        private static readonly StartPathFindDelegate StartPathFind = AccessTools.MethodDelegate<StartPathFindDelegate>(typeof(PoliceCarAI).GetMethod("StartPathFind", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, new ParameterModifier[0]), null, false);

        [HarmonyPatch(typeof(CarAI), "PathfindFailure")]
        [HarmonyPrefix]
        public static bool PathfindFailure(ushort vehicleID, ref Vehicle data)
	{
            Building police_building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding];
            if((police_building.Info.m_class.m_level >= ItemClass.Level.Level4 || police_building.Info.m_class.m_level < ItemClass.Level.Level4 && (police_building.m_flags & Building.Flags.Downgrading) == 0) && (data.m_flags & Vehicle.Flags.GoingBack) == 0)
            {
                data.m_flags |= Vehicle.Flags.GoingBack;
                if(!StartPathFind(vehicleID, ref data))
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

    }
}
