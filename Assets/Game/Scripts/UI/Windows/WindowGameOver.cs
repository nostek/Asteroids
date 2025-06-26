using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace mygame
{
	public class WindowGameOver : BaseWindow
	{
		[Header("Scenes")]
		[SerializeField] AssetReference _sceneMenu;

		MenuInput _input;

		float _validInputTime = 0f;

		override protected void Awake()
		{
			base.Awake();

			Assert.IsTrue(!string.IsNullOrEmpty(_sceneMenu.AssetGUID), "Game scene is invalid");

			_input = new MenuInput();
			_input.Actions.Accept.performed += OnAccept;
		}

		void OnEnable() => _input.Enable();
		void OnDisable() => _input.Disable();
		void OnDestroy() => _input.Dispose();

		void Start()
		{
			_validInputTime = Time.time + .5f; //A small delay so the user can't mistakenly press the Enter key.
		}

		void OnAccept(InputAction.CallbackContext ctx)
		{
			if (Time.time < _validInputTime)
				return;

			Addressables.LoadSceneAsync(_sceneMenu);
		}
	}
}
