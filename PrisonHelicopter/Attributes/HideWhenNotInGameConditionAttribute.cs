using System;
using PrisonHelicopter.OptionsFramework.Attibutes;

namespace PrisonHelicopter.Attibutes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class HideWhenNotInGameConditionAttribute : HideConditionAttribute
    {
        public override bool IsHidden()
        {
            return !SimulationManager.exists
                   || SimulationManager.instance.m_metaData is not {m_updateMode: SimulationManager.UpdateMode.LoadGame or SimulationManager.UpdateMode.NewGameFromMap or SimulationManager.UpdateMode.NewGameFromScenario};
        }
    }

}