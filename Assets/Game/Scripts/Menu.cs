using TMPro;
using UnityEngine;

namespace mygame
{
	public class Menu : MonoBehaviour
	{
		[Header("Connections")]
		[SerializeField] TextMeshProUGUI _textLastScore;

		void Start()
		{
			int points = PlayerPrefs.GetInt(GameConstants.PlayerPrefsLastScore, 0);
			_textLastScore.text = $"Last Score: {points}";
		}
	}
}
