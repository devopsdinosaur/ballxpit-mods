using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[assembly: MelonInfo(typeof(AutopickupPlugin), "Auto Pickup", "0.0.1", "devopsdinosaur")]

public static class PluginInfo {

    public static string TITLE;
    public static string NAME = "autopickup";
    public static string SHORT_DESCRIPTION = "Automatically picks up exp, gold, etc from everywhere on the field as soon as it drops!";
    public static string EXTRA_DETAILS = "";
    public static string VERSION;
    public static string AUTHOR;
    public static string GAME_TITLE = "BALLxPIT";
    public static string GAME = "ballxpit";
    public static string GUID;
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
    }

    public static Dictionary<string, string> to_dict() {
        Dictionary<string, string> info = new Dictionary<string, string>();
        foreach (FieldInfo field in typeof(PluginInfo).GetFields((BindingFlags) 0xFFFFFFF)) {
            info[field.Name.ToLower()] = (string) field.GetValue(null);
        }
        return info;
    }
}

public class AutopickupPlugin : DDPlugin {
	private static AutopickupPlugin m_plugin = null;
	
	public override void OnInitializeMelon() {
		try {
			m_plugin = this;
			logger = LoggerInstance;
			this.m_plugin_info = PluginInfo.to_dict();
			create_nexus_page();
			new HarmonyLib.Harmony(PluginInfo.GUID).PatchAll();
		} catch (Exception e) {
			_error_log("** OnInitializeMelon FATAL - " + e);
		}
	}

	[HarmonyPatch(typeof(GameMgr), "Awake")]
	class HarmonyPatch_GameMgr_Awake {
		private static void Postfix(LevelMgr __instance) {
			try {
				MelonCoroutines.Start(auto_pickup_routine());
			} catch (Exception e) {
				_error_log("** HarmonyPatch_LevelMgr_Awake.Postfix ERROR - " + e);
			}
		}
	}

	private static IEnumerator auto_pickup_routine() {
		for (;;) {
			yield return new WaitForSeconds(1);
			if (GameMgr.I == null || GameMgr.I.CurState != GameState.kPlaying) {
				continue;
			}
			foreach (PickupObj pickup in Resources.FindObjectsOfTypeAll<PickupObj>()) {
				if (pickup.name.Contains("(Clone)")) {
					pickup.StartPlayerPickUp(0);
				}
			}
		}
	}
}