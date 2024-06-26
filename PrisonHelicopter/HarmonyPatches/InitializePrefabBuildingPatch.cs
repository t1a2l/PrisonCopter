using System;
using HarmonyLib;
using PrisonHelicopter.AI;
using PrisonHelicopter.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PrisonHelicopter.HarmonyPatches
{
    [HarmonyPatch(typeof(BuildingInfo))]
    public static class InitializePrefabBuildingPatch
    {
        [HarmonyPatch(typeof(BuildingInfo), "InitializePrefab")]
        [HarmonyPrefix]
        public static void InitializePrefabPrefix(BuildingInfo __instance)
        {
            try
            {
                if (__instance.m_class.m_service == ItemClass.Service.PoliceDepartment && __instance.m_class.m_subService != ItemClass.SubService.PoliceDepartmentBank && __instance.m_class.m_level != ItemClass.Level.Level3)
                {
                    var oldAI = __instance.GetComponent<PrefabAI>();
                    if (oldAI is PoliceStationAI policeStation)
                    {
                        var count = policeStation.m_policeCarCount;
                        Object.DestroyImmediate(oldAI);
                        var newAI = (PrefabAI)__instance.gameObject.AddComponent<PrisonCopterPoliceStationAI>();

                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        if (newAI is PrisonCopterPoliceStationAI policeStationAI)
                        {
                            policeStationAI.m_policeCarCount = count;
                            policeStationAI.m_policeVanCount = count / 2;
                            policeStationAI.m_noiseAccumulation = policeStation.m_noiseAccumulation;
                        }
                    }

                }
                if (__instance.m_class.m_service == ItemClass.Service.PoliceDepartment && __instance.m_class.m_subService != ItemClass.SubService.PoliceDepartmentBank && __instance.m_class.m_level == ItemClass.Level.Level3)
                {
                    var oldAI = __instance.GetComponent<PrefabAI>();
                    if(oldAI is HelicopterDepotAI helicopter)
                    {
                        var count = helicopter.m_helicopterCount;
                        Object.DestroyImmediate(oldAI);
                        var newAI = (PrefabAI)__instance.gameObject.AddComponent<PoliceHelicopterDepotAI>();

                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        if(newAI is PoliceHelicopterDepotAI police)
                        {
                            police.m_policeHelicopterCount = count;
                            police.m_prisonHelicopterCount = count;
                        }
                    }
                    
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}