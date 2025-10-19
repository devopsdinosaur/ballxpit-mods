using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[assembly: MelonInfo(typeof(TestingPlugin), "Testing", "0.0.1", "devopsdinosaur")]

public static class PluginInfo {

    public static string TITLE;
    public static string NAME;
    public static string SHORT_DESCRIPTION = "For testing only";
    public static string EXTRA_DETAILS = "This mod does not make any permanent changes to the game files.  It simply modifies the strings in memory for the duration of the game.  Removing the mod and restarting the game will revert everything to its default state.";
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
        NAME = TITLE.ToLower().Replace(" ", "-");
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

public class TestingPlugin : DDPlugin {
	private static TestingPlugin m_plugin = null;
	private static LevelMgr m_level_manager = null;

	public override void OnInitializeMelon() {
		try {
			m_plugin = this;
			logger = LoggerInstance;
			set_log_level(LogLevel.Debug);
			this.m_plugin_info = PluginInfo.to_dict();
			new HarmonyLib.Harmony(PluginInfo.GUID).PatchAll();
		} catch (Exception e) {
			_error_log("** OnInitializeMelon FATAL - " + e);
		}
	}

	private static void dump_all_objects() {
		string directory = "C:/tmp/dump_" + Il2CppSystem.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
		Directory.CreateDirectory(directory);
		foreach (string file in Directory.GetFiles(directory, "*.json", SearchOption.TopDirectoryOnly)) {
			File.Delete(Path.Combine(directory, file));
		}
		foreach (GameObject obj in SceneManager.GetActiveScene().GetRootGameObjects()) {
			string path = null;
			int counter = 0;
			while (File.Exists(path = Path.Combine(directory, $"{obj.name}_{counter++:D4}.json")));
			UnityUtils.json_dump(obj.transform, path);
		}
	}

    public override void OnUpdate() {
		try {
			if (Input.GetKeyDown(KeyCode.F5)) {
				dump_all_objects();
				//Application.Quit();
			}
		} catch (Exception e) {
			_error_log("** OnUpdate ERROR - " + e);
		}
    }

	[HarmonyPatch(typeof(Player), "MyFixedUpdate")]
	class HarmonyPatch_Player_MyFixedUpdate {
		private static bool Prefix(Player __instance) {
			try {
				__instance.SpeedMult = 2f;
				return true;
			} catch (Exception e) {
				_error_log("** HarmonyPatch_Player_MyFixedUpdate.Prefix ERROR - " + e);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(LevelMgr), "Awake")]
	class HarmonyPatch_LevelMgr_Awake {
		private static void Postfix(LevelMgr __instance) {
			try {
				m_level_manager = __instance;
				
			} catch (Exception e) {
				_error_log("** HarmonyPatch_LevelMgr_Awake.Postfix ERROR - " + e);
			}
		}
	}

	/*
	[HarmonyPatch(typeof(), "")]
	class HarmonyPatch_ {
		private static bool Prefix() {
			
			return true;
		}
	}

	[HarmonyPatch(typeof(), "")]
	class HarmonyPatch_ {
		private static void Postfix() {
			
		}
	}

	[HarmonyPatch(typeof(), "")]
	class HarmonyPatch_ {
		private static bool Prefix() {
			try {

				return false;
			} catch (Exception e) {
				_error_log("** XXXXX.Prefix ERROR - " + e);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(), "")]
	class HarmonyPatch_ {
		private static void Postfix() {
			try {
				
			} catch (Exception e) {
				_error_log("** XXXXX.Postfix ERROR - " + e);
			}
		}
	}
	*/
}