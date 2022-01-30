using ColossalFramework.UI;
using HarmonyLib;
using PrisonHelicopter.AI;
using System.Reflection;
using ColossalFramework;
using UnityEngine;
using PrisonHelicopter.Utils;

namespace PrisonHelicopter.HarmonyPatches
{
    [HarmonyPatch(typeof(CityServiceWorldInfoPanel))]
    internal static class CityServiceWorldInfoPanelPatch
    {
        private static UICheckBox _prisonHelicopterCheckBox;

        public static void Reset() {
            _prisonHelicopterCheckBox = null;
        }

        [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "UpdateBindings")]
        [HarmonyPostfix]
        internal static void UpdateBindings(CityServiceWorldInfoPanel __instance, InstanceID ___m_InstanceID)
        {
            if(_prisonHelicopterCheckBox == null)
            {
                UIPanel park_buttons_panel = __instance.component.Find("ParkButtons").GetComponent<UIPanel>();
                _prisonHelicopterCheckBox = UiUtil.CreateCheckBox(park_buttons_panel, "PrisonHelicopterAllow", "", !GetEmptying(__instance));
                _prisonHelicopterCheckBox.width = 110f;
                _prisonHelicopterCheckBox.label.textColor = new Color32(185, 221, 254, 255);
                _prisonHelicopterCheckBox.label.width = 550f;
                _prisonHelicopterCheckBox.label.textScale = 0.8125f;
                _prisonHelicopterCheckBox.relativePosition = new Vector3(-5f, 6f);
                _prisonHelicopterCheckBox.eventCheckChanged += (component, value) =>
                {
                    SetEmptying(__instance, value);
                    ModSettings.Save();
                };
            }
            
            var building1 = ___m_InstanceID.Building;
            var instance = BuildingManager.instance;
            var building2 = instance.m_buildings.m_buffer[building1];
            var info = building2.Info;
            var buildingAi = info.m_buildingAI;
            var newPoliceStationAI = buildingAi as NewPoliceStationAI;
            var helicopterDepotAI = buildingAi as HelicopterDepotAI;
            var policeHelicopterDepot = info.m_class.m_service == ItemClass.Service.PoliceDepartment && helicopterDepotAI;
            var policeStation = info.m_class.m_service == ItemClass.Service.PoliceDepartment && info.m_class.m_level < ItemClass.Level.Level4 && newPoliceStationAI;

            if(policeHelicopterDepot)
            {
                _prisonHelicopterCheckBox.isVisible = true;
                _prisonHelicopterCheckBox.label.text = "Allow Prison Helicopters";
                _prisonHelicopterCheckBox.tooltip = "Disable this if you prefer to use this helicopter depot only for police helicopters";
            }
            else if(policeStation)
            {
                _prisonHelicopterCheckBox.isVisible = true;
                _prisonHelicopterCheckBox.label.text = "Allow Prison Helicopters & Police Vans";
                _prisonHelicopterCheckBox.tooltip = "Disable this if you prefer that prison helicopters would not land and no police vans fleet to pick up criminals from other stations";
            }
            else
            {
                _prisonHelicopterCheckBox.isVisible = false;
            }
        }

        private static bool GetEmptying(CityServiceWorldInfoPanel panel)
        {
            var building_id = GetInstanceID(panel).Building;
            var building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[building_id];
            if((building.m_flags & Building.Flags.Downgrading) != 0) return true;
            return false;
        }

        private static void SetEmptying(CityServiceWorldInfoPanel panel, bool value)
        {
            var building_id = GetInstanceID(panel).Building;
            var building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[building_id];
            building.Info.m_buildingAI.SetEmptying(building_id, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[building_id], value);
        }

        private static InstanceID GetInstanceID(CityServiceWorldInfoPanel panel)
        {
            return (InstanceID)GetInstanceIDField().GetValue(panel);
        }

        private static FieldInfo GetInstanceIDField()
        {
            return typeof(WorldInfoPanel).GetField("m_InstanceID", BindingFlags.Instance | BindingFlags.NonPublic);
        }
    }
}
