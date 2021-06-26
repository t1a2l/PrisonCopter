using ColossalFramework;
using UnityEngine;
using ColossalFramework.Math;
using HarmonyLib;
using System;

namespace PrisonCopter {

    [HarmonyPatch(typeof(PoliceCopterAI))]
    static class PrisonCopterAI  {
        public static int m_policeCount = 2;
        public static int m_criminalCapacity = 3;

        private static SimulationStepHelicopterAIDelegate SimulationStepHelicopterAI;
        private static TryCollectCrimeDelegate TryCollectCrime;
        private static LoadVehicleHelicopterAIDelegate LoadVehicleHelicopterAI;
        private static RemoveTargetDelegate RemoveTarget;
        private static StartPathFindDelegate StartPathFind;
        private static EnsureCitizenUnitsVehicleAIDelegate EnsureCitizenUnitsVehicleAI;
        private static ShouldReturnToSourceDelegate ShouldReturnToSource;
        private static CalculateTargetPointHelicopterAIDelegate CalculateTargetPoint;

        public static void Prepare() {
            SimulationStepHelicopterAI = GameConnectionManager.Instance.PrisonCopterAIConnection.SimulationStepHelicopterAI;
            TryCollectCrime = GameConnectionManager.Instance.PrisonCopterAIConnection.TryCollectCrime;
            LoadVehicleHelicopterAI = GameConnectionManager.Instance.PrisonCopterAIConnection.LoadVehicleHelicopterAI;
            RemoveTarget = GameConnectionManager.Instance.PrisonCopterAIConnection.RemoveTarget;
            StartPathFind = GameConnectionManager.Instance.PrisonCopterAIConnection.StartPathFind;
            EnsureCitizenUnitsVehicleAI = GameConnectionManager.Instance.PrisonCopterAIConnection.EnsureCitizenUnitsVehicleAI;
            ShouldReturnToSource = GameConnectionManager.Instance.PrisonCopterAIConnection.ShouldReturnToSource;
            CalculateTargetPoint = GameConnectionManager.Instance.PrisonCopterAIConnection.CalculateTargetPoint;
        }

        [HarmonyPatch(typeof(PoliceCopterAI), "GetLocalizedStatus")]
        [HarmonyPrefix]
        public static bool GetLocalizedStatus(PoliceCopterAI __instance, ushort vehicleID, ref Vehicle data, out InstanceID target, ref string __result)
	{
                if (__instance.m_info.m_class.m_level >= ItemClass.Level.Level4)
		{
			if ((data.m_flags & Vehicle.Flags.GoingBack) != 0)
			{
			    target = InstanceID.Empty;
			    __result = ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_PRISON_RETURN");
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
			} else {
                            target = InstanceID.Empty;
			    __result = ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_CONFUSED");
                        }
			
		} else {
                    if ((data.m_flags & Vehicle.Flags.GoingBack) != 0)
		    {
			    target = InstanceID.Empty;
			    __result = ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_POLICE_COPTER_RETURN");
		    }
		    else if ((data.m_flags & Vehicle.Flags.WaitingTarget) != 0)
		    {
			    target = InstanceID.Empty;
			    __result = ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_POLICE_COPTER_PATROL_WAIT");
		    }
		    else if ((data.m_flags & Vehicle.Flags.Emergency2) != 0)
		    {
			    target = InstanceID.Empty;
			    __result = ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_POLICE_COPTER_EMERGENCY");
		    }
                    else {
                        target = InstanceID.Empty;
		         __result = ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_POLICE_COPTER_PATROL");
                    }
                }
                return false;
	}

        [HarmonyPatch(typeof(PoliceCopterAI), "GetBufferStatus")]
        [HarmonyPrefix]
        public static bool GetBufferStatus(PoliceCopterAI __instance, ushort vehicleID, ref Vehicle data, out string localeKey, out int current, out int max)
	{
		if (__instance.m_info.m_class.m_level >= ItemClass.Level.Level4)
		{
			localeKey = "PrisonCopter";
		}
		else
		{
			localeKey = "PoliceCopter";
		}
		current = data.m_transferSize;
		max = __instance.m_crimeCapacity;
		if ((data.m_flags & Vehicle.Flags.GoingBack) == 0 && current == max)
		{
			current = max - 1;
		}
                return false;
	}

        [HarmonyPatch(typeof(PoliceCopterAI), "CreateVehicle")]
        [HarmonyPostfix]
        public static void CreateVehicle(ushort vehicleID, ref Vehicle data)
	{
		Singleton<CitizenManager>.instance.CreateUnits(out data.m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, 0, vehicleID, 0, 0, 0, m_policeCount + m_criminalCapacity, 0);
        }

        [HarmonyPatch(typeof(PoliceCopterAI), "ReleaseVehicle")]
        [HarmonyPrefix]
        public static void Prefix(PoliceCopterAI __instance, ushort vehicleID, ref Vehicle data) {
            UnloadCriminals(__instance, vehicleID, ref data);
        }

        [HarmonyPatch(typeof(PoliceCopterAI), "LoadVehicle")]
        [HarmonyPrefix]
        public static bool LoadVehicle(PoliceCopterAI __instance, ushort vehicleID, ref Vehicle data) {
            LoadVehicleHelicopterAI(__instance, vehicleID, ref data);
            EnsureCitizenUnitsVehicleAI(__instance, vehicleID, ref data, m_policeCount + m_criminalCapacity);
            if (data.m_sourceBuilding != 0) {
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].AddOwnVehicle(vehicleID, ref data);
            }
            if (data.m_targetBuilding != 0) {
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding].AddGuestVehicle(vehicleID, ref data);
            }
            return false;
        }

        [HarmonyPatch(typeof(PoliceCopterAI), "SetTarget")]
        [HarmonyPrefix]
        public static bool SetTarget(PoliceCopterAI __instance, ushort vehicleID, ref Vehicle data, ushort targetBuilding)
	{
		RemoveTarget(__instance, vehicleID, ref data);
		data.m_targetBuilding = targetBuilding;
		data.m_flags &= ~Vehicle.Flags.WaitingTarget;
		data.m_waitCounter = 0;
		if (targetBuilding != 0)
		{
                    if (__instance.m_info.m_class.m_level < ItemClass.Level.Level4) {
			data.m_flags &= ~Vehicle.Flags.Landing;
			if (PoliceCopterAI.CountCriminals(targetBuilding) != 0)
			{
				data.m_flags |= Vehicle.Flags.Emergency2;
			}
			else
			{
				data.m_flags &= ~Vehicle.Flags.Emergency2;
			}
			Singleton<BuildingManager>.instance.m_buildings.m_buffer[targetBuilding].AddGuestVehicle(vehicleID, ref data);
                    }
		}
		else
		{
			data.m_flags &= ~Vehicle.Flags.Emergency2;
			if (data.m_transferSize < __instance.m_crimeCapacity && !ShouldReturnToSource(__instance, vehicleID, ref data))
			{
				TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
				offer.Priority = 7;
				offer.Vehicle = vehicleID;
				offer.Position = data.GetLastFramePosition();
				offer.Amount = 1;
				offer.Active = true;
				Singleton<TransferManager>.instance.AddIncomingOffer((TransferManager.TransferReason)data.m_transferType, offer);
				data.m_flags |= Vehicle.Flags.WaitingTarget;
			}
			else
			{
				data.m_flags &= ~Vehicle.Flags.Landing;
				data.m_flags |= Vehicle.Flags.GoingBack;
			}
		}
		if (!StartPathFind(__instance, vehicleID, ref data))
		{
			data.Unspawn(vehicleID);
		}
                return false;
	}

        [HarmonyPatch(typeof(PoliceCopterAI), "SimulationStep",
            new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vehicle.Frame), typeof(ushort), typeof(Vehicle), typeof(int) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
        [HarmonyPrefix]
        public static bool SimulationStep(PoliceCopterAI __instance, ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics) {
            if (__instance.m_info.m_class.m_level >= ItemClass.Level.Level4) {
                SimulationStepHelicopterAI(__instance, vehicleID, ref vehicleData, ref frameData, leaderID, ref leaderData, lodPhysics);
                if ((vehicleData.m_flags & Vehicle.Flags.Stopped) != 0 && __instance.CanLeave(vehicleID, ref vehicleData)) {
                    vehicleData.m_flags &= ~Vehicle.Flags.Stopped;
                    vehicleData.m_flags |= Vehicle.Flags.Leaving;
                }
                if ((vehicleData.m_flags & Vehicle.Flags.GoingBack) == 0 && ShouldReturnToSource(__instance, vehicleID, ref vehicleData)) {
                    SetTarget(__instance, vehicleID, ref vehicleData, 0);
                }
                return false;
            }
            frameData.m_blinkState = (((vehicleData.m_flags & Vehicle.Flags.Emergency2) == 0) ? 0f : 10f);
            TryCollectCrime(__instance, vehicleID, ref vehicleData, ref frameData);
            SimulationStepHelicopterAI(__instance, vehicleID, ref vehicleData, ref frameData, leaderID, ref leaderData, lodPhysics);
            if ((vehicleData.m_flags & Vehicle.Flags.Stopped) != 0) {
                if (__instance.CanLeave(vehicleID, ref vehicleData)) {
                    vehicleData.m_flags &= ~Vehicle.Flags.Stopped;
                    vehicleData.m_flags |= Vehicle.Flags.Leaving;
                }
            } else if ((vehicleData.m_flags & Vehicle.Flags.Arriving) != 0 && vehicleData.m_targetBuilding != 0 && (vehicleData.m_flags & (Vehicle.Flags.Emergency2 | Vehicle.Flags.WaitingPath | Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget)) == 0) {
                Boolean result = false;
                ArriveAtTarget(__instance, vehicleID, ref vehicleData, ref result);
            }
            if ((vehicleData.m_flags & Vehicle.Flags.GoingBack) == 0 && (vehicleData.m_transferSize >= __instance.m_crimeCapacity || ShouldReturnToSource(__instance, vehicleID, ref vehicleData))) {
                SetTarget(__instance, vehicleID, ref vehicleData, 0);
            }
            return false;
        }

        [HarmonyPatch(typeof(PoliceCopterAI), "ArriveAtTarget")]
        [HarmonyPrefix]
        private static bool ArriveAtTarget(PoliceCopterAI __instance, ushort vehicleID, ref Vehicle data, ref bool __result)
	{
		if (data.m_targetBuilding == 0)
		{
                        __result = true;
                        return false;
		}
		if (__instance.m_info.m_class.m_level >= ItemClass.Level.Level4)
		{
			ArrestCriminals(__instance, vehicleID, ref data, data.m_targetBuilding);
			data.m_flags |= Vehicle.Flags.Stopped;
		}
		else
		{
			int amountDelta = -__instance.m_crimeCapacity;
			BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding].Info;
			info.m_buildingAI.ModifyMaterialBuffer(data.m_targetBuilding, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding], (TransferManager.TransferReason)data.m_transferType, ref amountDelta);
		}
		__instance.SetTarget(vehicleID, ref data, 0);
                __result = false;
                return false;
	}

        [HarmonyPatch(typeof(PoliceCopterAI), "UpdateBuildingTargetPositions")]
        [HarmonyPrefix]
        public static bool UpdateBuildingTargetPositions(PoliceCopterAI __instance, ushort vehicleID, ref Vehicle vehicleData, Vector3 refPos, ushort leaderID, ref Vehicle leaderData, ref int index, float minSqrDistance)
	{
		if ((leaderData.m_flags & Vehicle.Flags.WaitingTarget) != 0)
		{
			return false;
		}
		if ((leaderData.m_flags & Vehicle.Flags.GoingBack) != 0)
		{
			if (leaderData.m_sourceBuilding != 0)
			{
				BuildingManager instance = Singleton<BuildingManager>.instance;
				BuildingInfo info = instance.m_buildings.m_buffer[leaderData.m_sourceBuilding].Info;
				Randomizer randomizer = new Randomizer(vehicleID);
				info.m_buildingAI.CalculateUnspawnPosition(vehicleData.m_sourceBuilding, ref instance.m_buildings.m_buffer[leaderData.m_sourceBuilding], ref randomizer, __instance.m_info, out var _, out var target);
				vehicleData.SetTargetPos(index++, CalculateTargetPoint(__instance, refPos, target, minSqrDistance, 0f));
			}
		}
		else if (((leaderData.m_flags & Vehicle.Flags.Emergency2) != 0 || __instance.m_info.m_class.m_level >= ItemClass.Level.Level4) && leaderData.m_targetBuilding != 0)
		{
			BuildingManager instance2 = Singleton<BuildingManager>.instance;
			BuildingInfo info2 = instance2.m_buildings.m_buffer[leaderData.m_targetBuilding].Info;
			Randomizer randomizer2 = new Randomizer(vehicleID);
			info2.m_buildingAI.CalculateUnspawnPosition(vehicleData.m_targetBuilding, ref instance2.m_buildings.m_buffer[leaderData.m_targetBuilding], ref randomizer2, __instance.m_info, out var _, out var target2);
			vehicleData.SetTargetPos(index++, CalculateTargetPoint(__instance, refPos, target2, minSqrDistance, info2.m_size.y + 10f));
		}
                return false;
	}

        private static void UnloadCriminals(PoliceCopterAI __instance, ushort vehicleID, ref Vehicle data)
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
			instance.m_citizens.m_buffer[citizen].CurrentLocation = Citizen.Location.Visit;
			if (__instance.m_info.m_class.m_level >= ItemClass.Level.Level4)
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
	}

        private static void SpawnPrisoner(PoliceCopterAI __instance, ushort vehicleID, ref Vehicle data, uint citizen)
	{
	    if (data.m_sourceBuilding != 0)
	    {
		SimulationManager instance = Singleton<SimulationManager>.instance;
		CitizenManager instance2 = Singleton<CitizenManager>.instance;
		Citizen.Gender gender = Citizen.GetGender(citizen);
		CitizenInfo groupCitizenInfo = instance2.GetGroupCitizenInfo(ref instance.m_randomizer, __instance.m_info.m_class.m_service, gender, Citizen.SubCulture.Generic, Citizen.AgePhase.Young0);
		if (groupCitizenInfo != null && instance2.CreateCitizenInstance(out var instance3, ref instance.m_randomizer, groupCitizenInfo, citizen))
		{
		    Vector3 randomDoorPosition = data.GetRandomDoorPosition(ref instance.m_randomizer, VehicleInfo.DoorType.Exit);
		    groupCitizenInfo.m_citizenAI.SetCurrentVehicle(instance3, ref instance2.m_instances.m_buffer[instance3], 0, 0u, randomDoorPosition);
		    groupCitizenInfo.m_citizenAI.SetTarget(instance3, ref instance2.m_instances.m_buffer[instance3], data.m_sourceBuilding);
		}
	    }
	}

        private static void ArrestCriminals(PoliceCopterAI __instance, ushort vehicleID, ref Vehicle vehicleData, ushort building)
	{
	    if (vehicleData.m_transferSize >= m_criminalCapacity)
	    {
                return;
	    }
	    BuildingManager instance = Singleton<BuildingManager>.instance;
	    CitizenManager instance2 = Singleton<CitizenManager>.instance;
	    uint num = instance.m_buildings.m_buffer[building].m_citizenUnits;
	    int num2 = 0;
	    while (num != 0)
	    {
		uint nextUnit = instance2.m_units.m_buffer[num].m_nextUnit;
		for (int i = 0; i < 5; i++)
		{
		    uint citizen = instance2.m_units.m_buffer[num].GetCitizen(i);
		    if (citizen != 0 && (instance2.m_citizens.m_buffer[citizen].Criminal || instance2.m_citizens.m_buffer[citizen].Arrested) && !instance2.m_citizens.m_buffer[citizen].Dead && instance2.m_citizens.m_buffer[citizen].GetBuildingByLocation() == building)
		    {
			instance2.m_citizens.m_buffer[citizen].SetVehicle(citizen, vehicleID, 0u);
			if (instance2.m_citizens.m_buffer[citizen].m_vehicle != vehicleID)
			{
			    vehicleData.m_transferSize = (ushort)m_criminalCapacity;
			    return;
			}
			instance2.m_citizens.m_buffer[citizen].Arrested = true;
			ushort instance3 = instance2.m_citizens.m_buffer[citizen].m_instance;
			if (instance3 != 0)
			{
			    instance2.ReleaseCitizenInstance(instance3);
			}
			instance2.m_citizens.m_buffer[citizen].CurrentLocation = Citizen.Location.Moving;
			if (++vehicleData.m_transferSize >= m_criminalCapacity)
			{
			    return;
			}
		    }
		}
		num = nextUnit;
		if (++num2 > 524288)
		{
		    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
		    break;
		}
	    }
	}
    }
}
