using UnityEngine;

namespace PrisonHelicopter
{
    public static class PrisonHelicopterEditedAssetTransformer
    {
        public static void ToPrisonHelicopter() {
            var vehicleInfo = ToolsModifierControl.toolController?.m_editPrefabInfo as VehicleInfo;
            if (vehicleInfo?.m_vehicleType != VehicleInfo.VehicleType.Helicopter) 
            {
                Debug.LogWarning("Current asset is not a vehicle or is not a Helicopter");
                return;
            }
            vehicleInfo.m_requiredExpansion  |= SteamHelper.ExpansionBitMask.AfterDark;
            vehicleInfo.m_requiredExpansion |= SteamHelper.ExpansionBitMask.NaturalDisasters;
            vehicleInfo.m_class = ItemClasses.prisonHelicopterVehicle;
            vehicleInfo.m_isCustomContent = true;
        }
    }
}