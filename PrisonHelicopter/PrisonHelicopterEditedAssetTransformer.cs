using UnityEngine;

namespace PrisonHelicopter
{
    public static class PrisonHelicopterEditedAssetTransformer
    {

        public static void ToPrisonHelicopter() {
            var vehicleInfo = ToolsModifierControl.toolController?.m_editPrefabInfo as VehicleInfo;
            if (vehicleInfo?.m_vehicleAI is not PoliceCarAI policeCarAI)
            {
                Debug.LogWarning("Current asset is not a vehicle or is not PoliceCarAI");
                return;
            }
            vehicleInfo.m_dlcRequired |= SteamHelper.DLC_BitMask.AfterDarkDLC;
            vehicleInfo.m_dlcRequired |= SteamHelper.DLC_BitMask.NaturalDisastersDLC;
            vehicleInfo.m_vehicleType = VehicleInfo.VehicleType.Helicopter;
            vehicleInfo.m_class = ItemClasses.prisonHelicopterVehicle;
            vehicleInfo.m_isCustomContent = true;
            policeCarAI.m_info = PrefabCollection<VehicleInfo>.FindLoaded("Prison Van");
        }
    }
}