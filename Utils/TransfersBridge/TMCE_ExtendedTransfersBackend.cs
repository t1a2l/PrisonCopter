using System;
using ColossalFramework;
using ColossalFramework.Math;
using PrisonHelicopter.AI;
using UnityEngine;

namespace PrisonHelicopter.Utils.TransfersBridge
{
    internal sealed class TMCE_ExtendedTransfersBackend : ITransfersBackend
    {
        public string Name => "TMCE_Extended";

        public bool IsAvailable => true;

        public byte GetTransferType(PrisonHelicopterTransferReason reason) => MapReason(reason);

        public void AddOutgoingOffer(PrisonHelicopterTransferReason material, TransferOfferData offer)
        {
            Singleton<TransferManager>.instance.AddOutgoingOffer((TransferManager.TransferReason)MapReason(material), ToVanillaOffer(offer));
        }

        public void AddIncomingOffer(PrisonHelicopterTransferReason material, TransferOfferData offer)
        {
            Singleton<TransferManager>.instance.AddIncomingOffer((TransferManager.TransferReason)MapReason(material), ToVanillaOffer(offer));
        }

        public void RemoveOutgoingOffer(PrisonHelicopterTransferReason material, TransferOfferData offer)
        {
            Singleton<TransferManager>.instance.RemoveOutgoingOffer((TransferManager.TransferReason)MapReason(material), ToVanillaOffer(offer));
        }

        public void RemoveIncomingOffer(PrisonHelicopterTransferReason material, TransferOfferData offer)
        {
            Singleton<TransferManager>.instance.RemoveIncomingOffer((TransferManager.TransferReason)MapReason(material), ToVanillaOffer(offer));
        }

        public void CalculateOwnVehicles(ushort buildingID, ref Building data, PrisonHelicopterTransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside)
        {
            if(data.Info.GetAI() is PoliceHelicopterDepotAI policeHelicopterDepotAI)
            {
                policeHelicopterDepotAI.CalculateOwnVehicles(buildingID, ref data, (TransferManager.TransferReason)MapReason(material), ref count, ref cargo, ref capacity, ref outside);
            }
            else if (data.Info.GetAI() is PrisonCopterPoliceStationAI prisonCopterPoliceStationAI)
            {
                prisonCopterPoliceStationAI.CalculateOwnVehicles(buildingID, ref data, (TransferManager.TransferReason)MapReason(material), ref count, ref cargo, ref capacity, ref outside);
            }
        }

        public void CalculateGuestVehicles(ushort buildingID, ref Building data, PrisonHelicopterTransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside)
        {
            if (data.Info.GetAI() is PoliceHelicopterDepotAI policeHelicopterDepotAI)
            {
                policeHelicopterDepotAI.CalculateGuestVehicles(buildingID, ref data, (TransferManager.TransferReason)MapReason(material), ref count, ref cargo, ref capacity, ref outside);
            }
            else if (data.Info.GetAI() is PrisonCopterPoliceStationAI prisonCopterPoliceStationAI)
            {
                prisonCopterPoliceStationAI.CalculateGuestVehicles(buildingID, ref data, (TransferManager.TransferReason)MapReason(material), ref count, ref cargo, ref capacity, ref outside);
            }
        }

        public bool CreateVehicle(out ushort vehicle, ref Randomizer r, VehicleInfo info, Vector3 position, PrisonHelicopterTransferReason type, bool transferToSource, bool transferToTarget)
        {
            return Singleton<VehicleManager>.instance.CreateVehicle(out vehicle, ref r, info, position, (TransferManager.TransferReason)MapReason(type), transferToSource, transferToTarget);
        }

        private static byte MapReason(PrisonHelicopterTransferReason reason)
        {
            return reason switch
            {
                PrisonHelicopterTransferReason.PoliceVanCrimeMove => 223,
                PrisonHelicopterTransferReason.CrimePickup2 => 224,
                PrisonHelicopterTransferReason.CrimeMove2 => 225,
                _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, null),
            };
        }

        private static TransferManager.TransferOffer ToVanillaOffer(TransferOfferData src)
        {
            return new TransferManager.TransferOffer
            {
                Active = src.Active,
                Amount = src.Amount,
                Building = src.Building, 
                Citizen = src.Citizen,
                m_isLocalPark = src.m_isLocalPark,
                m_object = src.m_object,
                Position = src.Position,
                Priority = src.Priority,
                Vehicle = src.Vehicle
            };
        }
    }
}