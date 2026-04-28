using System;
using ColossalFramework.Math;
using UnityEngine;

namespace PrisonHelicopter.Utils.TransfersBridge
{
    internal static class TransfersAPI
    {
        public static ITransfersBackend Backend { get; private set; }

        public static bool IsAvailable => Backend != null && Backend.IsAvailable;
        public static string BackendName => Backend?.Name ?? "None";

        public static void Setup()
        {
            ITransfersBackend backend = new MoreTransferReasonsBackend();
            if (backend.IsAvailable)
            {
                Backend = backend;
                LogHelper.Information($"Transfers backend: {Backend.Name}");
                return;
            }

            backend = new TMCE_ExtendedTransfersBackend();
            if (backend.IsAvailable)
            {
                Backend = backend;
                LogHelper.Information($"Transfers backend: {Backend.Name}");
                return;
            }

            Backend = new NullTransfersBackend();
            LogHelper.Error("No supported transfers backend available.");
        }

        public static byte GetTransferType(PrisonHelicopterTransferReason reason) => Backend?.GetTransferType(reason)
            ?? throw new InvalidOperationException("Transfers backend is not initialized.");

        public static void AddOutgoingOffer(PrisonHelicopterTransferReason material, TransferOfferData offer) =>
            Backend?.AddOutgoingOffer(material, offer);

        public static void AddIncomingOffer(PrisonHelicopterTransferReason material, TransferOfferData offer) =>
            Backend?.AddIncomingOffer(material, offer);

        public static void RemoveOutgoingOffer(PrisonHelicopterTransferReason material, TransferOfferData offer) =>
            Backend?.RemoveOutgoingOffer(material, offer);

        public static void RemoveIncomingOffer(PrisonHelicopterTransferReason material, TransferOfferData offer) =>
            Backend?.RemoveIncomingOffer(material, offer);

        public static void CalculateOwnVehicles(ushort buildingID, ref Building data, PrisonHelicopterTransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside) =>
            Backend?.CalculateOwnVehicles(buildingID, ref data, material, ref count, ref cargo, ref capacity, ref outside);

        public static void CalculateGuestVehicles(ushort buildingID, ref Building data, PrisonHelicopterTransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside) =>
            Backend?.CalculateGuestVehicles(buildingID, ref data, material, ref count, ref cargo, ref capacity, ref outside);

        public static bool CreateVehicle(out ushort vehicle, ref Randomizer r, VehicleInfo info, Vector3 position, PrisonHelicopterTransferReason type, bool transferToSource, bool transferToTarget)
        {
            vehicle = 0;
            return Backend != null && Backend.CreateVehicle(out vehicle, ref r, info, position, type, transferToSource, transferToTarget);
        }
    }
}