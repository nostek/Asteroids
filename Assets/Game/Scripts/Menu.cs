using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace mygame
{
	public class Menu : MonoBehaviour
	{
		[Header("Connections")]
		[SerializeField] TextMeshProUGUI _textLastScore;

		[Header("Scenes")]
		[SerializeField] AssetReference _sceneGame;

		MenuInput _input;

		float _validInputTime = 0f;

		void Awake()
		{
			Assert.IsTrue(!string.IsNullOrEmpty(_sceneGame.AssetGUID), "Game scene is invalid");

			_input = new MenuInput();
			_input.Actions.Accept.performed += OnAccept;
		}

		void OnEnable() => _input.Enable();
		void OnDisable() => _input.Disable();
		void OnDestroy() => _input.Dispose();

		void Start()
		{
			_validInputTime = Time.time + .5f; //A small delay so the user can't mistakenly press the Enter key to early.

			int points = PlayerPrefs.GetInt(GameConstants.PlayerPrefsLastScore, 0);
			_textLastScore.text = $"Last Score: {points}";
		}

		void OnAccept(InputAction.CallbackContext ctx)
		{
			if (Time.time < _validInputTime)
				return;

			Addressables.LoadSceneAsync(_sceneGame);
		}
	}
}
