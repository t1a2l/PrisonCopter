using CitiesHarmony.API;
using ICities;

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

    }

}
