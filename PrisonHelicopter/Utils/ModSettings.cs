using System;
using System.IO;
using System.Xml.Serialization;

namespace PrisonHelicopter.Utils {
    class ModSettings {
        // Settings file name.
        [XmlIgnore]
        private static readonly string SettingsFileName = "PrisonHelicopter.xml";

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
                // Check to see if configuration file exists.
                if (File.Exists(SettingsFileName))
                {
                    // Read it.
                    using (StreamReader reader = new StreamReader(SettingsFileName))
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(ModSettings));
                        if (!(xmlSerializer.Deserialize(reader) is ModSettings settingsFile))
                        {
                            LogHelper.Error("couldn't deserialize settings file");
                        }
                    }
                }
                else
                {
                    LogHelper.Information("no settings file found");
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
                // Pretty straightforward.  Serialisation is within GBRSettingsFile class.
                using (StreamWriter writer = new StreamWriter(SettingsFileName))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ModSettings));
                    xmlSerializer.Serialize(writer, new ModSettings());
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("exception saving XML settings file", e);
            }
        }
    }
}
