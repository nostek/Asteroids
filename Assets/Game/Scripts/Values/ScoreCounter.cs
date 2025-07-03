using UnityEventsCenter;

namespace mygame
{
    public class ScoreCounter
    {
	    int _score = 0;

	    public void AddPoints(int points)
	    {
		    _score += points;
		    EventsCenter.Invoke(new GameEvents.Session.PointsChanged(_score));
	    }

	    public int Score => _score;
    }
}
