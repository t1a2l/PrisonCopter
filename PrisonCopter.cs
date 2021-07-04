using PrisonCopter.OptionsFramework.Extensions;
using CitiesHarmony.API;
using ICities;
using System;

namespace PrisonCopter {
    public class Mod : LoadingExtensionBase, IUserMod {

        string IUserMod.Name => "Prison Copter Mod";

        string IUserMod.Description => "Allow the police helicopter depot to spawn prison helicopters";

        public void OnEnabled() {
            HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());
        }

        public void OnDisabled() {
            if (HarmonyHelper.IsHarmonyInstalled) Patcher.UnpatchAll();
        }

        public static bool inGame = false;

        public override void OnLevelLoaded(LoadMode mode) {
            if (mode != LoadMode.NewGame && mode != LoadMode.LoadGame) {
                return;
            }
            try {
                inGame = true;
                var loadedVehicleInfoCount = PrefabCollection<VehicleInfo>.LoadedCount();
                for (uint i = 0; i < loadedVehicleInfoCount; i++) {
                    var vi = PrefabCollection<VehicleInfo>.GetLoaded(i);
                    if (vi is null) continue;
                    if (vi.name.Equals("PrisonHelicopter.PrisonHeli_Data")) {
                        LogHelper.Information(vi.name);
                        AIHelper.ApplyNewAIToVehicle(vi);
                    }
                }
                LogHelper.Information("Reloaded Mod");
            }
            catch (Exception e) {
                LogHelper.Information(e.ToString());
            }
        }

        public override void OnLevelUnloading() {
            base.OnLevelUnloading();
            if (!inGame)
                return;
            inGame = false;
            LogHelper.Information("Unloading done!" + Environment.NewLine);
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            helper.AddOptionsGroup<Options>();
        }

    }

}
