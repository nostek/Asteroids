using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace mygame
{
	public class WindowsManager : MonoBehaviour
	{
		readonly List<GameObject> _windows = new();

		public GameObject OpenWindow(GameObject prefabWindow)
		{
			var go = Instantiate(prefabWindow);
			_windows.Add(go);
			return go;
		}

		public void CloseWindow(GameObject instance)
		{
			Assert.IsTrue(_windows.IndexOf(instance) >= 0, "Window is not registred in WindowsManager");
			_windows.Remove(instance);
			Destroy(instance);
		}
	}
}
