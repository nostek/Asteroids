using UnityEngine;
using UnityServiceLocator;

namespace mygame
{
	public abstract class BaseWindow : MonoBehaviour
	{
		protected WindowsManager _windowsManager;

		protected virtual void Awake()
		{
			_windowsManager = ServiceLocator.Get<WindowsManager>();
		}

		protected void CloseWindow()
		{
			_windowsManager.CloseWindow(gameObject);
		}
	}
}
