using PrisonHelicopter.Utils;
using CitiesHarmony.API;
using ICities;
using System;
using ColossalFramework.UI;
using System.Linq;

namespace PrisonHelicopter {

    public class PrisonHelicopterMod : LoadingExtensionBase, IUserMod {

        public static int PriosnersPercentage;
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
                int index = Array.FindIndex(PercentNumList, item => { return item == "90"; });
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
                for (uint i = 0; i < loadedBuildingInfoCount; i++) {
                    var bi = PrefabCollection<BuildingInfo>.GetLoaded(i);
                    if (bi is null) continue;
                    if (bi.GetAI() is PoliceStationAI) {
                        AiReplacementHelper.ApplyNewAIToBuilding(bi);
                    }
                }
                LogHelper.Information("Reloaded Mod");
            }
            catch (Exception e) {
                LogHelper.Information(e.ToString());
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
