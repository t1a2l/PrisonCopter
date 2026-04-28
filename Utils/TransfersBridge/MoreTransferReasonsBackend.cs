using System;
using ColossalFramework.Math;
using UnityEngine;

namespace PrisonHelicopter.Utils.TransfersBridge
{
    internal sealed class MoreTransferReasonsBackend : ITransfersBackend
    {
        public string Name => "MoreTransferReasons";

        public bool IsAvailable { get; }

        public MoreTransferReasonsBackend()
        {
            IsAvailable = MoreTransferReasonsBridge.Setup();
        }

        public byte GetTransferType(PrisonHelicopterTransferReason reason) => MapReason(reason);

        public void AddOutgoingOffer(PrisonHelicopterTransferReason material, TransferOfferData offer) =>
            MoreTransferReasonsBridge.AddOutgoingOffer(MapReason(material), offer);

        public void AddIncomingOffer(PrisonHelicopterTransferReason material, TransferOfferData offer) =>
            MoreTransferReasonsBridge.AddIncomingOffer(MapReason(material), offer);

        public void RemoveOutgoingOffer(PrisonHelicopterTransferReason material, TransferOfferData offer) =>
            MoreTransferReasonsBridge.RemoveOutgoingOffer(MapReason(material), offer);

        public void RemoveIncomingOffer(PrisonHelicopterTransferReason material, TransferOfferData offer) =>
            MoreTransferReasonsBridge.RemoveIncomingOffer(MapReason(material), offer);

        public void CalculateOwnVehicles(ushort buildingID, ref Building data, PrisonHelicopterTransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside) =>
            MoreTransferReasonsBridge.CalculateOwnVehicles(buildingID, ref data, MapReason(material), ref count, ref cargo, ref capacity, ref outside);

        public void CalculateGuestVehicles(ushort buildingID, ref Building data, PrisonHelicopterTransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside) =>
            MoreTransferReasonsBridge.CalculateGuestVehicles(buildingID, ref data, MapReason(material), ref count, ref cargo, ref capacity, ref outside);

        public bool CreateVehicle(out ushort vehicle, ref Randomizer r, VehicleInfo info, Vector3 position, PrisonHelicopterTransferReason type, bool transferToSource, bool transferToTarget) =>
            MoreTransferReasonsBridge.CreateVehicle(out vehicle, ref r, info, position, MapReason(type), transferToSource, transferToTarget);

        public static byte MapReason(PrisonHelicopterTransferReason reason)
        {
            return reason switch
            {
                PrisonHelicopterTransferReason.PoliceVanCrimeMove => 15,
                PrisonHelicopterTransferReason.CrimePickup2 => 17,
                PrisonHelicopterTransferReason.CrimeMove2 => 19,
                _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, null),
            };
        }
    }
}