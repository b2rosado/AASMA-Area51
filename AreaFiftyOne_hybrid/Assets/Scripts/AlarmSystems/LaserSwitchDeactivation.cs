using UnityEngine;
using System.Collections;

public class LaserSwitchDeactivation : MonoBehaviour
{
	public GameObject laser;				// Reference to the laser that can we turned off at this switch.
	public Material unlockedMat;		 	// The screen's material to show the laser has been unloacked.
	public Material lockedMat;
	
	private GameObject player;				// Reference to the player.
	private GameObject[] guards;
	
	
	void Awake ()
	{
		// Setting up the reference.
		player = GameObject.FindGameObjectWithTag(Tags.player);
		guards = GameObject.FindGameObjectsWithTag(Tags.enemy);
		LaserDeactivation();
	}
	
	
	void OnTriggerStay (Collider other)
	{
		// If the colliding gameobject is the player...
		if(other.gameObject == player){
			// ... and the switch button is pressed...
			if(Input.GetButton("Switch"))
				// ... deactivate the laser.
				LaserDeactivation();
		}
		if(other.GetType() == typeof(CapsuleCollider) && other.gameObject.tag == Tags.enemy){
			Renderer screen = transform.Find("prop_switchUnit_screen_001").renderer;
			if(!laser.activeSelf)
				LaserActivation();
		}
	}
	
	
	void LaserDeactivation ()
	{
		// Deactivate the laser GameObject.
		laser.SetActive(false);
		
		// Store the renderer component of the screen.
		Renderer screen = transform.Find("prop_switchUnit_screen_001").renderer;
		
		// Change the material of the screen to the unlocked material.
		screen.material = unlockedMat;
		
		// Play switch deactivation audio clip.
		audio.Play();
	}

	void LaserActivation ()
	{
		laser.SetActive(true);
		Renderer screen = transform.Find("prop_switchUnit_screen_001").renderer;
		screen.material = lockedMat;
		audio.Play();
	}
}
