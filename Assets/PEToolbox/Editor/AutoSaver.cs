#define USE_AUTO_SAVER
#if USE_AUTO_SAVER
using UnityEditor;
using UnityEditor.SceneManagement;

namespace PeDev.AutoSaver {
	[InitializeOnLoad]
	public class AutoSaver {
		public static bool LogOnConsole = true;

		// Static costructor would be called when unity is ready.
		static AutoSaver() {
			EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
		}

		private static void EditorApplication_playModeStateChanged(PlayModeStateChange obj) {
			switch (obj) {
				case PlayModeStateChange.EnteredEditMode:
					break;
				case PlayModeStateChange.ExitingEditMode:
					AutoSave();
					break;
				case PlayModeStateChange.EnteredPlayMode:
					break;
				case PlayModeStateChange.ExitingPlayMode:
					break;
				default:
					break;
			}
		}

		private static void AutoSave() {
			AssetDatabase.SaveAssets();
			if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
				if (LogOnConsole) {
					UnityEngine.Debug.LogFormat("Auto Saved! [{0}]", System.DateTime.Now.ToString("hh:mm:ss"));
				}
			}
		}
	}
}
#endif