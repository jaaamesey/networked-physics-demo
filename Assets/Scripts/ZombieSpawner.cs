// James Karlsson 13203260

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ZombieSpawner : NetworkBehaviour
{

	public float SpawnRandomChancePerPhysicsFrame = 0.99f; // Can be adjusted in inspector
	
	private GameManager _gameManager;
	
	private void Start()
	{
		_gameManager = GameManager.GetCurrent();
	}

	private void FixedUpdate ()
	{
		if (!hasAuthority)
			return;
		
		// Spawn enemies at random intervals
		if (Random.value > SpawnRandomChancePerPhysicsFrame)
			_gameManager.CmdSpawnEnemy(transform.position + 0.5f *Vector3.up);
	}
}
