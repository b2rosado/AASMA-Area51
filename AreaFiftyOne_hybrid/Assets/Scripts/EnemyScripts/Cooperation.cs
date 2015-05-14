using UnityEngine;
using System.Collections.Generic;

public static class Cooperation{
	private static GameObject[] guardsGameObject;
	private static List<EnemyAI> guardsAI = new List<EnemyAI>();
	private static bool init = false;
	private static Vector3[] areas = {new Vector3(8,0,10), new Vector3(-6,0,4), new Vector3(-10,0,30), new Vector3(-23,0,26)};
	private static int[] areasOcupation = {0,0,0,0};

	public static void setup(){
		if(!init){
			Debug.Log("setup called");
			guardsGameObject = GameObject.FindGameObjectsWithTag(Tags.enemy);
			foreach(GameObject guard in guardsGameObject)
				guardsAI.Add(guard.GetComponent<EnemyAI>());
			init = true;
		}
	}
	
	public static void broadcastAmmunitionLocation(Vector3 pos){
		foreach(EnemyAI g in guardsAI){
			g.enemyBeliefs.addAmmunition(pos);
		}
	}

	public static void broadcastHealthPackageLocation(Vector3 pos){
		foreach(EnemyAI g in guardsAI){
			g.enemyBeliefs.addHealthPackage(pos);
		}
	}

	public static void broadcastPlayerPosition(Vector3 pos){
		foreach(EnemyAI g in guardsAI){
			g.enemyBeliefs.setLastPlayerPosition(pos);
		}
	}

	public static void patrolling(Vector3 oldPos, Vector3 newPos){
		for(int i=0; i<areas.Length; i++)
			if(areas[i] == oldPos)
				areasOcupation[i]--;
			else if(areas[i] == newPos)
				areasOcupation[i]++;
	}

	public static Vector3 getLessOcupiedArea(){
		int count = int.MaxValue;
		int lessOcupied = -1;
		for(int i=0; i<areasOcupation.Length; i++){
			if(areasOcupation[i] < count){
				count = areasOcupation[i];
				lessOcupied = i;
			}
		}
		Debug.Log("lessOcupiedArea");
		return areas[lessOcupied];
	}

	public static void askForHelp(Vector3 pos){

	}
}