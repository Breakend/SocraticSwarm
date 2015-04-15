using UnityEngine;
using System.Collections;

public class AltGoalCost : Cost {
	// public static double[] DEFAULT_WEIGHT_COSTS = {1.0};
	public static double[] DEFAULT_WEIGHT_COSTS = {0.376799464225769};
	public static int NUMBER_WEIGHTS = 1;
	public static float MAX_PENALTY_DISTANCE = 150.0f;

	public AltGoalCost(KnowledgeBase kb, SensorModule sm) : base(kb, sm){
	}

	public override double GetCostValue(){
		float distToGoal = 0;
		SearchTile goal = this.kb.GetCurrentGoal ();
		if (goal != null)
			distToGoal = Vector3.Distance (desiredPosition, goal.center);

//		return distToGoal;
		float cost = 2.0f * Mathf.Atan (distToGoal / MAX_PENALTY_DISTANCE) / Mathf.PI;

		return Mathf.Min (cost, 1);
	}

	public static double[] GetDefaultWeightCosts(){
		return DEFAULT_WEIGHT_COSTS;
	}

	public static int GetNumberWeights(){
		return NUMBER_WEIGHTS;
	}

}
