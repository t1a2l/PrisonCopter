using PrisonHelicopter.HarmonyPatches;

namespace PrisonHelicopter.Utils {
    public class GameConnectionManager {

        internal static GameConnectionManager Instance;
        static GameConnectionManager() {
            Instance = new GameConnectionManager();
        }

        GameConnectionManager() {
            PoliceCopterAIConnection = PoliceCopterAIHook.GetConnection();
        }

        public PoliceCopterAIConnection PoliceCopterAIConnection { get; }



    }
}
