using System;
using ColossalFramework;
using HarmonyLib;
using PrisonHelicopter.Utils;
using PrisonHelicopter.Utils.TransfersBridge;
using UnityEngine;

namespace PrisonHelicopter.HarmonyPatches
{
    internal class ExtendedTransferManagerPatch
    {
        private static bool patched = false;

        public static void TryPatch()
        {
            if (patched) return;

            if (TransfersAPI.Backend is not MoreTransferReasonsBackend)
            {
                LogHelper.Warning("MoreTransferReasonsBackend not found. ExtendedTransferManagerPatch will be unavailable.");
                return;
            }

            if (!TransfersAPI.Backend.IsAvailable)
            {
                LogHelper.Warning("TransfersAPI.Backend is not available. ExtendedTransferManagerPatch will be unavailable.");
                return;
            }

            if (MoreTransferReasonsBridge._startTransfer == null)
            {
                LogHelper.Warning("MoreTransferReasonsBridge._startTransfer method not found. ExtendedTransferManagerPatch will be unavailable.");
                return;
            }

            var prefix = AccessTools.Method(typeof(ExtendedTransferManagerPatch), nameof(Prefix));
            var harmony = new Harmony(PatchUtil.HarmonyId);
            harmony.Patch(MoreTransferReasonsBridge._startTransfer, prefix: new HarmonyMethod(prefix));

            patched = true;
            LogHelper.Information("ExtendedTransferManagerPatch successfully applied.");
        }

        private static bool Prefix(object __instance, object material, object offerOut, object offerIn, int delta)
        {
            if (!TryHandle(material, offerOut, offerIn, delta))
            {
                return true;
            }
            return false;
        }

        public static bool TryHandle(object materialObj, object offerOutObj, object offerInObj, int delta)
        {
            byte vanillaReason = Convert.ToByte(materialObj);

            if ((TransferManager.TransferReason)vanillaReason == TransferManager.TransferReason.None)
            {
                return false;
            }

            var offerOut = ConvertOffer(offerOutObj);
            var offerIn = ConvertOffer(offerInObj);

            bool activeIn = offerIn.Active;
            bool activeOut = offerOut.Active;

            if (activeIn && offerIn.Vehicle != 0)
            {
                ushort vehicle = offerIn.Vehicle;
                offerOut.Amount = delta;

                ref Vehicle vehicleData = ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicle];
                var ai = vehicleData.Info?.m_vehicleAI;
                ai?.StartTransfer(vehicle, ref vehicleData, (TransferManager.TransferReason)vanillaReason, offerOut);
                return true;
            }

            if (activeOut && offerOut.Vehicle != 0)
            {
                ushort vehicle = offerOut.Vehicle;
                offerIn.Amount = delta;

                ref Vehicle vehicleData = ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicle];
                var ai = vehicleData.Info?.m_vehicleAI;
                ai?.StartTransfer(vehicle, ref vehicleData, (TransferManager.TransferReason)vanillaReason, offerIn);
                return true;
            }

            if (activeOut && offerOut.Building != 0)
            {
                ushort building = offerOut.Building;
                offerIn.Amount = delta;

                ref Building buildingData = ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[building];
                var ai = buildingData.Info?.m_buildingAI;
                ai?.StartTransfer(building, ref buildingData, (TransferManager.TransferReason)vanillaReason, offerIn);
                return true;
            }

            if (activeIn && offerIn.Building != 0)
            {
                ushort building = offerIn.Building;
                offerOut.Amount = delta;

                ref Building buildingData = ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[building];
                var ai = buildingData.Info?.m_buildingAI;
                ai?.StartTransfer(building, ref buildingData, (TransferManager.TransferReason)vanillaReason, offerOut);
                return true;
            }

            return false;
        }

        private static TransferManager.TransferOffer ConvertOffer(object boxedOffer)
        {
            var t = boxedOffer.GetType();

            var building = (ushort)(t.GetField("Building")?.GetValue(boxedOffer) ?? (ushort)0);
            var vehicle = (ushort)(t.GetField("Vehicle")?.GetValue(boxedOffer) ?? (ushort)0);
            var citizen = (uint)(t.GetField("Citizen")?.GetValue(boxedOffer) ?? 0u);

            var transferOffer = new TransferManager.TransferOffer
            {
                Active = (bool)t.GetField("Active").GetValue(boxedOffer),
                Amount = (int)t.GetField("Amount").GetValue(boxedOffer),
                Position = (Vector3)t.GetField("Position").GetValue(boxedOffer),
                m_isLocalPark = (byte)(t.GetField("m_isLocalPark")?.GetValue(boxedOffer) ?? (byte)0),
                m_object = (InstanceID)(t.GetField("m_object")?.GetValue(boxedOffer) ?? default(InstanceID))
            };

            if (building != 0)
            {
                transferOffer.Building = building;
            }
            else if(vehicle != 0)
            {
                transferOffer.Vehicle = vehicle;
            }
            else if (citizen != 0)
            {
                transferOffer.Citizen = citizen;
            }

            return transferOffer;
        }
    }
}
