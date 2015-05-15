using UnityEngine;
using System.Collections;

public class Energy {
	public int MAX_ENERGY = 100;
	public int RESTING_DURATION = 2;
	private int REST_AMOUNT = 30;
	public int value;

	public Energy(){
		value = MAX_ENERGY;
	}

	public void TakeEnergy(int v){
		value = Mathf.Max(value-v, 0);
		//Debug.Log(value);
	}

	public void Rest(){
		value = Mathf.Min(value+REST_AMOUNT, MAX_ENERGY);
	}
}
