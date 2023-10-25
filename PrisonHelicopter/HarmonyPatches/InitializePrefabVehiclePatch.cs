using System;
using HarmonyLib;
using PrisonHelicopter.AI;
using PrisonHelicopter.Utils;
using UnityEngine;

namespace PrisonHelicopter.HarmonyPatches
{
    [HarmonyPatch(typeof(VehicleInfo))]
    internal static class InitializePrefabVehiclePatch
    { 
        [HarmonyPatch(typeof(VehicleInfo), "InitializePrefab")]
        [HarmonyPrefix]
        public static bool InitializePrefab(VehicleInfo __instance)
        {
            try
            {
                
                if (__instance.m_class.m_service == ItemClass.Service.PoliceDepartment && __instance.m_class.name == ItemClasses.prisonHelicopterVehicle.name && __instance.m_vehicleType == VehicleInfo.VehicleType.Helicopter)
                {
                    var oldAI = __instance.GetComponent<PrefabAI>();
                    UnityEngine.Object.DestroyImmediate(oldAI);
                    var newAI = (PrefabAI)__instance.gameObject.AddComponent<PrisonCopterAI>();
                    PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                }
                var component = __instance.GetComponent<PrefabAI>();
                if (component != null && component is PoliceCarAI && __instance.m_class.m_service == ItemClass.Service.PoliceDepartment && __instance.m_vehicleType == VehicleInfo.VehicleType.Car)
                {
                    var oldAI = __instance.GetComponent<PrefabAI>();
                    UnityEngine.Object.DestroyImmediate(oldAI);
                    var newAI = (PrefabAI)__instance.gameObject.AddComponent<ExtendedPoliceCarAI>();
                    PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return true;
        }
    }
}