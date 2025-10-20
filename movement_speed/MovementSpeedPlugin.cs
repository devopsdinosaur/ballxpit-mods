using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[assembly: MelonInfo(typeof(MovementSpeedPlugin), "Movement Speed", "0.0.1", "devopsdinosaur")]

public static class PluginInfo {

    public static string TITLE;
    public static string NAME = "movement_speed";
    public static string SHORT_DESCRIPTION = "Change player movement speed using configurable hotkeys.";
    public static string EXTRA_DETAILS = "Change speed using the hotkeys (see Hotkeys section below).  Speed info will be printed to the MelonLoader console window.";
    public static string VERSION;
    public static string AUTHOR;
    public static string GAME_TITLE = "BALLxPIT";
    public static string GAME = "ballxpit";
    public static string GUID;
	public static string UNDERSCORE_GUID;
	public static string REPO = GAME + "-mods";

    static PluginInfo() {
        System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
        MelonInfoAttribute info = assembly.GetCustomAttribute<MelonInfoAttribute>();
        TITLE = info.Name;
		if (string.IsNullOrEmpty(NAME)) {
			NAME = TITLE.ToLower().Replace(" ", "-");
		}
		VERSION = info.Version;
        AUTHOR = info.Author;
        GUID =  AUTHOR + "." + GAME + "." + NAME;
		UNDERSCORE_GUID = GUID.Replace(".", "_").Replace("-", "_");
	}

    public static Dictionary<string, string> to_dict() {
        Dictionary<string, string> info = new Dictionary<string, string>();
        foreach (FieldInfo field in typeof(PluginInfo).GetFields((BindingFlags) 0xFFFFFFF)) {
            info[field.Name.ToLower()] = (string) field.GetValue(null);
        }
        return info;
    }
}

public class MovementSpeedPlugin : DDPlugin {
	private static MovementSpeedPlugin m_plugin = null;
	private static float m_speed_multiplier = 0f;
	
	public override void OnInitializeMelon() {
		try {
			this.m_plugin_info = PluginInfo.to_dict();
			m_plugin = this;
			logger = LoggerInstance;
			Settings.Instance.early_load(m_plugin);
			Hotkeys.load(m_plugin, keypress_check_routine());
			m_speed_multiplier = Settings.m_speed_multiplier_start.Value;
			create_nexus_page();
			new HarmonyLib.Harmony(PluginInfo.GUID).PatchAll();
		} catch (Exception e) {
			_error_log("** OnInitializeMelon FATAL - " + e);
		}
	}

	private static IEnumerator keypress_check_routine() {
		for (;;) {
			yield return null;
			if (!Hotkeys.is_modifier_hotkey_down()) {
				continue;				
			}
			string info = null;
			if (Hotkeys.is_hotkey_down(Hotkeys.HOTKEY_SPEED_UP)) {
				info = $"Player speed multiplier INCREASED to {(m_speed_multiplier += Settings.m_speed_multiplier_delta.Value):0.00}";
			} else if (Hotkeys.is_hotkey_down(Hotkeys.HOTKEY_SPEED_DOWN) && m_speed_multiplier > 0) {
				info = $"Player speed multiplier DECREASED to {(m_speed_multiplier = Mathf.Max(0, m_speed_multiplier - Settings.m_speed_multiplier_delta.Value)):0.00}";
			}
			if (info != null) {
				_info_log(info);
			}
		}
	}

	[HarmonyPatch(typeof(Player), "MyFixedUpdate")]
	class HarmonyPatch_Player_MyFixedUpdate {
		private static bool Prefix(Player __instance) {
			try {
				__instance.SpeedMult = m_speed_multiplier;
				return true;
			} catch (Exception e) {
				_error_log("** HarmonyPatch_Player_MyFixedUpdate.Prefix ERROR - " + e);
			}
			return true;
		}
	}
}