using System;
using System.Collections.Generic;

namespace SPINACH.AI {
    public class NeuralNetwork {
        public double[][][] weights;
        public int[] layers;
        public double[][] bias;
        public double[][] neuronsOutputs;
        public IActivationFunction[][] activateFunctions;

        public NeuralNetwork(params int[] layers) {
            
            var layersLength = layers.Length;
            this.layers = layers;

            neuronsOutputs = new double[layersLength][];
            activateFunctions = new IActivationFunction[layersLength - 1][];
            weights = new double[layersLength - 1][][];
            bias = new double[layersLength - 1][];

            for (var i = 0; i < layersLength; i++) {
                neuronsOutputs[i] = new double[this.layers[i]];
            }
            
            var func = new SigmoidFunction();
            for (var i = 0; i < layersLength - 1; i++) {
                weights[i] = new double[this.layers[i]][];
                activateFunctions[i] = new IActivationFunction[this.layers[i]];
                for (var j = 0; j < this.layers[i]; j++) {
                    weights[i][j] = new double[this.layers[i + 1]];
                    activateFunctions[i][j] = func;
                }
                bias[i] = new double[this.layers[i + 1]];
            }
        }

        public void SetActivateFunction(IActivationFunction func) {
            for (var i = 0; i < layers.Length - 1; i++) {
                for (var j = 0; j < layers[i + 1]; j++) {
                    activateFunctions[i][j] = func;
                }
            }
        }

        public void SetActivationFunctionForLayer(int layerNum, IActivationFunction func) {
            for (var j = 0; j < layers[layerNum + 1]; j++) {
                activateFunctions[layerNum][j] = func;
            }
        }

        public double[] Compute(double[] input) {
            var res = new double[layers[layers.Length - 1]];
            for (var i = 0; i < layers[0]; i++) {
                neuronsOutputs[0][i] = input[i];
            }
            for (var i = 1; i < layers.Length; i++) {
                for (var j = 0; j < layers[i]; j++) {
                    double beforeActivation = 0;
                    for (var k = 0; k < layers[i - 1]; k++) {
                        beforeActivation += neuronsOutputs[i - 1][k] * weights[i - 1][k][j];
                    }
                    beforeActivation += bias[i - 1][j];
                    neuronsOutputs[i][j] = activateFunctions[i - 1][j].Compute(beforeActivation);
                }
            }
            res = neuronsOutputs[layers.Length - 1];
            return res;
        }

        public List<double> Serialize() {
            var res = new List<double>();
            for (var i = 0; i < layers.Length - 1; i++) {
                res.AddRange(bias[i]);
            }
            for (var i = 0; i < layers.Length - 1; i++) {
                for (var j = 0; j < layers[i]; j++) {
                    res.AddRange(weights[i][j]);
                }
            }
            return res;
        }

        public void Deserialize(List<double> data) {
            var id = 0;
            for (var i = 0; i < layers.Length - 1; i++) {
                for (var j = 0; j < layers[i + 1]; j++) {
                    bias[i][j] = data[id];
                    id++;
                }
            }

            for (var i = 0; i < layers.Length - 1; i++) {
                for (var j = 0; j < layers[i]; j++) {
                    for (var k = 0; k < layers[i + 1]; k++) {
                        weights[i][j][k] = data[id];
                        id++;
                    }
                }
            }
        }

        public NeuralNetwork Copy() {
            var clone = new NeuralNetwork(layers);
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
            return clone;
        }
    }
}



