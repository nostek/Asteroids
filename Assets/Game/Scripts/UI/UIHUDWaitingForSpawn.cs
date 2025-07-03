using UnityEngine;
using UnityEngine.Assertions;
using UnityEventsCenter;

namespace mygame
{
	[RequireComponent(typeof(Canvas))]
	public class UIHUDWaitingForSpawn : MonoBehaviour
	{
		Canvas canvas;

		void Awake()
		{
			canvas = GetComponent<Canvas>();
			Assert.IsNotNull(canvas);

			canvas.enabled = false; //enable and disable canvas instead of gameobject so we dont clear the mesh buffers on the gpu

			EventsCenter.Subscribe<GameEvents.Player.WaitingForSpawn>(OnWaitingForSpawn);
		}

		void OnDestroy() => EventsCenter.Unsubscribe<GameEvents.Player.WaitingForSpawn>(OnWaitingForSpawn);

		void OnWaitingForSpawn(GameEvents.Player.WaitingForSpawn ev) => canvas.enabled = ev.IsWaiting;
	}
}
