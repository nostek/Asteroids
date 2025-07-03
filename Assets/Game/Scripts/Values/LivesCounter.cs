using UnityEventsCenter;

namespace mygame
{
    public class LivesCounter
    {
	    int _lives = 0;

	    public void SetLives(int lives)
	    {
		    _lives = lives;
		    EventsCenter.Invoke(new GameEvents.Player.LivesChanged(_lives)); //So the UI can update with the number of lives we have left
	    }

	    public bool TakeLife()
	    {
		    _lives--;
		    EventsCenter.Invoke(new GameEvents.Player.LivesChanged(_lives)); //So the UI can update with the number of lives we have left
		    return IsGameOver;
	    }

	    public bool IsGameOver =>  _lives <= 0;
    }
}
