using System;
using ColossalFramework.Math;
using UnityEngine;

namespace PrisonHelicopter.Utils.TransfersBridge
{
    internal sealed class NullTransfersBackend : ITransfersBackend
    {
        public string Name => "None";

        public bool IsAvailable => false;

        public byte GetTransferType(PrisonHelicopterTransferReason reason) => throw new InvalidOperationException("No transfers backend is available.");

        public void AddOutgoingOffer(PrisonHelicopterTransferReason material, TransferOfferData offer) { }

        public void AddIncomingOffer(PrisonHelicopterTransferReason material, TransferOfferData offer) { }

        public void RemoveOutgoingOffer(PrisonHelicopterTransferReason material, TransferOfferData offer) { }

        public void RemoveIncomingOffer(PrisonHelicopterTransferReason material, TransferOfferData offer) { }

        public void CalculateOwnVehicles(ushort buildingID, ref Building data, PrisonHelicopterTransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside) { }

        public void CalculateGuestVehicles(ushort buildingID, ref Building data, PrisonHelicopterTransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside) { }

        public bool CreateVehicle(out ushort vehicle, ref Randomizer r, VehicleInfo info, Vector3 position, PrisonHelicopterTransferReason type, bool transferToSource, bool transferToTarget)
        {
            vehicle = 0;
            return false;
        }
    }
}