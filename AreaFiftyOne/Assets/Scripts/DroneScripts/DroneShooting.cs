using UnityEngine;
using System.Collections;

public class DroneShooting : MonoBehaviour {
	
	private float damage = 5f;					// The potential damage per shot.
	public AudioClip shotClip;					// An audio clip to play when a shot happens.
	public float flashIntensity = 3f;					// The intensity of the light when the shot happens.
	public float fadeSpeed = 1f;						// How fast the light will fade after the shot.
	
	private Animator anim;								// Reference to the animator.
	private HashIDs hash;							// Reference to the HashIDs script.
	private LineRenderer laserShotLine;					// Reference to the laser shot line renderer.
	private Light laserShotLight;						// Reference to the laser shot light.
	private SphereCollider col;							// Reference to the sphere collider.
	private EnemyHealth enemyHealth;				// Reference to the player's health..
	private bool shooting = false;								// A bool to say whether or not the enemy is currently shooting.
	private float scaledDamage;							// Amount of damage that is scaled by the distance from the player.
	private GameObject guard;

	private const float SHOT_DURATION = 0.15f;
	private float shotTakeStart;
	private bool startedShooting = false;

	public bool getShootingStatus() {
		return shooting;
	}

	void Awake ()
	{
		// Setting up the references.
		anim = GetComponent<Animator>();
		laserShotLine = GetComponentInChildren<LineRenderer>();
		laserShotLight = laserShotLine.gameObject.light;
		col = GetComponent<SphereCollider>();
		hash = GameObject.FindGameObjectWithTag(Tags.gameController).GetComponent<HashIDs>();
		
		// The line renderer and light are off to start.
		laserShotLine.enabled = false;
		laserShotLight.intensity = 0f;
	}
	
	void OnTriggerStay (Collider other)
	{
		transform.LookAt (other.gameObject.transform);
		// If the player has entered the trigger sphere...
		if(other.gameObject.tag == Tags.enemy)
		{
			// Create a vector from the enemy to the player and store the angle between it and forward.
			Vector3 direction = other.transform.position - transform.position;
			float angle = Vector3.Angle(direction, transform.forward);

			if(angle < 60.0f)
			{
				RaycastHit hit;
				
				// ... and if a raycast towards the player hits something...
				if(Physics.Raycast(transform.position + transform.up, direction.normalized, out hit, col.radius))
				{
					// ... and if the raycast hits the player...
					if(hit.collider.gameObject.tag == Tags.enemy)
					{
						guard = other.gameObject;
						if(!shooting) {
							Shoot(other);
						}
					}
				}
			}
		} else { shooting = false; }
	}

	void Update ()
	{
		if(guard == null || guard.gameObject.GetComponent<EnemyHealth> ().health <= 0.0f)
			shooting = false;
		// Fade the light out.
		if(!shooting)
			laserShotLine.enabled = false;
		laserShotLight.intensity = Mathf.Lerp(laserShotLight.intensity, 0f, fadeSpeed * Time.deltaTime);
	}
	
	
	void OnAnimatorIK (int layerIndex)
	{
		// Cache the current value of the AimWeight curve.
		float aimWeight = anim.GetFloat(hash.aimWeightFloat);
		
		// Set the IK position of the right hand to the player's centre.
		anim.SetIKPosition(AvatarIKGoal.RightHand, guard.transform.position + Vector3.up * 1.5f);
		
		// Set the weight of the IK compared to animation to that of the curve.
		anim.SetIKPositionWeight(AvatarIKGoal.RightHand, aimWeight);
	}
	
	
	void Shoot (Collider other)
	{
		if(!startedShooting){
			startedShooting = true;
			shotTakeStart = Time.time;
		}

		if(Time.time - shotTakeStart < SHOT_DURATION)
			return;
		
		shooting = true;
		// The player takes damage.
		transform.LookAt(other.gameObject.transform.position);
		enemyHealth = other.gameObject.GetComponent<EnemyHealth> ();
		enemyHealth.TakeDamage (damage);
		
		// Display the shot effects.
		ShotEffects (other);
		startedShooting = false;
	}
	
	void ShotEffects (Collider other)
	{
		// Set the initial position of the line renderer to the position of the muzzle.
		laserShotLine.SetPosition(0, laserShotLine.transform.position);
		
		// Set the end position of the player's centre of mass.
		laserShotLine.SetPosition(1, other.gameObject.transform.position + Vector3.up * 1.5f);
		
		// Turn on the line renderer.
		laserShotLine.enabled = true;
		
		// Make the light flash.
		laserShotLight.intensity = flashIntensity;
		
		// Play the gun shot clip at the position of the muzzle flare.
		AudioSource.PlayClipAtPoint(shotClip, laserShotLight.transform.position);
	}
}
