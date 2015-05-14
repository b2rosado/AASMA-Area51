using UnityEngine;
using System.Collections;

public class DroneMovement : MonoBehaviour {
	
	private Transform player;
	private DroneShooting droneShooting;
	private DroneHealth droneHealth;
	private const float HEALTH_TAKE_DURATION = 4f;
	private float dist;
	private float healthTakeStart;

	void Awake() {
		player = GameObject.FindGameObjectWithTag(Tags.player).transform;
		droneShooting = GetComponent<DroneShooting>();
		droneHealth = GetComponent<DroneHealth>();
	}

	void Update ()
	{
		//if (!droneShooting.getShootingStatus ()) {
		this.dist = Vector3.Distance (player.position, transform.position);
		if(droneHealth.health >= 100) {
			if (dist > 2.5) {
				if (!droneShooting.getShootingStatus ())
					transform.LookAt (player);
				transform.position += transform.forward * 3 * Time.deltaTime;
				Vector3 newpos = transform.position;
				newpos.y = 2.7f;
				transform.position = newpos;
			}
		} else {
			if (dist < 6) {
				transform.position += -transform.forward * 3 * Time.deltaTime;
				Vector3 newpos = transform.position;
				newpos.y = 2.7f;
				transform.position = newpos;
				healthTakeStart = Time.time;
			} else if(Time.time - healthTakeStart > HEALTH_TAKE_DURATION){
				droneHealth.AddHealth(50);
			}
		}
		//}
	}

}
