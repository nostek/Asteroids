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

			EventsCenter.Subscribe<GameEvents.WaitingForSpawnEvent>(OnWaitingForSpawn);
		}

		void OnDestroy() => EventsCenter.Unsubscribe<GameEvents.WaitingForSpawnEvent>(OnWaitingForSpawn);

		void OnWaitingForSpawn(GameEvents.WaitingForSpawnEvent ev) => canvas.enabled = ev.IsWaiting;
	}
}
