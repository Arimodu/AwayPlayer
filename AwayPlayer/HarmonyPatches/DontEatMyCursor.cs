using HarmonyLib;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

namespace AwayPlayer.HarmonyPatches
{
    // Written by the one and only ChatGPT, thank you for your service
    public class DontEatMyCursor : MonoBehaviour, IInitializable
    {
        private static SiraLog Log;
        private static bool _hooksLocked = false;
        private static bool HooksLocked 
        { 
            get => _hooksLocked;
            set
            {
                Log.Debug($"Hooks {(value ? "Locked" : "Unlocked")}!");
                _hooksLocked = value;
            }
        }

        // Save values for pending set
        private static bool pendingVisibleValue;
        private static CursorLockMode pendingLockStateValue;

        [HarmonyPatch(typeof(Cursor))]
        [HarmonyPatch("set_visible")]
        class CursorVisiblePatch_Set
        {
            [HarmonyPrefix]
            static bool PrefixVisible(bool value)
            {
                if (HooksLocked)
                {
                    // Save the value to be set once the application is refocused
                    pendingVisibleValue = value;
                    Log.Debug("Cursor.Visible SAVED!");
                    return false; // Block the actual property set
                }
                Log.Debug("Cursor.Visible SET!");
                pendingVisibleValue = value;
                return true;
            }
        }

        [HarmonyPatch(typeof(Cursor))]
        [HarmonyPatch("set_lockState")]
        class CursorLockStatePatch_Set
        {
            [HarmonyPrefix]
            static bool PrefixLockState(CursorLockMode value)
            {
                if (HooksLocked)
                {
                    // Save the value to be set once the application is refocused
                    pendingLockStateValue = value;
                    Log.Debug("Cursor.lockState SAVED!");
                    return false; // Block the actual property set
                }
                Log.Debug("Cursor.lockState SET!");
                pendingLockStateValue = value;
                return true;
            }
        }

        [Inject]
        public void Setup(SiraLog log)
        {
            Log = log;
#if DEBUG
            Log.DebugMode = true;
#endif
            //log.Debug("DontEatMyCursor Injected!");
        }

        public void Initialize()
        {
            Plugin.Harmony.PatchAll();
            //Log.Debug("DontEatMyCursor Ready!");
        }

        void Update()
        {
            // Check if the mouse is over the game window and the application is not focused
            if (HooksLocked)
            {
                if (Input.GetMouseButtonDown(0) && IsMouseOverGameWindow())
                {
                    HooksLocked = false;

                    // Application is refocused, set the saved values if they are different from the current values
                    if (Cursor.visible != pendingVisibleValue)
                    {
                        Cursor.visible = pendingVisibleValue;
                    }

                    if (Cursor.lockState != pendingLockStateValue)
                    {
                        Cursor.lockState = pendingLockStateValue;
                    }
                }
            }
        }

        void OnApplicationFocus(bool hasFocus)
        {
            Log.Debug($"Application {(hasFocus ? "focused" : "unfocused")}");
            Log.Debug($"Mouse {(IsMouseOverGameWindow() ? "IS" : "IS NOT")} over game window");
            if (!hasFocus && !HooksLocked)
            {
                var lockState = Cursor.lockState;
                var visible = Cursor.visible;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                HooksLocked = true;
                Cursor.lockState = lockState; // Set the pending values to whatever the previous state was
                Cursor.visible = visible;
            }
        }

        private static bool IsMouseOverGameWindow()
        {
            // Check if the mouse position is within the game window boundaries
            return WindowUtils.GetGameWindowRect().Contains(Input.mousePosition);
        }
    }
}
