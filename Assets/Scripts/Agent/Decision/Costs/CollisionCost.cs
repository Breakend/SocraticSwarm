using UnityEngine;
using System.Collections;

public class CollisionCost : Cost {

	public static double[] DEFAULT_WEIGHT_COSTS = {.01};
	public const double MIN_DISTANCE_BETWEEN_AGENTS = 10.0;

	public static int NUMBER_WEIGHTS = 1;

	public CollisionCost(KnowledgeBase kb, SensorModule sm) : base(kb, sm){
	}


	public static int GetNumberWeights(){
		return NUMBER_WEIGHTS;
	}

	public bool satisfiesAsConstraint(float comparedValue){
		if (comparedValue < mainWeight.weightValue * 50.0) {
			return false;
		}
		return true;
	}


	public static double[] GetDefaultWeightCosts(){
		return DEFAULT_WEIGHT_COSTS;
	}

	public override double GetCostValue(){
		// Hack, no cost just constraint.
		return 0.0;
	}

}
