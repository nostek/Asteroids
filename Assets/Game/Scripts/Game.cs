using UnityEngine;
using UnityEngine.Assertions;
using UnityEventsCenter;
using UnityServiceLocator;

namespace mygame
{
	[DefaultExecutionOrder(-1)] //We want the setup to run before any other scripts
	public class Game : MonoBehaviour
	{
		[Header("Prefabs Asteroids")]
		[SerializeField] GameObject _prefabAsteroidBig;
		[SerializeField] GameObject _prefabAsteroidMedium;
		[SerializeField] GameObject _prefabAsteroidSmall;

		[Header("Prefabs Player")]
		[SerializeField] GameObject _prefabPlayer;
		[SerializeField] GameObject _prefabMissile;

		WorldBoundsManager _worldBoundsManager;
		EntitiesManager _entitiesManager;
		Tweaktable _tweaktable;

		float _halfScaleBigAsteroid, _halfScaleMediumAsteroid, _halfScaleSmallAsteroid;

		void Awake()
		{
			GetComponent<ServiceBehaviour>().Install();

			ServiceLocator.Lookup
				.Get(out _worldBoundsManager)
				.Get(out _entitiesManager)
				.Get(out _tweaktable)
				.Done();

			Assert.IsNotNull(_prefabAsteroidBig, "Prefab object is not assigned. Please assign a prefab in the inspector.");
			Assert.IsNotNull(_prefabAsteroidMedium, "Prefab object is not assigned. Please assign a prefab in the inspector.");
			Assert.IsNotNull(_prefabAsteroidSmall, "Prefab object is not assigned. Please assign a prefab in the inspector.");
			Assert.IsNotNull(_prefabPlayer, "Prefab object is not assigned. Please assign a prefab in the inspector.");
			Assert.IsNotNull(_prefabMissile, "Prefab object is not assigned. Please assign a prefab in the inspector.");
		}

		void Start()
		{
			_halfScaleBigAsteroid = _prefabAsteroidBig.transform.localScale.x * 0.5f;
			_halfScaleMediumAsteroid = _prefabAsteroidMedium.transform.localScale.x * 0.5f;
			_halfScaleSmallAsteroid = _prefabAsteroidSmall.transform.localScale.x * 0.5f;

			_entitiesManager.RegisterEntity(_prefabAsteroidBig);
			_entitiesManager.RegisterEntity(_prefabAsteroidMedium);
			_entitiesManager.RegisterEntity(_prefabAsteroidSmall);
			_entitiesManager.RegisterEntity(_prefabMissile);
			_entitiesManager.RegisterEntity(_prefabPlayer, ensureCapacity: 1); //Only want one of these

			_entitiesManager.RegisterCollisionSolver(_prefabAsteroidBig, OnBigAsteroid);
			_entitiesManager.RegisterCollisionSolver(_prefabAsteroidMedium, OnMediumAsteroid);
			_entitiesManager.RegisterCollisionSolver(_prefabAsteroidSmall, OnDespawn);
			_entitiesManager.RegisterCollisionSolver(_prefabAsteroidMedium, OnMediumAsteroid, _prefabAsteroidBig, OnBigAsteroid);
			_entitiesManager.RegisterCollisionSolver(_prefabAsteroidSmall, OnDespawn, _prefabAsteroidMedium, OnMediumAsteroid);
			_entitiesManager.RegisterCollisionSolver(_prefabAsteroidSmall, OnDespawn, _prefabAsteroidBig, OnBigAsteroid);

			_entitiesManager.RegisterCollisionSolver(_prefabMissile, OnMissileBigAsteroid, _prefabAsteroidBig, OnNoop);
			_entitiesManager.RegisterCollisionSolver(_prefabMissile, OnMissileMediumAsteroid, _prefabAsteroidMedium, OnNoop);
			_entitiesManager.RegisterCollisionSolver(_prefabMissile, OnMissileSmallAsteroid, _prefabAsteroidSmall, OnNoop);

			_entitiesManager.RegisterCollisionSolver(_prefabPlayer, OnPlayerHit, _prefabAsteroidBig, OnNoop);
			_entitiesManager.RegisterCollisionSolver(_prefabPlayer, OnPlayerHit, _prefabAsteroidMedium, OnNoop);
			_entitiesManager.RegisterCollisionSolver(_prefabPlayer, OnPlayerHit, _prefabAsteroidSmall, OnNoop);

			for (int i = 0; i < 3; i++)
				_entitiesManager.Spawn(
					_prefabAsteroidBig,
					_worldBoundsManager.GetRandomInsideBounds(_halfScaleBigAsteroid), //Assuming its uniform scale
					Random.insideUnitCircle.normalized * Random.Range(_tweaktable.RandomBigAsteroidSpeedBetween.x, _tweaktable.RandomBigAsteroidSpeedBetween.y)
				);

			// Spawn the player at the center of the world bounds
			//TODO: EntityReference index is not stable. It can be invalid after flushing despawned entities
			var entity = _entitiesManager.Spawn(_prefabPlayer, Vector2.zero, Vector2.zero);
			entity.GetGameObject().GetComponent<Player>().Entity = entity;
		}

		void OnPlayerHit(EntityReference player, EntityReference asteroid)
		{
			Debug.Log("We died");
		}

		void OnMissileBigAsteroid(EntityReference missile, EntityReference asteroid)
		{
			missile.Despawn();
			asteroid.Despawn();

			var pos = missile.GetPosition();
			var posOther = asteroid.GetPosition();

			var dir = (pos - posOther).normalized;
			var right = Vector3.Cross(Vector3.forward, dir);

			var speed = Random.Range(_tweaktable.RandomMediumAsteroidSpeedBetween.x, _tweaktable.RandomMediumAsteroidSpeedBetween.y);

			_entitiesManager.Spawn(_prefabAsteroidMedium, posOther + (Vector2)right * _halfScaleMediumAsteroid, right * speed);
			_entitiesManager.Spawn(_prefabAsteroidMedium, posOther - (Vector2)right * _halfScaleMediumAsteroid, -right * speed);

			EventsCenter.Invoke(new GameEvents.AddPointsEvent(_tweaktable.PointsForBigAsteroid));
		}

		void OnMissileMediumAsteroid(EntityReference missile, EntityReference asteroid)
		{
			missile.Despawn();
			asteroid.Despawn();

			var pos = missile.GetPosition();
			var posOther = asteroid.GetPosition();

			var dir = (pos - posOther).normalized;
			var right = Vector3.Cross(Vector3.forward, dir);

			var speed = Random.Range(_tweaktable.RandomSmallAsteroidSpeedBetween.x, _tweaktable.RandomSmallAsteroidSpeedBetween.y);

			_entitiesManager.Spawn(_prefabAsteroidSmall, posOther + (Vector2)right * _halfScaleSmallAsteroid, right * speed);
			_entitiesManager.Spawn(_prefabAsteroidSmall, posOther - (Vector2)right * _halfScaleSmallAsteroid, -right * speed);

			EventsCenter.Invoke(new GameEvents.AddPointsEvent(_tweaktable.PointsForMediumAsteroid));
		}

		void OnMissileSmallAsteroid(EntityReference missile, EntityReference asteroid)
		{
			missile.Despawn();
			asteroid.Despawn();
			EventsCenter.Invoke(new GameEvents.AddPointsEvent(_tweaktable.PointsForSmallAsteroid));
		}

		void OnBigAsteroid(EntityReference collider, EntityReference otherCollider)
		{
			collider.Despawn();

			var speed = Random.Range(_tweaktable.RandomMediumAsteroidSpeedBetween.x, _tweaktable.RandomMediumAsteroidSpeedBetween.y);
			var pos = collider.GetPosition();
			var dir = (pos - otherCollider.GetPosition()).normalized;
			_entitiesManager.Spawn(_prefabAsteroidMedium, pos, dir * speed);
		}

		void OnMediumAsteroid(EntityReference collider, EntityReference otherCollider)
		{
			collider.Despawn();

			var speed = Random.Range(_tweaktable.RandomSmallAsteroidSpeedBetween.x, _tweaktable.RandomSmallAsteroidSpeedBetween.y);
			var pos = collider.GetPosition();
			var dir = (pos - otherCollider.GetPosition()).normalized;
			_entitiesManager.Spawn(_prefabAsteroidSmall, pos, dir * speed);
		}

		void OnDespawn(EntityReference collider, EntityReference otherCollider)
		{
			collider.Despawn();
		}

		void OnNoop(EntityReference _, EntityReference __)
		{
		}
	}
}
