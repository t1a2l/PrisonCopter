using ColossalFramework;
using ColossalFramework.UI;
using HarmonyLib;
using UnityEngine;
using PrisonHelicopter.AI;
using PrisonHelicopter.Utils;

namespace PrisonHelicopter.HarmonyPatches
{

    [HarmonyPatch(typeof(CityServiceWorldInfoPanel))]
    internal static class CityServiceWorldInfoPanelPatch
    {
        public static UICheckBox _checkBox;
        private static ushort _cachedBuilding;

        public static void Reset() {
            _checkBox = null;
            _cachedBuilding = 0;
        }

        [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "UpdateBindings")]
        [HarmonyPostfix]
        internal static void Postfix1(CityServiceWorldInfoPanel __instance, InstanceID ___m_InstanceID)
        {
            if (_checkBox == null)
            {
                _checkBox = UiUtil.CreateCheckBox(__instance.component.Find<UIPanel>("MainBottom"), "AllowMovingPrisonersCheckBox", "", false);
                _checkBox.label.textColor = new Color32(185, 221, 254, 255);
                _checkBox.label.textScale = 0.8125f;
                _checkBox.AlignTo(__instance.component, UIAlignAnchor.BottomLeft);
                _checkBox.relativePosition = new Vector3(190, 290);
                _checkBox.label.width = 300;
            }

            var building_id = ___m_InstanceID.Building;
            var instance = BuildingManager.instance;
            var building = instance.m_buildings.m_buffer[building_id];
            var info = building.Info;
            var buildingAi = info.m_buildingAI;
            var prisonCopterPoliceStationAI = buildingAi as PrisonCopterPoliceStationAI;
            var helicopterDepotAI = buildingAi as HelicopterDepotAI;
            var policeHelicopterDepot = info.m_class.m_service == ItemClass.Service.PoliceDepartment && helicopterDepotAI;
            var policeStation = info.m_class.m_service == ItemClass.Service.PoliceDepartment && info.m_class.m_level < ItemClass.Level.Level4 && prisonCopterPoliceStationAI;

            if(policeHelicopterDepot)
            {
                _checkBox.isVisible = true;
                UpdateCheckedState(building_id);
                _checkBox.text = "Allow Prison Helicopters";
                _checkBox.tooltip = "Enable this to allow prison helicopters to also spawn";
                _checkBox.eventCheckChanged += SetAllowMovingPrisoners;
            }
            else if(policeStation)
            {
                _checkBox.isVisible = true;
                UpdateCheckedState(building_id);
                _checkBox.text = "Allow Prison Helicopters & Police Vans";
                _checkBox.tooltip = "Enable this if you want prison helicopters to land and have a police vans fleet to pick up criminals from other stations";
                _checkBox.eventCheckChanged += SetAllowMovingPrisoners;
            }
            else
            {
                _checkBox.isVisible = false;
            }
        }

        private static void UpdateCheckedState(ushort building)
        {
            if (building == _cachedBuilding)
            {
                return;
            }
            _cachedBuilding = building;
            _checkBox.isChecked = IsMovingPrisonersAllowed(building);
        }

        private static bool IsMovingPrisonersAllowed(ushort building)
        {
            if(Singleton<BuildingManager>.exists && building != 0)
            {
                Building building_to_check = Singleton<BuildingManager>.instance.m_buildings.m_buffer[building];
                if((building_to_check.m_flags & Building.Flags.Downgrading) != 0) return true;
            }
            return false;
        }

        private static void SetAllowMovingPrisoners(UIComponent _, bool value)
        {
            if (!Singleton<SimulationManager>.exists || _cachedBuilding == 0) return;
            Singleton<SimulationManager>.instance.AddAction(() => ToggleEmptying(_cachedBuilding, value));
        }       

        private static void ToggleEmptying(ushort building, bool value)
        {
            if (Singleton<BuildingManager>.exists)
            {
                var building_to_set = Singleton<BuildingManager>.instance.m_buildings.m_buffer[building];
                building_to_set.Info.m_buildingAI.SetEmptying(building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[building], value);
            }
        }
    }
}
