using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ColossalFramework.Plugins;
using ICities;
using UnityEngine;

namespace PrisonHelicopter.Utils
{
    public static class Util
    {
        public static bool oldDataVersion = false;

        public static object GetInstanceField(Type type, object instance, string fieldName)
        {
            const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                           | BindingFlags.Static;
            var field = type.GetField(fieldName, bindFlags);
            if (field == null)
            {
                throw new Exception($"Type '{type}' doesn't have field '{fieldName}");
            }
            return field.GetValue(instance);
        }
        
        public static bool IsModActive(string modNamePart)
        {
            try
            {
                var plugins = PluginManager.instance.GetPluginsInfo();
                return (from plugin in plugins.Where(p => p.isEnabled)
                    select plugin.GetInstances<IUserMod>()
                    into instances
                    where instances.Any()
                    select instances[0].Name
                    into name
                    where name != null && name.Contains(modNamePart)
                    select name).Any();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to detect if mod with name containing {modNamePart} is active");
                Debug.LogException(e);
                return false;
            }
        }

        public static bool IsModActive(ulong modId)
        {
            try
            {
                var plugins = PluginManager.instance.GetPluginsInfo();
                return (from plugin in plugins.Where(p => p.isEnabled)
                    select plugin.publishedFileID
                    into workshopId
                    where workshopId.AsUInt64 == modId
                    select workshopId).Any();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to detect if mod {modId} is active");
                Debug.LogException(e);
                return false;
            }
        }

         /// <summary>
        /// Deserializes savegame data.
        /// </summary>
        /// <param name="reader">Reader to deserialize from.</param>
        /// <param name="dataVersion">Data version.</param>
        internal static void Deserialize(int dataVersion)
        {
            Debug.Log("deserializing save data");

            if (dataVersion <= 1)
            {
                oldDataVersion = true;
            }



        }

    }
}