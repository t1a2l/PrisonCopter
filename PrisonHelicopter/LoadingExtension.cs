using CitiesHarmony.API;
using ICities;
using System;

namespace PrisonHelicopter
{
    using HarmonyPatches;

    public class LoadingExtension : LoadingExtensionBase
    {

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            ItemClasses.Register();
            if (loading.currentMode != AppMode.Game)
            {
                return;
            }
            if (!HarmonyHelper.IsHarmonyInstalled)
            {
                return;
            }

        }

        public override void OnReleased()
        {
            base.OnReleased();
            ItemClasses.Unregister();
            if (!HarmonyHelper.IsHarmonyInstalled)
            {
                return;
            }
        }
    }
}