
namespace PrisonCopter {

    internal class GameConnectionManager {

        internal static GameConnectionManager Instance;
        static GameConnectionManager() {
            Instance = new GameConnectionManager();
        }

        GameConnectionManager() {
            PrisonCopterAIConnection = PrisonCopterAIHook.GetConnection();
        }

        public PrisonCopterAIConnection PrisonCopterAIConnection { get; }
    }
}