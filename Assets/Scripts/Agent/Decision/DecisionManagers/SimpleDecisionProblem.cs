/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Copyright (C) 2003-2011 Magnus Erik Hvass Pedersen.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System;
using System.Diagnostics;
using UnityEngine;
using System.Collections;

/// <summary>
/// Constrained optimization problem, example.
/// This is the 2-dimensional Rosenbrock problem
/// with some example constraints.
/// The optimal feasible solution seems to be:
/// a ~ 1.5937
/// b ~ 2.5416
/// </summary>
class SimpleDecisionProblem : SwarmOps.Problem
{

	CostManager costManager;
	SensorModule sensorModule;
	KnowledgeBase kb;

	public void CleanUp(){
		costManager = null;
		sensorModule = null;
		kb = null;
	}

	#region Constructors.
	/// <summary>
	/// Construct the object.
	/// </summary>
	public SimpleDecisionProblem(KnowledgeBase kb, CostManager cm, SensorModule sm, int MaxIterations)
		: base(MaxIterations)
	{
		this.kb = kb;
		this.costManager = cm;
		this.sensorModule = sm;
	}
	#endregion


	/// <summary>
	/// Lower search-space boundary.
	/// </summary>
	public override double[] LowerBound
	{
		get {
			return new double[]{	this.sensorModule.gps.position.x - 15,
									this.sensorModule.gps.position.y - 15,
									this.sensorModule.gps.position.z - 15};
		}
	}


	/// <summary>
	/// Upper search-space boundary.
	/// </summary>
	public override double[] UpperBound
	{
		get {
			return new double[]{	this.sensorModule.gps.position.x + 15,
									this.sensorModule.gps.position.y + 15,
									this.sensorModule.gps.position.z + 15};
		}
	}

	/// <summary>
	/// Lower initialization boundary.
	/// </summary>
	public override double[] LowerInit
	{
		get { return LowerBound; }
	}

	/// <summary>
	/// Upper initialization boundary.
	/// </summary>
	public override double[] UpperInit
	{
		get { return UpperBound; }
	}



	#region Base-class overrides.
	/// <summary>
	/// Name of the optimizer.
	/// </summary>
	public override string Name
	{
		get { return "SimpleDecisionProblem"; }
	}

	/// <summary>
	/// Dimensionality of the problem.
	/// </summary>
	public override int Dimensionality
	{
		get { return 3; }
	}



	/// <summary>
	/// Minimum possible fitness for this problem.
	/// </summary>
	public override double MinFitness
	{
		get { return 0; }
	}

	/// <summary>
	/// Compute and return fitness for the given parameters.
	/// </summary>
	/// <param name="x">Candidate solution.</param>
	public override double Fitness(double[] x)
	{
		Vector3 desiredPosition = new Vector3 ((float)x [0], (float)x [1], (float)x [2]);
		foreach (Cost c in costManager.GetCosts()) {
			c.SetDesiredPosition(desiredPosition);
		}

		double cost = costManager.GetTotalCost ();
		// If it's less than zero it must have wrapped around, set it to max
		if (cost < 0)
			cost = double.MaxValue;

		return cost;
	}

	/// <summary>
	/// Enforce and evaluate constraints.
	/// </summary>
	/// <param name="x">Candidate solution.</param>
	public override bool EnforceConstraints(ref double[] x)
	{
		// Return feasibility.
		return Feasible(x);
	}

	/// <summary>
	/// Evaluate constraints.
	/// </summary>
	/// <param name="x">Candidate solution.</param>
	public override bool Feasible(double[] x)
	{
		Vector3 desiredPosition = new Vector3 ((float)x [0], (float) x [1], (float) x [2]);

		// Override to avoid collisions, don't even consider it if you're going to be within a couple meters
		// of another agent.
		for (int i = 0; i < kb.updates.Count; i++) {
			double distToAgent = Vector3.Distance (desiredPosition, ((AgentUpdate)this.kb.updates [i]).lastUpdate.position);

			//Should be normalized to 0 between 0 and comm_range
			if(distToAgent < 4) return false;
		}
		//TODO: this should correlate to velocity not just be generic
		return (Vector3.Distance (desiredPosition, this.sensorModule.gps.position) < 10);
	}
	#endregion
}

