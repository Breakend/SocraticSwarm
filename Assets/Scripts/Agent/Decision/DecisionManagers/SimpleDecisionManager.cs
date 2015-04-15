using UnityEngine;
using System.Collections;
using NumUtils.NelderMeadSimplex;

public class SimpleDecisionManager : IDecisionManager{
	
	private CostManager costManager;
	private SensorModule sensorModule;

	// WEIGHTS

	public void CleanUp(){
		costManager = null;
		sensorModule = null;
	}

	public KnowledgeBase knowledgeBase;

	public SimpleDecisionManager (KnowledgeBase knowledgeBase, SensorModule sensorModule) {
		this.knowledgeBase = knowledgeBase;
		this.sensorModule = sensorModule;
		this.costManager = new CostManager (knowledgeBase, sensorModule);
	}

	// Run cost minimization
	Vector3 IDecisionManager.getNextDesiredPoint() {
		
		// What are the variables to minimize (position)
		SimplexConstant[] constants = new SimplexConstant[] { new SimplexConstant(this.sensorModule.gps.position.x, 1), 
			new SimplexConstant(this.sensorModule.gps.position.y, 1),
			new SimplexConstant(this.sensorModule.gps.position.z, 1)};
		double tolerance = 1e-30;
		int maxEvals = 10000;
		
		// What's the objective function
		ObjectiveFunctionDelegate objFunction = new ObjectiveFunctionDelegate(_objFunction1);
		RegressionResult result = NelderMeadSimplex.Regress(constants, tolerance, maxEvals, objFunction);
		Vector3 r = new Vector3((float)result.Constants[0], (float)result.Constants[1], (float)result.Constants[2]);
//		Debug.Log (result.TerminationReason.ToString ());
		return r;
	}

	/**
	 * This is abstracted out from the objective function so we can debug the cost outside of the Nelson-Mead optimization
	 * */
	private double getCost(Vector3 unconstrainedPosition){
		// Constrain position to only possible movements with current velocity 
		// TODO: REMOVE HARDCODING OF VELOCITY
		Vector3 desiredPosition = Vector3.MoveTowards(this.sensorModule.gps.position, unconstrainedPosition, 10*Time.deltaTime);
		
		
		foreach (Cost c in costManager.GetCosts()) {
			c.SetDesiredPosition(desiredPosition);
		}

		double cost = costManager.GetTotalCost ();
		// If it's less than zero it must have wrapped around, set it to max
		if (cost < 0)
			cost = double.MaxValue;
		
		return cost;
	}

	/**
	 * Here we have the objective function for the simplex algorithm
	 */
	private double _objFunction1(double[] constants)
	{
		Vector3 testPos = new Vector3 ((float)constants [0], (float)constants [1], (float)constants [2]);
		
		// abstracted out cost to be able to use function elsewhere
		return getCost (testPos);
	}

	public CostManager GetCostManager(){
		return costManager;
	}
}
