using HarmonyLib;
using ColossalFramework.DataBinding;
using System;
using System.Reflection;
using ColossalFramework;
using UnityEngine;

namespace PrisonCopter {

    [HarmonyPatch(typeof(HelicopterDepotAI))]
    public static class PrisonHelicopterDepotAI {
        private delegate void StartTransferDelegate(CommonBuildingAI instance, ushort buildingID, ref Building data, TransferManager.TransferReason material, TransferManager.TransferOffer offer);
        private static StartTransferDelegate BaseStartTransfer = AccessTools.MethodDelegate<StartTransferDelegate>(typeof(CommonBuildingAI).GetMethod("StartTransfer", BindingFlags.Instance | BindingFlags.Public), null, false);

        private delegate TransferManager.TransferReason GetTransferReason2Delegate(HelicopterDepotAI instance);
        private static GetTransferReason2Delegate GetTransferReason2 = AccessTools.MethodDelegate<GetTransferReason2Delegate>(typeof(HelicopterDepotAI).GetMethod("GetTransferReason2", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate void CalculateOwnVehiclesDelegate(CommonBuildingAI instance, ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside);
        private static CalculateOwnVehiclesDelegate CalculateOwnVehicles = AccessTools.MethodDelegate<CalculateOwnVehiclesDelegate>(typeof(CommonBuildingAI).GetMethod("CalculateOwnVehicles", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        [HarmonyPatch(typeof(HelicopterDepotAI), "GetTransferReason1")]
        [HarmonyPostfix]
        private static void GetTransferReason1(HelicopterDepotAI __instance, ref TransferManager.TransferReason __result) {
            switch (__instance.m_info.m_class.m_service) {
                case ItemClass.Service.HealthCare:
                    __result = TransferManager.TransferReason.Sick2;
                    break;
                case ItemClass.Service.PoliceDepartment:
                    if (__instance.m_info.m_class.name == "Prison Vehicle") {
                        __result = TransferManager.TransferReason.CriminalMove;
                    } else {
                        __result = TransferManager.TransferReason.Crime;
                    }
                    break;
                case ItemClass.Service.FireDepartment:
                    __result = TransferManager.TransferReason.ForestFire;
                    break;
                default:
                    __result = TransferManager.TransferReason.None;
                    break;
            }
        }

        [HarmonyPatch(typeof(HelicopterDepotAI), "StartTransfer")]
        [HarmonyPrefix]
        public static bool StartTransfer(HelicopterDepotAI __instance, ushort buildingID, ref Building data, TransferManager.TransferReason material, TransferManager.TransferOffer offer) {
            TransferManager.TransferReason transferReason = TransferManager.TransferReason.None;
            GetTransferReason1(__instance, ref transferReason);
            TransferManager.TransferReason transferReason2 = GetTransferReason2(__instance);
            if (material != TransferManager.TransferReason.None && (material == transferReason || material == transferReason2)) {
                VehicleInfo randomVehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, __instance.m_info.m_class.m_service, __instance.m_info.m_class.m_subService, __instance.m_info.m_class.m_level, VehicleInfo.VehicleType.Helicopter);
                if (randomVehicleInfo != null) {
                    Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
                    ushort num;
                    if (Singleton<VehicleManager>.instance.CreateVehicle(out num, ref Singleton<SimulationManager>.instance.m_randomizer, randomVehicleInfo, data.m_position, material, true, false)) {
                        randomVehicleInfo.m_vehicleAI.SetSource(num, ref vehicles.m_buffer[(int)num], buildingID);
                        randomVehicleInfo.m_vehicleAI.StartTransfer(num, ref vehicles.m_buffer[(int)num], material, offer);
                    }
                }
            } else {
                BaseStartTransfer(__instance, buildingID, ref data, material, offer);
            }
            return false;
        }

        [HarmonyPatch(typeof(HelicopterDepotAI), "GetLocalizedStats")]
        [HarmonyPrefix]
        public static bool GetLocalizedStats(HelicopterDepotAI __instance, ushort buildingID, ref Building data, ref string __result)
	{
		int budget = Singleton<EconomyManager>.instance.GetBudget(__instance.m_info.m_class);
		int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
		int num = (productionRate * __instance.m_helicopterCount + 99) / 100;
                int num1 = (productionRate * __instance.m_helicopterCount + 99) / 100;
		int count = 0;
                int count1 = 0;
		int cargo = 0;
                int cargo1 = 0;
		int capacity = 0;
                int capacity1 = 0;
		int outside = 0;
                int outside1 = 0;
                string text = string.Empty;
		TransferManager.TransferReason transferReason = TransferManager.TransferReason.None;
                GetTransferReason1(__instance, ref transferReason);
		TransferManager.TransferReason transferReason2 =  GetTransferReason2(__instance);
		if (transferReason != TransferManager.TransferReason.None)
		{
                    if(transferReason == TransferManager.TransferReason.Crime)
                    {
                        CalculateOwnVehicles(__instance, buildingID, ref data, transferReason, ref count, ref cargo, ref capacity, ref outside);
                        CalculateOwnVehicles(__instance, buildingID, ref data, TransferManager.TransferReason.CriminalMove, ref count1, ref cargo1, ref capacity1, ref outside1);
                    }
                    else
                    {
                        CalculateOwnVehicles(__instance, buildingID, ref data, transferReason, ref count, ref cargo, ref capacity, ref outside);
                    }
		}
		if (transferReason2 != TransferManager.TransferReason.None)
		{
			CalculateOwnVehicles(__instance, buildingID, ref data, transferReason2, ref count, ref cargo, ref capacity, ref outside);
		}
                if(transferReason == TransferManager.TransferReason.Crime)
                {
                    text += "Police "  + LocaleFormatter.FormatGeneric("AIINFO_HELICOPTERS", count, num);
                    text += Environment.NewLine;
                    text += "Prison " +  LocaleFormatter.FormatGeneric("AIINFO_HELICOPTERS", count1, num1);
                    __result = text;
                }
                else
                {
                    __result = LocaleFormatter.FormatGeneric("AIINFO_HELICOPTERS", count, num);
                }
		return false;
	}
    }
}
