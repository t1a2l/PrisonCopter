using HarmonyLib;
using System;

namespace PrisonCopter {
    public static class PrisonCopterAIHook {
        private delegate void SimulationStepTarget(ushort vehicleID,
                                                   ref Vehicle vehicleData,
                                                   ref Vehicle.Frame frameData,
                                                   ushort leaderID,
                                                   ref Vehicle leaderData,
                                                   int lodPhysics);

        private delegate void TryCollectCrimeTarget(ushort vehicleID,
                                                      ref Vehicle vehicleData,
                                                      ref Vehicle.Frame frameData);

        internal static PrisonCopterAIConnection GetConnection() {
            try {
                SimulationStepHelicopterAIDelegate simulationStepHelicopterAI =
                    AccessTools.MethodDelegate<SimulationStepHelicopterAIDelegate>(
                    TranspilerUtil.DeclaredMethod<SimulationStepTarget>(typeof(HelicopterAI), "SimulationStep"),
                    null,
                    true);
                TryCollectCrimeDelegate tryCollectCrime =
                    AccessTools.MethodDelegate<TryCollectCrimeDelegate>(
                    TranspilerUtil.DeclaredMethod<TryCollectCrimeTarget>(typeof(PoliceCopterAI), "TryCollectCrime"),
                    null,
                    true);
                LoadVehicleHelicopterAIDelegate loadVehicleHelicopterAI =
                    TranspilerUtil.CreateDelegate<LoadVehicleHelicopterAIDelegate>(
                        typeof(HelicopterAI),
                        "LoadVehicle",
                        true);
                RemoveTargetDelegate removeTarget =
                    TranspilerUtil.CreateDelegate<RemoveTargetDelegate>(
                        typeof(PoliceCopterAI),
                        "RemoveTarget",
                        true);
                StartPathFindDelegate startPathFind =
                    TranspilerUtil.CreateDelegate<StartPathFindDelegate>(
                        typeof(PoliceCopterAI),
                        "StartPathFind",
                        true);
                EnsureCitizenUnitsVehicleAIDelegate ensureCitizenUnitsVehicleAI =
                    TranspilerUtil.CreateDelegate<EnsureCitizenUnitsVehicleAIDelegate>(
                        typeof(VehicleAI),
                        "EnsureCitizenUnits",
                        true);
                ShouldReturnToSourceDelegate shouldReturnToSource =
                    TranspilerUtil.CreateDelegate<ShouldReturnToSourceDelegate>(
                        typeof(PoliceCopterAI),
                        "ShouldReturnToSource",
                        true);
                CalculateTargetPointHelicopterAIDelegate calculateTargetPoint =
                    TranspilerUtil.CreateDelegate<CalculateTargetPointHelicopterAIDelegate>(
                        typeof(HelicopterAI),
                        "CalculateTargetPoint",
                        true);

                return new PrisonCopterAIConnection(
                    simulationStepHelicopterAI,
                    tryCollectCrime,
                    loadVehicleHelicopterAI,
                    removeTarget,
                    startPathFind,
                    ensureCitizenUnitsVehicleAI,
                    shouldReturnToSource,
                    calculateTargetPoint);
            } catch (Exception e) {
                LogHelper.Error(e.Message);
                return null;
            }
        }
    }
}
