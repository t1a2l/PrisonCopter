using System;
using HarmonyLib;
using PrisonHelicopter.AI;
using PrisonHelicopter.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

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
                if (__instance?.m_class?.name != ItemClasses.prisonHelicopterVehicle.name)
                {
                    return true;
                }
                var oldAi = __instance.GetComponent<PrefabAI>();
                Object.DestroyImmediate(oldAi);
                var newAI = (PrefabAI)__instance.gameObject.AddComponent<PrisonCopterAI>();
                PrefabUtil.TryCopyAttributes(oldAi, newAI, false);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return true;
        }
    }
}