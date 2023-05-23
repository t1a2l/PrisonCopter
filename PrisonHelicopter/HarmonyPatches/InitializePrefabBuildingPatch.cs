using System;
using HarmonyLib;
using PrisonHelicopter.AI;
using PrisonHelicopter.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PrisonHelicopter.HarmonyPatches
{
    [HarmonyPatch(typeof(BuildingInfo), "InitializePrefab")]
    public static class InitializePrefabBuildingPatch
    { 
        public static void Prefix(BuildingInfo __instance)
        {
            try
            {
                if (__instance.m_class.m_service == ItemClass.Service.PoliceDepartment && __instance.m_class.m_subService != ItemClass.SubService.PoliceDepartmentBank && __instance.m_class.m_level != ItemClass.Level.Level3)
                {
                    var oldAI = __instance.GetComponent<PrefabAI>();
                    Object.DestroyImmediate(oldAI);
                    var newAI = (PrefabAI)__instance.gameObject.AddComponent<PrisonCopterPoliceStationAI>();
                    PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                }
                if (__instance.m_class.m_service == ItemClass.Service.PoliceDepartment && __instance.m_class.m_subService != ItemClass.SubService.PoliceDepartmentBank && __instance.m_class.m_level == ItemClass.Level.Level3)
                {
                    var oldAI = __instance.GetComponent<PrefabAI>();
                    Object.DestroyImmediate(oldAI);
                    var newAI = (PrefabAI)__instance.gameObject.AddComponent<PoliceHelicopterDepotAI>();
                    PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}