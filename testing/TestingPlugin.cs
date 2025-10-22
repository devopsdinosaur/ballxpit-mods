using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
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
    public static string EXTRA_DETAILS = "";
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
        NAME = TITLE.ToLower().Replace(" ", "-");
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

public class TestingPlugin : DDPlugin {
	private static TestingPlugin m_plugin = null;
	private const float TIME_SCALE_DELTA = 0.5f;
	private static float m_desired_time_scale = 1f; 
	
	public override void OnInitializeMelon() {
		try {
			this.m_plugin_info = PluginInfo.to_dict();
			m_plugin = this;
			logger = LoggerInstance;
			Settings.Instance.early_load(m_plugin);
			create_nexus_page();
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
				//dump_all_objects();
				//Application.Quit();
				foreach (AchType achievement_type in Enum.GetValues(typeof(AchType))) {
					if (!AchMgr.I.IsEarned(achievement_type)) {
						AchMgr.I.Earn(achievement_type);
					}
				}
			}
			string info = null;
			if (Input.GetKeyDown(KeyCode.Period)) {
				info = $"Increasing time scale to {(m_desired_time_scale += TIME_SCALE_DELTA):0.0}.";
			} else if (Input.GetKeyDown(KeyCode.Comma) && m_desired_time_scale > 0f) {
				info = $"Decreasing time scale to {(m_desired_time_scale = Mathf.Max(0, m_desired_time_scale - TIME_SCALE_DELTA)):0.0}.";
			}
			Time.timeScale = m_desired_time_scale;
			if (info != null) {
				_info_log(info);
			}
			if (GameMgr.I != null && GameMgr.I.CurState == GameState.kPlaying && BattleSaveData.I != null) {
				BattleSaveData.I.CurHealth = 999;
				BattleSaveData.I.NumFreeRerolls = 999;
				foreach (GridPieceInst enemy in BattleSaveData.I.Pieces) {
					enemy.CurHealth = 1;
				}
			}
		} catch (Exception e) {
			_error_log("** OnUpdate ERROR - " + e);
		}
    }

	[HarmonyPatch(typeof(GameMgr), "Awake")]
	class HarmonyPatch_GameMgr_Awake {
		private static void Postfix(GameMgr __instance) {
			try {
				MelonCoroutines.Start(sneaky_enemies_routine());			
			} catch (Exception e) {
				_error_log("** HarmonyPatch_GameMgr_Awake.Postfix ERROR - " + e);
			}
		}
	}

	private static IEnumerator sneaky_enemies_routine() {
		for (;;) {
			yield return new WaitForSeconds(1);
			if (GameMgr.I == null || GameMgr.I.CurState != GameState.kPlaying) {
				continue;
			}
			foreach (GridPieceObj obj in Resources.FindObjectsOfTypeAll<GridPieceObj>()) {
				if (obj.Inst != null) {
					obj.Inst.CurHealth = 1;
				}
			}
		}
	}

	[HarmonyPatch(typeof(GridPieceObjMoon), "InitChildren")]
	class HarmonyPatch_GridPieceObjMoon_InitChildren {
		private static void Postfix(GridPieceObjMoon __instance) {
			try {
				foreach (GridPieceObjMoonBaby baby in __instance.MoonBabies) {
					baby.Inst.CurHealth = 1;
				}
			} catch (Exception e) {
				_error_log("** XXXXX.Postfix ERROR - " + e);
			}
		}
	}

	[HarmonyPatch(typeof(BaseMgr), "MyUpdate")]
	class HarmonyPatch_BaseMgr_MyUpdate {
		private static bool m_have_increased_harvest_time = false;
		private static bool Prefix(BaseMgr __instance) {
			try {
				if (__instance.CurState == BaseState.kBounceWorkers) {
					if (!m_have_increased_harvest_time) {
						__instance.RemainingHarvestSecs += 9999f;
						m_have_increased_harvest_time = true;
					}
				} else if (m_have_increased_harvest_time) {
					m_have_increased_harvest_time = false;
				}
				return true;
			} catch (Exception e) {
				_error_log("** HarmonyPatch_BaseMgr_IncreaseHarvestClock.Prefix ERROR - " + e);
			}
			return true;
		}
	}

	/*
	[HarmonyPatch(typeof(CharBattleInst), "GetPropArray")]
	class HarmonyPatch_CharBattleInst_GetPropArray {
		private static bool m_one_shot = false;
		private static void Postfix(ref Il2CppStructArray<bool> __result) {
			if (__result != null && !m_one_shot) {
				//m_one_shot = true;
				for (int index = 0; index < __result.Count; index++) {
					//_info_log($"{(CharProp) index}: {__result[index]}");
					_info_log(__result[index]);
				}
			}
		}
	}
	*/

	//[HarmonyPatch(typeof(Player), "IsAIActive")]
	//class HarmonyPatch_Player_IsAIActive {
	//	private static bool Prefix(ref bool __result) {
	//		__result = true;
	//		return false;
	//	}
	//}

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