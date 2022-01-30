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
        private static UICheckBox _checkBox;
        private static ushort _cachedBuilding;

        public static void Reset() {
            _checkBox = null;
            _cachedBuilding = 0;
        }

        [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "UpdateBindings")]
        [HarmonyPostfix]
        internal static void Postfix1(CityServiceWorldInfoPanel __instance, InstanceID ___m_InstanceID)
        {

            if (_checkBox == null) {
                _checkBox = UiUtil.CreateCheckBox(
                    __instance.component.Find<UIPanel>("MainBottom"),
                    "AllowMovingPrisonersCheckBox",
                    "",
                    false);
                _checkBox.label.textColor = new Color32(185, 221, 254, 255);
                _checkBox.label.textScale = 0.8125f;
                _checkBox.AlignTo(__instance.component, UIAlignAnchor.BottomLeft);
                _checkBox.relativePosition = new Vector3(125, 290);
                _checkBox.label.width = 300;
            }

            var building1 = ___m_InstanceID.Building;
            var instance = BuildingManager.instance;
            var building2 = instance.m_buildings.m_buffer[building1];
            var info = building2.Info;
            var buildingAi = info.m_buildingAI;
            var newPoliceStationAI = buildingAi as PrisonCopterPoliceStationAI;
            var helicopterDepotAI = buildingAi as HelicopterDepotAI;
            var policeHelicopterDepot = info.m_class.m_service == ItemClass.Service.PoliceDepartment && helicopterDepotAI;
            var policeStation = info.m_class.m_service == ItemClass.Service.PoliceDepartment &&  info.m_class.m_level < ItemClass.Level.Level4 && newPoliceStationAI;
            if(policeHelicopterDepot)
            {
                 _checkBox.isVisible = true;
                 UpdateCheckedState(building1);
                _checkBox.text = "Allow Prison Helicopters";
                _checkBox.tooltip = "Disable this if you prefer to use this helicopter depot only for police helicopters";
            }
            else if(policeStation)
            {
                _checkBox.isVisible = true;
                UpdateCheckedState(building1);
                _checkBox.text = "Allow Prison Helicopters & Police Vans";
                _checkBox.tooltip = "Disable this if you prefer that prison helicopters would not land and no police vans fleet to pick up criminals from other stations";
            }
            else {
                _checkBox.isVisible = false;
            }
        }

        private static void UpdateCheckedState(ushort building) {
            var allowMovingPrisoners = GetAllowMovingPrisoners(building);
            if (allowMovingPrisoners == _checkBox.isChecked && building == _cachedBuilding) {
                return;
            }
            _cachedBuilding = building;
            _checkBox.eventCheckChanged -= HandleCheckBox;
            _checkBox.isChecked = allowMovingPrisoners;
            _checkBox.eventCheckChanged += HandleCheckBox;
        }

        private static void HandleCheckBox(UIComponent _, bool value) {
            SetAllowMovingPrisoners(_cachedBuilding, value);
        }

        private static bool GetAllowMovingPrisoners(ushort building) {
            return Singleton<BuildingManager>.exists &&
                   building != 0 &&
                   (Singleton<BuildingManager>.instance.m_buildings
                                              .m_buffer[building]
                                              .m_flags & Building.Flags.Downgrading) ==
                   Building.Flags.None;
        }

        private static void SetAllowMovingPrisoners(ushort building, bool value) {
            if (!Singleton<SimulationManager>.exists || building == 0)
                return;
            Singleton<SimulationManager>.instance.AddAction(() => ToggleEmptying(building, !value));
        }

        private static void ToggleEmptying(ushort building, bool value) {
            if (Singleton<BuildingManager>.exists)
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[building].Info.m_buildingAI.SetEmptying(building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[building], value);
        }
    }
}
