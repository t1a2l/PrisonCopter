using HarmonyLib;
using System;
using PrisonHelicopter.Utils;

namespace PrisonHelicopter.HarmonyPatches {

    public delegate void SimulationStepHelicopterAIDelegate(HelicopterAI instance, ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics);

    public delegate void TryCollectCrimeDelegate(PoliceCopterAI instance, ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData);

    public delegate void ArriveAtTargetDelegate(PoliceCopterAI instance, ushort vehicleID, ref Vehicle data);

    public delegate bool ShouldReturnToSourceDelegate(PoliceCopterAI instance, ushort vehicleID, ref Vehicle data);

    public class PoliceCopterAIConnection {
        internal PoliceCopterAIConnection(SimulationStepHelicopterAIDelegate simulationStepHelicopterAI,
                                          TryCollectCrimeDelegate tryCollectCrime,
                                          ArriveAtTargetDelegate arriveAtTarget,
                                          ShouldReturnToSourceDelegate shouldReturnToSource) {
            SimulationStepHelicopterAI = simulationStepHelicopterAI ?? throw new ArgumentNullException(nameof(simulationStepHelicopterAI));
            TryCollectCrime = tryCollectCrime ?? throw new ArgumentNullException(nameof(tryCollectCrime));
            ArriveAtTarget = arriveAtTarget ?? throw new ArgumentNullException(nameof(arriveAtTarget));
            ShouldReturnToSource = shouldReturnToSource ?? throw new ArgumentNullException(nameof(shouldReturnToSource));
        }

        public SimulationStepHelicopterAIDelegate SimulationStepHelicopterAI { get; }

        public TryCollectCrimeDelegate TryCollectCrime { get; }

        public ArriveAtTargetDelegate ArriveAtTarget { get; }

        public ShouldReturnToSourceDelegate ShouldReturnToSource { get; }
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
                TryCollectCrimeDelegate tryCollectCrime =
                    AccessTools.MethodDelegate<TryCollectCrimeDelegate>(
                    TranspilerUtil.DeclaredMethod<TryCollectCrimeTarget>(typeof(PoliceCopterAI), "TryCollectCrime"),
                    null,
                    true);
                ArriveAtTargetDelegate arriveAtTarget =
                    TranspilerUtil.CreateDelegate<ArriveAtTargetDelegate>(
                        typeof(PoliceCopterAI),
                        "RemoveTarget",
                        true);
                ShouldReturnToSourceDelegate shouldReturnToSource =
                    TranspilerUtil.CreateDelegate<ShouldReturnToSourceDelegate>(
                        typeof(PoliceCopterAI),
                        "ShouldReturnToSource",
                        true);
                return new PoliceCopterAIConnection(simulationStepHelicopterAI, tryCollectCrime, arriveAtTarget, shouldReturnToSource);
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

        private static TryCollectCrimeDelegate TryCollectCrime;

        private static ArriveAtTargetDelegate ArriveAtTarget;

        private static ShouldReturnToSourceDelegate ShouldReturnToSource;

        public static void Prepare() {
            SimulationStepHelicopterAI = GameConnectionManager.Instance.PoliceCopterAIConnection.SimulationStepHelicopterAI;
            TryCollectCrime = GameConnectionManager.Instance.PoliceCopterAIConnection.TryCollectCrime;
            ArriveAtTarget = GameConnectionManager.Instance.PoliceCopterAIConnection.ArriveAtTarget;
            ShouldReturnToSource = GameConnectionManager.Instance.PoliceCopterAIConnection.ShouldReturnToSource;
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
            frameData.m_blinkState = (((vehicleData.m_flags & Vehicle.Flags.Emergency2) == 0) ? 0f : 10f);
            TryCollectCrime(__instance, vehicleID, ref vehicleData, ref frameData);
            SimulationStepHelicopterAI(__instance, vehicleID, ref vehicleData, ref frameData, leaderID, ref leaderData, lodPhysics);
            if ((vehicleData.m_flags & Vehicle.Flags.Stopped) != 0) {
                if (__instance.CanLeave(vehicleID, ref vehicleData)) {
                    vehicleData.m_flags &= ~Vehicle.Flags.Stopped;
                    vehicleData.m_flags |= Vehicle.Flags.Leaving;
                }
            } else if ((vehicleData.m_flags & Vehicle.Flags.Arriving) != 0 && vehicleData.m_targetBuilding != 0 && (vehicleData.m_flags & (Vehicle.Flags.Emergency2 | Vehicle.Flags.WaitingPath | Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget)) == 0) {
                ArriveAtTarget(__instance, vehicleID, ref vehicleData);
            }
            if ((vehicleData.m_flags & Vehicle.Flags.GoingBack) == 0 && (vehicleData.m_transferSize >= __instance.m_crimeCapacity || ShouldReturnToSource(__instance, vehicleID, ref vehicleData))) {
                __instance.SetTarget(vehicleID, ref vehicleData, 0);
            }
            return false;
        }


    }

}