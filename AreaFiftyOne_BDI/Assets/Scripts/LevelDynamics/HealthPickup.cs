using UnityEngine;
using System.Collections;

public class HealthPickup : MonoBehaviour {

	public AudioClip keyGrab;							// Audioclip to play when the key is picked up.

	private GameObject player;							// Reference to the player.
	private PlayerHealth playerHealth;

	void Awake ()
	{
		// Setting up the references.
		player = GameObject.FindGameObjectWithTag(Tags.player);
		playerHealth = player.gameObject.GetComponent<PlayerHealth>();
	}

	void OnTriggerEnter (Collider other)
	{
		// If the colliding gameobject is the player...
		if(other.gameObject == player)
		{
			// ... play the clip at the position of the key...
			AudioSource.PlayClipAtPoint(keyGrab, transform.position);
			
			playerHealth.AddHealth(25);
			
			// ... and destroy this gameobject.
			Destroy(gameObject);
		}
	}
}
