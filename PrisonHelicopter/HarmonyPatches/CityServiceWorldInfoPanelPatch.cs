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
    internal static class UpdateBindingsPatch
    {
        private static UICheckBox _prisonHelicopterCheckBox;

        [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "UpdateBindings")]
        [HarmonyPostfix]
        internal static void Postfix1(CityServiceWorldInfoPanel __instance, InstanceID ___m_InstanceID, UIPanel ___m_intercityTrainsPanel)
        {
            UIButton budget_btn = __instance.component.Find("Budget").GetComponent<UIButton>();
            _prisonHelicopterCheckBox = UiUtil.CreateCheckBox(__instance.component, "", "", GetEmptying(__instance));
            _prisonHelicopterCheckBox.width = 110f;
            _prisonHelicopterCheckBox.label.textColor = new Color32(185, 221, 254, 255);
            _prisonHelicopterCheckBox.label.textScale = 0.8125f;
            _prisonHelicopterCheckBox.AlignTo(budget_btn, UIAlignAnchor.TopLeft);
            _prisonHelicopterCheckBox.relativePosition = new Vector3(___m_intercityTrainsPanel.width - _prisonHelicopterCheckBox.width, 6f);
            _prisonHelicopterCheckBox.eventCheckChanged += (component, value) =>
            {
                SetEmptying(__instance, value);
                ModSettings.Save();
            };

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
            if(Singleton<BuildingManager>.exists)
            {
                var building_id = GetInstanceID(panel).Building;
                Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[building_id];
                if((building.m_flags & Building.Flags.Downgrading) != 0)
                {
                    return true;
                }
            }
            return false;
        }

        private static void SetEmptying(CityServiceWorldInfoPanel panel, bool value)
        {
            if (!Singleton<SimulationManager>.exists || GetInstanceID(panel).Building == 0) return;
            Singleton<SimulationManager>.instance.AddAction(() => ToggleEmptying(GetInstanceID(panel).Building, value));
        }

        private static InstanceID GetInstanceID(CityServiceWorldInfoPanel panel)
        {
            return (InstanceID)GetInstanceIDField().GetValue(panel);
        }

        private static FieldInfo GetInstanceIDField()
        {
            return typeof(WorldInfoPanel).GetField("m_InstanceID", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private static void ToggleEmptying(ushort building, bool value)
        {
            if (Singleton<BuildingManager>.exists)
            {
                var my_building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[building];
                my_building.Info.m_buildingAI.SetEmptying(building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[building], value);
            }
        }

    }
}
