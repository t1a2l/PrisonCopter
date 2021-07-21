using ColossalFramework.UI;
using HarmonyLib;

namespace PrisonHelicopter.HarmonyPatches.CityServiceWorldInfoPanelPatches
{
    [HarmonyPatch(typeof(CityServiceWorldInfoPanel))]
    internal static class UpdateBindingsPatch
    {
        private static string _originalLabel;

        private static string _originalTooltip;

        private static string _originalCheckBoxText;

        private static string _originalCheckBoxTooltip;

        [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "UpdateBindings")]
        [HarmonyPostfix]
        internal static void UpdateBindings(CityServiceWorldInfoPanel __instance, InstanceID ___m_InstanceID, UIPanel ___m_intercityTrainsPanel)
        {
            var label = ___m_intercityTrainsPanel.Find<UILabel>("Label");
            var checkbox = ___m_intercityTrainsPanel.Find<UICheckBox>("AcceptIntercityTrains");
            _originalLabel ??= label.text;
            _originalTooltip ??= label.tooltip;
            _originalCheckBoxText ??= checkbox.text;
            _originalCheckBoxTooltip ??= checkbox.tooltip;
            var building1 = ___m_InstanceID.Building;
            var instance = BuildingManager.instance;
            var building2 = instance.m_buildings.m_buffer[building1];
            var info = building2.Info;
            var buildingAi = info.m_buildingAI;
            var policeHelicopterDepotAi = buildingAi as HelicopterDepotAI;
            if (policeHelicopterDepotAi == null)
            {
                label.text = _originalLabel;
                return;
            }
            if(info.m_class.m_service == ItemClass.Service.PoliceDepartment) {
                ___m_intercityTrainsPanel.isVisible = true;
                label.text = "Allow Prison Helicopters";
                label.tooltip = "Disable this if you prefer to use this helicopter depot only for police helicopters";
                checkbox.text = "Allow Prison Helicopters";
                checkbox.tooltip = "Disable this if you prefer to use this helicopter depot only for police helicopters";
            }
            else
            {
                ___m_intercityTrainsPanel.isVisible = false;
                label.text = _originalLabel;
                label.tooltip = _originalTooltip;
                checkbox.text = _originalCheckBoxText;
                checkbox.tooltip = _originalCheckBoxTooltip;
            }
        }
    }
}
