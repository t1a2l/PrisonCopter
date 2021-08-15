using HarmonyLib;
using System;
using System.Reflection;
using ColossalFramework;
using UnityEngine;
using PrisonHelicopter.AI;

namespace PrisonHelicopter.HarmonyPatches {

    [HarmonyPatch(typeof(HelicopterDepotAI))]
    public static class HelicopterDepotAIPatch {
        private delegate void StartTransferDelegate(CommonBuildingAI instance, ushort buildingID, ref Building data, TransferManager.TransferReason material, TransferManager.TransferOffer offer);
        private static StartTransferDelegate BaseStartTransfer = AccessTools.MethodDelegate<StartTransferDelegate>(typeof(CommonBuildingAI).GetMethod("StartTransfer", BindingFlags.Instance | BindingFlags.Public), null, false);

        private delegate TransferManager.TransferReason GetTransferReason2Delegate(HelicopterDepotAI instance);
        private static GetTransferReason2Delegate GetTransferReason2 = AccessTools.MethodDelegate<GetTransferReason2Delegate>(typeof(HelicopterDepotAI).GetMethod("GetTransferReason2", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        [HarmonyPatch(typeof(HelicopterDepotAI), "GetTransferReason1")]
        [HarmonyPostfix]
        public static void GetTransferReason1(HelicopterDepotAI __instance, ref TransferManager.TransferReason __result) {
            switch (__instance.m_info.m_class.m_service) {
                case ItemClass.Service.HealthCare:
                    __result = TransferManager.TransferReason.Sick2;
                    break;
                case ItemClass.Service.PoliceDepartment:
                    if (__instance.m_info.m_class.m_level < ItemClass.Level.Level4)
                    {
                        __result = TransferManager.TransferReason.Crime;
                    }
                    else
                    {
                        __result = TransferManager.TransferReason.CriminalMove;
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
                // if thereis no prison don't spawn prison helicopters
                if(material == TransferManager.TransferReason.CriminalMove && FindClosestPrison(data.m_position) == 0)
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
                                if (info.GetAI() is NewPoliceStationAI
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
