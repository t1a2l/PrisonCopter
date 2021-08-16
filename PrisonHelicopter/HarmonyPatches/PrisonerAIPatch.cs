using ColossalFramework;
using HarmonyLib;
using System;
using UnityEngine;

namespace PrisonHelicopter.PrisonHelicopter.HarmonyPatches {

    [HarmonyPatch(typeof(PrisonerAI))]
    public static class PrisonerAIPatch {

        [HarmonyPatch(typeof(PrisonerAI), "GetLocalizedStatus")]
        [HarmonyPrefix]
        public static bool GetLocalizedStatus(PrisonerAI __instance, ushort instanceID, ref CitizenInstance data, out InstanceID target, ref string __result)
	{
	    if ((data.m_flags & (CitizenInstance.Flags.Blown | CitizenInstance.Flags.Floating)) != 0)
	    {
		target = InstanceID.Empty;
		__result = ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_CONFUSED");
	    }
            else
            {
                ushort targetBuilding = data.m_targetBuilding;
	        if (targetBuilding != 0)
	        {
		    target = InstanceID.Empty;
		    target.Building = targetBuilding;
		    __result = "Serving time at ";
	        }
                else
                {
                    target = InstanceID.Empty;
	            __result = ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_CONFUSED");
                }
            }
            target = InstanceID.Empty;
	    return false;
	}
    }
}
