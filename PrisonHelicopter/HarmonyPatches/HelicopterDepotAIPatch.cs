using HarmonyLib;
using System;
using System.Reflection;
using ColossalFramework;
using UnityEngine;
using PrisonHelicopter.AI;

namespace PrisonHelicopter.HarmonyPatches {

    [HarmonyPatch(typeof(HelicopterDepotAI))]
    public static class HelicopterDepotAIPatch {

        private delegate TransferManager.TransferReason GetTransferReason1Delegate(HelicopterDepotAI instance);
        private static readonly GetTransferReason1Delegate GetTransferReason1 = AccessTools.MethodDelegate<GetTransferReason1Delegate>(typeof(HelicopterDepotAI).GetMethod("GetTransferReason1", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate void CalculateOwnVehiclesDelegate(CommonBuildingAI instance, ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside);
        private static readonly CalculateOwnVehiclesDelegate CalculateOwnVehicles = AccessTools.MethodDelegate<CalculateOwnVehiclesDelegate>(typeof(CommonBuildingAI).GetMethod("CalculateOwnVehicles", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate void BaseProduceGoodsDelegate(PlayerBuildingAI instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount);
        private static readonly BaseProduceGoodsDelegate BaseProduceGoods = AccessTools.MethodDelegate<BaseProduceGoodsDelegate>(typeof(PlayerBuildingAI).GetMethod("ProduceGoods", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate void HandleDeadDelegate(CommonBuildingAI instance, ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, int citizenCount);
        private static readonly HandleDeadDelegate HandleDead = AccessTools.MethodDelegate<HandleDeadDelegate>(typeof(CommonBuildingAI).GetMethod("HandleDead", BindingFlags.Instance | BindingFlags.NonPublic), null, false);


        [HarmonyPatch(typeof(HelicopterDepotAI), "StartTransfer")]
        [HarmonyPostfix]
        public static void StartTransfer(HelicopterDepotAI __instance, ushort buildingID, ref Building data, TransferManager.TransferReason material, TransferManager.TransferOffer offer)
        {
            if (material == (TransferManager.TransferReason)126)
            {
                // if no prison was found or
                // the offering building is not a big police station or
                // the target building has alreadya vehicle on the way --
                // -- don't spawn a prison helicopter
                if(FindClosestPrison(data.m_position) == 0 || (data.m_flags & Building.Flags.Downgrading) != 0 || (data.m_flags & Building.Flags.Incoming) != 0)
                {
                    return;
                }
                // spawn only level 4 helicopters (prison helicopters)
                VehicleInfo randomVehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, __instance.m_info.m_class.m_service, __instance.m_info.m_class.m_subService, ItemClass.Level.Level4, VehicleInfo.VehicleType.Helicopter);
		if (randomVehicleInfo != null)
		{
		    Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
		    if (Singleton<VehicleManager>.instance.CreateVehicle(out var vehicle, ref Singleton<SimulationManager>.instance.m_randomizer, randomVehicleInfo, data.m_position, material, transferToSource: true, transferToTarget: false))
		    {
			randomVehicleInfo.m_vehicleAI.SetSource(vehicle, ref vehicles.m_buffer[vehicle], buildingID);
			randomVehicleInfo.m_vehicleAI.StartTransfer(vehicle, ref vehicles.m_buffer[vehicle], material, offer);
		    }
		}
            }
        }

        [HarmonyPatch(typeof(HelicopterDepotAI), "GetLocalizedStats")]
        [HarmonyPrefix]
        public static bool GetLocalizedStats(HelicopterDepotAI __instance, ushort buildingID, ref Building data, ref string __result)
	{
            if (__instance.m_info.m_class.m_service == ItemClass.Service.PoliceDepartment && (data.m_flags & Building.Flags.Downgrading) == 0)
            {
                int budget = Singleton<EconomyManager>.instance.GetBudget(__instance.m_info.m_class);
	        int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
	        int num = (productionRate * __instance.m_helicopterCount + 99) / 100;
                int num1 = (productionRate * __instance.m_helicopterCount + 99) / 100;
                string text = string.Empty;
                int count = 0;
                int count1 = 0;
	        int cargo = 0;
                int cargo1 = 0;
	        int capacity = 0;
                int capacity1 = 0;
	        int outside = 0;
                int outside1 = 0;
                CalculateOwnVehicles(__instance, buildingID, ref data, TransferManager.TransferReason.Crime, ref count, ref cargo, ref capacity, ref outside);
                CalculateOwnVehicles(__instance, buildingID, ref data, (TransferManager.TransferReason)126, ref count1, ref cargo1, ref capacity1, ref outside1);
                text += "Police "  + LocaleFormatter.FormatGeneric("AIINFO_HELICOPTERS", count, num);
                text += Environment.NewLine;
                text += "Prison " +  LocaleFormatter.FormatGeneric("AIINFO_HELICOPTERS", count1, num1);
                __result = text;
                return false;
            }
            return true;
	}

        [HarmonyPatch(typeof(HelicopterDepotAI), "BuildingDeactivated")]
        [HarmonyPrefix]
        public static void BuildingDeactivated(HelicopterDepotAI __instance, ushort buildingID, ref Building data)
	{
	    TransferManager.TransferOffer offer = default;
	    offer.Building = buildingID;
            ItemClass.Service service = __instance.m_info.m_class.m_service;
            if (service == ItemClass.Service.PoliceDepartment && (data.m_flags & Building.Flags.Downgrading) == 0)
	    {
		Singleton<TransferManager>.instance.RemoveIncomingOffer((TransferManager.TransferReason)126, offer);
	    }
	}

        [HarmonyPatch(typeof(HelicopterDepotAI), "ProduceGoods")]
        [HarmonyPrefix]
        public static bool ProduceGoods(HelicopterDepotAI __instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
	{
            ItemClass.Service service = __instance.m_info.m_class.m_service;
            if (service == ItemClass.Service.PoliceDepartment && (buildingData.m_flags & Building.Flags.Downgrading) == 0)
            {
	        BaseProduceGoods(__instance, buildingID, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
	        if (finalProductionRate == 0)
	        {
		    return false;
	        }
                VehicleManager vehicleManager = Singleton<VehicleManager>.instance;
                uint numVehicles = vehicleManager.m_vehicles.m_size;
	        int num = finalProductionRate * __instance.m_noiseAccumulation / 100;
	        if (num != 0)
	        {
		    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.NoisePollution, num, buildingData.m_position, __instance.m_noiseRadius);
	        }
	        HandleDead(__instance, buildingID, ref buildingData, ref behaviour, totalWorkerCount);
	        TransferManager.TransferReason transferReason = GetTransferReason1(__instance);
	        int num2 = (finalProductionRate * __instance.m_helicopterCount + 99) / 100;
	        int num3 = 0;
	        int num4 = 0;
	        ushort num5 = 0;
	        VehicleManager instance = Singleton<VehicleManager>.instance;
	        ushort num6 = buildingData.m_ownVehicles;
	        int num7 = 0;
	        while (num6 != 0)
	        {
		    TransferManager.TransferReason transferType = (TransferManager.TransferReason)instance.m_vehicles.m_buffer[num6].m_transferType;
                    if(transferType == TransferManager.TransferReason.Crime || (transferType == (TransferManager.TransferReason)126))
		    {
		        VehicleInfo info = instance.m_vehicles.m_buffer[num6].Info;
		        info.m_vehicleAI.GetSize(num6, ref instance.m_vehicles.m_buffer[num6], out var _, out var _);
		        num3++;
		        if ((instance.m_vehicles.m_buffer[num6].m_flags & Vehicle.Flags.GoingBack) != 0)
		        {
			    num4++;
		        }
		        else if ((instance.m_vehicles.m_buffer[num6].m_flags & Vehicle.Flags.WaitingTarget) != 0)
		        {
			    num5 = num6;
		        }
		    }
		    num6 = instance.m_vehicles.m_buffer[num6].m_nextOwnVehicle;
		    if (++num7 > numVehicles)
		    {
		        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
		        break;
		    }
	        }
	        if (__instance.m_helicopterCount < numVehicles && num3 - num4 > num2 && num5 != 0)
	        {
		    VehicleInfo info2 = instance.m_vehicles.m_buffer[num5].Info;
		    info2.m_vehicleAI.SetTarget(num5, ref instance.m_vehicles.m_buffer[num5], buildingID);
	        }
	        if (num3 < num2)
	        {
		    int num8 = num2 - num3;
		    bool flag = (num8 >= 2 || Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) == 0);
		    bool flag2 = num8 >= 2 || !flag;
		    if (flag2 && flag)
		    {
		        num8 = num8 + 1 >> 1;
		    }
		    if (flag2)
		    {
		        TransferManager.TransferOffer offer = default;
		        offer.Priority = 6;
		        offer.Building = buildingID;
		        offer.Position = buildingData.m_position;
		        offer.Amount = Mathf.Min(2, num8);
		        offer.Active = true;
		        Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.Crime, offer);
		    }
		    if (flag)
		    {
		        TransferManager.TransferOffer offer2 = default;
		        offer2.Priority = 6;
		        offer2.Building = buildingID;
		        offer2.Position = buildingData.m_position;
		        offer2.Amount = Mathf.Min(2, num8);
		        offer2.Active = true;
		        Singleton<TransferManager>.instance.AddIncomingOffer((TransferManager.TransferReason)126, offer2);
		    }
	        }
                return false;
            }
            return true;
	}

        [HarmonyPatch(typeof(BuildingAI), "SetEmptying")]
        [HarmonyPostfix]
        public static void SetEmptying(ushort buildingID, ref Building data, bool emptying)
        {
            if(data.Info.GetAI() is HelicopterDepotAI && data.Info.m_class.m_service == ItemClass.Service.PoliceDepartment)
            {
               data.m_flags = data.m_flags.SetFlags(Building.Flags.Downgrading, emptying);
            }
        }

        private static ushort FindClosestPrison(Vector3 pos)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            uint numBuildings = instance.m_buildings.m_size;
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
            while (num != num5 || num2 != num6 || num3 != num7 || num4 != num8)
            {
                for (int i = num2; i <= num4; i++)
                {
                    for (int j = num; j <= num3; j++)
                    {
                        if (j >= num5 && i >= num6 && j <= num7 && i <= num8)
                        {
                            j = num7;
                            continue;
                        }
                        ushort num12 = instance.m_buildingGrid[i * 270 + j];
                        int num13 = 0;
                        while (num12 != 0)
                        {
                            if ((instance.m_buildings.m_buffer[num12].m_flags & (Building.Flags.Created | Building.Flags.Deleted | Building.Flags.Untouchable | Building.Flags.Collapsed)) == Building.Flags.Created && instance.m_buildings.m_buffer[num12].m_fireIntensity == 0 && instance.m_buildings.m_buffer[num12].GetLastFrameData().m_fireDamage == 0)
                            {
                                BuildingInfo info = instance.m_buildings.m_buffer[num12].Info;
                                if (info.GetAI() is PrisonCopterPoliceStationAI prisonCopterPoliceStationAI
                                    && info.m_class.m_service == ItemClass.Service.PoliceDepartment
                                    && info.m_class.m_level >= ItemClass.Level.Level4
                                    && prisonCopterPoliceStationAI.m_jailOccupancy < prisonCopterPoliceStationAI.JailCapacity - 10)
                                {
                                    Vector3 position = instance.m_buildings.m_buffer[num12].m_position;
                                    float num14 = Vector3.SqrMagnitude(position - pos);
                                    if (num14 < num10)
                                    {
                                        num9 = num12;
                                        num10 = num14;
                                    }
                                }
                            }
                            num12 = instance.m_buildings.m_buffer[num12].m_nextGridBuilding;
                            if (++num13 >= numBuildings)
                            {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                break;
                            }
                        }
                    }
                }
                if (num9 != 0 && num10 <= num11 * num11)
                {
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
