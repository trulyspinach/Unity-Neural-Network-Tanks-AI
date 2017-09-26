using System.Collections.Generic;

namespace SPINACH.AI {
	public class GeneticOptimizeableNerualNetwork : NeuralNetwork, IGeneticOptimizeable {
		
		public double fitness { get; set; }
	
		public List<double> optimizeableValues {
			get { return Serialize(); }
			set { Deserialize(value);}
		}
	
		public List<double> B { get; set; }
		
		public GeneticOptimizeableNerualNetwork(params int[] topology): base(topology){}

		public IGeneticOptimizeable Reproduce() {
			var clone = new GeneticOptimizeableNerualNetwork(layers);
			clone.Deserialize(Serialize());
			for (var i = 0; i < layers.Length; i++) {
				for (var j = 0; j < layers[i]; j++) {
					clone.neuronsOutputs[i][j] = neuronsOutputs[i][j];
				}
			}
		
			for (var i = 0; i < layers.Length - 1; i++) {
				for (var j = 0; j < layers[i + 1]; j++) {
					clone.activateFunctions[i][j] = activateFunctions[i][j];
				}
			}

			clone.fitness = fitness;
			return clone;
		}
	}
}

