using UnityEngine;
using System.Collections;

public class TileDestinationCost : Cost {
	public static double[] DEFAULT_WEIGHT_COSTS = {.01};
	public static int NUMBER_WEIGHTS = 1;

	public TileDestinationCost(KnowledgeBase kb, SensorModule sm) : base(kb, sm){
	}

	public override double GetCostValue(){
			double distToGoal = 0;
			SearchTile goal = this.kb.GetCurrentGoal ();
			if (goal != null)
					distToGoal = Vector3.Distance (desiredPosition, goal.center);
		return distToGoal;
	}

	public static double[] GetDefaultWeightCosts(){
		return DEFAULT_WEIGHT_COSTS;
	}

	public static int GetNumberWeights(){
		return NUMBER_WEIGHTS;
	}

}
