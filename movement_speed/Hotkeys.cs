using MelonLoader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Hotkeys {
    private static Hotkeys m_instance = null;
    public static Hotkeys Instance {
        get {
            if (m_instance == null) {
                m_instance = new Hotkeys();
            }
            return m_instance;
        }
    }
    public const int HOTKEY_MODIFIER = 0;
    public const int HOTKEY_SPEED_UP = 1;
    public const int HOTKEY_SPEED_DOWN = 2;
    private static Dictionary<int, List<KeyCode>> m_hotkeys = null;

    public static void load(DDPlugin plugin, IEnumerator callback_routine) {
        m_hotkeys = new Dictionary<int, List<KeyCode>>();
        set_hotkey(Settings.m_hotkey_modifier.Value, HOTKEY_MODIFIER);
        set_hotkey(Settings.m_hotkey_speed_up.Value, HOTKEY_SPEED_UP);
        set_hotkey(Settings.m_hotkey_speed_down.Value, HOTKEY_SPEED_DOWN);
        MelonCoroutines.Start(callback_routine);
    }

    private static void set_hotkey(string keys_string, int key_index) {
        m_hotkeys[key_index] = new List<KeyCode>();
        foreach (string key in keys_string.Split(',')) {
            string trimmed_key = key.Trim();
            if (trimmed_key != "") {
                m_hotkeys[key_index].Add((KeyCode) System.Enum.Parse(typeof(KeyCode), trimmed_key));
            }
        }
    }

    public static bool is_modifier_hotkey_down() {
        if (m_hotkeys[HOTKEY_MODIFIER].Count == 0) {
            return true;
        }
        foreach (KeyCode key in m_hotkeys[HOTKEY_MODIFIER]) {
            if (Input.GetKey(key)) {
                return true;
            }
        }
        return false;
    }

    public static bool is_hotkey_down(int key_index) {
        foreach (KeyCode key in m_hotkeys[key_index]) {
            if (Input.GetKeyDown(key)) {
                return true;
            }
        }
        return false;
    }
}
