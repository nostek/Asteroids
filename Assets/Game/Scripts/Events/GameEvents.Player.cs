using UnityEventsCenter;

namespace mygame
{
	public static partial class GameEvents
    {
	    public static class Player
	    {
		    public readonly struct LivesChanged : IEvent
		    {
			    public LivesChanged(int lives) => (Lives) = (lives);
			    public readonly int Lives;
		    }

		    public readonly struct WaitingForSpawn : IEvent
		    {
			    public WaitingForSpawn(bool isWaiting) => IsWaiting = isWaiting;
			    public readonly bool IsWaiting;
		    }
	    }
    }
}
