using ColossalFramework.UI;
using HarmonyLib;
using PrisonHelicopter.AI;

namespace PrisonHelicopter.HarmonyPatches
{
    using System.Reflection;
    using ColossalFramework;
    using PrisonHelicopter;
    using UnityEngine;

    [HarmonyPatch(typeof(CityServiceWorldInfoPanel))]
    internal static class UpdateBindingsPatch
    {
        private static string _originalLabel;
        private static string _originalTooltip;

        private static UISprite _sprite;
        private static UILabel _label;

        public static void Reset() {
            _label = null;
            _sprite = null;
            _originalLabel = null;
            _originalTooltip = null;
        }

        [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "UpdateBindings")]
        [HarmonyPostfix]
        internal static void Postfix1(CityServiceWorldInfoPanel __instance, InstanceID ___m_InstanceID, UIPanel ___m_intercityTrainsPanel)
        {

            if (_sprite == null) {
                _sprite = UIUtil.CreateSprite(__instance.component,
                                                  (_, _) => {
                                                      SetEmptying(
                                                          __instance,
                                                          !GetEmptying(__instance));
                                                  }, new Vector3 (100, 300) );
            }

            if (_label == null) {
                _label = UIUtil.CreateLabel(
                    "",
                    __instance.component,
                    new Vector3(125, 300));
                _label.textColor = new Color32(185, 221, 254, 255);
                _label.textScale = 0.8125f;
            }


            var label = _label;
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
                 _label.isVisible = true;
                 _sprite.isVisible = true;
                 UpdateSprite(__instance);
                label.text = "Allow Prison Helicopters";
                label.tooltip = "Disable this if you prefer to use this helicopter depot only for police helicopters";
            }
            else if(policeStation)
            {
                _label.isVisible = true;
                _sprite.isVisible = true;
                UpdateSprite(__instance);
                label.text = "Allow Prison Helicopters & Police Vans";
                label.tooltip = "Disable this if you prefer that prison helicopters would not land and no police vans fleet to pick up criminals from other stations";
            }
            else
            {
                _label.isVisible = false;
                _sprite.isVisible = false;
                label.text = _originalLabel;
                label.tooltip =  _originalTooltip;
            }
        }

        private static void UpdateSprite(CityServiceWorldInfoPanel __instance) {
            _sprite.spriteName = GetEmptying(__instance) ? "check-checked" : "check-unchecked";
        }

        private static bool GetEmptying(CityServiceWorldInfoPanel panel) {
            return Singleton<BuildingManager>.exists &&
                   GetInstanceID(panel).Building != 0 &&
                   (Singleton<BuildingManager>.instance.m_buildings
                                              .m_buffer[GetInstanceID(panel).Building]
                                              .m_flags & Building.Flags.Downgrading) !=
                   Building.Flags.None;
        }

        private static void SetEmptying(CityServiceWorldInfoPanel panel, bool value) {
            if (!Singleton<SimulationManager>.exists || GetInstanceID(panel).Building == 0)
                return;
            Singleton<SimulationManager>.instance.AddAction(() => ToggleEmptying(GetInstanceID(panel).Building, value));
        }

        private static InstanceID GetInstanceID(CityServiceWorldInfoPanel panel) {
            return (InstanceID)GetInstanceIDField().GetValue(panel);
        }

        private static FieldInfo GetInstanceIDField() {
            return typeof(WorldInfoPanel)
                .GetField("m_InstanceID", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private static void ToggleEmptying(ushort building, bool value) {
            if (Singleton<BuildingManager>.exists)
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[building].Info.m_buildingAI.SetEmptying(building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[building], value);
        }
    }
}
