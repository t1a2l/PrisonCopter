using HarmonyLib;
using System.Reflection;

namespace PrisonHelicopter.Utils
{
    public static class PatchUtil
    {
        public const string HarmonyId = "t1a2l.PrisonCopter";

        private static bool patched = false;

        public static void PatchAll()
        {
            if (patched) return;

            LogHelper.Information("Prison Helicopter: Patching...");

            patched = true;

            // Apply your patches here!
            // Harmony.DEBUG = true;
            var harmony = new Harmony("t1a2l.PrisonCopter");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static void UnpatchAll()
        {
            if (!patched) return;

            var harmony = new Harmony(HarmonyId);
            harmony.UnpatchAll(HarmonyId);

            patched = false;

            LogHelper.Information("Prison Helicopter: Reverted...");
        }
    }

    // Random example patch
    [HarmonyPatch(typeof(SimulationManager), "CreateRelay")]
    public static class SimulationManagerCreateRelayPatch
    {
        public static void Prefix()
        {
            LogHelper.Information("CreateRelay Prefix");
        }
    }

    // Random example patch
    [HarmonyPatch(typeof(LoadingManager), "MetaDataLoaded")]
    public static class LoadingManagerMetaDataLoadedPatch
    {
        public static void Prefix()
        {
            LogHelper.Information("MetaDataLoaded Prefix");
        }
    }
}