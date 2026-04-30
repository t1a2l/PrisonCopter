using System;
using ColossalFramework;
using HarmonyLib;
using PrisonHelicopter.Utils.TransfersBridge;
using UnityEngine;

namespace PrisonHelicopter.HarmonyPatches
{
    [HarmonyPatch]
    internal class ExtendedTransferManagerPatch
    {
        public static void TryPatch(Harmony harmony)
        {
            if (TransfersAPI.Backend is not MoreTransferReasonsBackend)
                return;

            if (!TransfersAPI.Backend.IsAvailable)
                return;

            var prefix = AccessTools.Method(typeof(ExtendedTransferManagerPatch), nameof(ExtendedTransferManagerPatch.Prefix));
            harmony.Patch(MoreTransferReasonsBridge._startTransfer, prefix: new HarmonyMethod(prefix));
        }

        private static bool Prefix(object __instance, object material, object offerOut, object offerIn, int delta)
        {
            if (!TryHandle(material, offerOut, offerIn, delta))
                return true;

            return false;
        }

        public static bool TryHandle(object materialObj, object offerOutObj, object offerInObj, int delta)
        {
            byte vanillaReason = Convert.ToByte(materialObj);

            if ((TransferManager.TransferReason)vanillaReason == TransferManager.TransferReason.None)
                return false;

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
            return new TransferManager.TransferOffer
            {
                Active = (bool)t.GetField("Active").GetValue(boxedOffer),
                Amount = (int)t.GetField("Amount").GetValue(boxedOffer),
                Building = (ushort)t.GetField("Building").GetValue(boxedOffer),
                Vehicle = (ushort)t.GetField("Vehicle").GetValue(boxedOffer),
                Citizen = (uint)t.GetField("Citizen").GetValue(boxedOffer),
                Position = (Vector3)t.GetField("Position").GetValue(boxedOffer),
                m_isLocalPark = (byte)(t.GetField("m_isLocalPark")?.GetValue(boxedOffer) ?? (byte)0),
                m_object = (InstanceID)(t.GetField("m_object")?.GetValue(boxedOffer) ?? default(InstanceID))
            };
        }
    }
}
