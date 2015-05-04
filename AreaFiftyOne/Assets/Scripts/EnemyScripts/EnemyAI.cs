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
	private const float AMMUNITION_TAKE_DURATION = 1.5f;
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
		else if(ammunitionQt < MAX_AMMUNITION && enemySight.ammunitionInSight)
			TakeAmmunition();
		else if(enemySight.switchInSight)
			ActivateLaser();
		else if (ammunitionQt > 0 && enemySight.playerInSight && playerHealth.health > 0){
			if(isCloseEnough(player.position, 2))
				Shooting ();
			else
				Chasing ();
		}else
			Patrolling ();
	}

	bool isCloseEnough(Vector3 pos, int distance){
		int dist = (int)Vector3.Distance (player.position, pos);
		if(dist < distance)
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

	void ActivateLaser(){
		Transform screenTransf = enemySight.currentSwitch.GetComponentInChildren<Transform>();

		Renderer screen = screenTransf.Find("prop_switchUnit_screen_001").renderer;
		if(screen.material.name == "prop_switchUnit_screen_unlocked_mat (Instance)")
			nav.destination = enemySight.currentSwitch.transform.position;
	}

	void TakeAmmunition() {
		nav.speed = chaseSpeed;
		nav.destination = ammuntionLocation;
		if(nav.remainingDistance < 1){
			if(!takingAmmunition){
				takingAmmunition = true;
				ammunitionTakeStart = Time.time;
			}else if(Time.time - ammunitionTakeStart > AMMUNITION_TAKE_DURATION){
				ammunitionQt = MAX_AMMUNITION;
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
