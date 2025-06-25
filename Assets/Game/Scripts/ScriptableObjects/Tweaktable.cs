using UnityEngine;

namespace mygame
{
    [CreateAssetMenu(fileName = "Tweaktable", menuName = "Scriptable Objects/Tweaktable")]
    public class Tweaktable : ScriptableObject
    {
	    [Header("Asteroids")]
	    [SerializeField] Vector2 randomSpeedBetween;
	    public Vector2 RandomSpeedBetween => randomSpeedBetween;
    }
}
