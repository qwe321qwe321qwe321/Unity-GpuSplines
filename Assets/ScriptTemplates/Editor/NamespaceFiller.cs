/*
 * NamespaceFiller is to replace the "#NAMESPACE#" string into the specific namespace made by yourself when you create a new C# script.
 * It has to be used with the custum script template which has "#NAMESPACE#" keyword.
 * Created by PeDev 2020
 */

using UnityEngine;
using UnityEditor;

namespace Pedev.EditorExtensions {
	public class NamespaceFiller : UnityEditor.AssetModificationProcessor {
		public static void OnWillCreateAsset(string path) {
			path = path.Replace(".meta", "");
			int index = path.LastIndexOf(".");
			if (index < 0) return;
			string file = path.Substring(index);
			if (file != ".cs") return;
			index = Application.dataPath.LastIndexOf("Assets");
			path = Application.dataPath.Substring(0, index) + path;
			file = System.IO.File.ReadAllText(path);

			// Replace Namespace.
			// You can choose to fill with fixed namespace or path related namespace.
			string _namespace = "PeDev";
			//string _namespace = GetNamespaceByPath(path);
			file = file.Replace("#NAMESPACE#", _namespace);

			// Replace script name without editor.
			index = path.LastIndexOf('/') + 1;
			int extIndex = path.LastIndexOf('.');
			string filename = path.Substring(index, extIndex - index);
			file = file.Replace("#SCRIPTNAME_WITHOUT_EDITOR#", filename.Replace("Editor", ""));

			System.IO.File.WriteAllText(path, file);
			AssetDatabase.Refresh();
		}

		private static string GetNamespaceByPath(string path) {
			string lastPart = path.Substring(path.IndexOf("Assets"));
			lastPart = lastPart.Substring("Assets/".Length);
			string _namespace = lastPart.Substring(0, lastPart.LastIndexOf('/'));
			_namespace = _namespace.Replace("/Scripts", "");
			_namespace = _namespace.Replace("/Editor", "");
			_namespace = _namespace.Replace('/', '.');
			return _namespace;
		}
	}
}