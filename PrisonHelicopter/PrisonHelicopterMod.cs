using PrisonHelicopter.Utils;
using CitiesHarmony.API;
using ICities;
using System;
using ColossalFramework.UI;
using System.Linq;
using ColossalFramework;
using PrisonHelicopter.AI;

namespace PrisonHelicopter
{

    public class PrisonHelicopterMod : LoadingExtensionBase, IUserMod
    {

        public static int PriosnersPercentage = 90;
        string IUserMod.Name => "Prison Helicopter Mod";
        string IUserMod.Description => "Allow the police helicopter depot to spawn prison helicopters to transport prisoners to jail";

        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelper uiHelper = helper.AddGroup("PrisonHelicopter-Options") as UIHelper;
            UIPanel self = uiHelper.self as UIPanel;

            if (IsInAssetEditor())
            {
                uiHelper.AddButton("To prison helicopter", PrisonHelicopterEditedAssetTransformer.ToPrisonHelicopter);
            }

            if (IsInGame())
            {
                string[] PercentNumList = Enum.GetValues(typeof(PercentNum)).Cast<int>().Select(x => x.ToString()).ToArray();
                int index = Array.FindIndex(PercentNumList, item => { return item == PriosnersPercentage.ToString(); });
                uiHelper.AddDropdown("Wait for this percentage capacity before calling a transport", PercentNumList, index, b => { PriosnersPercentage = Int16.Parse(PercentNumList[b]); ModSettings.Save(); });
            }

        }

        public void OnEnabled()
        {
            ModSettings.Load();
            HarmonyHelper.DoOnHarmonyReady(() => PatchUtil.PatchAll());
        }

        public void OnDisabled()
        {
            if (HarmonyHelper.IsHarmonyInstalled) PatchUtil.UnpatchAll();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            try {
                var loadedBuildingInfoCount = PrefabCollection<BuildingInfo>.LoadedCount();
                for (uint buildingId = 0; buildingId < loadedBuildingInfoCount; buildingId++) {
                    var bi = PrefabCollection<BuildingInfo>.GetLoaded(buildingId);
                    if (bi is null) continue;
                    if (bi.GetAI() is PoliceStationAI policeStationAI) {
                        AiReplacementHelper.ApplyNewAIToBuilding(bi);
                        ref Building building = ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingId];
                    }
                }

                if(Util.oldDataVersion)
                {
                    var buildings = Singleton<BuildingManager>.instance.m_buildings;
                    for (int i = 0; i < buildings.m_size; i++) {
                        ref Building building = ref buildings.m_buffer[i];
                        if((building.m_flags & Building.Flags.Created) != 0)
                        { 
                            if((building.Info.GetAI() is PrisonCopterPoliceStationAI && building.Info.m_class.m_level < ItemClass.Level.Level4) ||
                            (building.Info.GetAI() is HelicopterDepotAI && building.Info.m_class.m_service == ItemClass.Service.PoliceDepartment))
                            {
                                if((building.m_flags & Building.Flags.Downgrading) == 0) {
                                    building.m_flags |= Building.Flags.Downgrading;
                                }
                                else if((building.m_flags & Building.Flags.Downgrading) != 0) {
                                    building.m_flags &= ~Building.Flags.Downgrading;
                                }
                            }
                        }
                    }
                }
                
                LogHelper.Information("Reloaded Mod");
            }
            catch (Exception e) {
                LogHelper.Error(e.ToString());
            }
        }

        private bool IsInAssetEditor()
        {           
            return !SimulationManager.exists
                   || SimulationManager.instance.m_metaData is {m_updateMode: SimulationManager.UpdateMode.LoadAsset or SimulationManager.UpdateMode.NewAsset};
        }

        private bool IsInGame()
        {
            return !SimulationManager.exists
                   || SimulationManager.instance.m_metaData is {m_updateMode: SimulationManager.UpdateMode.LoadGame or SimulationManager.UpdateMode.NewGameFromMap or SimulationManager.UpdateMode.NewGameFromScenario};
        }
    }

}
