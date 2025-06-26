using UnityEngine;

namespace mygame
{
	[CreateAssetMenu(fileName = "WindowsDatabase", menuName = "Scriptable Objects/WindowsDatabase")]
	public class WindowsDatabase : ScriptableObject
	{
		[SerializeField] GameObject _windowGameOver;
		public GameObject WindowGameOver => _windowGameOver;
	}
}
