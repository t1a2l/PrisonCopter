using System;
using System.IO;
using ICities;

namespace PrisonHelicopter
{
    /// <summary>
    /// Handles savegame data saving and loading.
    /// </summary>
    public class Serializer : SerializableDataExtensionBase
    {
        // Current data version.
        private const int DataVersion = 2;

        // Unique data ID.
        private readonly string dataID = "PrisonHelicopter";

        /// <summary>
        /// Serializes data to the savegame.
        /// Called by the game on save.
        /// </summary>
        public override void OnSaveData()
        {
            base.OnSaveData();

            using (MemoryStream stream = new MemoryStream())
            {
                // Serialise savegame settings.
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    // Write version.
                    writer.Write(DataVersion);

                    // Write to savegame.
                    serializableDataManager.SaveData(dataID, stream.ToArray());

                    LogHelper.Information("wrote ", stream.Length);
                }
            }
        }

        /// <summary>
        /// Deserializes data from a savegame (or initialises new data structures when none available).
        /// Called by the game on load (including a new game).
        /// </summary>
        public override void OnLoadData()
        {
            base.OnLoadData();

            // Read data from savegame.
            byte[] data = serializableDataManager.LoadData(dataID);

            // Check to see if anything was read.
            if (data != null && data.Length != 0)
            {
                // Data was read - go ahead and deserialise.
                using (MemoryStream stream = new MemoryStream(data))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        // Read version.
                        int version = reader.ReadInt32();
                        LogHelper.Information("found data version ", version);

                        // Deserialise building settings.
                        Utils.Util.Deserialize(version);

                        LogHelper.Information("read ", stream.Length);
                    }
                }
            }
            else
            {
                // No data read.
                LogHelper.Information("no data read");
                Utils.Util.Deserialize(0);
            }
        }

    }
}