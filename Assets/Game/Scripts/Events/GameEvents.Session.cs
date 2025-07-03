using UnityEventsCenter;

namespace mygame
{
	public static partial class GameEvents
    {
	    public static class Session
	    {
		    public readonly struct AddPoints : IEvent
		    {
			    public AddPoints(int addedPoints, int totalPoints) => (AddedPoints, TotalPoints) = (addedPoints, totalPoints);
			    public readonly int AddedPoints;
			    public readonly int TotalPoints;
		    }
	    }
    }
}
