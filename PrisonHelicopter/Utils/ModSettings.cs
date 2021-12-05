using System;
using System.IO;
using System.Xml.Serialization;

namespace PrisonHelicopter.Utils {

    /// <summary>
    /// Global mod settings.
    /// </summary>
    [XmlRoot("PrisonHelicopter")]
    public class ModSettings {

        // Settings file name.
        [XmlIgnore]
        private static readonly string SettingsFileName = "PrisonHelicopter.xml";

        // User settings directory.
        [XmlIgnore]
        private static readonly string UserSettingsDir = ColossalFramework.IO.DataLocation.localApplicationData;

        // Full userdir settings file name.
        [XmlIgnore]
        private static readonly string SettingsFile = Path.Combine(UserSettingsDir, SettingsFileName);

        [XmlElement("DropDown")]
        public int PriosnersPercentage
        {
            get => PrisonHelicopterMod.PriosnersPercentage;

            set => PrisonHelicopterMod.PriosnersPercentage = value;
        } 

        /// <summary>
        /// Load settings from XML file.
        /// </summary>
        internal static void Load()
        {
            try
            {
                // Attempt to read new settings file (in user settings directory).
                string fileName = SettingsFile;
                if (!File.Exists(fileName))
                {
                    // No settings file in user directory; use application directory instead.
                    fileName = SettingsFileName;

                    if (!File.Exists(fileName))
                    {
                        LogHelper.Information("no settings file found");
                        return;
                    }
                }

                // Read settings file.
                using (StreamReader reader = new StreamReader(fileName))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ModSettings));
                    if (!(xmlSerializer.Deserialize(reader) is ModSettings settingsFile))
                    {
                        LogHelper.Error("couldn't deserialize settings file");
                    }
                }
  
            }
            catch (Exception e)
            {
                LogHelper.Error("exception reading XML settings file", e);
            }
        }


        /// <summary>
        /// Save settings to XML file.
        /// </summary>
        internal static void Save()
        {
            try
            {
                // Pretty straightforward.
                using (StreamWriter writer = new StreamWriter(SettingsFile))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ModSettings));
                    xmlSerializer.Serialize(writer, new ModSettings());
                }

                // Cleaning up after ourselves - delete any old config file in the application direcotry.
                if (File.Exists(SettingsFileName))
                {
                    File.Delete(SettingsFileName);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("exception saving XML settings file", e);
            }
        }
    }
}
