using HarmonyLib;
using System;
using System.Reflection;
using ColossalFramework;
using UnityEngine;

namespace PrisonCopter {
    [HarmonyPatch(typeof(PoliceStationAI))]
    class PoliceStationAIOverride {

        private delegate void StartTransferDelegate(CommonBuildingAI instance, ushort buildingID, ref Building data, TransferManager.TransferReason material, TransferManager.TransferOffer offer);
        private static StartTransferDelegate BaseStartTransfer = AccessTools.MethodDelegate<StartTransferDelegate>(typeof(CommonBuildingAI).GetMethod("StartTransfer", BindingFlags.Instance | BindingFlags.Public), null, false);

        [HarmonyPatch(typeof(PoliceStationAI), "StartTransfer")]
        [HarmonyPrefix]
        public static bool StartTransfer(PoliceStationAI __instance, ushort buildingID, ref Building data, TransferManager.TransferReason material, TransferManager.TransferOffer offer) {
            if (material == TransferManager.TransferReason.Crime || material == TransferManager.TransferReason.CriminalMove) {
                VehicleInfo randomVehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, __instance.m_info.m_class.m_service, __instance.m_info.m_class.m_subService, __instance.m_info.m_class.m_level);
                if (randomVehicleInfo != null) {
                    ushort bnum = buildingID;
                    var position = data.m_position;
                    if (material == TransferManager.TransferReason.CriminalMove && randomVehicleInfo.m_vehicleType == VehicleInfo.VehicleType.Helicopter) {
                        ushort prison_id = FindClosestPrison(data.m_position);
                        // check for prison in the map
                        if (prison_id == 0) {
                            return false;
                        }
                        bnum = FindClosestPoliceStation(data.m_position);
                        if (bnum != 0) {
                            BuildingManager instance = Singleton<BuildingManager>.instance;
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

        private static ushort FindClosestPoliceStation(Vector3 pos) {
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
