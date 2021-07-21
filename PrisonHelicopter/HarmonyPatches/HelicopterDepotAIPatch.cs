using HarmonyLib;
using System;
using System.Reflection;
using ColossalFramework;
using UnityEngine;

namespace PrisonHelicopter.HarmonyPatches.HelicopterDepotAIPatch {

    [HarmonyPatch(typeof(HelicopterDepotAI))]
    public static class HelicopterDepotAIPatch {
        private delegate void StartTransferDelegate(CommonBuildingAI instance, ushort buildingID, ref Building data, TransferManager.TransferReason material, TransferManager.TransferOffer offer);
        private static StartTransferDelegate BaseStartTransfer = AccessTools.MethodDelegate<StartTransferDelegate>(typeof(CommonBuildingAI).GetMethod("StartTransfer", BindingFlags.Instance | BindingFlags.Public), null, false);

        private delegate TransferManager.TransferReason GetTransferReason2Delegate(HelicopterDepotAI instance);
        private static GetTransferReason2Delegate GetTransferReason2 = AccessTools.MethodDelegate<GetTransferReason2Delegate>(typeof(HelicopterDepotAI).GetMethod("GetTransferReason2", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate void CalculateOwnVehiclesDelegate(CommonBuildingAI instance, ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside);
        private static CalculateOwnVehiclesDelegate CalculateOwnVehicles = AccessTools.MethodDelegate<CalculateOwnVehiclesDelegate>(typeof(CommonBuildingAI).GetMethod("CalculateOwnVehicles", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate void BuildingDeactivatedDelegate(PlayerBuildingAI instance, ushort buildingID, ref Building data);
        private static BuildingDeactivatedDelegate BaseBuildingDeactivated = AccessTools.MethodDelegate<BuildingDeactivatedDelegate>(typeof(PlayerBuildingAI).GetMethod("BuildingDeactivated", BindingFlags.Instance | BindingFlags.Public), null, false);

        [HarmonyPatch(typeof(HelicopterDepotAI), "GetTransferReason1")]
        [HarmonyPostfix]
        public static void GetTransferReason1(HelicopterDepotAI __instance, ref TransferManager.TransferReason __result) {
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
                // no prison don't spawn prison helicopter
                if(material == TransferManager.TransferReason.CriminalMove && (FindClosestPrison(data.m_position) == 0 || (data.m_flags & Building.Flags.Downgrading) != 0))
                {
                    return false;
                }
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
                    else if(transferReason != TransferManager.TransferReason.CriminalMove)
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
                    if((data.m_flags & Building.Flags.Downgrading) == 0)
                    {
                        text += Environment.NewLine;
                        text += "Prison " +  LocaleFormatter.FormatGeneric("AIINFO_HELICOPTERS", count1, num1);
                    }
                    __result = text;
                }
                else if(transferReason != TransferManager.TransferReason.CriminalMove)
                {
                    __result = LocaleFormatter.FormatGeneric("AIINFO_HELICOPTERS", count, num);
                }
		return false;
	}

        [HarmonyPatch(typeof(HelicopterDepotAI), "BuildingDeactivated")]
        [HarmonyPrefix]
        public static bool BuildingDeactivated(HelicopterDepotAI __instance, ushort buildingID, ref Building data)
	{
	    TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
	    offer.Building = buildingID;
	    TransferManager.TransferReason transferReason = TransferManager.TransferReason.None;
            GetTransferReason1(__instance, ref transferReason);
            TransferManager.TransferReason transferReason2 = GetTransferReason2(__instance);
	    if (transferReason != TransferManager.TransferReason.None)
	    {
		    Singleton<TransferManager>.instance.RemoveIncomingOffer(transferReason, offer);
	    }
	    if (transferReason2 != TransferManager.TransferReason.None)
	    {
		    Singleton<TransferManager>.instance.RemoveIncomingOffer(transferReason2, offer);
	    }
	    BaseBuildingDeactivated(__instance, buildingID, ref data);
            return false;
	}

        [HarmonyPatch(typeof(BuildingAI), "SetEmptying")]
        [HarmonyPostfix]
        public static void SetEmptying(ushort buildingID, ref Building data, bool emptying)
        {
            if(data.Info.GetAI() is HelicopterDepotAI && data.Info.m_class.m_service == ItemClass.Service.PoliceDepartment) {
               data.m_flags = data.m_flags.SetFlags(Building.Flags.Downgrading, emptying);
            }
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
