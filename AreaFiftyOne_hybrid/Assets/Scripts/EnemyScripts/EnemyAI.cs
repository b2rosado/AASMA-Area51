using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
	private float patrollingStartTime;
	private const float PATROLLING_DURATION = 2.5f;
	public Vector3 patrollingArea = new Vector3(0,0,0);

	private bool helping = false;
	private Vector3 helpingPos;

	// Energy
	private Energy energy;
	private bool resting = false;
	private float restingStart;
	private float energyTakeRate = 0.5f;
	private float nextEnergyTake = 0.0f;

	// Ammunition
	private const int MAX_AMMUNITION = 4; 
	private const float AMMUNITION_TAKE_DURATION = 1.5f;
	public int ammunitionQt;
	private float ammunitionTakeStart;
	private bool takingAmmunition = false;

	// Health
	private EnemyHealth enemyHealth;
	private const int MAX_HEALTH = 100; 
	private const float HEALTH_TAKE_DURATION = 1.5f;
	private float healthTakeStart;
	private bool takingHealthPackage = false;

	//Beliefs
	public EnemyBeliefs enemyBeliefs;

	// BDI Options
	private const int GET_AMMUNITION = 1;
	private const int REST = 2;
	private const int RUN_AWAY = 3;
	private const int GET_HP = 4;
	private const int KILL_ETHAN = 5;
	private const int KILL_DRONE = 6;
	private const int ACTIVATE_LASER = 7;
	private const int PATROL = 8;

	private const int CHASE_ETHAN = 9;
	private const int SHOOT_ETHAN = 10;
	private const int GO_HELP = 11;
	private const int ASK_HELP = 12;

	void Awake ()
	{
		// Setting up the references.
		energy = new Energy ();
		enemySight = GetComponent<EnemySight>();
		nav = GetComponent<NavMeshAgent>();
		player = GameObject.FindGameObjectWithTag(Tags.player).transform;
		drone = GameObject.FindGameObjectWithTag(Tags.player).transform;
		playerHealth = player.GetComponent<PlayerHealth>();
		enemyHealth = GetComponent<EnemyHealth>();
		anim = GetComponent<Animator>();
		enemyBeliefs = new EnemyBeliefs(transform);
		hash = GameObject.FindGameObjectWithTag(Tags.gameController).GetComponent<HashIDs>();
		Cooperation.setup();
		enemyBeliefs.setAreaToPatrol(Cooperation.getLessOcupiedArea());
	}
	
	
	void Update ()
	{
		// loosing energy
		if (Time.time > nextEnergyTake && !resting) {
			nextEnergyTake = Time.time + energyTakeRate;
			energy.TakeEnergy((int)nav.speed/2);
		}

		//reactive loop
		if(enemyHealth.health < 30){
			if (energy.value < 10)
				Resting ();
			else if(takingHealthPackage || enemyHealth.health <= 80 && enemySight.healthInSight)
				TakeHealthPackage();
			else if(takingAmmunition || ammunitionQt < MAX_AMMUNITION && enemySight.ammunitionInSight)
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
		}else{
			// BDI Loop
			List<int> D = options();
			int I = filter(D);
			enemyBeliefs.reconsider = false;
			//Debug.Log(I + " speed: "+nav.speed);
			Stack<int> pi = plan(I);
			while(pi.Count > 0){
				int action = pi.Pop();
				//Debug.Log("action: "+action);
				execute(action);
				if(enemyBeliefs.reconsider){
					D = options();
					I = filter (D);
					enemyBeliefs.reconsider = false;
				}
				if(!sound(I, pi))
					pi = plan(I);
			}
		}
	}

	// BDI functions
	List<int> options(){
		List<int> opt = new List<int>();

		if(energy.value < 40+(Random.value*15))
			opt.Add(REST);
		if(takingHealthPackage || enemyHealth.health <= 80)
			opt.Add(GET_HP);
		if(takingAmmunition || ammunitionQt < 2)
			opt.Add(GET_AMMUNITION);
		if(enemySight.switchInSight)
			opt.Add(ACTIVATE_LASER);
		if(enemySight.playerInSight)
			opt.Add(KILL_ETHAN);
		if(helping && !enemySight.playerInSight)
			opt.Add(GO_HELP);
		opt.Add(PATROL);
		
		return opt;
	}
	
	int filter(List<int> desires){
		if(desires.Count == 1)
			return desires[0];
		
		foreach(int d in desires){
			switch(d){
			case GO_HELP:
				return GO_HELP;
				break;
			case GET_AMMUNITION : 
				if(enemyBeliefs.knowsAmmunitionLocation() && (!enemySight.playerInSight || ammunitionQt < 1))
					return GET_AMMUNITION;
				break;
			case REST :
				if(!enemySight.playerInSight || energy.value < 10)
					return REST;
				break;
			case GET_HP :
				if(enemyBeliefs.knowsHealthPackageLocation())
					return GET_HP;
				break;
			case KILL_ETHAN :
				if(ammunitionQt > 0)
					return KILL_ETHAN;
				else
					return ASK_HELP;
				break;
			case ACTIVATE_LASER :
				if(isLaserDeactivated())
					return ACTIVATE_LASER;
				break;
			}
		}
		return PATROL;
	}
	
	Stack<int> plan(int intention){
		Stack<int> actions = new Stack<int>();
		if(intention == KILL_ETHAN){
			if(isCloseEnough(player.position, 2))
				actions.Push(SHOOT_ETHAN);
			else{
				actions.Push(SHOOT_ETHAN);
				actions.Push(CHASE_ETHAN);
			}
		}else
			actions.Push(intention);
		return actions;
	}
	
	void execute(int action){
		//Debug.Log(action);
		switch(action){
		case GET_AMMUNITION :
			TakeAmmunition();
			break;
		case REST :
			Resting();
			break;
		case GET_HP :
			TakeHealthPackage();
			break;
		case KILL_DRONE :
			break;
		case ACTIVATE_LASER :
			ActivateLaser();
			break;
		case PATROL :
			Patrolling();
			break;
		case CHASE_ETHAN :
			Chasing();
			break;
		case SHOOT_ETHAN:
			Shooting();
			break;
		case ASK_HELP:
			Cooperation.askForHelp(transform.position);
			break;
		case GO_HELP:
			goHelp();
			break;
		}
	}
	
	bool sound(int intention, Stack<int> plan){
		if(intention == KILL_ETHAN)
			return plan.Contains(SHOOT_ETHAN) || plan.Count == 0;
		else
			return plan.Contains(intention) || plan.Count == 0;
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

	bool isLaserDeactivated(){
		Transform screenTransf = enemySight.currentSwitch.GetComponentInChildren<Transform>();
		Renderer screen = screenTransf.Find("prop_switchUnit_screen_001").renderer;
		return screen.material.name == "prop_switchUnit_screen_unlocked_mat (Instance)";
	}

	void ActivateLaser(){
		if(isLaserDeactivated())
			nav.destination = enemySight.currentSwitch.transform.position;
	}

	void TakeAmmunition() {
		nav.speed = chaseSpeed;
		nav.destination = enemyBeliefs.getClosestAmmunition();

		if(nav.remainingDistance < 1.2)
			if(!takingAmmunition){
				ammunitionTakeStart = Time.time;
				takingAmmunition = true;
			}else if(Time.time - ammunitionTakeStart > AMMUNITION_TAKE_DURATION){
				ammunitionQt = MAX_AMMUNITION;
				takingAmmunition = false;
			}
	}
	
	void TakeHealthPackage() {
		nav.speed = chaseSpeed;
		nav.destination = enemyBeliefs.getClosestHealthPackage();
		if(nav.remainingDistance < 1.2)
			if(!takingHealthPackage){
				healthTakeStart = Time.time;
				takingHealthPackage = true;
			}else if(Time.time - healthTakeStart > HEALTH_TAKE_DURATION){
				enemyHealth.AddHealth(20);
				takingHealthPackage = false;
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
		//Debug.Log("I should patroll: "+enemyBeliefs.getAreaToPatrol());
		if(!isCloseEnough(enemyBeliefs.getAreaToPatrol(), 15)){
			nav.destination = enemyBeliefs.getAreaToPatrol();
		}else{
			Vector3 newPatrollingArea = enemyBeliefs.getAreaToPatrol();
			Cooperation.patrolling(patrollingArea, newPatrollingArea);
			patrollingArea = newPatrollingArea;

			if(!patrolling){
				patrolling = true;
				patrollingStartTime = Time.time;
				nav.speed = patrolSpeed;
				int xFactor = Random.value > 0.5? 2: -2;
				int zFactor = Random.value > 0.5? 2: -2;
				nextPosition = new Vector3 (transform.position.x+xFactor,
				                                    transform.position.y,
				                                    transform.position.z+zFactor);
				nav.destination = nextPosition;
			}else{
				if (nav.remainingDistance < 1 || Time.time - patrollingStartTime > PATROLLING_DURATION)
					patrolling = false;
			}
		}
	}

	public void goHelp(Vector3 pos){
		helping = true;
		helpingPos = pos;
	}

	void goHelp(){
		if(!isCloseEnough(enemyBeliefs.getAreaToPatrol(), 1)){
			nav.speed = chaseSpeed*2;
			nav.destination = helpingPos;
		}
		else
			helping = false;
	}
}
