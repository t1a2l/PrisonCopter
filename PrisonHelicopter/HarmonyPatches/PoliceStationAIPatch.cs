using HarmonyLib;
using System;
using System.Reflection;
using ColossalFramework;
using UnityEngine;
using PrisonHelicopter.Utils;

namespace PrisonHelicopter.HarmonyPatches.PoliceStationAIPatch {

    public delegate void CalculateGuestVehiclesCommonBuildingAIDelegate(CommonBuildingAI instance, ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside);

    public class PoliceStationAIConnection {
        internal PoliceStationAIConnection(CalculateGuestVehiclesCommonBuildingAIDelegate calculateGuestVehiclesCommonBuildingAI) {
            CalculateGuestVehiclesCommonBuildingAI = calculateGuestVehiclesCommonBuildingAI ?? throw new ArgumentNullException(nameof(calculateGuestVehiclesCommonBuildingAI));
        }

        public CalculateGuestVehiclesCommonBuildingAIDelegate CalculateGuestVehiclesCommonBuildingAI { get; }

    }

    public static class PoliceStationAIHook {

        private delegate void CalculateGuestVehiclesTarget(ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside);

        internal static PoliceStationAIConnection GetConnection() {
            try {
                CalculateGuestVehiclesCommonBuildingAIDelegate calculateGuestVehiclesCommonBuildingAI =
                    AccessTools.MethodDelegate<CalculateGuestVehiclesCommonBuildingAIDelegate>(
                    TranspilerUtil.DeclaredMethod<CalculateGuestVehiclesTarget>(typeof(CommonBuildingAI), "CalculateGuestVehicles"),
                    null,
                    false);
                return new PoliceStationAIConnection(calculateGuestVehiclesCommonBuildingAI);
            }
            catch (Exception e) {
                LogHelper.Error(e.Message);
                return null;
            }
        }
    }

    [HarmonyPatch(typeof(PoliceStationAI))]
    class PoliceStationAIPatch {
        private delegate void StartTransferDelegate(CommonBuildingAI instance, ushort buildingID, ref Building data, TransferManager.TransferReason material, TransferManager.TransferOffer offer);
        private static StartTransferDelegate BaseStartTransfer = AccessTools.MethodDelegate<StartTransferDelegate>(typeof(CommonBuildingAI).GetMethod("StartTransfer", BindingFlags.Instance | BindingFlags.Public), null, false);

        private delegate void CalculateOwnVehiclesDelegate(CommonBuildingAI instance, ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside);
        private static CalculateOwnVehiclesDelegate CalculateOwnVehicles = AccessTools.MethodDelegate<CalculateOwnVehiclesDelegate>(typeof(CommonBuildingAI).GetMethod("CalculateOwnVehicles", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate void ProduceGoodsDelegate(PlayerBuildingAI instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount);
        private static ProduceGoodsDelegate BaseProduceGoods = AccessTools.MethodDelegate<ProduceGoodsDelegate>(typeof(PlayerBuildingAI).GetMethod("ProduceGoods", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate void HandleDeadDelegate(CommonBuildingAI instance, ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, int citizenCount);
        private static HandleDeadDelegate HandleDead = AccessTools.MethodDelegate<HandleDeadDelegate>(typeof(CommonBuildingAI).GetMethod("HandleDead", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private static CalculateGuestVehiclesCommonBuildingAIDelegate CalculateGuestVehiclesCommonBuildingAI;


        public static void Prepare() {
            CalculateGuestVehiclesCommonBuildingAI = GameConnectionManager.Instance.PoliceStationAIConnection.CalculateGuestVehiclesCommonBuildingAI;
        }



        [HarmonyPatch(typeof(PoliceStationAI), "StartTransfer")]
        [HarmonyPrefix]
        public static bool StartTransfer(PoliceStationAI __instance, ushort buildingID, ref Building data, TransferManager.TransferReason material, TransferManager.TransferOffer offer) {
            if (material == TransferManager.TransferReason.Crime || material == TransferManager.TransferReason.CriminalMove) {
                VehicleInfo randomVehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, __instance.m_info.m_class.m_service, __instance.m_info.m_class.m_subService, __instance.m_info.m_class.m_level);
                if (randomVehicleInfo != null) {
                    ushort bnum = buildingID;
                    var position = data.m_position;
                    if (material == TransferManager.TransferReason.CriminalMove && randomVehicleInfo.m_vehicleType == VehicleInfo.VehicleType.Helicopter) {
                        BuildingManager instance = Singleton<BuildingManager>.instance;
                        BuildingInfo police_building_info = instance.m_buildings.m_buffer[bnum].Info;
                        if(police_building_info.GetAI() is PoliceStationAI policeStationAI && police_building_info.m_class.m_level < ItemClass.Level.Level4 && policeStationAI.JailCapacity < 60)
                        {
                            return false;
                        }
                        ushort prison_id = FindClosestPrison(data.m_position);
                        // check for prison in the map
                        if (prison_id == 0) {
                            return false;
                        }
                        bnum = FindClosestPoliceHelicopterDepot(data.m_position);
                        if (bnum != 0) {
                            
                            Building building = instance.m_buildings.m_buffer[bnum];
                            BuildingInfo info = building.Info;
                            if (info.GetAI() is HelicopterDepotAI && info.m_class.m_service == ItemClass.Service.PoliceDepartment && (building.m_flags & Building.Flags.Active) != 0) {
                                position = building.m_position;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
                    ushort num;
                    if (Singleton<VehicleManager>.instance.CreateVehicle(out num, ref Singleton<SimulationManager>.instance.m_randomizer, randomVehicleInfo, position, material, true, false)) {
                        randomVehicleInfo.m_vehicleAI.SetSource(num, ref vehicles.m_buffer[(int)num], bnum);
                        randomVehicleInfo.m_vehicleAI.StartTransfer(num, ref vehicles.m_buffer[(int)num], material, offer);
                    }
                }
            } else {
                BaseStartTransfer(__instance, buildingID, ref data, material, offer);
            }
            return false;
        }

        [HarmonyPatch(typeof(PoliceStationAI), "GetLocalizedStats")]
        [HarmonyPrefix]
        public static bool GetLocalizedStats(PoliceStationAI __instance, ushort buildingID, ref Building data, ref string __result)
	{
	    CitizenManager instance = Singleton<CitizenManager>.instance;
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
		    if (++num2 > 524288)
		    {
			    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
			    break;
		    }
	    }
	    int budget = Singleton<EconomyManager>.instance.GetBudget(__instance.m_info.m_class);
	    int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
	    int num4 = (productionRate * __instance.PoliceCarCount + 99) / 100;
            int num5 = (productionRate * __instance.PoliceCarCount + 99) / 100;
	    int count = 0;
            int count1 = 0;
	    int cargo = 0;
            int cargo1 = 0;
	    int capacity = 0;
            int capacity1 = 0;
	    int outside = 0;
            int outside1 = 0;
	    if (__instance.m_info.m_class.m_level >= ItemClass.Level.Level4)
	    {
		CalculateOwnVehicles(__instance, buildingID, ref data, TransferManager.TransferReason.CriminalMove, ref count, ref cargo, ref capacity, ref outside);
	    }
            else if(__instance.m_info.m_class.m_level < ItemClass.Level.Level4 && __instance.JailCapacity >= 60)
            {
                CalculateOwnVehicles(__instance, buildingID, ref data, TransferManager.TransferReason.Crime, ref count, ref cargo, ref capacity, ref outside);
                CalculateOwnVehicles(__instance, buildingID, ref data, TransferManager.TransferReason.CriminalMove, ref count1, ref cargo1, ref capacity1, ref outside1);
            }
	    else
	    {
		CalculateOwnVehicles(__instance, buildingID, ref data, TransferManager.TransferReason.Crime, ref count, ref cargo, ref capacity, ref outside);
	    }
	    string text;
	    if (__instance.m_info.m_class.m_level >= ItemClass.Level.Level4)
	    {
		    text = LocaleFormatter.FormatGeneric("AIINFO_PRISON_CRIMINALS", num3, __instance.JailCapacity) + Environment.NewLine;
		    __result = text + LocaleFormatter.FormatGeneric("AIINFO_PRISON_CARS", count, num4);
	    }
            else if(__instance.m_info.m_class.m_level < ItemClass.Level.Level4 && __instance.JailCapacity >= 60)
            {
                text = LocaleFormatter.FormatGeneric("AIINFO_POLICESTATION_CRIMINALS", num3, __instance.JailCapacity) + Environment.NewLine;
                text += LocaleFormatter.FormatGeneric("AIINFO_POLICE_CARS", count, num4);
                __result = text + LocaleFormatter.FormatGeneric("AIINFO_PRISON_CARS", count, num5);
            }
            else
            {
                text = LocaleFormatter.FormatGeneric("AIINFO_POLICESTATION_CRIMINALS", num3, __instance.JailCapacity) + Environment.NewLine;
		__result = text + LocaleFormatter.FormatGeneric("AIINFO_POLICE_CARS", count, num4);
            }
            return false;
		
	}

        
	[HarmonyPatch(typeof(PoliceStationAI), "ProduceGoods")]
        [HarmonyPrefix]
        protected static bool ProduceGoods(PoliceStationAI __instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
	{
	    BaseProduceGoods(__instance, buildingID, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
	    DistrictManager instance = Singleton<DistrictManager>.instance;
	    byte district = instance.GetDistrict(buildingData.m_position);
	    DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[district].m_servicePolicies;
	    if ((servicePolicies & DistrictPolicies.Services.RecreationalUse) != 0)
	    {
		instance.m_districts.m_buffer[district].m_servicePoliciesEffect |= DistrictPolicies.Services.RecreationalUse;
		int num = __instance.GetMaintenanceCost() / 100;
		num = (finalProductionRate * num + 666) / 667;
		if (num != 0)
		{
		    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Maintenance, num, __instance.m_info.m_class);
		}
	    }
	    int num2 = productionRate * __instance.PoliceDepartmentAccumulation / 100;
	    if (num2 != 0)
	    {
		Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.PoliceDepartment, num2, buildingData.m_position, __instance.m_policeDepartmentRadius);
	    }
	    if (finalProductionRate == 0)
	    {
		return false;
	    }
	    int num3 = finalProductionRate * __instance.m_noiseAccumulation / 100;
	    if (num3 != 0)
	    {
		Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.NoisePollution, num3, buildingData.m_position, __instance.m_noiseRadius);
	    }
	    int num4 = __instance.m_sentenceWeeks;
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
		if (++num9 > 524288)
		{
		    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
		    break;
		}
	    }
	    HandleDead(__instance, buildingID, ref buildingData, ref behaviour, totalWorkerCount + totalVisitorCount);
	    if (__instance.JailCapacity != 0)
	    {
		instance.m_districts.m_buffer[district].m_productionData.m_tempCriminalAmount += (uint)num8;
		instance.m_districts.m_buffer[district].m_productionData.m_tempCriminalCapacity += (uint)__instance.JailCapacity;
	    }
	    int count = 0;
	    int cargo = 0;
	    int capacity = 0;
	    int outside = 0;
	    int count2 = 0;
	    int cargo2 = 0;
	    int capacity2 = 0;
	    int outside2 = 0;
	    if (__instance.m_info.m_class.m_level >= ItemClass.Level.Level4)
	    {
		CalculateOwnVehicles(__instance, buildingID, ref buildingData, TransferManager.TransferReason.CriminalMove, ref count, ref cargo, ref capacity, ref outside);
		cargo = Mathf.Max(0, Mathf.Min(__instance.JailCapacity - num8, cargo));
		instance.m_districts.m_buffer[district].m_productionData.m_tempCriminalAmount += (uint)cargo;
	    }
	    else
	    {
		CalculateOwnVehicles(__instance, buildingID, ref buildingData, TransferManager.TransferReason.Crime, ref count, ref cargo, ref capacity, ref outside);
		CalculateGuestVehiclesCommonBuildingAI(__instance, buildingID, ref buildingData, TransferManager.TransferReason.CriminalMove, ref count2, ref cargo2, ref capacity2, ref outside2);
	    }
	    int num10 = (finalProductionRate * __instance.PoliceCarCount + 99) / 100;
	    if (__instance.m_info.m_class.m_level >= ItemClass.Level.Level4)
	    {
		if (count < num10 && capacity + num7 <= __instance.JailCapacity - 20)
		{
		    TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
		    offer.Priority = 2 - count;
		    offer.Building = buildingID;
		    offer.Position = buildingData.m_position;
		    offer.Amount = 1;
		    offer.Active = true;
		    Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.CriminalMove, offer);
		}
		return false;
	    }
            if (__instance.m_info.m_class.m_level < ItemClass.Level.Level4 && __instance.JailCapacity >= 60)
	    {
		TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
		offer.Priority = 2 - count;
		offer.Building = buildingID;
		offer.Position = buildingData.m_position;
		offer.Amount = 1;
		offer.Active = true;
		Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.CriminalMove, offer);
	    }
	    if (count < num10)
	    {
		TransferManager.TransferOffer offer2 = default(TransferManager.TransferOffer);
		offer2.Priority = 2 - count;
		offer2.Building = buildingID;
		offer2.Position = buildingData.m_position;
		offer2.Amount = 1;
		offer2.Active = true;
		Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.Crime, offer2);
	    }
	    if (num8 - capacity2 > 0)
	    {
		TransferManager.TransferOffer offer3 = default(TransferManager.TransferOffer);
		offer3.Priority = (num8 - capacity2) * 8 / Mathf.Max(1, __instance.JailCapacity);
		offer3.Building = buildingID;
		offer3.Position = buildingData.m_position;
		offer3.Amount = 1;
		offer3.Active = false;
		Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.CriminalMove, offer3);
	    }
            return false;
	}


        private static ushort FindClosestPoliceHelicopterDepot(Vector3 pos) {
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
                                if (info.GetAI() is HelicopterDepotAI helicopterDepotAI && info.m_class.m_service == ItemClass.Service.PoliceDepartment) {
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
                                if (info.GetAI() is PoliceStationAI policeStationAI
                                    && info.m_class.m_service == ItemClass.Service.PoliceDepartment
                                    && info.m_class.m_level >= ItemClass.Level.Level4) {
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
