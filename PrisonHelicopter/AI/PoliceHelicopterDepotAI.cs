using ColossalFramework;
using System;
using UnityEngine;
using MoreTransferReasons;
using PrisonHelicopter.Utils;
using ColossalFramework.DataBinding;

namespace PrisonHelicopter.AI
{
    public class PoliceHelicopterDepotAI : PlayerBuildingAI, IExtendedBuildingAI
    {
        [CustomizableProperty("Uneducated Workers", "Workers", 0)]
        public int m_workPlaceCount0 = 3;

        [CustomizableProperty("Educated Workers", "Workers", 1)]
        public int m_workPlaceCount1 = 18;

        [CustomizableProperty("Well Educated Workers", "Workers", 2)]
        public int m_workPlaceCount2 = 21;

        [CustomizableProperty("Highly Educated Workers", "Workers", 3)]
        public int m_workPlaceCount3 = 18;

        [CustomizableProperty("Police Helicopter Count")]
        public int m_policeHelicopterCount = 10;

        [CustomizableProperty("Prison Helicopter Count")]
        public int m_prisonHelicopterCount = 10;

        [CustomizableProperty("Noise Accumulation")]
        public int m_noiseAccumulation = 100;

        [CustomizableProperty("Noise Radius")]
        public float m_noiseRadius = 100f;

        public override ImmaterialResourceManager.ResourceData[] GetImmaterialResourceRadius(ushort buildingID, ref Building data)
        {
            return new ImmaterialResourceManager.ResourceData[1]
            {
                new ImmaterialResourceManager.ResourceData
                {
                    m_resource = ImmaterialResourceManager.Resource.NoisePollution,
                    m_radius = ((m_noiseAccumulation == 0) ? 0f : m_noiseRadius)
                }
            };
        }

        public override Color GetColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode, InfoManager.SubInfoMode subInfoMode)
        {
            GetPlacementInfoMode(out var mode, out var subMode, 0f);
            if (infoMode == mode)
            {
                if (subInfoMode == subMode)
                {
                    if ((data.m_flags & Building.Flags.Active) != 0)
                    {
                        return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColor;
                    }
                    return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_inactiveColor;
                }
                return base.GetColor(buildingID, ref data, infoMode, subInfoMode);
            }
            if (infoMode == InfoManager.InfoMode.NoisePollution)
            {
                return CommonBuildingAI.GetNoisePollutionColor(m_noiseAccumulation);
            }
            return base.GetColor(buildingID, ref data, infoMode, subInfoMode);
        }

        public override void GetPlacementInfoMode(out InfoManager.InfoMode mode, out InfoManager.SubInfoMode subMode, float elevation)
        {
            mode = InfoManager.InfoMode.CrimeRate;
            subMode = InfoManager.SubInfoMode.Default;
        }

        public override void CreateBuilding(ushort buildingID, ref Building data)
        {
            base.CreateBuilding(buildingID, ref data);
            int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            Singleton<CitizenManager>.instance.CreateUnits(out data.m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, buildingID, 0, 0, workCount);
        }

        public override void BuildingLoaded(ushort buildingID, ref Building data, uint version)
        {
            base.BuildingLoaded(buildingID, ref data, version);
            int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            EnsureCitizenUnits(buildingID, ref data, 0, workCount);
        }

        public override void ReleaseBuilding(ushort buildingID, ref Building data)
        {
            base.ReleaseBuilding(buildingID, ref data);
        }

        public override void EndRelocating(ushort buildingID, ref Building data)
        {
            base.EndRelocating(buildingID, ref data);
            int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            EnsureCitizenUnits(buildingID, ref data, 0, workCount);
        }

        protected override void ManualActivation(ushort buildingID, ref Building buildingData)
        {
            if (m_noiseAccumulation != 0)
            {
                Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Sad, ImmaterialResourceManager.Resource.NoisePollution, m_noiseAccumulation, m_noiseRadius);
            }
        }

        protected override void ManualDeactivation(ushort buildingID, ref Building buildingData)
        {
            if ((buildingData.m_flags & Building.Flags.Collapsed) != 0)
            {
                Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Happy, ImmaterialResourceManager.Resource.Abandonment, -buildingData.Width * buildingData.Length, 64f);
            }
            else if (m_noiseAccumulation != 0)
            {
                Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Happy, ImmaterialResourceManager.Resource.NoisePollution, -m_noiseAccumulation, m_noiseRadius);
            }
        }

        public override void SimulationStep(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            base.SimulationStep(buildingID, ref buildingData, ref frameData);
        }

        void IExtendedBuildingAI.ExtendedStartTransfer(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, ExtendedTransferManager.Offer offer)
        {
            if (material == ExtendedTransferManager.TransferReason.PrisonHelicopterCriminalPickup)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                ref Building target_building = ref instance.m_buildings.m_buffer[offer.Building];
                // if no prison was found or helicopter depot has no prison helis enabled or offer building has already heli on the way, dont spawn prison helis
                if(!FindPrison(data.m_position) || (data.m_flags & Building.Flags.Downgrading) == 0 || (target_building.m_flags & Building.Flags.Upgrading) != 0)
                {
                    return;
                }
                // spawn only level 4 helicopters (prison helicopters)
                VehicleInfo randomVehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, m_info.m_class.m_service, m_info.m_class.m_subService, ItemClass.Level.Level4, VehicleInfo.VehicleType.Helicopter);
		if (randomVehicleInfo != null)
		{
		    Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
		    if (ExtedndedVehicleManager.CreateVehicle(out var vehicle, ref Singleton<SimulationManager>.instance.m_randomizer, randomVehicleInfo, data.m_position, material, transferToSource: true, transferToTarget: false) && randomVehicleInfo.m_vehicleAI is PrisonCopterAI prisonCopterAI)
		    {
			randomVehicleInfo.m_vehicleAI.SetSource(vehicle, ref vehicles.m_buffer[vehicle], buildingID);
			((IExtendedVehicleAI)prisonCopterAI).ExtendedStartTransfer(vehicle, ref vehicles.m_buffer[vehicle], material, offer);
		    }
		}
            }
        }

        void IExtendedBuildingAI.ExtendedModifyMaterialBuffer(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, ref int amountDelta)
        {

        }

        void IExtendedBuildingAI.ExtendedGetMaterialAmount(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, out int amount, out int max)
        {
            amount = 0;
            max = 0;
        }

        public override void StartTransfer(ushort buildingID, ref Building data, TransferManager.TransferReason material, TransferManager.TransferOffer offer)
        {
            TransferManager.TransferReason transferReason = TransferManager.TransferReason.Crime;
            if (material != TransferManager.TransferReason.None && (material == transferReason))
            {
                VehicleInfo vehicleInfo = GetSelectedVehicle(buildingID);
                if (vehicleInfo == null)
                {
                    vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, m_info.m_class.m_service, m_info.m_class.m_subService, m_info.m_class.m_level, VehicleInfo.VehicleType.Helicopter);
                }
                if (vehicleInfo != null)
                {
                    Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
                    if (Singleton<VehicleManager>.instance.CreateVehicle(out var vehicle, ref Singleton<SimulationManager>.instance.m_randomizer, vehicleInfo, data.m_position, material, transferToSource: true, transferToTarget: false))
                    {
                        vehicleInfo.m_vehicleAI.SetSource(vehicle, ref vehicles.m_buffer[vehicle], buildingID);
                        vehicleInfo.m_vehicleAI.StartTransfer(vehicle, ref vehicles.m_buffer[vehicle], material, offer);
                    }
                }
            }
            else
            {
                base.StartTransfer(buildingID, ref data, material, offer);
            }
        }

        public override void BuildingDeactivated(ushort buildingID, ref Building data)
        {
            TransferManager.TransferOffer offer = default;
            ExtendedTransferManager.Offer extended_offer = default;
            offer.Building = buildingID;
            Singleton<TransferManager>.instance.RemoveIncomingOffer(TransferManager.TransferReason.Crime, offer);
            Singleton<ExtendedTransferManager>.instance.RemoveIncomingOffer(ExtendedTransferManager.TransferReason.PoliceVanCriminalMove, extended_offer);
            base.BuildingDeactivated(buildingID, ref data);
        }

        protected override void HandleWorkAndVisitPlaces(ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, ref int aliveWorkerCount, ref int totalWorkerCount, ref int workPlaceCount, ref int aliveVisitorCount, ref int totalVisitorCount, ref int visitPlaceCount)
        {
            workPlaceCount += m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            GetWorkBehaviour(buildingID, ref buildingData, ref behaviour, ref aliveWorkerCount, ref totalWorkerCount);
            HandleWorkPlaces(buildingID, ref buildingData, m_workPlaceCount0, m_workPlaceCount1, m_workPlaceCount2, m_workPlaceCount3, ref behaviour, aliveWorkerCount, totalWorkerCount);
        }

        protected override void ProduceGoods(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            base.ProduceGoods(buildingID, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
            if (finalProductionRate == 0)
            {
                return;
            }
            int num = finalProductionRate * m_noiseAccumulation / 100;
            if (num != 0)
            {
                Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.NoisePollution, num, buildingData.m_position, m_noiseRadius);
            }
            VehicleManager vehicleManager = Singleton<VehicleManager>.instance;
            uint numVehicles = vehicleManager.m_vehicles.m_size;
            HandleDead(buildingID, ref buildingData, ref behaviour, totalWorkerCount);
            int num2 = (finalProductionRate * m_policeHelicopterCount + 99) / 100;
            int num3 = 0;
            int num4 = 0;
            ushort num5 = 0;
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort num6 = buildingData.m_ownVehicles;
            int num7 = 0;
            while (num6 != 0)
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
                num6 = instance.m_vehicles.m_buffer[num6].m_nextOwnVehicle;
                if (++num7 > numVehicles)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            TransferManager.TransferReason transferType = (TransferManager.TransferReason)instance.m_vehicles.m_buffer[num6].m_transferType;
            ExtendedTransferManager.TransferReason extendedTransferType = (ExtendedTransferManager.TransferReason)instance.m_vehicles.m_buffer[num6].m_transferType;
            if (m_policeHelicopterCount < numVehicles && num3 - num4 > num2 && num5 != 0 && transferType == TransferManager.TransferReason.Crime)
            {
                VehicleInfo info2 = instance.m_vehicles.m_buffer[num5].Info;
                info2.m_vehicleAI.SetTarget(num5, ref instance.m_vehicles.m_buffer[num5], buildingID);
            }
            if (m_prisonHelicopterCount < numVehicles && num3 - num4 > num2 && num5 != 0 && extendedTransferType == ExtendedTransferManager.TransferReason.PrisonHelicopterCriminalPickup)
            {
                VehicleInfo info3 = instance.m_vehicles.m_buffer[num5].Info;
                info3.m_vehicleAI.SetTarget(num5, ref instance.m_vehicles.m_buffer[num5], buildingID);
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
                    ExtendedTransferManager.Offer offer2 = default;
                    offer2.Building = buildingID;
                    offer2.Position = buildingData.m_position;
                    offer2.Amount = Mathf.Min(2, num8);
                    offer2.Active = true;
                    Singleton<ExtendedTransferManager>.instance.AddIncomingOffer(ExtendedTransferManager.TransferReason.PrisonHelicopterCriminalPickup, offer2);
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

        public override VehicleInfo.VehicleType GetVehicleType()
        {
            return VehicleInfo.VehicleType.Helicopter;
        }

        public override string GetLocalizedTooltip()
        {
            string text = LocaleFormatter.FormatGeneric("AIINFO_WATER_CONSUMPTION", GetWaterConsumption() * 16) + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_ELECTRICITY_CONSUMPTION", GetElectricityConsumption() * 16);
            string text2 = "Police "  + LocaleFormatter.FormatGeneric("AIINFO_HELICOPTER_CAPACITY", m_policeHelicopterCount);
            text2 += Environment.NewLine;
            text2 += "Prison " +  LocaleFormatter.FormatGeneric("AIINFO_HELICOPTER_CAPACITY", m_prisonHelicopterCount);
            return TooltipHelper.Append(base.GetLocalizedTooltip(), TooltipHelper.Format(LocaleFormatter.Info1, text, LocaleFormatter.Info2, text2));
        }

        public override string GetLocalizedStats(ushort buildingID, ref Building data)
        {
            int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
            int productionRate = GetProductionRate(100, budget);
            int num = (productionRate * m_policeHelicopterCount + 99) / 100;
            int num1 = (productionRate * m_prisonHelicopterCount + 99) / 100;
            int count = 0;
            int count1 = 0;
            int cargo = 0;
            int cargo1 = 0;
            int capacity = 0;
            int capacity1 = 0;
            int outside = 0;
            int outside1 = 0;
            CalculateOwnVehicles(buildingID, ref data, TransferManager.TransferReason.Crime, ref count, ref cargo, ref capacity, ref outside);
            ExtedndedVehicleManager.CalculateOwnVehicles(buildingID, ref data, ExtendedTransferManager.TransferReason.PrisonHelicopterCriminalPickup, ref count1, ref cargo1, ref capacity1, ref outside1);
            string text = "Police "  + LocaleFormatter.FormatGeneric("AIINFO_HELICOPTERS", count, num);
            text += Environment.NewLine;
            text += "Prison " +  LocaleFormatter.FormatGeneric("AIINFO_HELICOPTERS", count1, num1);
            return text;
        }

        public override void GetPollutionAccumulation(out int ground, out int noise)
        {
            ground = 0;
            noise = m_noiseAccumulation;
        }

        public override bool RequireRoadAccess()
        {
            return true;
        }

        public override void SetEmptying(ushort buildingID, ref Building data, bool emptying)
        {
            if(data.Info.GetAI() is PoliceHelicopterDepotAI && data.Info.m_class.m_service == ItemClass.Service.PoliceDepartment)
            {
               data.m_flags = data.m_flags.SetFlags(Building.Flags.Downgrading, emptying);
            }
        }

        private static bool FindPrison(Vector3 pos)
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
                                    return true;
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
                    return false;
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
            return false;
        }
    }

}
