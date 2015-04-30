using UnityEngine;
using System.Collections;

public class DroneMovement : MonoBehaviour {
	
	private Transform player;

	void Awake() {
		player = GameObject.FindGameObjectWithTag(Tags.player).transform;
	}

	void Update ()
	{
		float dist = Vector3.Distance(player.position, transform.position);
		if (dist > 2) {
			transform.LookAt(player);
			transform.position += transform.forward * 3 * Time.deltaTime;
			Vector3 newpos = transform.position;
			newpos.y = 2.7f;
			transform.position = newpos;
		}
	}

}
