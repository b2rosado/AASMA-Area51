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
			if(areas[i] == oldPos && !oldPos.Equals(new Vector3(0,0,0)))
				areasOcupation[i]--;
			else if(areas[i] == newPos)
				areasOcupation[i]++;
	}

	public static void notPatrolling(Vector3 pos){
		for(int i=0; i<areas.Length; i++)
			if(areas[i] == pos){
				areasOcupation[i]--;
				return;
			}
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
		return areas[lessOcupied];
	}

	/*public static Vector3 getMostOcupiedArea(){
		int count = int.MinValue;
		int mostOcupied = int.MaxValue;
		for(int i=0; i<areasOcupation.Length; i++){
			if(areasOcupation[i] > count){
				count = areasOcupation[i];
				mostOcupied = i;
			}
		}
		return areas[mostOcupied];
	}

	public static void rearrangePatrolling(){
		for(int i=0; i<guardsAI.Count; i++)
			if(guardsAI[i].patrollingArea.Equals(getMostOcupiedArea()){

			}
	}*/

	public static void askForHelp(Vector3 pos){
		foreach(EnemyAI ai in guardsAI)
			ai.goHelp(pos);
	}
}