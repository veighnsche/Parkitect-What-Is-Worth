using System;
using System.Collections.Generic;
using System.Linq;
using Parkitect;
using UnityEngine;
using HarmonyLib;
using System.Windows.Markup;
using System.IO;
using MiniJSON;
using Mono.Security.Authenticode;

namespace WhatIsWorth {
    public class WhatIsWorth : AbstractMod, IModSettings
    {
        public const string VERSION_NUMBER = "1.0";
        public override string getIdentifier() => "cc.yal.WhatIsWorth";
        public override string getName() => "What It's Worth";
        public override string getDescription() => @"Shows excitement contribution breakdown";

        public override string getVersionNumber() => VERSION_NUMBER;
        public override bool isMultiplayerModeCompatible() => true;
        public override bool isRequiredByAllPlayersInMultiplayerMode() => true;

        private string _modPath;
        public string _settingsFilePath;

        public static WhatIsWorthConfig _config;
        public static WhatIsWorth Instance;
        private Harmony _harmony;
        private string editMultiplier = "";

        public override void onEnabled() {
            Instance = this;
            Debug.LogWarning("[WhatIsWorth] Loading!");
            MarketResearchValueEstimateCache.clear();

            _modPath = ModManager.Instance.getMod(this.getIdentifier()).path;
            _settingsFilePath = System.IO.Path.Combine(_modPath, "WhatIsWorth.json");

            reloadSettingsFromFile();

            Debug.Log("[WhatIsWorth] Doing a Harmony patch!");
			_harmony = new Harmony(getIdentifier());
			_harmony.PatchAll();
			Debug.Log("[WhatIsWorth] Patched alright!");
		}

        public override void onDisabled() {
            MarketResearchValueEstimateCache.clear();
            _harmony?.UnpatchAll(getIdentifier());
		}

        public void onDrawSettingsUI() {
			// GUI settings style
			const float leftWidth = 200;

			GUIStyle guistyleTextLeft = new GUIStyle(GUI.skin.label);
			guistyleTextLeft.margin = new RectOffset(10, 10, 10, 0);
			guistyleTextLeft.alignment = TextAnchor.MiddleLeft;

			GUIStyle guistyleTextMiddle = new GUIStyle(GUI.skin.label);
			guistyleTextMiddle.margin = new RectOffset(0, 10, 10, 0);
			guistyleTextMiddle.alignment = TextAnchor.MiddleCenter;

			GUIStyle guistyleField = new GUIStyle(GUI.skin.textField);
			guistyleField.margin = new RectOffset(0, 10, 10, 0);
			guistyleField.alignment = TextAnchor.MiddleCenter;

			GUIStyle guistyleButton = new GUIStyle(GUI.skin.button);
			guistyleButton.margin = new RectOffset(0, 10, 10, 0);
			guistyleButton.alignment = TextAnchor.MiddleCenter;

			// GUI settings layout
			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Version", guistyleTextLeft, GUILayout.Width(leftWidth));
			GUILayout.Label(VERSION_NUMBER, guistyleTextMiddle);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Precision", guistyleTextLeft, GUILayout.Width(leftWidth));
			editMultiplier = GUILayout.TextField(editMultiplier, 7, guistyleField);
			GUILayout.EndHorizontal();

			// Check the values when enter is pressed
			if (Event.current.isKey && Event.current.keyCode == KeyCode.Return) {
				// Try to convert the input text to a float
				if (float.TryParse(editMultiplier, out float result)) {
                    _config.multiplier = result;
				}
				// Clear the focus from the TextField
				GUI.FocusControl(null);
			}

			GUILayout.EndVertical();
		}
        public void onSettingsOpened() {
            reloadSettingsFromFile();
        }

        public void onSettingsClosed() {
            saveSettingsToFile();
            GUI.FocusControl(null);
        }

        private void reloadSettingsFromFile() {
            if (File.Exists(_settingsFilePath)) {
                // Load existing settings from JSON file
                Debug.Log("[WhatIsWorth] Loading config!");
                string json = File.ReadAllText(_settingsFilePath);
                _config = JsonUtility.FromJson<WhatIsWorthConfig>(json);
            } else {
                // Create new settings with default values
                Debug.Log("[WhatIsWorth] Creating a new config!");
                _config = new WhatIsWorthConfig();
                saveSettingsToFile();
            }
            editMultiplier = _config.multiplier.ToString();
        }

        private void saveSettingsToFile() {
            Debug.Log("[WhatIsWorth] Saving config!");
            string json = JsonUtility.ToJson(_config, true);
            File.WriteAllText(_settingsFilePath, json);
        }
    }
}
