using HarmonyLib;
using ColossalFramework.DataBinding;
using System;
using System.Reflection;
using ColossalFramework;
using UnityEngine;
using System.Runtime;

namespace PrisonCopter {

    [HarmonyPatch(typeof(HelicopterDepotAI))]
    public static class PrisonHelicopterDepotAI
    {
        private delegate void StartTransferDelegate(CommonBuildingAI instance, ushort buildingID, ref Building data, TransferManager.TransferReason material, TransferManager.TransferOffer offer);
        private static StartTransferDelegate BaseStartTransfer = AccessTools.MethodDelegate<StartTransferDelegate>(typeof(CommonBuildingAI).GetMethod("StartTransfer", BindingFlags.Instance | BindingFlags.Public), null, false);

        private delegate TransferManager.TransferReason GetTransferReason1Delegate(HelicopterDepotAI instance);
        private static GetTransferReason1Delegate GetTransferReason1 = AccessTools.MethodDelegate<GetTransferReason1Delegate>(typeof(HelicopterDepotAI).GetMethod("GetTransferReason1", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate TransferManager.TransferReason GetTransferReason2Delegate(HelicopterDepotAI instance);
        private static GetTransferReason2Delegate GetTransferReason2 = AccessTools.MethodDelegate<GetTransferReason2Delegate>(typeof(HelicopterDepotAI).GetMethod("GetTransferReason2", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate string GetLocalizedTooltipDelegate(PlayerBuildingAI instance);
        private static GetLocalizedTooltipDelegate BaseGetLocalizedTooltip = AccessTools.MethodDelegate<GetLocalizedTooltipDelegate>(typeof(PlayerBuildingAI).GetMethod("GetLocalizedTooltip", BindingFlags.Instance | BindingFlags.Public), null, false);

        private delegate void CalculateOwnVehiclesDelegate(CommonBuildingAI instance, ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside);
        private static CalculateOwnVehiclesDelegate CalculateOwnVehicles = AccessTools.MethodDelegate<CalculateOwnVehiclesDelegate>(typeof(CommonBuildingAI).GetMethod("CalculateOwnVehicles", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate void ProduceGoodsDelegate(PlayerBuildingAI instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount);
        private static ProduceGoodsDelegate BaseProduceGoods = AccessTools.MethodDelegate<ProduceGoodsDelegate>(typeof(PlayerBuildingAI).GetMethod("ProduceGoods", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate void HandleDeadDelegate(CommonBuildingAI instance, ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, int citizenCount);
        private static HandleDeadDelegate HandleDead = AccessTools.MethodDelegate<HandleDeadDelegate>(typeof(CommonBuildingAI).GetMethod("HandleDead", BindingFlags.Instance | BindingFlags.NonPublic), null, false);


        [HarmonyPatch(typeof(HelicopterDepotAI), "StartTransfer")]
        [HarmonyPrefix]
        public static bool StartTransfer(HelicopterDepotAI __instance, ushort buildingID, ref Building data, TransferManager.TransferReason material, TransferManager.TransferOffer offer)
	{
            TransferManager.TransferReason transferReason = GetTransferReason1(__instance);
	    TransferManager.TransferReason transferReason2 = GetTransferReason2(__instance);
	    if (material != TransferManager.TransferReason.None && (material == transferReason || material == transferReason2 || material == TransferManager.TransferReason.CriminalMove))
	    {
	        VehicleInfo randomVehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, __instance.m_info.m_class.m_service, __instance.m_info.m_class.m_subService, __instance.m_info.m_class.m_level, VehicleInfo.VehicleType.Helicopter);
	        if (randomVehicleInfo != null)
	        {
		    Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
		    ushort num;
		    if (Singleton<VehicleManager>.instance.CreateVehicle(out num, ref Singleton<SimulationManager>.instance.m_randomizer, randomVehicleInfo, data.m_position, material, true, false))
		    {
			    randomVehicleInfo.m_vehicleAI.SetSource(num, ref vehicles.m_buffer[(int)num], buildingID);
			    randomVehicleInfo.m_vehicleAI.StartTransfer(num, ref vehicles.m_buffer[(int)num], material, offer);
		    }
	        }
	    }
	    else
	    {
	        BaseStartTransfer(__instance, buildingID, ref data, material, offer);
	    }
            return false;
	}

        [HarmonyPatch(typeof(HelicopterDepotAI), "GetLocalizedTooltip")]
        [HarmonyPostfix]
        public static void GetLocalizedTooltip(HelicopterDepotAI __instance, ref string __result)
	{
	    string text = LocaleFormatter.FormatGeneric("AIINFO_WATER_CONSUMPTION", __instance.GetWaterConsumption() * 16) + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_ELECTRICITY_CONSUMPTION", __instance.GetElectricityConsumption() * 16);
            __result = TooltipHelper.Append(BaseGetLocalizedTooltip(__instance),
                TooltipHelper.Format(
                    LocaleFormatter.Info1,
                    text,
                    LocaleFormatter.Info2,
                    LocaleFormatter.FormatGeneric("AIINFO_HELICOPTER_CAPACITY", __instance.m_helicopterCount)
                    )
            );
	}

        [HarmonyPatch(typeof(HelicopterDepotAI), "ProduceGoods")]
        [HarmonyPostfix]
        private static void ProduceGoods(HelicopterDepotAI __instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
	{
	    BaseProduceGoods(__instance, buildingID, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
            if (finalProductionRate == 0)
	    {
		return;
	    }
	    int num = finalProductionRate * __instance.m_noiseAccumulation / 100;
	    if (num != 0)
	    {
		Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.NoisePollution, num, buildingData.m_position, __instance.m_noiseRadius);
	    }
            HandleDead(__instance, buildingID, ref buildingData, ref behaviour, totalWorkerCount);
            
	    TransferManager.TransferReason transferReason = GetTransferReason1(__instance);
	    TransferManager.TransferReason transferReason2 = GetTransferReason2(__instance);
	    if (transferReason == TransferManager.TransferReason.ForestFire || transferReason2 == TransferManager.TransferReason.ForestFire)
	    {
		if (finalProductionRate != 0)
		{
		    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.FirewatchCoverage, finalProductionRate);
		}
	    }
	    if (transferReason == TransferManager.TransferReason.None)
	    {
		return;
	    }
	    int num2 = (finalProductionRate * __instance.m_helicopterCount + 99) / 100;
	    int num3 = 0;
	    int num4 = 0;
	    ushort num5 = 0;
	    VehicleManager instance = Singleton<VehicleManager>.instance;
	    ushort num6 = buildingData.m_ownVehicles;
	    int num7 = 0;
            int num15 = 0;
            int num16 = 0;
	    while (num6 != 0)
	    {
		TransferManager.TransferReason transferType = (TransferManager.TransferReason)instance.m_vehicles.m_buffer[num6].m_transferType;
		if (transferType == transferReason || transferType == TransferManager.TransferReason.CriminalMove || (transferType == transferReason2 && transferReason2 != TransferManager.TransferReason.None))
		{
		    VehicleInfo info = instance.m_vehicles.m_buffer[num6].Info;
		    info.m_vehicleAI.GetSize(num6, ref instance.m_vehicles.m_buffer[num6], out var size, out var max);
                    num15 = size;
                    num16 = max;
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
		if (++num7 > 16384)
		{
		    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
		    break;
		}
	    }
	    if (__instance.m_helicopterCount < 16384 && num3 - num4 > num2 && num5 != 0)
	    {
		VehicleInfo info2 = instance.m_vehicles.m_buffer[num5].Info;
		info2.m_vehicleAI.SetTarget(num5, ref instance.m_vehicles.m_buffer[num5], buildingID);
	    }
	    if (num3 < num2)
	    {
		int num8 = num2 - num3;
		bool flag = transferReason2 != TransferManager.TransferReason.None && (num8 >= 2 || Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) == 0);
		bool flag2 = num8 >= 2 || !flag;
		if (flag2 && flag)
		{
		    num8 = num8 + 1 >> 1;
		}
                var loadedBuildingInfoCount = PrefabCollection<BuildingInfo>.LoadedCount();
                for (uint i = 0; i < loadedBuildingInfoCount; i++) {
                    var bi = PrefabCollection<BuildingInfo>.GetLoaded(i);
                    if(bi.GetAI() is PoliceStationAI policeStationAI && bi.m_class.m_level >= ItemClass.Level.Level4) {
                        var JailCapacity = policeStationAI.m_jailCapacity;
                        if (num15 + num16 <= JailCapacity - 20)
		        {
		            TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
		            offer.Priority = 2 - num3;
		            offer.Building = buildingID;
		            offer.Position = buildingData.m_position;
		            offer.Amount = 1;
		            offer.Active = true;
		            Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.CriminalMove, offer);
		        }
                    }
                }
		if (flag2)
		{
		    TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
		    offer.Priority = 6;
		    offer.Building = buildingID;
		    offer.Position = buildingData.m_position;
		    offer.Amount = Mathf.Min(2, num8);
		    offer.Active = true;
		    Singleton<TransferManager>.instance.AddIncomingOffer(transferReason, offer);
		}
		if (flag)
		{
		    TransferManager.TransferOffer offer2 = default(TransferManager.TransferOffer);
		    offer2.Priority = 6;
		    offer2.Building = buildingID;
		    offer2.Position = buildingData.m_position;
		    offer2.Amount = Mathf.Min(2, num8);
		    offer2.Active = true;
		    Singleton<TransferManager>.instance.AddIncomingOffer(transferReason2, offer2);
		}
	    }
	}

    }
}
