using UnityEngine;
using System.Collections.Generic;

public class EnemyBeliefs{
	List<Vector3> ammunitionLocations = new List<Vector3>();
	List<Vector3> healthPackageLocations = new List<Vector3>();
	List<Vector3> switchLocations = new List<Vector3>();	
	Vector3 lastPlayerPosition = new Vector3(0,0,0);
	Vector3 areaToPatrol = new Vector3(0,0,0);
	Transform guard;
	public bool reconsider = false;

	public EnemyBeliefs(Transform guardTransform){
		guard = guardTransform;
	}

	public void addAmmunition(Vector3 pos){
		if(!ammunitionLocations.Contains(pos)){
			ammunitionLocations.Add(pos);
			Cooperation.broadcastAmmunitionLocation(pos);
			reconsider = true;
		}
	}

	public void addHealthPackage(Vector3 pos){
		if(!healthPackageLocations.Contains(pos)){
			healthPackageLocations.Add(pos);
			Cooperation.broadcastHealthPackageLocation(pos);
			reconsider = true;
		}
	}

	public void addSwitch(Vector3 pos){
		if(!switchLocations.Contains(pos)){
			switchLocations.Add(pos);
			reconsider = true;
		}
	}

	public void setLastPlayerPosition(Vector3 pos){
		lastPlayerPosition = pos;
		reconsider = true;
	}

	public Vector3 getClosestAmmunition(){
		int closestDist = int.MaxValue;
		Vector3 closest = new Vector3(0,0,0);

		foreach(Vector3 pos in ammunitionLocations){
			int dist = (int)Vector3.Distance(guard.position, pos);
			if(dist < closestDist){
				closestDist = dist;
				closest = pos;
			}
		}
		return closest;
	}

	public Vector3 getClosestHealthPackage(){
		int closestDist = int.MaxValue;
		Vector3 closest = new Vector3(0,0,0);
		
		foreach(Vector3 pos in healthPackageLocations){
			int dist = (int)Vector3.Distance(guard.position, pos);
			if(dist < closestDist){
				closestDist = dist;
				closest = pos;
			}
		}
		return closest;
	}

	public Vector3 getClosestSwitch(){
		int closestDist = int.MaxValue;
		Vector3 closest = new Vector3(0,0,0);
		
		foreach(Vector3 pos in switchLocations){
			int dist = (int)Vector3.Distance(guard.position, pos);
			if(dist < closestDist){
				closestDist = dist;
				closest = pos;
			}
		}
		return closest;
	}

	public void setAreaToPatrol(Vector3 pos){
		Debug.Log("setAreaToPatrol" + pos);
		Cooperation.patrolling(areaToPatrol, pos);
		areaToPatrol = pos;
	}

	public Vector3 getAreaToPatrol(){
		return areaToPatrol;
	}

	public bool knowsAmmunitionLocation(){
		return ammunitionLocations.Count > 0;
	}

	public bool knowsHealthPackageLocation(){
		return healthPackageLocations.Count > 0;
	}

	public bool knowsSwitchLocation(){
		return switchLocations.Count > 0;
	}

	public Vector3 getLastPlayerPosition(){
		return lastPlayerPosition;
	}
}