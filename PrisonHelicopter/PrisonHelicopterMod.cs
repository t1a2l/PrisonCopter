using PrisonHelicopter.OptionsFramework.Extensions;
using PrisonHelicopter.Utils;
using CitiesHarmony.API;
using ICities;
using System;

namespace PrisonHelicopter {

    public class PrisonHelicopterMod : LoadingExtensionBase, IUserMod {

        string IUserMod.Name => "Prison Helicopter Mod";
        string IUserMod.Description => "Allow the police helicopter depot to spawn prison helicopters to transport prisoners to jail";

        public void OnSettingsUI(UIHelperBase helper)
        {
            helper.AddOptionsGroup<Options>();
        }

        public void OnEnabled() {
            HarmonyHelper.DoOnHarmonyReady(() => PatchUtil.PatchAll());
        }

        public void OnDisabled() {
            if (HarmonyHelper.IsHarmonyInstalled) PatchUtil.UnpatchAll();
        }

        public override void OnLevelLoaded(LoadMode mode) {
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
    }

}
