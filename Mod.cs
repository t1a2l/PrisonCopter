using System;
using System.Linq;
using CitiesHarmony.API;
using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using PrisonHelicopter.AI;
using PrisonHelicopter.HarmonyPatches;
using PrisonHelicopter.Utils;
using PrisonHelicopter.Utils.TransfersBridge;

namespace PrisonHelicopter
{
    public class Mod : LoadingExtensionBase, IUserMod
    {
        public static int PrisonersPercentage = 90;

        public static bool IsVehicleSelectorModEnabled = false;

        string IUserMod.Name => "Prison Helicopter Mod";

        string IUserMod.Description => "Allow the police helicopter depot to spawn prison helicopters to transport prisoners to jail";

        private static bool _setupDone;

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
                string[] PercentNumList = [.. Enum.GetValues(typeof(PercentNum)).Cast<int>().Select(x => x.ToString())];
                int index = Array.FindIndex(PercentNumList, item => { return item == PrisonersPercentage.ToString(); });
                uiHelper.AddDropdown("Wait for this percentage capacity before calling a transport", PercentNumList, index, b => { PrisonersPercentage = short.Parse(PercentNumList[b]); ModSettings.Save(); });
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

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            ItemClasses.Register();
            if (loading.currentMode != AppMode.Game)
            {
                return;
            }
            if (!HarmonyHelper.IsHarmonyInstalled)
            {
                return;
            }
            if (_setupDone)
            {
                return;
            }
            if(!Util.IsNaturalDisastersDLC() || !Util.IsAfterDarkDLC())
            {
                return;
            }

            TransfersAPI.Setup();
            _setupDone = true;

            if (Util.IsModActive("Vehicle Selector") || Util.IsModActive("2882769913"))
            {
                IsVehicleSelectorModEnabled = true;
            }
        }

        public override void OnReleased()
        {
            _setupDone = false;
            base.OnReleased();
            ItemClasses.Unregister();
            CityServiceWorldInfoPanelPatch.Reset();
            if (!HarmonyHelper.IsHarmonyInstalled)
            {
                return;
            }
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            try
            {
                if (Util.oldDataVersion)
                {
                    var buildings = Singleton<BuildingManager>.instance.m_buildings;
                    for (int i = 0; i < buildings.m_size; i++)
                    {
                        ref Building building = ref buildings.m_buffer[i];
                        if ((building.m_flags & Building.Flags.Created) != 0)
                        {
                            if (building.Info.GetAI() is PrisonCopterPoliceStationAI && building.Info.m_class.m_level < ItemClass.Level.Level4 ||
                            building.Info.GetAI() is HelicopterDepotAI && building.Info.m_class.m_service == ItemClass.Service.PoliceDepartment)
                            {
                                if ((building.m_flags & Building.Flags.Downgrading) == 0)
                                {
                                    building.m_flags |= Building.Flags.Downgrading;
                                }
                                else if ((building.m_flags & Building.Flags.Downgrading) != 0)
                                {
                                    building.m_flags &= ~Building.Flags.Downgrading;
                                }
                            }
                        }
                    }
                }

                LogHelper.Information("Reloaded Mod");
            }
            catch (Exception e)
            {
                LogHelper.Error(e.ToString());
            }
        }

        private bool IsInAssetEditor()
        {
            return !SimulationManager.exists
                   || SimulationManager.instance.m_metaData is { m_updateMode: SimulationManager.UpdateMode.LoadAsset or SimulationManager.UpdateMode.NewAsset };
        }

        private bool IsInGame()
        {
            return !SimulationManager.exists
                   || SimulationManager.instance.m_metaData is { m_updateMode: SimulationManager.UpdateMode.LoadGame or SimulationManager.UpdateMode.NewGameFromMap or SimulationManager.UpdateMode.NewGameFromScenario };
        }
    }

}
