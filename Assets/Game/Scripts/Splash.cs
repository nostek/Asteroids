using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;

namespace mygame
{
	public class Splash : MonoBehaviour
	{
		[SerializeField] AssetReference _sceneMenu;

		void Start()
		{
			Assert.IsTrue(!string.IsNullOrEmpty(_sceneMenu.AssetGUID), "Menu scene is invalid");
			Addressables.LoadSceneAsync(_sceneMenu);
		}
	}
}
