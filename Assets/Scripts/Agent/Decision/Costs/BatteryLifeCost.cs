using UnityEngine;
using System.Collections;

public class BatteryLifeCost : Cost {
	public static double[] DEFAULT_WEIGHT_COSTS = {1.0};
	private float beta_exponentiator = 10.0f;
	public static int NUMBER_WEIGHTS = 1;

	public BatteryLifeCost(KnowledgeBase kb, SensorModule sm) : base(kb, sm){
	}

	public override double GetCostValue(){
		//TODO: this should really depend on the distance from the home base and the drain rate
		// i.e. if you have just enough time to get to the base, you need to head for it ASAP
		double cost = 0.0;
		if (sm.battery.batteryLife < .25f) {
			cost = Mathf.Pow (Vector3.Distance (sm.gps.position, kb.BaseStationPosition), beta_exponentiator*(1-sm.battery.batteryLife)) ;
		}

		return cost;
	}

	public static double[] GetDefaultWeightCosts(){
		return DEFAULT_WEIGHT_COSTS;
	}

	public static int GetNumberWeights(){
		return NUMBER_WEIGHTS;
	}

}
