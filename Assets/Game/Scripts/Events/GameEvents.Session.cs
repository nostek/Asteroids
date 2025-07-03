using UnityEventsCenter;

namespace mygame
{
	public static partial class GameEvents
    {
	    public static class Session
	    {
		    public readonly struct PointsChanged : IEvent
		    {
			    public PointsChanged(int totalPoints) => (TotalPoints) = (totalPoints);
			    public readonly int TotalPoints;
		    }
	    }
    }
}
