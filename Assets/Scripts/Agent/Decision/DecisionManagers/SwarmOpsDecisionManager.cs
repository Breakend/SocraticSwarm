using UnityEngine;
using System.Collections;
using SwarmOps;
using SwarmOps.Optimizers;

public class SwarmOpsDecisionManager : IDecisionManager{

	private CostManager costManager;
	private SensorModule sensorModule;
	private SimpleDecisionProblem prob;
	private Optimizer opt;
	private int MAX_OPTIMIZATION_ITERATIONS = 1000;


	public KnowledgeBase knowledgeBase;

	void IDecisionManager.CleanUp(){
		knowledgeBase = null;
		opt = null;
		prob.CleanUp ();
		sensorModule = null;
		costManager.CleanUp ();
		costManager = null;
	}

	public SwarmOpsDecisionManager (KnowledgeBase knowledgeBase, SensorModule sensorModule) {
		this.knowledgeBase = knowledgeBase;
		this.sensorModule = sensorModule;
		this.costManager = new CostManager (knowledgeBase, sensorModule);
		this.prob = new SimpleDecisionProblem (knowledgeBase, costManager, sensorModule, MAX_OPTIMIZATION_ITERATIONS);
		this.opt = new DE (this.prob);
		Globals.Random = new RandomOps.MersenneTwister();

	}

	// Run cost minimization
	Vector3 IDecisionManager.getNextDesiredPoint() {

		Result result = this.opt.Optimize();
//		Debug.Log ("Feasible: " + result.Feasible);
		return new Vector3((float)result.Parameters[0], (float)result.Parameters[1], (float)result.Parameters[2]);
	}

	public CostManager GetCostManager(){
		return costManager;
	}
}
