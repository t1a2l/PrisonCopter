using HarmonyLib;
using System;
using PrisonHelicopter.Utils;

namespace PrisonHelicopter.HarmonyPatches {

    public delegate void SimulationStepHelicopterAIDelegate(HelicopterAI instance, ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics);

    public class PoliceCopterAIConnection {
        internal PoliceCopterAIConnection(SimulationStepHelicopterAIDelegate simulationStepHelicopterAI) {
            SimulationStepHelicopterAI = simulationStepHelicopterAI ?? throw new ArgumentNullException(nameof(simulationStepHelicopterAI));
        }

        public SimulationStepHelicopterAIDelegate SimulationStepHelicopterAI { get; }
    }

    public static class PoliceCopterAIHook {

        private delegate void SimulationStepTarget(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics);

        private delegate void TryCollectCrimeTarget(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData);

        internal static PoliceCopterAIConnection GetConnection() {
            try {
                SimulationStepHelicopterAIDelegate simulationStepHelicopterAI =
                    AccessTools.MethodDelegate<SimulationStepHelicopterAIDelegate>(
                    TranspilerUtil.DeclaredMethod<SimulationStepTarget>(typeof(HelicopterAI), "SimulationStep"),
                    null,
                    false);
                return new PoliceCopterAIConnection(simulationStepHelicopterAI);
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

        public static void Prepare() {
            SimulationStepHelicopterAI = GameConnectionManager.Instance.PoliceCopterAIConnection.SimulationStepHelicopterAI;
        }


        [HarmonyPatch(typeof(PoliceCopterAI), "SimulationStep",
            new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vehicle.Frame), typeof(ushort), typeof(Vehicle), typeof(int) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
        [HarmonyPrefix]
        public static bool SimulationStep(PoliceCopterAI __instance, ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics) {
            if (__instance.m_info.m_class.m_level >= ItemClass.Level.Level4) {
                SimulationStepHelicopterAI(__instance, vehicleID, ref vehicleData, ref frameData, leaderID, ref leaderData, lodPhysics);
                return false;
            }
            return true;
        }


    }

}