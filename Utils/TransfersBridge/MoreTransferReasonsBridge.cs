using System;
using System.Linq;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;
using static TransferManager;

namespace PrisonHelicopter.Utils.TransfersBridge
{
    internal static class MoreTransferReasonsBridge
    {
        public static bool Available { get; private set; }

        private static object _managerInstance;

        private static Type _managerType;
        private static Type _managerVehicleType;
        private static Type _offerType;
        private static Type _reasonType;

        private static MethodInfo _addOutgoingOffer;
        private static MethodInfo _addIncomingOffer;
        private static MethodInfo _removeOutgoingOffer;
        private static MethodInfo _removeIncomingOffer;

        public static MethodInfo _startTransfer;
        public static MethodInfo _startDistrictTransfer;

        private static MethodInfo _calculateOwnVehicles;
        private static MethodInfo _calculateGuestVehicles;
        private static MethodInfo _createVehicle;

        private static FieldInfo _offerActive;
        private static FieldInfo _offerAmount;
        private static FieldInfo _offerBuilding;
        private static FieldInfo _offerCitizen;
        private static FieldInfo _offerPosition;
        private static FieldInfo _offerPriority;
        private static FieldInfo _offerVehicle;
        
        public static bool Setup()
        {
            try
            {
                Assembly asm = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "MoreTransferReasons")
                    ?? Assembly.Load("MoreTransferReasons");

                if (asm == null)
                {
                    LogHelper.Warning("MoreTransferReasons assembly not found. MoreTransferReasonsBridge will be unavailable.");
                    return false;
                }

                _managerType = asm.GetType("MoreTransferReasons.ExtendedTransferManager");
                if (_managerType == null)
                {
                    LogHelper.Warning("ExtendedTransferManager not found. MoreTransferReasonsBridge will be unavailable.");
                    return false;
                }

                _managerVehicleType = asm.GetType("MoreTransferReasons.ExtendedVehicleManager");
                if (_managerVehicleType == null)
                {
                    LogHelper.Warning("ExtendedVehicleManager not found. MoreTransferReasonsBridge will be unavailable.");
                    return false;
                }

                _offerType = _managerType.GetNestedType("Offer", BindingFlags.Public | BindingFlags.NonPublic);
                if (_offerType == null)
                {
                    LogHelper.Warning("Offer type not found. MoreTransferReasonsBridge will be unavailable.");
                    return false;
                }

                _reasonType = _managerType.GetNestedType("TransferReason", BindingFlags.Public | BindingFlags.NonPublic);
                if (_reasonType == null)
                {
                    LogHelper.Warning("TransferReason type not found. MoreTransferReasonsBridge will be unavailable.");
                    return false;
                }

                _managerInstance = GetManagerInstance(_managerType);

                if (_managerInstance == null)
                {
                    LogHelper.Warning("Could not get Singleton<ExtendedTransferManager>.instance. MoreTransferReasonsBridge will be unavailable.");
                    return false;
                }

                _addOutgoingOffer = _managerType.GetMethod("AddOutgoingOffer", BindingFlags.Public | BindingFlags.Instance);
                _addIncomingOffer = _managerType.GetMethod("AddIncomingOffer", BindingFlags.Public | BindingFlags.Instance);
                _removeOutgoingOffer = _managerType.GetMethod("RemoveOutgoingOffer", BindingFlags.Public | BindingFlags.Instance);
                _removeIncomingOffer = _managerType.GetMethod("RemoveIncomingOffer", BindingFlags.Public | BindingFlags.Instance);
                _startTransfer = _managerType.GetMethod("StartTransfer", BindingFlags.NonPublic | BindingFlags.Instance);
                _startDistrictTransfer = _managerType.GetMethod("StartDistrictTransfer", BindingFlags.NonPublic | BindingFlags.Instance);

                _calculateOwnVehicles = _managerVehicleType.GetMethod("CalculateOwnVehicles", BindingFlags.Public | BindingFlags.Static);
                _calculateGuestVehicles = _managerVehicleType.GetMethod("CalculateGuestVehicles", BindingFlags.Public | BindingFlags.Static);
                _createVehicle = _managerVehicleType.GetMethod("CreateVehicle", BindingFlags.Public | BindingFlags.Static);

                _offerActive = _offerType.GetField("Active");
                _offerAmount = _offerType.GetField("Amount");
                _offerBuilding = _offerType.GetField("Building");
                _offerCitizen = _offerType.GetField("Citizen");
                _offerPosition = _offerType.GetField("Position");
                _offerPriority = _offerType.GetField("Priority");
                _offerVehicle = _offerType.GetField("Vehicle");

                Available =
                    _addOutgoingOffer != null &&
                    _addIncomingOffer != null &&
                    _removeOutgoingOffer != null &&
                    _removeIncomingOffer != null &&
                    _startTransfer != null &&
                    _startDistrictTransfer != null &&
                    _calculateOwnVehicles != null &&
                    _calculateGuestVehicles != null &&
                    _createVehicle != null &&
                    _offerPosition != null &&
                    _offerAmount != null;

                return Available;
            }
            catch (Exception e)
            {
                LogHelper.Error($"Failed to setup MoreTransferReasons bridge: {e}");
                Available = false;
                return false;
            }
        }

        public static void AddOutgoingOffer(byte material, TransferOfferData offer)
        {
            if (!Available) return;
            object reason = Enum.ToObject(_reasonType, material);
            object reflectedOffer = BuildOffer(offer);
            _addOutgoingOffer.Invoke(_managerInstance, [reason, reflectedOffer]);
        }

        public static void AddIncomingOffer(byte material, TransferOfferData offer)
        {
            if (!Available) return;
            object reason = Enum.ToObject(_reasonType, material);
            object reflectedOffer = BuildOffer(offer);
            _addIncomingOffer.Invoke(_managerInstance, [reason, reflectedOffer]);
        }

        public static void RemoveOutgoingOffer(byte material, TransferOfferData offer)
        {
            if (!Available) return;
            object reason = Enum.ToObject(_reasonType, material);
            object reflectedOffer = BuildOffer(offer);
            _removeOutgoingOffer.Invoke(_managerInstance, [reason, reflectedOffer]);
        }

        public static void RemoveIncomingOffer(byte material, TransferOfferData offer)
        {
            if (!Available) return;
            object reason = Enum.ToObject(_reasonType, material);
            object reflectedOffer = BuildOffer(offer);
            _removeIncomingOffer.Invoke(_managerInstance, [reason, reflectedOffer]);
        }

        public static void CalculateOwnVehicles(ushort buildingID, ref Building data, byte material, ref int count, ref int cargo, ref int capacity, ref int outside)
        {
            if (!Available) return;
            object reason = Enum.ToObject(_reasonType, material);
            object[] args = [buildingID, data, reason, count, cargo, capacity, outside];
            _calculateOwnVehicles.Invoke(null, args);
            data = (Building)args[1];
            count = (int)args[3];
            cargo = (int)args[4];
            capacity = (int)args[5];
            outside = (int)args[6];
        }

        public static void CalculateGuestVehicles(ushort buildingID, ref Building data, byte material, ref int count, ref int cargo, ref int capacity, ref int outside)
        {
            if (!Available) return;
            object reason = Enum.ToObject(_reasonType, material);
            object[] args = [buildingID, data, reason, count, cargo, capacity, outside];
            _calculateGuestVehicles.Invoke(null, args);
            data = (Building)args[1];
            count = (int)args[3];
            cargo = (int)args[4];
            capacity = (int)args[5];
            outside = (int)args[6];
        }

        public static bool CreateVehicle(out ushort vehicle, ref Randomizer r, VehicleInfo info, Vector3 position, byte type, bool transferToSource, bool transferToTarget)
        {
            vehicle = 0;

            if (!Available) return false;

            object[] args =
            [
                (ushort)0,   // out vehicle placeholder
                r,           // ref Randomizer
                info,
                position,
                type,        // byte, no enum conversion needed
                transferToSource,
                transferToTarget
            ];

            object result = _createVehicle.Invoke(null, args);

            vehicle = (ushort)args[0];
            r = (Randomizer)args[1];

            return result is bool b && b;
        }

        private static object BuildOffer(TransferOfferData src)
        {
            object boxed = Activator.CreateInstance(_offerType);
            _offerActive?.SetValue(boxed, src.Active);
            _offerAmount?.SetValue(boxed, src.Amount);
            _offerBuilding?.SetValue(boxed, src.Building);
            _offerCitizen?.SetValue(boxed, src.Citizen);
            _offerPosition?.SetValue(boxed, src.Position);
            _offerPriority?.SetValue(boxed, src.Priority);
            _offerVehicle?.SetValue(boxed, src.Vehicle);
            return boxed;
        }

        private static object GetManagerInstance(Type managerType)
        {
            Type singletonType = typeof(Singleton<>).MakeGenericType(managerType);

            PropertyInfo instanceProp =
                singletonType.GetProperty("instance", BindingFlags.Public | BindingFlags.Static) ??
                singletonType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);

            if (instanceProp != null)
                return instanceProp.GetValue(null, null);

            FieldInfo instanceField =
                singletonType.GetField("instance", BindingFlags.Public | BindingFlags.Static) ??
                singletonType.GetField("Instance", BindingFlags.Public | BindingFlags.Static);

            return instanceField?.GetValue(null);
        }
    }
}