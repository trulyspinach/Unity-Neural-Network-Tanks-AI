using System;
using UnityEngine;

using SPINACH.AI;

namespace ArtificialTankDriver_by_QI {
	
	public class TankDriver : MonoBehaviour {

		public Tank target;
		public float viewRange;
		
		
		public GeneticOptimizeableNerualNetwork network;
		
		private void Awake() {
			target = GetComponent<Tank>();
			
			network = new GeneticOptimizeableNerualNetwork(5,3);
			var actvationFunction = new TanhFunction();
			for (var i = 0; i < network.activateFunctions.Length; i++) {
				network.SetActivationFunctionForLayer(i, actvationFunction);
			}
		}

		public double CalculateFitness() {
			network.fitness = target.score;
			return network.fitness;
		}
		
		//call per training update.
		public void DoSomethingUseful() {
			// calculate all input features

			var inputs = new double[5];
			var closestEnemy = target.ClosestEnemy(viewRange);
			
			//assuming that closest one is always the one it trying to attack.
			
			//distance between enemy.
			inputs[0] = closestEnemy != null ? Vector3.Distance(transform.position, closestEnemy.position) / viewRange : 1d;
			//cos to enemy.
			inputs[1] = closestEnemy != null ? Vector3.Dot(transform.right, (closestEnemy.position - transform.position).normalized) : 1d;
			//is weapon ready ?
			inputs[2] = target.weaponReady ? 1d : 0d;
			// current speed.
			inputs[3] = target.rigidbody.velocity.magnitude / target.maxSpeed;
			// current torque.
			inputs[4] = target.rigidbody.angularVelocity.magnitude / target.maxTorque;

			//feedforward
			var output = network.Compute(inputs);
			
			//drive
			target.SetMove((float)output[0]);
			target.SetRotate((float)output[1]);
			if(output[2] > 0) target.Shoot();
		}

		private void OnDrawGizmosSelected() {
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(transform.position,viewRange);
		}
	}

}

