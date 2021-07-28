using ColossalFramework;
using HarmonyLib;
using System;
using UnityEngine;


namespace PrisonHelicopter.HarmonyPatches.PoliceCarAIPatch {

    [HarmonyPatch(typeof(PoliceCarAI))]
    public static class PoliceCarAIPatch {


        [HarmonyPatch(typeof(PoliceCarAI), "GetLocalizedStatus")]
        [HarmonyPrefix]
        public static bool GetLocalizedStatus(PoliceCarAI __instance, ushort vehicleID, ref Vehicle data, out InstanceID target, ref string __result)
	{
	    if (__instance.m_info.m_class.m_level >= ItemClass.Level.Level4)
	    {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                BuildingInfo police_source = instance.m_buildings.m_buffer[data.m_sourceBuilding].Info;
                BuildingInfo police_target = instance.m_buildings.m_buffer[data.m_targetBuilding].Info;
		if ((data.m_flags & Vehicle.Flags.GoingBack) != 0)
		{
                    target = InstanceID.Empty;
                    if(police_source.m_class.m_level < ItemClass.Level.Level4)
                    {
                        target.Building = data.m_sourceBuilding;
                        __result = "Transporting criminals to ";
                    }
                    else
                    {
                        __result = ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_PRISON_RETURN");
                    }  
		}
		else if ((data.m_flags & (Vehicle.Flags.Stopped | Vehicle.Flags.WaitingTarget)) != 0)
		{
		    target = InstanceID.Empty;
		    __result = ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_PRISON_WAIT");
		}
		else if (data.m_targetBuilding != 0)
                {
                    target = InstanceID.Empty;
                    target.Building = data.m_targetBuilding;
                    __result = ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_PRISON_PICKINGUP");
		}
                else
                {
                    target = InstanceID.Empty;
		    __result = ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_CONFUSED");
                }
                return false;
	    }
            else
            {
                if ((data.m_flags & Vehicle.Flags.GoingBack) != 0)
	        {
		    target = InstanceID.Empty;
		    __result =  ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_POLICE_RETURN");
	        }
	        else if ((data.m_flags & Vehicle.Flags.Stopped) != 0)
	        {
		    target = InstanceID.Empty;
		    __result =  ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_POLICE_STOPPED");
	        }
	        else if ((data.m_flags & Vehicle.Flags.WaitingTarget) != 0)
	        {
		    if ((data.m_flags & Vehicle.Flags.Leaving) != 0)
		    {
		        target = InstanceID.Empty;
		        __result =  ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_POLICE_STOP_WAIT");
		    }
                    else
                    {
                        target = InstanceID.Empty;
		        __result = ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_POLICE_PATROL_WAIT");
                    }   
	        }
	        else if ((data.m_flags & Vehicle.Flags.Emergency2) != 0)
	        {
		    if (data.m_targetBuilding != 0)
		    {
		        target = InstanceID.Empty;
		        target.Building = data.m_targetBuilding;
		        __result = ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_POLICE_EMERGENCY");
		    }
                    else
                    {
                        target = InstanceID.Empty;
		        __result = ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_CONFUSED");
                    }
	        }
                else
                {
                    target = InstanceID.Empty;
	            __result = ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_POLICE_PATROL");
                }
	        return false;
            }
	   
	}

        [HarmonyPatch(typeof(PoliceCarAI), "UnloadCriminals")]
        [HarmonyPrefix]
        public static bool UnloadCriminals(PoliceCarAI __instance, ushort vehicleID, ref Vehicle data)
	{
	    CitizenManager instance = Singleton<CitizenManager>.instance;
	    uint num = data.m_citizenUnits;
	    int num2 = 0;
	    int num3 = 0;
	    while (num != 0)
	    {
		uint nextUnit = instance.m_units.m_buffer[num].m_nextUnit;
		for (int i = 0; i < 5; i++)
		{
		    uint citizen = instance.m_units.m_buffer[num].GetCitizen(i);
		    if (citizen == 0 || instance.m_citizens.m_buffer[citizen].CurrentLocation != Citizen.Location.Moving || !instance.m_citizens.m_buffer[citizen].Arrested)
		    {
			continue;
		    }
		    ushort instance2 = instance.m_citizens.m_buffer[citizen].m_instance;
		    if (instance2 != 0)
		    {
			instance.ReleaseCitizenInstance(instance2);
		    }
		    instance.m_citizens.m_buffer[citizen].SetVisitplace(citizen, data.m_sourceBuilding, 0u);
		    if (instance.m_citizens.m_buffer[citizen].m_visitBuilding != 0)
		    {
                        BuildingManager binstance = Singleton<BuildingManager>.instance;
                        BuildingInfo police_building_info = binstance.m_buildings.m_buffer[data.m_sourceBuilding].Info;
			instance.m_citizens.m_buffer[citizen].CurrentLocation = Citizen.Location.Visit;
			if (__instance.m_info.m_class.m_level >= ItemClass.Level.Level4 && police_building_info.m_class.m_level >= ItemClass.Level.Level4)
			{
			    SpawnPrisoner(__instance, vehicleID, ref data, citizen);
			}
		    }
		    else
		    {
			instance.m_citizens.m_buffer[citizen].CurrentLocation = Citizen.Location.Home;
			instance.m_citizens.m_buffer[citizen].Arrested = false;
			num3++;
		    }
		}
		num = nextUnit;
		if (++num2 > 524288)
		{
		    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
		    break;
		}
	    }
	    data.m_transferSize = 0;
	    if (num3 != 0 && data.m_sourceBuilding != 0)
	    {
		BuildingManager instance3 = Singleton<BuildingManager>.instance;
		DistrictManager instance4 = Singleton<DistrictManager>.instance;
		byte district = instance4.GetDistrict(instance3.m_buildings.m_buffer[data.m_sourceBuilding].m_position);
		instance4.m_districts.m_buffer[district].m_productionData.m_tempCriminalExtra += (uint)num3;
	    }
            return false;
	}

        private static void SpawnPrisoner(PoliceCarAI __instance, ushort vehicleID, ref Vehicle data, uint citizen)
	{
	    if (data.m_sourceBuilding != 0)
	    {
		SimulationManager instance = Singleton<SimulationManager>.instance;
		CitizenManager instance2 = Singleton<CitizenManager>.instance;
		Citizen.Gender gender = Citizen.GetGender(citizen);
		CitizenInfo groupCitizenInfo = instance2.GetGroupCitizenInfo(ref instance.m_randomizer, __instance.m_info.m_class.m_service, gender, Citizen.SubCulture.Generic, Citizen.AgePhase.Young0);
		if ((object)groupCitizenInfo != null && instance2.CreateCitizenInstance(out var instance3, ref instance.m_randomizer, groupCitizenInfo, citizen))
		{
		    Vector3 randomDoorPosition = data.GetRandomDoorPosition(ref instance.m_randomizer, VehicleInfo.DoorType.Exit);
		    groupCitizenInfo.m_citizenAI.SetCurrentVehicle(instance3, ref instance2.m_instances.m_buffer[instance3], 0, 0u, randomDoorPosition);
		    groupCitizenInfo.m_citizenAI.SetTarget(instance3, ref instance2.m_instances.m_buffer[instance3], data.m_sourceBuilding);
		}
	    }
	}

    }
}
