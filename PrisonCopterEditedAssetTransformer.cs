using UnityEngine;

namespace PrisonCopter
{
    public static class PrisonCopterEditedAssetTransformer
    {

        public static void ToPrisonCopter() {
            var vehicleInfo = ToolsModifierControl.toolController?.m_editPrefabInfo as VehicleInfo;
            vehicleInfo.m_dlcRequired |= SteamHelper.DLC_BitMask.AfterDarkDLC;
            vehicleInfo.m_dlcRequired |= SteamHelper.DLC_BitMask.NaturalDisastersDLC;
            vehicleInfo.m_vehicleType = VehicleInfo.VehicleType.Helicopter;
            vehicleInfo.m_class = ItemClasses.prisonHeliVehicle;
            vehicleInfo.m_isCustomContent = true;
        }
    }
}