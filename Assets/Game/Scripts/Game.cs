using UnityEngine;
using UnityEngine.Assertions;

public class Game : MonoBehaviour
{
	[Header("Prefabs Asteroids")]
	[SerializeField] PooledObjectManager _poolAsteroidsBig;
	[SerializeField] PooledObjectManager _poolAsteroidsMedium;

	void Awake()
	{
		Assert.IsNotNull(_poolAsteroidsBig, "PooledObjectManager for big asteroids is not assigned. Please assign it in the inspector.");
		Assert.IsNotNull(_poolAsteroidsMedium, "PooledObjectManager for medium asteroids is not assigned. Please assign it in the inspector.");
	}

	void Start()
	{
		for (int i = 0; i < 10; i++)
			_poolAsteroidsBig.Spawn(Random.insideUnitCircle * 5, Random.insideUnitCircle.normalized * Random.Range(1f, 3f));
	}

	// Update is called once per frame
	void Update()
	{
		var jobBig = _poolAsteroidsBig.ScheduleUpdate();
		var jobMedium = _poolAsteroidsMedium.ScheduleUpdate();
		jobBig.Complete();
		jobMedium.Complete();

		var jobCollisionBigVsBig = _poolAsteroidsBig.ScheduleCollisionsVs(_poolAsteroidsBig, out var collisions);
		jobCollisionBigVsBig.Complete();
		for (int i = 0; i < collisions.Length; i++)
			if (collisions[i] > 0)
			{
				_poolAsteroidsBig.Despawn(i);
				_poolAsteroidsBig.Despawn(collisions[i] - 1); // -1 because collisions[i] is 1-based index
			}
		collisions.Dispose();
	}
}
