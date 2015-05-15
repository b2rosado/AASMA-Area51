using UnityEngine;
using System.Collections;

public class DroneHealth : MonoBehaviour {

	public float health;							// How much health the player has left.
	private const float HEALTH = 200f;
	private Animator anim;								// Reference to the animator component.
	private HashIDs hash;							// Reference to the HashIDs.
	private float timer;								// A timer for counting to the reset of the level once the player is dead.
	public bool droneDead;							// A bool to show if the player is dead or not.
	private float destroyTime = 3f;

	void Awake ()
	{
		// Setting up the references.
		anim = GetComponent<Animator>();
		hash = GameObject.FindGameObjectWithTag(Tags.gameController).GetComponent<HashIDs>();
		health = HEALTH;
	}

	void Update ()
	{
		// If health is less than or equal to 0...
		if(health <= 0f)
		{
			// ... and if the drone is not yet dead...
			if(!droneDead)
				DroneDying();
			else {
				DroneDead();
				DestroyDrone();
			}
		}
	}

	void DroneDying ()
	{
		// The player is now dead.
		droneDead = true;
		
		// Set the animator's dead parameter to true also.
		anim.SetBool(hash.deadBool, droneDead);
		
		// Play the dying sound effect at the player's location.
		//AudioSource.PlayClipAtPoint(deathClip, transform.position);
	}

	void DroneDead ()
	{
		// If the player is in the dying state then reset the dead parameter.
		if(anim.GetCurrentAnimatorStateInfo(0).nameHash == hash.dyingState)
			anim.SetBool(hash.deadBool, false);

		Destroy (this.gameObject);
	}

	void DestroyDrone() {
		timer += Time.deltaTime;
		
		//If the timer is greater than or equal to the time before the level resets...
		if(timer >= destroyTime)
			Destroy (this.gameObject);
	}

	public void TakeDamage (float amount)
	{
		// Decrement the drone's health by amount.
		health -= amount;
	}

	public void AddHealth(float h) {
		if (this.health + h <= HEALTH)
			health += h;
	}
}
