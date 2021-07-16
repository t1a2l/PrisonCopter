using PrisonHelicopter.HarmonyPatches.PoliceCopterAIPatch;
using PrisonHelicopter.HarmonyPatches.PoliceStationAIPatch;

namespace PrisonHelicopter.Utils {
    public class GameConnectionManager {

        internal static GameConnectionManager Instance;
        static GameConnectionManager() {
            Instance = new GameConnectionManager();
        }

        GameConnectionManager() {
            PoliceCopterAIConnection = PoliceCopterAIHook.GetConnection();
            PoliceStationAIConnection = PoliceStationAIHook.GetConnection();
        }

        public PoliceCopterAIConnection PoliceCopterAIConnection { get; }

        public PoliceStationAIConnection PoliceStationAIConnection { get; }

    }
}
