using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PrisonCopter
{
    public static class ItemClasses
    {
        public static readonly ItemClass prisonHeliVehicle = CreatePoliceCopterItemClass("Prison Helicopter");

        public static void Register()
        {
            var dictionary = ((Dictionary<string, ItemClass>)typeof(ItemClassCollection).GetField("m_classDict", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null));
            if (!dictionary.ContainsKey(ItemClasses.prisonHeliVehicle.name))
            {
                dictionary.Add(ItemClasses.prisonHeliVehicle.name, ItemClasses.prisonHeliVehicle);
            }
        }

        public static void Unregister()
        {
            var dictionary = ((Dictionary<string, ItemClass>)typeof(ItemClassCollection).GetField("m_classDict", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null));            dictionary.Remove(ItemClasses.prisonHeliVehicle.name);
        }
        
        private static ItemClass CreatePoliceCopterItemClass(string name)
        {
            var createInstance = ScriptableObject.CreateInstance<ItemClass>();
            createInstance.name = name;
            createInstance.m_level = ItemClass.Level.Level4;
            createInstance.m_service = ItemClass.Service.PoliceDepartment;
            createInstance.m_subService = ItemClass.SubService.None;
            return createInstance;
        }
    }
}