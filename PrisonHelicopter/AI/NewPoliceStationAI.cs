using System;
using ColossalFramework;
using UnityEngine;
using ColossalFramework.DataBinding;

namespace PrisonHelicopter.AI {

    class NewPoliceStationAI : PlayerBuildingAI {

        [CustomizableProperty("Uneducated Workers", "Workers", 0)]
	public int m_workPlaceCount0 = 4;

	[CustomizableProperty("Educated Workers", "Workers", 1)]
	public int m_workPlaceCount1 = 16;

	[CustomizableProperty("Well Educated Workers", "Workers", 2)]
	public int m_workPlaceCount2 = 16;

	[CustomizableProperty("Highly Educated Workers", "Workers", 3)]
	public int m_workPlaceCount3 = 4;

	[CustomizableProperty("Police Car Count")]
	public int m_policeCarCount = 10;

        [CustomizableProperty("Police Van Count")]
	public int m_policeVanCount = 5;

	[CustomizableProperty("Jail Capacity")]
	public int m_jailCapacity = 20;

	[CustomizableProperty("Average Sentence Length")]
	public int m_sentenceWeeks = 15;

	[CustomizableProperty("Police Department Accumulation")]
	public int m_policeDepartmentAccumulation = 100;

	[CustomizableProperty("Police Department Radius")]
	public float m_policeDepartmentRadius = 500f;

	[CustomizableProperty("Noise Accumulation")]
	public int m_noiseAccumulation;

	[CustomizableProperty("Noise Radius")]
	public float m_noiseRadius = 200f;

        public int m_jailOccupancy;

        public int PoliceCarCount => UniqueFacultyAI.IncreaseByBonus(UniqueFacultyAI.FacultyBonus.Police, m_policeCarCount);
        public int PoliceVanCount => UniqueFacultyAI.IncreaseByBonus(UniqueFacultyAI.FacultyBonus.Police, m_policeVanCount);
        public int JailCapacity => UniqueFacultyAI.IncreaseByBonus(UniqueFacultyAI.FacultyBonus.Police, m_jailCapacity);
	public int PoliceDepartmentAccumulation => UniqueFacultyAI.IncreaseByBonus(UniqueFacultyAI.FacultyBonus.Police, m_policeDepartmentAccumulation);

        public override void GetImmaterialResourceRadius(ushort buildingID, ref Building data, out ImmaterialResourceManager.Resource resource1, out float radius1, out ImmaterialResourceManager.Resource resource2, out float radius2)
	{
	    if (m_noiseAccumulation != 0)
	    {
		resource1 = ImmaterialResourceManager.Resource.NoisePollution;
		radius1 = m_noiseRadius;
	    }
	    else
	    {
		resource1 = ImmaterialResourceManager.Resource.None;
		radius1 = 0f;
	    }
	    resource2 = ImmaterialResourceManager.Resource.None;
	    radius2 = 0f;
	}

        public override Color GetColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode)
	{
	    switch (infoMode)
	    {
	    case InfoManager.InfoMode.CrimeRate:
		if ((data.m_flags & Building.Flags.Active) != 0)
		{
		    return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColor;
		}
		return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_inactiveColor;
	    case InfoManager.InfoMode.NoisePollution:
		if (m_noiseAccumulation != 0)
		{
		    return CommonBuildingAI.GetNoisePollutionColor(m_noiseAccumulation);
		}
		break;
	    }
	    return base.GetColor(buildingID, ref data, infoMode);
	}

        public override int GetResourceRate(ushort buildingID, ref Building data, ImmaterialResourceManager.Resource resource)
	{
	    if (resource == ImmaterialResourceManager.Resource.NoisePollution)
	    {
		return m_noiseAccumulation;
	    }
	    return base.GetResourceRate(buildingID, ref data, resource);
	}

        public override void GetPlacementInfoMode(out InfoManager.InfoMode mode, out InfoManager.SubInfoMode subMode, float elevation)
	{
	    mode = InfoManager.InfoMode.CrimeRate;
	    if (m_info.m_class.m_level >= ItemClass.Level.Level4)
	    {
		subMode = InfoManager.SubInfoMode.WaterPower;
	    }
	    else
	    {
		subMode = InfoManager.SubInfoMode.Default;
	    }
	}

        public override void CreateBuilding(ushort buildingID, ref Building data)
	{
	    base.CreateBuilding(buildingID, ref data);
	    int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
	    Singleton<CitizenManager>.instance.CreateUnits(out data.m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, buildingID, 0, 0, workCount, JailCapacity, 0, 0);
	}

        public override void BuildingLoaded(ushort buildingID, ref Building data, uint version)
	{
	    base.BuildingLoaded(buildingID, ref data, version);
	    int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
	    EnsureCitizenUnits(buildingID, ref data, 0, workCount, JailCapacity, 0);
	}

        public override void ReleaseBuilding(ushort buildingID, ref Building data)
	{
	    base.ReleaseBuilding(buildingID, ref data);
	}

	public override void EndRelocating(ushort buildingID, ref Building data)
	{
	    base.EndRelocating(buildingID, ref data);
	    int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
	    EnsureCitizenUnits(buildingID, ref data, 0, workCount, JailCapacity, 0);
	}

        protected override void ManualActivation(ushort buildingID, ref Building buildingData)
	{
	    if (PoliceDepartmentAccumulation != 0)
	    {
		Vector3 position = buildingData.m_position;
		position.y += m_info.m_size.y;
		Singleton<NotificationManager>.instance.AddEvent(NotificationEvent.Type.GainHappiness, position, 1.5f);
	    }
	    if (PoliceDepartmentAccumulation != 0 || m_noiseAccumulation != 0)
	    {
		Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Happy, ImmaterialResourceManager.Resource.PoliceDepartment, PoliceDepartmentAccumulation, m_policeDepartmentRadius, buildingData.m_position, NotificationEvent.Type.Sad, ImmaterialResourceManager.Resource.NoisePollution, m_noiseAccumulation, m_noiseRadius);
	    }
	}

        protected override void ManualDeactivation(ushort buildingID, ref Building buildingData)
	{
	    if ((buildingData.m_flags & Building.Flags.Collapsed) != 0)
	    {
		Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Happy, ImmaterialResourceManager.Resource.Abandonment, -buildingData.Width * buildingData.Length, 64f);
		return;
	    }
	    if (PoliceDepartmentAccumulation != 0)
	    {
		Vector3 position = buildingData.m_position;
		position.y += m_info.m_size.y;
		Singleton<NotificationManager>.instance.AddEvent(NotificationEvent.Type.LoseHappiness, position, 1.5f);
	    }
	    if (PoliceDepartmentAccumulation != 0 || m_noiseAccumulation != 0)
	    {
		Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Sad, ImmaterialResourceManager.Resource.PoliceDepartment, -PoliceDepartmentAccumulation, m_policeDepartmentRadius, buildingData.m_position, NotificationEvent.Type.Happy, ImmaterialResourceManager.Resource.NoisePollution, -m_noiseAccumulation, m_noiseRadius);
	    }
	}

        public override void BuildingDeactivated(ushort buildingID, ref Building data)
	{
	    TransferManager.TransferOffer offer = default;
	    offer.Building = buildingID;
            BuildingManager instance = Singleton<BuildingManager>.instance;
            Building building = instance.m_buildings.m_buffer[buildingID];
	    if (m_info.m_class.m_level >= ItemClass.Level.Level4) // prison
	    {
                Singleton<TransferManager>.instance.RemoveIncomingOffer(TransferManager.TransferReason.CriminalMove, offer); // send prison vans from prison
	    }
            else if (m_info.m_class.m_level < ItemClass.Level.Level4 && (building.m_flags & Building.Flags.Downgrading) == 0) // police station with prison vans
            {
                Singleton<TransferManager>.instance.RemoveIncomingOffer(TransferManager.TransferReason.Crime, offer); // send police cars from police station
                Singleton<TransferManager>.instance.RemoveIncomingOffer((TransferManager.TransferReason)125, offer); // send prison vans from police station
                Singleton<TransferManager>.instance.RemoveIncomingOffer((TransferManager.TransferReason)126, offer); // send prison helicopters from police helicopter depot
                Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.CriminalMove, offer); // ask for prison vans from prison
            }
	    else // normal police station
	    {
		Singleton<TransferManager>.instance.RemoveIncomingOffer(TransferManager.TransferReason.Crime, offer); // send police cars from police station
                Singleton<TransferManager>.instance.RemoveOutgoingOffer((TransferManager.TransferReason)125, offer); // ask for prison vans from police station
                Singleton<TransferManager>.instance.RemoveOutgoingOffer((TransferManager.TransferReason)126, offer); // ask for prison helicopters from police helicopter depot
	    }
	    base.BuildingDeactivated(buildingID, ref data);
	}

        public override void StartTransfer(ushort buildingID, ref Building data, TransferManager.TransferReason material, TransferManager.TransferOffer offer) {
            if (material == TransferManager.TransferReason.Crime || material == (TransferManager.TransferReason)125 || material == TransferManager.TransferReason.CriminalMove) {
                ushort bnum = buildingID;
                BuildingManager instance = Singleton<BuildingManager>.instance;
                BuildingInfo police_building_info = instance.m_buildings.m_buffer[bnum].Info;
                var vehicle_level = m_info.m_class.m_level;
                if(material == TransferManager.TransferReason.CriminalMove && police_building_info.m_class.m_level >= ItemClass.Level.Level4 // prison vans from prison
                    || material == (TransferManager.TransferReason)125 && police_building_info.m_class.m_level < ItemClass.Level.Level4 && (data.m_flags & Building.Flags.Downgrading) == 0) // prison vans from big police station
                {
                    vehicle_level = ItemClass.Level.Level4;
                }
                VehicleInfo randomVehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, m_info.m_class.m_service, m_info.m_class.m_subService, vehicle_level, VehicleInfo.VehicleType.Car);
                if (randomVehicleInfo != null) {
                    Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
                    if (Singleton<VehicleManager>.instance.CreateVehicle(out ushort num, ref Singleton<SimulationManager>.instance.m_randomizer, randomVehicleInfo, data.m_position, material, true, false)) {
                        randomVehicleInfo.m_vehicleAI.SetSource(num, ref vehicles.m_buffer[(int)num], bnum);
                        randomVehicleInfo.m_vehicleAI.StartTransfer(num, ref vehicles.m_buffer[(int)num], material, offer);
                    }
                }
            } else {
                base.StartTransfer(buildingID, ref data, material, offer);
            }
        }

        public override void ModifyMaterialBuffer(ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int amountDelta)
	{
	    if (material == TransferManager.TransferReason.Crime)
	    {
		amountDelta = Mathf.Max(amountDelta, 0);
	    }
	    else
	    {
		base.ModifyMaterialBuffer(buildingID, ref data, material, ref amountDelta);
	    }
	}

        public override void GetMaterialAmount(ushort buildingID, ref Building data, TransferManager.TransferReason material, out int amount, out int max)
	{
	    if (material == TransferManager.TransferReason.Crime)
	    {
		amount = 0;
		max = 1000000;
	    }
	    else
	    {
		base.GetMaterialAmount(buildingID, ref data, material, out amount, out max);
	    }
	}

        public override float GetCurrentRange(ushort buildingID, ref Building data)
	{
	    int num = data.m_productionRate;
	    if ((data.m_flags & (Building.Flags.Evacuating | Building.Flags.Active)) != Building.Flags.Active)
	    {
		num = 0;
	    }
	    else if ((data.m_flags & Building.Flags.RateReduced) != 0)
	    {
		num = Mathf.Min(num, 50);
	    }
	    int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
	    num = PlayerBuildingAI.GetProductionRate(num, budget);
	    return (float)num * m_policeDepartmentRadius * 0.01f;
	}

        protected override void HandleWorkAndVisitPlaces(ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, ref int aliveWorkerCount, ref int totalWorkerCount, ref int workPlaceCount, ref int aliveVisitorCount, ref int totalVisitorCount, ref int visitPlaceCount)
	{
	    workPlaceCount += m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
	    GetWorkBehaviour(buildingID, ref buildingData, ref behaviour, ref aliveWorkerCount, ref totalWorkerCount);
	    HandleWorkPlaces(buildingID, ref buildingData, m_workPlaceCount0, m_workPlaceCount1, m_workPlaceCount2, m_workPlaceCount3, ref behaviour, aliveWorkerCount, totalWorkerCount);
	    visitPlaceCount += JailCapacity;
	    GetVisitBehaviour(buildingID, ref buildingData, ref behaviour, ref aliveVisitorCount, ref totalVisitorCount);
	    behaviour.m_crimeAccumulation = 0;
	}

        protected override int AdjustMaintenanceCost(ushort buildingID, ref Building data, int maintenanceCost)
	{
	    int value = base.AdjustMaintenanceCost(buildingID, ref data, maintenanceCost);
	    return UniqueFacultyAI.DecreaseByBonus(UniqueFacultyAI.FacultyBonus.Law, value);
	}

        protected override void ProduceGoods(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
	{
	    base.ProduceGoods(buildingID, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
	    DistrictManager instance = Singleton<DistrictManager>.instance;
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;
            uint numCitizenUnits = citizenManager.m_units.m_size;
	    byte district = instance.GetDistrict(buildingData.m_position);
	    DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[district].m_servicePolicies;
	    if ((servicePolicies & DistrictPolicies.Services.RecreationalUse) != 0)
	    {
		instance.m_districts.m_buffer[district].m_servicePoliciesEffect |= DistrictPolicies.Services.RecreationalUse;
		int num = GetMaintenanceCost() / 100;
		num = (finalProductionRate * num + 666) / 667;
		if (num != 0)
		{
		    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Maintenance, num, m_info.m_class);
		}
	    }
	    int num2 = productionRate * PoliceDepartmentAccumulation / 100;
	    if (num2 != 0)
	    {
		Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.PoliceDepartment, num2, buildingData.m_position, m_policeDepartmentRadius);
	    }
	    if (finalProductionRate == 0)
	    {
		return;
	    }
	    int num3 = finalProductionRate * m_noiseAccumulation / 100;
	    if (num3 != 0)
	    {
		Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.NoisePollution, num3, buildingData.m_position, m_noiseRadius);
	    }
	    int num4 = m_sentenceWeeks;
	    if ((servicePolicies & DistrictPolicies.Services.DoubleSentences) != 0)
	    {
		instance.m_districts.m_buffer[district].m_servicePoliciesEffect |= DistrictPolicies.Services.DoubleSentences;
		num4 <<= 1;
	    }
	    int num5 = 1000000 / Mathf.Max(1, num4 * 16);
	    CitizenManager instance2 = Singleton<CitizenManager>.instance;
	    uint num6 = buildingData.m_citizenUnits;
	    int num7 = 0;
	    int num8 = 0;
	    int num9 = 0;
	    while (num6 != 0)
	    {
		uint nextUnit = instance2.m_units.m_buffer[num6].m_nextUnit;
		if ((instance2.m_units.m_buffer[num6].m_flags & CitizenUnit.Flags.Visit) != 0)
		{
		    for (int i = 0; i < 5; i++)
		    {
			uint citizen = instance2.m_units.m_buffer[num6].GetCitizen(i);
			if (citizen == 0)
			{
			    continue;
			}
			if (!instance2.m_citizens.m_buffer[citizen].Dead && instance2.m_citizens.m_buffer[citizen].Arrested && instance2.m_citizens.m_buffer[citizen].CurrentLocation == Citizen.Location.Visit)
			{
			    if (Singleton<SimulationManager>.instance.m_randomizer.Int32(1000000u) < num5)
			    {
				instance2.m_citizens.m_buffer[citizen].Arrested = false;
				ushort instance3 = instance2.m_citizens.m_buffer[citizen].m_instance;
				if (instance3 != 0)
				{
				    instance2.ReleaseCitizenInstance(instance3);
				}
				ushort homeBuilding = instance2.m_citizens.m_buffer[citizen].m_homeBuilding;
				if (homeBuilding != 0)
				{
				    CitizenInfo citizenInfo = instance2.m_citizens.m_buffer[citizen].GetCitizenInfo(citizen);
				    HumanAI humanAI = citizenInfo.m_citizenAI as HumanAI;
				    if (humanAI != null)
				    {
					instance2.m_citizens.m_buffer[citizen].m_flags &= ~Citizen.Flags.Evacuating;
					humanAI.StartMoving(citizen, ref instance2.m_citizens.m_buffer[citizen], buildingID, homeBuilding);
				    }
				}
				if (instance2.m_citizens.m_buffer[citizen].CurrentLocation != Citizen.Location.Visit && instance2.m_citizens.m_buffer[citizen].m_visitBuilding == buildingID)
				{
				    instance2.m_citizens.m_buffer[citizen].SetVisitplace(citizen, 0, 0u);
				}
			    }
			    num8++;
			}
			num7++;
		    }
		}
		num6 = nextUnit;
		if (++num9 > numCitizenUnits)
		{
		    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
		    break;
		}
	    }
	    HandleDead(buildingID, ref buildingData, ref behaviour, totalWorkerCount + totalVisitorCount);
	    if (JailCapacity != 0)
	    {
		instance.m_districts.m_buffer[district].m_productionData.m_tempCriminalAmount += (uint)num8;
		instance.m_districts.m_buffer[district].m_productionData.m_tempCriminalCapacity += (uint)JailCapacity;
	    }
	    int count = 0;
	    int cargo = 0;
	    int capacity = 0;
	    int outside = 0;
	    int count2 = 0;
	    int cargo2 = 0;
	    int capacity2 = 0;
	    int outside2 = 0;
            int count3 = 0;
	    int cargo3 = 0;
	    int capacity3 = 0;
	    int outside3 = 0;
            int count4 = 0;
	    int cargo4 = 0;
	    int capacity4 = 0;
	    int outside4 = 0;
	    if (m_info.m_class.m_level >= ItemClass.Level.Level4) // prison 
	    {
                CalculateOwnVehicles(buildingID, ref buildingData, TransferManager.TransferReason.CriminalMove, ref count4, ref cargo4, ref capacity4, ref outside4); // own prison vans
		CalculateGuestVehicles(buildingID, ref buildingData, (TransferManager.TransferReason)126, ref count3, ref cargo3, ref capacity3, ref outside3); // guest prison helicopters
                cargo4 = Mathf.Max(0, Mathf.Min(JailCapacity - num8, cargo4));
                instance.m_districts.m_buffer[district].m_productionData.m_tempCriminalAmount += (uint)cargo4;
                m_jailOccupancy = num7;
	    }
            else if (m_info.m_class.m_level < ItemClass.Level.Level4 && (buildingData.m_flags & Building.Flags.Downgrading) == 0) // big police station
            {
                CalculateOwnVehicles(buildingID, ref buildingData, TransferManager.TransferReason.Crime, ref count, ref cargo, ref capacity, ref outside); // own police cars
                CalculateOwnVehicles(buildingID, ref buildingData, (TransferManager.TransferReason)125, ref count2, ref cargo2, ref capacity2, ref outside2); // own prison vans
                cargo2 = Mathf.Max(0, Mathf.Min(JailCapacity - num8, cargo2));
		instance.m_districts.m_buffer[district].m_productionData.m_tempCriminalAmount += (uint)cargo2;
                CalculateGuestVehicles(buildingID, ref buildingData, (TransferManager.TransferReason)126, ref count3, ref cargo3, ref capacity3, ref outside3); // guest prison helicopters
                CalculateGuestVehicles(buildingID, ref buildingData, TransferManager.TransferReason.CriminalMove, ref count4, ref cargo4, ref capacity4, ref outside4); // guest prison vans from prison
                m_jailOccupancy = num8;
            }
	    else // small police station
	    {
		CalculateOwnVehicles(buildingID, ref buildingData, TransferManager.TransferReason.Crime, ref count, ref cargo, ref capacity, ref outside); // own police cars
                CalculateGuestVehicles(buildingID, ref buildingData, (TransferManager.TransferReason)125, ref count2, ref cargo2, ref capacity2, ref outside2); // guest prison vans from police station
                CalculateGuestVehicles(buildingID, ref buildingData, TransferManager.TransferReason.CriminalMove, ref count4, ref cargo4, ref capacity4, ref outside4); // guest prison vans from prison 
                m_jailOccupancy = num8;
            }
	    int num10 = (finalProductionRate * PoliceCarCount + 99) / 100;
            int num11 = (finalProductionRate * PoliceVanCount + 99) / 100;
	    if (m_info.m_class.m_level >= ItemClass.Level.Level4) // prison
	    {
		if (count4 < num10 && capacity4 + num7 <= JailCapacity - 20)
		{
                    TransferManager.TransferOffer offer4 = default; // prison offer prison vans
		    offer4.Priority = 2 - count4;
		    offer4.Building = buildingID;
		    offer4.Position = buildingData.m_position;
		    offer4.Amount = 1;
		    offer4.Active = true;
		    Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.CriminalMove, offer4);
		}
		return;
	    }
            if (m_info.m_class.m_level < ItemClass.Level.Level4 && (buildingData.m_flags & Building.Flags.Downgrading) == 0) // big police station
	    {
                if (count2 < num11 && capacity2 + num7 <= JailCapacity - 20)
                {
                    TransferManager.TransferOffer offer2 = default; // police station offer prison vans
		    offer2.Priority = 2 - count2;
		    offer2.Building = buildingID;
		    offer2.Position = buildingData.m_position;
		    offer2.Amount = 1;
		    offer2.Active = true;
		    Singleton<TransferManager>.instance.AddIncomingOffer((TransferManager.TransferReason)125, offer2);
                }
	    }
	    if (count < num10)
	    {
		TransferManager.TransferOffer offer = default; // police station offer police cars
		offer.Priority = 2 - count;
		offer.Building = buildingID;
		offer.Position = buildingData.m_position;
		offer.Amount = 1;
		offer.Active = true;
		Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.Crime, offer);
	    }
	    if (num8 >= (JailCapacity * PrisonHelicopterMod.PriosnersPercentage / 100)) // check if prisoner count is above or equal percentage option
	    {
                if ((buildingData.m_flags & Building.Flags.Downgrading) == 0) // big police station
                {
                    if(num8 - capacity3 > 0)
                    {
                        TransferManager.TransferOffer offer3 = default; // ask for guest prison helicopter
		        offer3.Priority = (num8 - capacity3) * 8 / Mathf.Max(1, JailCapacity);
		        offer3.Building = buildingID;
		        offer3.Position = buildingData.m_position;
		        offer3.Amount = 1;
		        offer3.Active = false;
                        Singleton<TransferManager>.instance.AddOutgoingOffer((TransferManager.TransferReason)126, offer3);
                    }

                    if(num8 - capacity4 > 0)
                    {
                        TransferManager.TransferOffer offer4 = default; // ask for guest prison vans from prison
		        offer4.Priority = (num8 - capacity4) * 8 / Mathf.Max(1, JailCapacity);
		        offer4.Building = buildingID;
		        offer4.Position = buildingData.m_position;
		        offer4.Amount = 1;
		        offer4.Active = false;
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.CriminalMove, offer4);
                    }
                }
                else if ((buildingData.m_flags & Building.Flags.Downgrading) != 0) // small police station
                {
                    if(num8 - capacity4 > 0)
                    {
                        TransferManager.TransferOffer offer4 = default; // ask for guest prison vans from prison
		        offer4.Priority = (num8 - capacity4) * 8 / Mathf.Max(1, JailCapacity);
		        offer4.Building = buildingID;
		        offer4.Position = buildingData.m_position;
		        offer4.Amount = 1;
		        offer4.Active = false;
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.CriminalMove, offer4);
                    }
                    if(num8 - capacity2 > 0)
                    {
                        TransferManager.TransferOffer offer2 = default; // ask for guest prison vans from police station
		        offer2.Priority = (num8 - capacity2) * 8 / Mathf.Max(1, JailCapacity);
		        offer2.Building = buildingID;
		        offer2.Position = buildingData.m_position;
		        offer2.Amount = 1;
		        offer2.Active = false;
                        Singleton<TransferManager>.instance.AddOutgoingOffer((TransferManager.TransferReason)125, offer2);
                    }

                }
	    }
	}

        protected override bool CanEvacuate()
	{
	    return false;
	}

        public override bool EnableNotUsedGuide()
	{
	    return true;
	}

        public override void GetPollutionAccumulation(out int ground, out int noise)
	{
	    ground = 0;
	    noise = m_noiseAccumulation;
	}

        public override string GetLocalizedTooltip()
	{
	    if (m_info.m_class.m_level >= ItemClass.Level.Level4) // prison
	    {
		string text = LocaleFormatter.FormatGeneric("AIINFO_WATER_CONSUMPTION", GetWaterConsumption() * 16) + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_ELECTRICITY_CONSUMPTION", GetElectricityConsumption() * 16);
		return TooltipHelper.Append(base.GetLocalizedTooltip(), TooltipHelper.Format(LocaleFormatter.Info1, text, LocaleFormatter.Info2, LocaleFormatter.FormatGeneric("AIINFO_PRISONCAR_COUNT", m_policeCarCount)));
	    }
            else if(m_info.m_class.m_level < ItemClass.Level.Level4 && (Building.Flags.Downgrading) == 0) // big police station
            {
                string text2 = LocaleFormatter.FormatGeneric("AIINFO_WATER_CONSUMPTION", GetWaterConsumption() * 16) + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_ELECTRICITY_CONSUMPTION", GetElectricityConsumption() * 16);
                string text3 = Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_PRISONCAR_COUNT", m_policeVanCount);
                return TooltipHelper.Append(base.GetLocalizedTooltip(), TooltipHelper.Format(LocaleFormatter.Info1, text2, LocaleFormatter.Info2, LocaleFormatter.FormatGeneric("AIINFO_POLICECAR_COUNT", m_policeCarCount), text3));
            }
            else // small police station
            {
                string text4 = LocaleFormatter.FormatGeneric("AIINFO_WATER_CONSUMPTION", GetWaterConsumption() * 16) + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_ELECTRICITY_CONSUMPTION", GetElectricityConsumption() * 16);
	        return TooltipHelper.Append(base.GetLocalizedTooltip(), TooltipHelper.Format(LocaleFormatter.Info1, text4, LocaleFormatter.Info2, LocaleFormatter.FormatGeneric("AIINFO_POLICECAR_COUNT", m_policeCarCount)));
            }
	    
	}

        public override string GetLocalizedStats(ushort buildingID, ref Building data)
	{
	    CitizenManager instance = Singleton<CitizenManager>.instance;
            uint numCitizenUnits = instance.m_units.m_size;
	    uint num = data.m_citizenUnits;
	    int num2 = 0;
	    int num3 = 0;
	    while (num != 0)
	    {
		uint nextUnit = instance.m_units.m_buffer[num].m_nextUnit;
		if ((instance.m_units.m_buffer[num].m_flags & CitizenUnit.Flags.Visit) != 0)
		{
		    for (int i = 0; i < 5; i++)
		    {
			uint citizen = instance.m_units.m_buffer[num].GetCitizen(i);
			if (citizen != 0 && instance.m_citizens.m_buffer[citizen].CurrentLocation == Citizen.Location.Visit)
			{
			    num3++;
			}
		    }
		}
		num = nextUnit;
		if (++num2 > numCitizenUnits)
		{
		    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
		    break;
		}
	    }
	    int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
	    int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
	    int num4 = (productionRate * PoliceCarCount + 99) / 100;
            int num5 = (productionRate * PoliceVanCount + 99) / 100;
	    int count = 0;
            int count1 = 0;
	    int cargo = 0;
            int cargo1 = 0;
	    int capacity = 0;
            int capacity1 = 0;
	    int outside = 0;
            int outside1 = 0;
            string text;
	    if (m_info.m_class.m_level >= ItemClass.Level.Level4) // prison
	    {
		CalculateOwnVehicles(buildingID, ref data, TransferManager.TransferReason.CriminalMove, ref count, ref cargo, ref capacity, ref outside);
                text = LocaleFormatter.FormatGeneric("AIINFO_PRISON_CRIMINALS", num3, JailCapacity) + Environment.NewLine;
                return text + LocaleFormatter.FormatGeneric("AIINFO_PRISON_CARS", count, num4);
	    }
            else if(m_info.m_class.m_level < ItemClass.Level.Level4 && (data.m_flags & Building.Flags.Downgrading) == 0) // big police station
            {
                CalculateOwnVehicles(buildingID, ref data, TransferManager.TransferReason.Crime, ref count, ref cargo, ref capacity, ref outside);
                CalculateOwnVehicles(buildingID, ref data, (TransferManager.TransferReason)125, ref count1, ref cargo1, ref capacity1, ref outside1);
                text = LocaleFormatter.FormatGeneric("AIINFO_POLICESTATION_CRIMINALS", num3, JailCapacity) + Environment.NewLine;
                text += LocaleFormatter.FormatGeneric("AIINFO_POLICE_CARS", count, num4) + Environment.NewLine;
                return text + LocaleFormatter.FormatGeneric("AIINFO_PRISON_CARS", count1, num5);
            }
	    else // small police station
	    {
		CalculateOwnVehicles(buildingID, ref data, TransferManager.TransferReason.Crime, ref count, ref cargo, ref capacity, ref outside);
                text = LocaleFormatter.FormatGeneric("AIINFO_POLICESTATION_CRIMINALS", num3, JailCapacity) + Environment.NewLine;
		return text + LocaleFormatter.FormatGeneric("AIINFO_POLICE_CARS", count, num4);
	    }	
	}

        public override bool RequireRoadAccess()
	{
	    return true;
	}

        public override void SetEmptying(ushort buildingID, ref Building data, bool emptying) // decide if big or small police station
        {
            if(data.Info.GetAI() is NewPoliceStationAI && data.Info.m_class.m_service == ItemClass.Service.PoliceDepartment) {
               data.m_flags = data.m_flags.SetFlags(Building.Flags.Downgrading, emptying);
            }
        }

    }
}