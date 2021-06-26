using System;
using UnityEngine;

namespace PrisonCopter {

    public delegate void SimulationStepHelicopterAIDelegate(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics);

    public delegate void TryCollectCrimeDelegate(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData);

    public delegate void LoadVehicleHelicopterAIDelegate(HelicopterAI instance, ushort vehicleID, ref Vehicle data);

    public delegate void RemoveTargetDelegate(PoliceCopterAI instance, ushort vehicleID, ref Vehicle data);

    public delegate bool StartPathFindDelegate(PoliceCopterAI instance, ushort vehicleID, ref Vehicle vehicleData);

    public delegate void EnsureCitizenUnitsVehicleAIDelegate(VehicleAI instance, ushort vehicleID, ref Vehicle data, int passengerCount);

    public delegate bool ShouldReturnToSourceDelegate(ushort vehicleID, ref Vehicle data);

    public delegate Vector4 CalculateTargetPointHelicopterAIDelegate(Vector3 refPos, Vector3 targetPos, float maxSqrDistance, float height);

    class PrisonCopterAIConnection {
        internal PrisonCopterAIConnection(SimulationStepHelicopterAIDelegate simulationStepHelicopterAI,
                                   TryCollectCrimeDelegate tryCollectCrime,
                                   LoadVehicleHelicopterAIDelegate loadVehicleHelicopterAI,
                                   RemoveTargetDelegate removeTarget,
                                   StartPathFindDelegate startPathFind,
                                   EnsureCitizenUnitsVehicleAIDelegate ensureCitizenUnitsVehicleAI,
                                   ShouldReturnToSourceDelegate shouldReturnToSource,
                                   CalculateTargetPointHelicopterAIDelegate calculateTargetPoint) {
            SimulationStepHelicopterAI = simulationStepHelicopterAI ?? throw new ArgumentNullException(nameof(simulationStepHelicopterAI));
            TryCollectCrime = tryCollectCrime ?? throw new ArgumentNullException(nameof(tryCollectCrime));
            LoadVehicleHelicopterAI = loadVehicleHelicopterAI ?? throw new ArgumentNullException(nameof(loadVehicleHelicopterAI));
            RemoveTarget = removeTarget ?? throw new ArgumentNullException(nameof(removeTarget));
            StartPathFind = startPathFind ?? throw new ArgumentNullException(nameof(startPathFind));
            EnsureCitizenUnitsVehicleAI = ensureCitizenUnitsVehicleAI ?? throw new ArgumentNullException( nameof(ensureCitizenUnitsVehicleAI));
            ShouldReturnToSource = shouldReturnToSource ?? throw new ArgumentNullException(nameof(shouldReturnToSource));
            CalculateTargetPoint = calculateTargetPoint ?? throw new ArgumentNullException(nameof(calculateTargetPoint));
        }

        public SimulationStepHelicopterAIDelegate SimulationStepHelicopterAI { get; }
        public TryCollectCrimeDelegate TryCollectCrime { get; }
        public LoadVehicleHelicopterAIDelegate LoadVehicleHelicopterAI { get; }
        public RemoveTargetDelegate RemoveTarget { get; }
        public StartPathFindDelegate StartPathFind { get; }
        public EnsureCitizenUnitsVehicleAIDelegate EnsureCitizenUnitsVehicleAI { get; }
        public ShouldReturnToSourceDelegate ShouldReturnToSource { get; }
        public CalculateTargetPointHelicopterAIDelegate CalculateTargetPoint { get; }
       
    }
}
