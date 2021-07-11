using CitiesHarmony.API;
using ICities;
using UnityEngine;

namespace PrisonHelicopter
{
    public class LoadingExtension : LoadingExtensionBase
    {
        private static GameObject _gameObject;
        
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

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            if (mode != LoadMode.NewGame && mode != LoadMode.LoadGame)
            {
                return;
            }
            if (_gameObject != null)
            {
                return;
            }
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            if (_gameObject == null)
            {
                return;
            }
            Object.Destroy(_gameObject);
            _gameObject = null;
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