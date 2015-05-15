using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour {
	public float health = 100f;							// How much health the player has left.
	public AudioClip deathClip;							// The sound effect of the player dying.

	private Animator anim;								// Reference to the animator component.
	private HashIDs hash;							// Reference to the HashIDs.
	private float timer;								// A timer for counting to the reset of the level once the player is dead.
	private bool enemyDead;							// A bool to show if the player is dead or not.
	private float destroyTime = 3f;
	private EnemyAI ai;
	
	void Awake ()
	{
		// Setting up the references.
		anim = GetComponent<Animator>();
		hash = GameObject.FindGameObjectWithTag(Tags.gameController).GetComponent<HashIDs>();
		ai = transform.gameObject.GetComponent<EnemyAI>();
	}
	
	
	void Update ()
	{
		// If health is less than or equal to 0...
		if(health <= 0f)
		{
			// ... and if the player is not yet dead...
			if(!enemyDead)
				EnemyDying();
			else {
				EnemyDead();
				DestroyEnemy();
			}
		}
	}
	
	
	void EnemyDying ()
	{
		// The player is now dead.
		enemyDead = true;
		
		// Set the animator's dead parameter to true also.
		anim.SetBool(hash.deadBool, enemyDead);

	}
	
	
	void EnemyDead ()
	{
		// If the player is in the dying state then reset the dead parameter.
		if(anim.GetCurrentAnimatorStateInfo(0).nameHash == hash.dyingState)
			anim.SetBool(hash.deadBool, false);
		
		// Disable the movement.
		anim.SetFloat(hash.speedFloat, 0f);
	}

	void DestroyEnemy() {
		timer += Time.deltaTime;
		//If the timer is greater than or equal to the time before the level resets...
		if(timer >= destroyTime){
			Cooperation.notPatrolling(ai.patrollingArea);
			Destroy (this.gameObject);
		}
	}
	
	
	public void TakeDamage (float amount)
	{
		// Decrement the player's health by amount.
		health -= amount;
	}

	public void AddHealth(float h) {
		if (this.health + h <= 100)
			health += h;
	}
}
