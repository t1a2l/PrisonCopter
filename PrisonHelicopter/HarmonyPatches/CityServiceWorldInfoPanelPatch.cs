using ColossalFramework.UI;
using HarmonyLib;
using PrisonHelicopter.AI;

namespace PrisonHelicopter.HarmonyPatches
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
        internal static void Postfix1(CityServiceWorldInfoPanel __instance, InstanceID ___m_InstanceID, UIPanel ___m_intercityTrainsPanel)
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
            if(info.m_class.m_service == ItemClass.Service.PoliceDepartment && info.m_class.m_level < ItemClass.Level.Level4 && buildingAi is NewPoliceStationAI)
            {
                ___m_intercityTrainsPanel.isVisible = true;
                label.text = "Allow Prison Helicopters to land and a Prison Vans fleet";
                label.tooltip = "Disable this if you prefer to get only prison vans from prisons and other police station";
                checkbox.text = "Allow Prison Helicopters to land and a Prison Vans fleet";
                checkbox.tooltip = "Disable this if you prefer to get only prison vans from prisons and other police station";
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
