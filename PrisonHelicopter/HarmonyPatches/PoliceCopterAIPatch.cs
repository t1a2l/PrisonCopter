using HarmonyLib;
using System;
using PrisonHelicopter.Utils;

namespace PrisonHelicopter.HarmonyPatches {

    public delegate void SimulationStepHelicopterAIDelegate(HelicopterAI instance, ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics);

    public delegate void LoadVehicleHelicopterAIDelegate(HelicopterAI instance, ushort vehicleID, ref Vehicle data);

    public delegate void ReleaseVehicleVehicleAIDelegate(VehicleAI instance, ushort vehicleID, ref Vehicle data);

    public class PoliceCopterAIConnection {
        internal PoliceCopterAIConnection(SimulationStepHelicopterAIDelegate simulationStepHelicopterAI,
                                          LoadVehicleHelicopterAIDelegate loadVehicleHelicopterAI,
                                          ReleaseVehicleVehicleAIDelegate releaseVehicleVehicleAI) {
            SimulationStepHelicopterAI = simulationStepHelicopterAI ?? throw new ArgumentNullException(nameof(simulationStepHelicopterAI));
            LoadVehicleHelicopterAI = loadVehicleHelicopterAI ?? throw new ArgumentNullException(nameof(loadVehicleHelicopterAI));
            ReleaseVehicleVehicleAI = releaseVehicleVehicleAI ?? throw new ArgumentNullException(nameof(releaseVehicleVehicleAI));
        }

        public SimulationStepHelicopterAIDelegate SimulationStepHelicopterAI { get; }

        public LoadVehicleHelicopterAIDelegate LoadVehicleHelicopterAI { get; }

        public ReleaseVehicleVehicleAIDelegate ReleaseVehicleVehicleAI { get; }
    }

    public static class PoliceCopterAIHook {

        private delegate void SimulationStepTarget(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics);

        private delegate void LoadVehicleTarget(ushort vehicleID, ref Vehicle data);

        private delegate void ReleaseVehicleVehicleTarget(ushort vehicleID, ref Vehicle data);

        internal static PoliceCopterAIConnection GetConnection() {
            try {
                SimulationStepHelicopterAIDelegate simulationStepHelicopterAI =
                    AccessTools.MethodDelegate<SimulationStepHelicopterAIDelegate>(
                    TranspilerUtil.DeclaredMethod<SimulationStepTarget>(typeof(HelicopterAI), "SimulationStep"),
                    null,
                    false);
                LoadVehicleHelicopterAIDelegate loadVehicleHelicopterAI =
                    AccessTools.MethodDelegate<LoadVehicleHelicopterAIDelegate>(
                    TranspilerUtil.DeclaredMethod<LoadVehicleTarget>(typeof(HelicopterAI), "LoadVehicle"),
                    null,
                    false);
                ReleaseVehicleVehicleAIDelegate releaseVehicleVehicleAI =
                    AccessTools.MethodDelegate<ReleaseVehicleVehicleAIDelegate>(
                    TranspilerUtil.DeclaredMethod<ReleaseVehicleVehicleTarget>(typeof(VehicleAI), "ReleaseVehicle"),
                    null,
                    false);
                return new PoliceCopterAIConnection(simulationStepHelicopterAI, loadVehicleHelicopterAI, releaseVehicleVehicleAI);
            }
            catch (Exception e) {
                LogHelper.Error(e.Message);
                return null;
            }
        }
    }


    [HarmonyPatch(typeof(PoliceCopterAI))]
    public static class PoliceCopterAIPatch {

        private static SimulationStepHelicopterAIDelegate SimulationStepHelicopterAI;

        private static LoadVehicleHelicopterAIDelegate LoadVehicleHelicopterAI;

        private static ReleaseVehicleVehicleAIDelegate ReleaseVehicleVehicleAI;

        public static void Prepare() {
            SimulationStepHelicopterAI = GameConnectionManager.Instance.PoliceCopterAIConnection.SimulationStepHelicopterAI;
            LoadVehicleHelicopterAI = GameConnectionManager.Instance.PoliceCopterAIConnection.LoadVehicleHelicopterAI;
            ReleaseVehicleVehicleAI = GameConnectionManager.Instance.PoliceCopterAIConnection.ReleaseVehicleVehicleAI;
        }


        [HarmonyPatch(typeof(PoliceCopterAI), "SimulationStep",
            [typeof(ushort), typeof(Vehicle), typeof(Vehicle.Frame), typeof(ushort), typeof(Vehicle), typeof(int)],
            [ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal])]
        [HarmonyPrefix]
        public static bool SimulationStep(PoliceCopterAI __instance, ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics)
        {
            if (__instance.m_info.m_class.m_level >= ItemClass.Level.Level4) {
                SimulationStepHelicopterAI(__instance, vehicleID, ref vehicleData, ref frameData, leaderID, ref leaderData, lodPhysics);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(PoliceCopterAI), "LoadVehicle")]
        [HarmonyPrefix]
        public static bool LoadVehicle(PoliceCopterAI __instance, ushort vehicleID, ref Vehicle data)
        {
            if (__instance.m_info.m_class.m_level >= ItemClass.Level.Level4) {
                LoadVehicleHelicopterAI(__instance, vehicleID, ref data);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(PoliceCopterAI), "ReleaseVehicle")]
        [HarmonyPrefix]
        public static bool ReleaseVehicle(PoliceCopterAI __instance, ushort vehicleID, ref Vehicle data)
        {
            if (__instance.m_info.m_class.m_level >= ItemClass.Level.Level4) {
                ReleaseVehicleVehicleAI(__instance, vehicleID, ref data);
                return false;
            }
            return true;
        }


    }

}