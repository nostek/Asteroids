using UnityEventsCenter;

namespace mygame
{
    public static class GameEvents
    {
	    public readonly struct AddPointsEvent : IEvent
	    {
		    public AddPointsEvent(int points) => Points = points;
		    public readonly int Points;
	    }
    }
}
