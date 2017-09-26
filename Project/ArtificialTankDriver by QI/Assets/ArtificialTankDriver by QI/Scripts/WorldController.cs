using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using SPINACH.AI;
using Random = UnityEngine.Random;

namespace ArtificialTankDriver_by_QI {
	
	public class WorldController : MonoBehaviour {

		public int stepsPerSecond = 1;
		public float physicStepLength = 0.02f;

		public int totalStepsPerEpoch = 1000;
		
		public int tankCount;
		public Transform generatePoint;
		public float generateRadius;
		public GameObject tankPrefab;

		public int currentStepsInEpoch { get; private set; }
		public int epoch { get; private set; }
		
		private readonly List<Vector3> m_initPosition = new List<Vector3>();
		private readonly List<TankDriver> m_drivers = new List<TankDriver>();

		private GeneticOptimisation m_evolver;
		
		public void GenerateInitial() {
			m_evolver = new GeneticOptimisation(tankCount,0.15,SelectionMethod.Natural);
			
			for (var i = 0; i < tankCount; i++) {
				var rp = Random.insideUnitSphere;
				rp.y = 0;
				rp = generatePoint.position + rp.normalized * Random.Range(1, generateRadius);
				m_initPosition.Add(rp);
				
				m_drivers.Add(Instantiate(tankPrefab, rp, Quaternion.identity).GetComponent<TankDriver>());
				m_drivers[i].GetComponent<Tank>().Setup();
				m_evolver.population.Add(m_drivers[i].network);
			}
			
			m_evolver.RandomizePopulation();
		}

		public void Evolve() {
			var fitnesses = new double[tankCount];
			for (var i = 0; i < m_drivers.Count; i++) {
				fitnesses[i] = m_drivers[i].CalculateFitness();
			}
			var max = new List<double>(fitnesses).OrderByDescending(x => x).FirstOrDefault();
			Debug.Log($"<color=#E91E63>Epoch {epoch} finished with highest fitnesses {max}.</color>");
			
			m_evolver.Evolve(fitnesses);
			for (var i = 0; i < m_drivers.Count; i++) {
				m_drivers[i].network = m_evolver.population[i] as GeneticOptimizeableNerualNetwork;
			}

			epoch++;
			RestoreInitial();
		}
		
		public void RestoreInitial() {
			currentStepsInEpoch = 0;
			for (var i = 0; i < tankCount; i++) {
				m_drivers[i].transform.position = m_initPosition[i];
				m_drivers[i].GetComponent<Tank>().Setup();
			}
		}

		private void Start() {
			GenerateInitial();
		}

		private void Update() {
			Physics.autoSimulation = false;
			if(currentStepsInEpoch > totalStepsPerEpoch) Evolve();
			for (var i = 0; i < stepsPerSecond; i++) {
				TrainingUpdate();
			}
		}

		public void TrainingUpdate() {
			currentStepsInEpoch++;
			Physics.Simulate(physicStepLength);
			foreach (var tankDriver in m_drivers) {
				tankDriver.DoSomethingUseful();
			}
		}
	
		private void OnDrawGizmosSelected() {
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireSphere(generatePoint.position,generateRadius);
		}
	}

}

