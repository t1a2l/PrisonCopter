using ColossalFramework;
using UnityEngine;
using ColossalFramework.Math;
using System;

namespace PrisonHelicopter.AI {

    public class PrisonHelicopterAI :  HelicopterAI {

        public int m_policeCount = 2;

        [CustomizableProperty("Criminal capacity")]
        public int m_criminalCapacity = 10;

        public override Color GetColor(ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode)
	{
	    if (infoMode == InfoManager.InfoMode.CrimeRate)
	    {
		return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColor;
	    }
	    return base.GetColor(vehicleID, ref data, infoMode);
	}

        public override string GetLocalizedStatus(ushort vehicleID, ref Vehicle data, out InstanceID target)
	{
            uint arrestedCitizen = GetArrestedCitizen(vehicleID, ref data);
	    bool flag = false;
	    if (arrestedCitizen != 0)
	    {
		flag = Singleton<CitizenManager>.instance.m_citizens.m_buffer[arrestedCitizen].CurrentLocation == Citizen.Location.Moving;
	    }
            if(flag) {
                if ((data.m_flags & Vehicle.Flags.GoingBack) != 0)
	        {
		    target = InstanceID.Empty;
		    return "Returning to depot";
	        }
	        if ((data.m_flags & (Vehicle.Flags.Stopped | Vehicle.Flags.WaitingTarget)) != 0)
	        {
		    target = InstanceID.Empty;
		    return "Loading criminals";
	        }
	        if ((data.m_flags & Vehicle.Flags.Emergency2) != 0)
	        {
		    target = InstanceID.Empty;
		    target.Building = data.m_targetBuilding;
		    return "Transporting criminals to ";
	        } 
            }
            else
            {
                if ((data.m_flags & Vehicle.Flags.GoingBack) != 0)
		{
		    target = InstanceID.Empty;
		    return "Returning to depot";
		}
		if ((data.m_flags & Vehicle.Flags.WaitingTarget) != 0)
		{
		    target = InstanceID.Empty;
		    return "Making flight plan";
		}
		if ((data.m_flags & Vehicle.Flags.Emergency2) != 0)
		{
		    target = InstanceID.Empty;
		    target.Building = data.m_targetBuilding;
		    return "Picking up criminals from ";
		}
            }
            target = InstanceID.Empty;
	    return ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_CONFUSED");
	}

        public override void GetBufferStatus(ushort vehicleID, ref Vehicle data, out string localeKey, out int current, out int max)
	{
            localeKey = "PrisonVan";
            current = data.m_transferSize;
            max = m_criminalCapacity;
	    if ((data.m_flags & Vehicle.Flags.GoingBack) == 0 && current == max)
	    {
		current = max - 1;
	    }
	}

        public override void CreateVehicle(ushort vehicleID, ref Vehicle data)
	{
            base.CreateVehicle(vehicleID, ref data);
	    data.m_flags |= Vehicle.Flags.WaitingTarget;
            Singleton<CitizenManager>.instance.CreateUnits(out data.m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, 0, vehicleID, 0, 0, 0, m_policeCount + m_criminalCapacity, 0);   
        }

        public override void ReleaseVehicle(ushort vehicleID, ref Vehicle data)
        {
            UnloadCriminals(vehicleID, ref data);
            RemoveOffers(vehicleID, ref data);
	    RemoveSource(vehicleID, ref data);
	    RemoveTarget(vehicleID, ref data);
	    base.ReleaseVehicle(vehicleID, ref data);
        }

        public override void SimulationStep(ushort vehicleID, ref Vehicle data, Vector3 physicsLodRefPos)
	{
	    if ((data.m_flags & Vehicle.Flags.WaitingTarget) != 0 && ++data.m_waitCounter > 20)
	    {
		RemoveOffers(vehicleID, ref data);
		data.m_flags &= ~(Vehicle.Flags.Emergency2 | Vehicle.Flags.Landing | Vehicle.Flags.WaitingTarget);
		data.m_flags |= Vehicle.Flags.GoingBack;
		data.m_waitCounter = 0;
		if (!StartPathFind(vehicleID, ref data))
		{
		    data.Unspawn(vehicleID);
		}
	    }
	    base.SimulationStep(vehicleID, ref data, physicsLodRefPos);
	}

        public override void LoadVehicle(ushort vehicleID, ref Vehicle data)
        {
            base.LoadVehicle(vehicleID, ref data);
            EnsureCitizenUnits(vehicleID, ref data, m_policeCount + m_criminalCapacity);
            if (data.m_sourceBuilding != 0)
            {
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].AddOwnVehicle(vehicleID, ref data);
            }
            if (data.m_targetBuilding != 0)
            {
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding].AddGuestVehicle(vehicleID, ref data);
            }
        }

        public override void SetSource(ushort vehicleID, ref Vehicle data, ushort sourceBuilding)
	{
	    RemoveSource(vehicleID, ref data);
	    data.m_sourceBuilding = sourceBuilding;
	    if (sourceBuilding != 0)
	    {
		BuildingManager instance = Singleton<BuildingManager>.instance;
		BuildingInfo info = instance.m_buildings.m_buffer[sourceBuilding].Info;
		data.Unspawn(vehicleID);
		Randomizer randomizer = new Randomizer(vehicleID);
		info.m_buildingAI.CalculateSpawnPosition(sourceBuilding, ref instance.m_buildings.m_buffer[sourceBuilding], ref randomizer, m_info, out var position, out var target);
		Quaternion rotation = Quaternion.identity;
		Vector3 forward = target - position;
		if (forward.sqrMagnitude > 0.01f)
		{
			rotation = Quaternion.LookRotation(forward);
		}
		data.m_frame0 = new Vehicle.Frame(position, rotation);
		data.m_frame1 = data.m_frame0;
		data.m_frame2 = data.m_frame0;
		data.m_frame3 = data.m_frame0;
		data.m_targetPos0 = position;
		data.m_targetPos0.w = 0f;
		data.m_targetPos1 = target;
		data.m_targetPos1.w = 0f;
		data.m_targetPos2 = data.m_targetPos1;
		data.m_targetPos3 = data.m_targetPos1;
		FrameDataUpdated(vehicleID, ref data, ref data.m_frame0);
		Singleton<BuildingManager>.instance.m_buildings.m_buffer[sourceBuilding].AddOwnVehicle(vehicleID, ref data);
	    }
	}

        public override void SetTarget(ushort vehicleID, ref Vehicle data, ushort targetBuilding)
	{
	    RemoveTarget(vehicleID, ref data);
	    data.m_targetBuilding = targetBuilding;
	    data.m_flags &= ~Vehicle.Flags.WaitingTarget;
	    data.m_waitCounter = 0;
            BuildingManager instance = Singleton<BuildingManager>.instance;
            BuildingInfo building_info = instance.m_buildings.m_buffer[targetBuilding].Info;
	    if (targetBuilding != 0 && building_info.GetAI() is NewPoliceStationAI newPoliceStationAI)
	    {
                if((building_info.m_class.m_level < ItemClass.Level.Level4 && newPoliceStationAI.JailCapacity >= 60 && data.m_transferSize == 0) ||  (building_info.m_class.m_level >= ItemClass.Level.Level4 && data.m_transferSize > 0))
                {
                    data.m_flags &= ~Vehicle.Flags.Landing;
                    data.m_flags |= Vehicle.Flags.Emergency2;
                    Singleton<BuildingManager>.instance.m_buildings.m_buffer[targetBuilding].AddGuestVehicle(vehicleID, ref data);
                } else {
                    return;
                }
	    }
            else if(ShouldReturnToSource(vehicleID, ref data))
            {
                data.m_flags &= ~Vehicle.Flags.Landing;
		data.m_flags |= Vehicle.Flags.GoingBack;
            }
            else if(GetArrestedCitizen(vehicleID, ref data) != 0) 
            {
		data.m_flags &= ~Vehicle.Flags.Emergency2;
		TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
		offer.Priority = 7;
		offer.Vehicle = vehicleID;
		offer.Position = data.GetLastFramePosition();
		offer.Amount = 1;
		offer.Active = true;
		Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.CriminalMove, offer);
		data.m_flags |= Vehicle.Flags.WaitingTarget;
	    }
            else
            {
                data.m_flags &= ~Vehicle.Flags.Emergency2;
		TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
		offer.Priority = 7;
		offer.Vehicle = vehicleID;
		offer.Position = data.GetLastFramePosition();
		offer.Amount = 1;
		offer.Active = true;
		Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.CriminalMove, offer);
		data.m_flags |= Vehicle.Flags.WaitingTarget;
            }
	    if (!StartPathFind(vehicleID, ref data))
	    {
		data.Unspawn(vehicleID);
	    }
	}

        public override void BuildingRelocated(ushort vehicleID, ref Vehicle data, ushort building)
	{
	    base.BuildingRelocated(vehicleID, ref data, building);
	    if (building == data.m_sourceBuilding)
	    {
		if ((data.m_flags & Vehicle.Flags.GoingBack) != 0)
		{
		    InvalidPath(vehicleID, ref data, vehicleID, ref data);
		}
	    }
	    else if (building == data.m_targetBuilding && (data.m_flags & Vehicle.Flags.GoingBack) == 0)
	    {
		InvalidPath(vehicleID, ref data, vehicleID, ref data);
	    }
	}

        public override void StartTransfer(ushort vehicleID, ref Vehicle data, TransferManager.TransferReason material, TransferManager.TransferOffer offer)
	{
	    if (material == (TransferManager.TransferReason)data.m_transferType)
	    {
		if ((data.m_flags & Vehicle.Flags.WaitingTarget) != 0)
		{
                    SetTarget(vehicleID, ref data, offer.Building);  
		}
	    }
	    else
	    {
		(this as VehicleAI).StartTransfer(vehicleID, ref data, material, offer);
	    }
	}

        public override void GetSize(ushort vehicleID, ref Vehicle data, out int size, out int max)
	{
	    size = data.m_transferSize; // how many people needs to be transffered
            max = m_criminalCapacity; // how many places inside the vehicle
	}

        public override void SimulationStep(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics) {
            base.SimulationStep(vehicleID, ref vehicleData, ref frameData, leaderID, ref leaderData, lodPhysics);
            if ((vehicleData.m_flags & Vehicle.Flags.Stopped) != 0 && CanLeave(vehicleID, ref vehicleData))
            {
                vehicleData.m_flags &= ~Vehicle.Flags.Stopped;
                vehicleData.m_flags |= Vehicle.Flags.Leaving;
            }
            if ((vehicleData.m_flags & Vehicle.Flags.GoingBack) == 0 && ShouldReturnToSource(vehicleID, ref vehicleData))
            {
                SetTarget(vehicleID, ref vehicleData, 0);
            }
        }

        private bool ShouldReturnToSource(ushort vehicleID, ref Vehicle data)
	{
	    if (data.m_sourceBuilding != 0)
	    {
		BuildingManager instance = Singleton<BuildingManager>.instance;
		if ((instance.m_buildings.m_buffer[data.m_sourceBuilding].m_flags & Building.Flags.Active) == 0 && instance.m_buildings.m_buffer[data.m_sourceBuilding].m_fireIntensity == 0 && GetArrestedCitizen(vehicleID, ref data) == 0)
		{
		    return true;
		}
	    }
	    return false;
	}

        private void RemoveOffers(ushort vehicleID, ref Vehicle data)
	{
	    if ((data.m_flags & Vehicle.Flags.WaitingTarget) != 0)
	    {
		TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
		offer.Vehicle = vehicleID;
		Singleton<TransferManager>.instance.RemoveIncomingOffer((TransferManager.TransferReason)data.m_transferType, offer);
	    }
	}

        private void RemoveSource(ushort vehicleID, ref Vehicle data)
	{
	    if (data.m_sourceBuilding != 0)
	    {
		Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].RemoveOwnVehicle(vehicleID, ref data);
		data.m_sourceBuilding = 0;
	    }
	}

        private void RemoveTarget(ushort vehicleID, ref Vehicle data)
	{
	    if (data.m_targetBuilding != 0)
	    {
		Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding].RemoveGuestVehicle(vehicleID, ref data);
		data.m_targetBuilding = 0;
	    }
	}

        private bool ArriveAtTarget(ushort vehicleID, ref Vehicle data)
	{
	    if (data.m_targetBuilding == 0)
	    {
                Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleID);
                return true;
	    }
            BuildingManager instance = Singleton<BuildingManager>.instance;
            Building building = instance.m_buildings.m_buffer[data.m_targetBuilding];
            data.m_flags |= Vehicle.Flags.Stopped;
            if(building.Info.m_class.m_level < ItemClass.Level.Level4 && data.m_transferSize < m_criminalCapacity) {
                var targetBuilding = FindClosestPrison(building.m_position);
                data.m_flags &= ~Vehicle.Flags.Emergency2;
                if(targetBuilding == 0) { // don't take criminals if no space at prison
                    SetTarget(vehicleID, ref data, 0);
                } else {
                    ArrestCriminals(vehicleID, ref data, data.m_targetBuilding);
                    SetTarget(vehicleID, ref data, targetBuilding);
                }
            }
            else if(building.Info.m_class.m_level >= ItemClass.Level.Level4)
            {
                int amountDelta = data.m_transferSize;
	        BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding].Info;
	        info.m_buildingAI.ModifyMaterialBuffer(data.m_targetBuilding, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding], (TransferManager.TransferReason)data.m_transferType, ref amountDelta);
	        data.m_transferSize = (ushort)Mathf.Clamp(data.m_transferSize - amountDelta, 0, data.m_transferSize);
                UnloadCriminals(vehicleID, ref data);
                data.m_flags &= ~Vehicle.Flags.Emergency2;
                SetTarget(vehicleID, ref data, 0);
            }
            return false;
	}

        private bool ArriveAtSource(ushort vehicleID, ref Vehicle data)
	{
	    if (data.m_sourceBuilding == 0)
	    {
		Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleID);
		return true;
	    }
	    RemoveSource(vehicleID, ref data);
	    return true;
	}

        public override bool ArriveAtDestination(ushort vehicleID, ref Vehicle vehicleData)
	{
	    if ((vehicleData.m_flags & Vehicle.Flags.WaitingTarget) != 0)
	    {
		return false;
	    }
	    if ((vehicleData.m_flags & Vehicle.Flags.GoingBack) != 0)
	    {
		return ArriveAtSource(vehicleID, ref vehicleData);
	    }
	    return ArriveAtTarget(vehicleID, ref vehicleData);
	}

        public override void UpdateBuildingTargetPositions(ushort vehicleID, ref Vehicle vehicleData, Vector3 refPos, ushort leaderID, ref Vehicle leaderData, ref int index, float minSqrDistance)
	{
	    if ((leaderData.m_flags & Vehicle.Flags.WaitingTarget) != (Vehicle.Flags)0)
	    {
		return;
	    }
	    if ((leaderData.m_flags & Vehicle.Flags.GoingBack) != (Vehicle.Flags)0)
	    {
		if (leaderData.m_sourceBuilding != 0)
		{
		    BuildingManager instance = Singleton<BuildingManager>.instance;
		    BuildingInfo info = instance.m_buildings.m_buffer[(int)leaderData.m_sourceBuilding].Info;
		    Randomizer randomizer = new Randomizer((int)vehicleID);
		    Vector3 vector;
		    Vector3 targetPos;
		    info.m_buildingAI.CalculateUnspawnPosition(vehicleData.m_sourceBuilding, ref instance.m_buildings.m_buffer[(int)leaderData.m_sourceBuilding], ref randomizer, m_info, out vector, out targetPos);
		    vehicleData.SetTargetPos(index++, CalculateTargetPoint(refPos, targetPos, minSqrDistance, 0f));
		    return;
		}
	    }
	    else if (leaderData.m_targetBuilding != 0)
	    {
		BuildingManager instance2 = Singleton<BuildingManager>.instance;
		BuildingInfo info2 = instance2.m_buildings.m_buffer[(int)leaderData.m_targetBuilding].Info;
		Randomizer randomizer2 = new Randomizer((int)vehicleID);
		Vector3 vector2;
		Vector3 targetPos2;
		info2.m_buildingAI.CalculateUnspawnPosition(vehicleData.m_targetBuilding, ref instance2.m_buildings.m_buffer[(int)leaderData.m_targetBuilding], ref randomizer2, m_info, out vector2, out targetPos2);
		vehicleData.SetTargetPos(index++, CalculateTargetPoint(refPos, targetPos2, minSqrDistance, 0));
		return;
	    }
	}

        protected override bool StartPathFind(ushort vehicleID, ref Vehicle vehicleData)
	{
	    if ((vehicleData.m_flags & Vehicle.Flags.WaitingTarget) != 0)
	    {
		return true;
	    }
	    if ((vehicleData.m_flags & Vehicle.Flags.GoingBack) != 0)
	    {
		if (vehicleData.m_sourceBuilding != 0)
		{
		    BuildingManager instance = Singleton<BuildingManager>.instance;
		    BuildingInfo info = instance.m_buildings.m_buffer[vehicleData.m_sourceBuilding].Info;
		    Randomizer randomizer = new Randomizer(vehicleID);
		    info.m_buildingAI.CalculateUnspawnPosition(vehicleData.m_sourceBuilding, ref instance.m_buildings.m_buffer[vehicleData.m_sourceBuilding], ref randomizer, m_info, out var _, out var target);
		    return StartPathFind(vehicleID, ref vehicleData, vehicleData.m_targetPos3, target);
		}
	    }
	    else if (vehicleData.m_targetBuilding != 0)
	    {
		BuildingManager instance2 = Singleton<BuildingManager>.instance;
		BuildingInfo info2 = instance2.m_buildings.m_buffer[vehicleData.m_targetBuilding].Info;
		Randomizer randomizer2 = new Randomizer(vehicleID);
		info2.m_buildingAI.CalculateUnspawnPosition(vehicleData.m_targetBuilding, ref instance2.m_buildings.m_buffer[vehicleData.m_targetBuilding], ref randomizer2, m_info, out var _, out var target2);
		return StartPathFind(vehicleID, ref vehicleData, vehicleData.m_targetPos3, target2);
	    }
	    return false;
	}

        public override InstanceID GetTargetID(ushort vehicleID, ref Vehicle vehicleData)
	{
	    InstanceID result = default(InstanceID);
	    if ((vehicleData.m_flags & Vehicle.Flags.GoingBack) != 0)
	    {
		result.Building = vehicleData.m_sourceBuilding;
	    }
	    else
	    {
		result.Building = vehicleData.m_targetBuilding;
	    }
	    return result;
	}

        public override bool CanLeave(ushort vehicleID, ref Vehicle vehicleData)
	{
	    CitizenManager instance = Singleton<CitizenManager>.instance;
	    bool flag = true;
	    bool flag2 = false;
	    uint num = vehicleData.m_citizenUnits;
	    int num2 = 0;
	    while (num != 0)
	    {
		uint nextUnit = instance.m_units.m_buffer[num].m_nextUnit;
		for (int i = 0; i < 5; i++)
		{
		    uint citizen = instance.m_units.m_buffer[num].GetCitizen(i);
		    if (citizen == 0)
		    {
			continue;
		    }
		    ushort instance2 = instance.m_citizens.m_buffer[citizen].m_instance;
		    if (instance2 == 0)
		    {
			continue;
		    }
		    CitizenInfo info = instance.m_instances.m_buffer[instance2].Info;
		    if (info.m_class.m_service == m_info.m_class.m_service)
		    {
			if ((instance.m_instances.m_buffer[instance2].m_flags & CitizenInstance.Flags.EnteringVehicle) == 0 && (instance.m_instances.m_buffer[instance2].m_flags & CitizenInstance.Flags.Character) != 0)
			{
			    flag2 = true;
			}
			flag = false;
		    }
		}
		num = nextUnit;
		if (++num2 > 524288)
		{
		    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
		    break;
		}
	    }
	    if (!flag2 && !flag)
	    {
		num = vehicleData.m_citizenUnits;
		num2 = 0;
		while (num != 0)
		{
		    uint nextUnit2 = instance.m_units.m_buffer[num].m_nextUnit;
		    for (int j = 0; j < 5; j++)
		    {
			uint citizen2 = instance.m_units.m_buffer[num].GetCitizen(j);
			if (citizen2 != 0)
			{
			    ushort instance3 = instance.m_citizens.m_buffer[citizen2].m_instance;
			    if (instance3 != 0 && (instance.m_instances.m_buffer[instance3].m_flags & CitizenInstance.Flags.EnteringVehicle) == 0)
			    {
				CitizenInfo info2 = instance.m_instances.m_buffer[instance3].Info;
				info2.m_citizenAI.SetTarget(instance3, ref instance.m_instances.m_buffer[instance3], 0);
			    }
			}
		    }
		    num = nextUnit2;
		    if (++num2 > 524288)
		    {
			CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
			break;
		    }
		}
	    }
	    return flag;
	}

        private void UnloadCriminals(ushort vehicleID, ref Vehicle data)
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
		    instance.m_citizens.m_buffer[citizen].SetVisitplace(citizen, data.m_targetBuilding, 0u);
		    if (instance.m_citizens.m_buffer[citizen].m_visitBuilding != 0)
		    {
			instance.m_citizens.m_buffer[citizen].CurrentLocation = Citizen.Location.Visit;
			SpawnPrisoner(vehicleID, ref data, citizen);
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
	    if (num3 != 0 && data.m_targetBuilding != 0)
	    {
		BuildingManager instance3 = Singleton<BuildingManager>.instance;
		DistrictManager instance4 = Singleton<DistrictManager>.instance;
		byte district = instance4.GetDistrict(instance3.m_buildings.m_buffer[data.m_targetBuilding].m_position);
		instance4.m_districts.m_buffer[district].m_productionData.m_tempCriminalExtra += (uint)num3;
	    }
	}

        private void SpawnPrisoner( ushort vehicleID, ref Vehicle data, uint citizen)
	{
	    if (data.m_targetBuilding != 0)
	    {
		SimulationManager instance = Singleton<SimulationManager>.instance;
		CitizenManager instance2 = Singleton<CitizenManager>.instance;
		Citizen.Gender gender = Citizen.GetGender(citizen);
		CitizenInfo groupCitizenInfo = instance2.GetGroupCitizenInfo(ref instance.m_randomizer, m_info.m_class.m_service, gender, Citizen.SubCulture.Generic, Citizen.AgePhase.Young0);
		if (groupCitizenInfo != null && instance2.CreateCitizenInstance(out var instance3, ref instance.m_randomizer, groupCitizenInfo, citizen))
		{
		    Vector3 randomDoorPosition = data.GetRandomDoorPosition(ref instance.m_randomizer, VehicleInfo.DoorType.Exit);
		    groupCitizenInfo.m_citizenAI.SetCurrentVehicle(instance3, ref instance2.m_instances.m_buffer[instance3], 0, 0u, randomDoorPosition);
		    groupCitizenInfo.m_citizenAI.SetTarget(instance3, ref instance2.m_instances.m_buffer[instance3], data.m_targetBuilding);
		}
	    }
	}

        private void ArrestCriminals(ushort vehicleID, ref Vehicle vehicleData, ushort building)
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

        public uint GetArrestedCitizen(ushort vehicleID, ref Vehicle data)
	{
	    CitizenManager instance = Singleton<CitizenManager>.instance;
	    uint num = data.m_citizenUnits;
	    int num2 = 0;
	    while (num != 0)
	    {
		uint nextUnit = instance.m_units.m_buffer[num].m_nextUnit;
		for (int i = 0; i < 5; i++)
		{
		    uint citizen = instance.m_units.m_buffer[num].GetCitizen(i);
		    if (citizen != 0 && instance.m_citizens.m_buffer[citizen].Arrested)
		    {
			return citizen;
		    }
		}
		num = nextUnit;
		if (++num2 > 524288)
		{
		    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
		    break;
		}
	    }
	    return 0u;
	}

        private static ushort FindClosestPrison(Vector3 pos) {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            int num = Mathf.Max((int)(pos.x / 64f + 135f), 0);
            int num2 = Mathf.Max((int)(pos.z / 64f + 135f), 0);
            int num3 = Mathf.Min((int)(pos.x / 64f + 135f), 269);
            int num4 = Mathf.Min((int)(pos.z / 64f + 135f), 269);
            int num5 = num + 1;
            int num6 = num2 + 1;
            int num7 = num3 - 1;
            int num8 = num4 - 1;
            ushort num9 = 0;
            float num10 = 1E+12f;
            float num11 = 0f;
            while (num != num5 || num2 != num6 || num3 != num7 || num4 != num8) {
                for (int i = num2; i <= num4; i++) {
                    for (int j = num; j <= num3; j++) {
                        if (j >= num5 && i >= num6 && j <= num7 && i <= num8) {
                            j = num7;
                            continue;
                        }
                        ushort num12 = instance.m_buildingGrid[i * 270 + j];
                        int num13 = 0;
                        while (num12 != 0) {
                            if ((instance.m_buildings.m_buffer[num12].m_flags & (Building.Flags.Created | Building.Flags.Deleted | Building.Flags.Untouchable | Building.Flags.Collapsed)) == Building.Flags.Created && instance.m_buildings.m_buffer[num12].m_fireIntensity == 0 && instance.m_buildings.m_buffer[num12].GetLastFrameData().m_fireDamage == 0) {

                                BuildingInfo info = instance.m_buildings.m_buffer[num12].Info;
                                if (info.GetAI() is NewPoliceStationAI newPoliceStationAI
                                    && info.m_class.m_service == ItemClass.Service.PoliceDepartment
                                    && info.m_class.m_level >= ItemClass.Level.Level4
                                    && newPoliceStationAI.m_jailOccupancy < newPoliceStationAI.JailCapacity - 10) {
                                    Vector3 position = instance.m_buildings.m_buffer[num12].m_position;
                                    float num14 = Vector3.SqrMagnitude(position - pos);
                                    if (num14 < num10) {
                                        num9 = num12;
                                        num10 = num14;
                                    }
                                }
                            }
                            num12 = instance.m_buildings.m_buffer[num12].m_nextGridBuilding;
                            if (++num13 >= 49152) {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                break;
                            }
                        }
                    }
                }
                if (num9 != 0 && num10 <= num11 * num11) {
                    return num9;
                }
                num11 += 64f;
                num5 = num;
                num6 = num2;
                num7 = num3;
                num8 = num4;
                num = Mathf.Max(num - 1, 0);
                num2 = Mathf.Max(num2 - 1, 0);
                num3 = Mathf.Min(num3 + 1, 269);
                num4 = Mathf.Min(num4 + 1, 269);
            }
            return num9;
        }
    }
}