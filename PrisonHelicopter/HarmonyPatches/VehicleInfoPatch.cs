using System;
using HarmonyLib;
using PrisonHelicopter.AI;
using PrisonHelicopter.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PrisonHelicopter.HarmonyPatches.VehicleInfoPatch
{
    [HarmonyPatch(typeof(VehicleInfo))]
    internal static class VehicleInfoPatch
    {
 
        [HarmonyPatch(typeof(VehicleInfo), "InitializePrefab")]
        [HarmonyPrefix]
        private static bool InitializePrefab(VehicleInfo __instance)
        {
            try
            {
                if (__instance?.m_class?.name != ItemClasses.prisonHelicopterVehicle.name)
                {
                    return true;
                }

                var oldAi = __instance.GetComponent<PrefabAI>();
                Object.DestroyImmediate(oldAi);
                var newAI = (PrefabAI)__instance.gameObject.AddComponent<PrisonHelicopterAI>();
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