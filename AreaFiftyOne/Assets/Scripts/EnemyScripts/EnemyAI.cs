using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
	public const int patrolSpeed = 2;							// The nav mesh agent's speed when patrolling.
	public const int chaseSpeed = 4;							// The nav mesh agent's speed when chasing.
	public Transform[] patrolWayPoints;						// An array of transforms for the patrol route.

	private Animator anim;								// Reference to the animator component.
	private HashIDs hash;							// Reference to the HashIDs.
	private float timer;								// A timer for counting the restoring

	private EnemySight enemySight;						// Reference to the EnemySight script.
	private NavMeshAgent nav;								// Reference to the nav mesh agent.
	private Transform player;								// Reference to the player's transform.
	private Transform drone;
	private PlayerHealth playerHealth;					// Reference to the PlayerHealth script.

	// Patrolling
	private bool patrolling = false;
	private Vector3 nextPosition;

	// Energy
	private Energy energy;
	private bool resting = false;
	private float restingStart;
	private float energyTakeRate = 0.5f;
	private float nextEnergyTake = 0.0f;

	// Ammunition
	private const int MAX_AMMUNITION = 4; 
	private const float AMMUNITION_TAKE_DURATION = 0.4f;
	public int ammunitionQt = MAX_AMMUNITION;
	public Vector3 ammuntionLocation;
	private float ammunitionTakeStart;
	private bool takingAmmunition = false;

	void Awake ()
	{
		// Setting up the references.
		energy = new Energy ();
		enemySight = GetComponent<EnemySight>();
		nav = GetComponent<NavMeshAgent>();
		player = GameObject.FindGameObjectWithTag(Tags.player).transform;
		drone = GameObject.FindGameObjectWithTag(Tags.player).transform;
		playerHealth = player.GetComponent<PlayerHealth>();
		anim = GetComponent<Animator>();
		hash = GameObject.FindGameObjectWithTag(Tags.gameController).GetComponent<HashIDs>();
	}
	
	
	void Update ()
	{
		// loosing energy
		if (Time.time > nextEnergyTake && !resting) {
			nextEnergyTake = Time.time + energyTakeRate;
			energy.TakeEnergy((int)nav.speed);
		}

		//reactive loop
		if (energy.value < 10)
			Resting ();
		else if(ammunitionQt < MAX_AMMUNITION && isNearAmmo())
			TakeAmmunition();
		else if (ammunitionQt > 0 && enemySight.playerInSight && playerHealth.health > 0){
			if(isCloseEnough())
				Shooting ();
			else
				Chasing ();
		}else
			Patrolling ();
	}

	bool isCloseEnough(){
		int dist = (int)Vector3.Distance (player.position, transform.position);
		if(dist < 2)
			return true;
		return false;
	}
	
	void Resting(){
		if(!resting){
			resting = true;
			restingStart = Time.time;
			anim.SetBool (hash.tiredBool, true);
		}else{
			if(Time.time - restingStart > energy.RESTING_DURATION){
				energy.Rest();
				anim.SetBool (hash.tiredBool, false);
				resting = false;
				//Debug.Log ("Rest: " + energy.value);
			}
		}
	}

	bool isNearAmmo(){
		int dist = (int)Vector3.Distance (ammuntionLocation, transform.position);
		if(dist < 3)
			return true;
		return false;
	}
	void TakeAmmunition() {
		nav.speed = chaseSpeed;
		nav.destination = ammuntionLocation;

		if(nav.remainingDistance < 1){
			if(!takingAmmunition){
				takingAmmunition = true;
				ammunitionTakeStart = Time.time;
			}else if(Time.time - ammunitionTakeStart > AMMUNITION_TAKE_DURATION){
				ammunitionQt = Mathf.Min(ammunitionQt+1, MAX_AMMUNITION);
				takingAmmunition = false;
			}
		}
	}
	
	
	void Shooting ()
	{
		if (energy.value >= 2) {
			nav.Stop ();
		}
	}

	void Chasing ()
	{
		nav.Resume();
		nav.speed = chaseSpeed;
		Vector3 sightingDeltaPos = enemySight.personalLastSighting - transform.position;
		
		if(sightingDeltaPos.sqrMagnitude > 4f)
			nav.destination = enemySight.personalLastSighting;
	}
	
	void Patrolling ()
	{
		nav.Resume();
		if(!patrolling){
			patrolling = true;
			nav.speed = patrolSpeed;
			int xFactor = Random.value > 0.5? 2: -2;
			int zFactor = Random.value > 0.5? 2: -2;
			nextPosition = new Vector3 (transform.position.x+xFactor,
			                                    transform.position.y,
			                                    transform.position.z+zFactor);
			nav.destination = nextPosition;
		}else{
			if (nav.remainingDistance < 1)
				patrolling = false;
		}
	}
}
