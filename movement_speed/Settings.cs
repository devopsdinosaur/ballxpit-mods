using HarmonyLib;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class Settings {
    private static Settings m_instance = null;
    public static Settings Instance {
        get {
            if (m_instance == null) {
                m_instance = new Settings();
            }
            return m_instance;
        }
    }
    private DDPlugin m_plugin = null;

    // General
    public static MelonPreferences_Category m_category_general;
    public static MelonPreferences_Entry<bool> m_enabled;
    public static MelonPreferences_Entry<string> m_log_level;
    public static MelonPreferences_Entry<string> m_player_name;

    public static MelonPreferences_Entry<float> m_speed_multiplier_start;
    public static MelonPreferences_Entry<float> m_speed_multiplier_delta;

    // Hotkeys
    public static MelonPreferences_Category m_category_hotkeys;
    public static MelonPreferences_Entry<string> m_hotkey_modifier;
    public static MelonPreferences_Entry<string> m_hotkey_speed_up;
    public static MelonPreferences_Entry<string> m_hotkey_speed_down;

    public MelonPreferences_Entry<T> create_entry<T>(MelonPreferences_Category category, string name, T default_value, string description) {
        return category.CreateEntry(name, default_value, description);
    }

    public void early_load(DDPlugin plugin) {
        this.m_plugin = plugin;
        
        // General
        string category_prefix = plugin.UNDERSCORE_GUID + "_";
        m_category_general = MelonPreferences.CreateCategory(category_prefix + "General");
        m_enabled = m_category_general.CreateEntry("Enabled", true, description: "Set to false to disable this mod.");
        m_log_level = m_category_general.CreateEntry("Log Level", "info", description: "[Advanced] Logging level, one of: 'none' (no logging), 'error' (only errors), 'warn' (errors and warnings), 'info' (normal logging), 'debug' (extra log messages for debugging issues).  Not case sensitive [string, default info].  Debug level not recommended unless you're noticing issues with the mod.  Changes to this setting require an application restart.");
        DDPlugin.set_log_level(m_log_level.Value);

        m_speed_multiplier_start = m_category_general.CreateEntry("Initial Player Speed Multiplier", 1f, description: "Starting player speed multiplier (float, default 1 [no change])");
        m_speed_multiplier_delta = m_category_general.CreateEntry("Player Speed Multiplier Delta", 0.25f, description: "Amount the speed multiplier is changed +/- with each hotkey press (float, default 0.25f)");

        // Hotkeys
        m_category_hotkeys = MelonPreferences.CreateCategory(category_prefix + "Hotkeys");
        m_hotkey_modifier = m_category_hotkeys.CreateEntry("Hotkey - Modifier", "", description: "Comma-separated list of Unity Keycodes used as the special modifier key (i.e. ctrl,alt,command) one of which is required to be down for hotkeys to work.  Set to '' (blank string) to not require a special key (not recommended).  See this link for valid Unity KeyCode strings (https://docs.unity3d.com/ScriptReference/KeyCode.html)");
        m_hotkey_speed_up = m_category_hotkeys.CreateEntry("Hotkey - Player Speed - Up", "Equals,KeypadPlus", description: "Comma-separated list of Unity Keycodes, any of which will increase player movement speed.  See this link for valid Unity KeyCode strings (https://docs.unity3d.com/ScriptReference/KeyCode.html)");
        m_hotkey_speed_down = m_category_hotkeys.CreateEntry("Hotkey - Player Speed - Down", "Minus,KeypadMinus", description: "Comma-separated list of Unity Keycodes, any of which will decrease player movement speed.  See this link for valid Unity KeyCode strings (https://docs.unity3d.com/ScriptReference/KeyCode.html)");
    }

    public void late_load() {
        
    }

    public static void on_setting_changed(object sender, EventArgs e) {
		
	}
}