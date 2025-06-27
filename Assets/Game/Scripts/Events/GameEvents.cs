using UnityEventsCenter;

namespace mygame
{
	public static class GameEvents
	{
		public readonly struct AddPointsEvent : IEvent
		{
			public AddPointsEvent(int addedPoints, int totalPoints) => (AddedPoints, TotalPoints) = (addedPoints, totalPoints);
			public readonly int AddedPoints;
			public readonly int TotalPoints;
		}

		public readonly struct LivesChangedEvent : IEvent
		{
			public LivesChangedEvent(int lives) => (Lives) = (lives);
			public readonly int Lives;
		}

		public readonly struct WaitingForSpawnEvent : IEvent
		{
			public WaitingForSpawnEvent(bool isWaiting) => IsWaiting = isWaiting;
			public readonly bool IsWaiting;
		}
	}
}
