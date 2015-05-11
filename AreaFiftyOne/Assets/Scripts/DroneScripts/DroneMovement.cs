using UnityEngine;
using System.Collections;

public class DroneMovement : MonoBehaviour {
	
	private Transform player;
	private DroneShooting droneShooting;
	private DroneHealth droneHealth;
	private float dist;

	void Awake() {
		player = GameObject.FindGameObjectWithTag(Tags.player).transform;
		droneShooting = GetComponent<DroneShooting>();
		droneHealth = GetComponent<DroneHealth>();
	}

	void Update ()
	{
		//if (!droneShooting.getShootingStatus ()) {
		this.dist = Vector3.Distance (player.position, transform.position);
		if(droneHealth.health >= 20.0f) {
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
			}
		}
		//}
	}

}
