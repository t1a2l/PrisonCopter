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

        [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "UpdateBindings")]
        [HarmonyPostfix]
        internal static void Postfix1(CityServiceWorldInfoPanel __instance, InstanceID ___m_InstanceID, UIPanel ___m_intercityTrainsPanel)
        {
            var label = ___m_intercityTrainsPanel.Find<UILabel>("Label");
            _originalLabel ??= label.text;
            _originalTooltip ??= label.tooltip;
            var building1 = ___m_InstanceID.Building;
            var instance = BuildingManager.instance;
            var building2 = instance.m_buildings.m_buffer[building1];
            var info = building2.Info;
            var buildingAi = info.m_buildingAI;
            var newPoliceStationAI = buildingAi as NewPoliceStationAI;
            var helicopterDepotAI = buildingAi as HelicopterDepotAI;
            var policeHelicopterDepot = info.m_class.m_service == ItemClass.Service.PoliceDepartment && helicopterDepotAI;
            var policeStation = info.m_class.m_service == ItemClass.Service.PoliceDepartment &&  info.m_class.m_level < ItemClass.Level.Level4 && newPoliceStationAI;
            if(policeHelicopterDepot)
            {
                 ___m_intercityTrainsPanel.isVisible = true;
                label.text = "Allow Prison Helicopters";
                label.tooltip = "Disable this if you prefer to use this helicopter depot only for police helicopters";
            }
            else if(policeStation)
            {
                 ___m_intercityTrainsPanel.isVisible = true;
                label.text = "Allow Prison Helicopters and Police Vans fleet";
                label.tooltip = "Disable this if you prefer to allow only police vans to pick up prisoners from large police stations and prison vans from prison";
            }
            else
            {
                label.text = _originalLabel;
                label.tooltip =  _originalTooltip;
            }
        }
    }
}
