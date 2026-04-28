using ColossalFramework.Math;
using UnityEngine;

namespace PrisonHelicopter.Utils.TransfersBridge
{
    internal interface ITransfersBackend
    {
        string Name { get; }

        bool IsAvailable { get; }

        byte GetTransferType(PrisonHelicopterTransferReason reason);

        void AddOutgoingOffer(PrisonHelicopterTransferReason material, TransferOfferData offer);

        void AddIncomingOffer(PrisonHelicopterTransferReason material, TransferOfferData offer);

        void RemoveOutgoingOffer(PrisonHelicopterTransferReason material, TransferOfferData offer);

        void RemoveIncomingOffer(PrisonHelicopterTransferReason material, TransferOfferData offer);

        void CalculateOwnVehicles(ushort buildingID, ref Building data, PrisonHelicopterTransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside);

        void CalculateGuestVehicles(ushort buildingID, ref Building data, PrisonHelicopterTransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside);

        bool CreateVehicle(out ushort vehicle, ref Randomizer r, VehicleInfo info, Vector3 position, PrisonHelicopterTransferReason type, bool transferToSource, bool transferToTarget);

    }
}