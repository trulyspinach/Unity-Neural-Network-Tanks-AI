using System;
using System.Collections.Generic;
using System.Linq;
using Random = System.Random;

namespace SPINACH.AI {

    public enum SelectionMethod {
        Competitive,
        Natural,
        Random_ForFun
    }

    public class GeneticOptimisation {

        public SelectionMethod selectionMethod;
        public int populationCount;
        public List<IGeneticOptimizeable> population;
        public double mutateProbability;
        public static Random rnd = new Random(Guid.NewGuid().GetHashCode());

        public GeneticOptimisation(int populationCount, double mutateProbability, SelectionMethod selectionMethod) {
            this.selectionMethod = selectionMethod;
            this.populationCount = populationCount;
            this.mutateProbability = mutateProbability;
            population = new List<IGeneticOptimizeable>(populationCount);
        }

        private int GetRandomGen(IList<double> probs) {
            switch (selectionMethod) {
                case SelectionMethod.Competitive:
                    var dis = 1.0 - Math.Abs(ISMath.GaussianRandomDistributed - 0.5d);
                    return (int)Math.Round(dis * (probs.Count-1));

                case SelectionMethod.Natural:
                    double top = 0;
                    var randomValue = rnd.NextDouble();
                    for (var i = 0; i < probs.Count; i++) {
                        var bot = top;
                        top += probs[i];
                        if (randomValue >= bot && randomValue <= top) return i;
                    }
                    return 0;

                case SelectionMethod.Random_ForFun:
                    return rnd.Next(probs.Count);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private List<IGeneticOptimizeable> GetPairs() {
            var res = new List<IGeneticOptimizeable>(4 * populationCount);
            var sum = population.Sum(item => item.fitness);
            var lifeProb = new double[populationCount];
            for (var i = 0; i < lifeProb.Length; i++) lifeProb[i] = population[i].fitness / sum;
            lifeProb.OrderByDescending(x => x);
            var counter = 0;
            while (counter < populationCount) {
                var leftGen = GetRandomGen(lifeProb);
                var rightGen = GetRandomGen(lifeProb);
                if (leftGen == rightGen) continue;
                res.Add(population[leftGen].Reproduce());
                res.Add(population[rightGen].Reproduce());
                counter += 1;
            }
            return res;
        }

        private void ReproduceAll(IReadOnlyList<IGeneticOptimizeable> pairs) {
            population.Clear();
            for (var i = 0; i < pairs.Count; i += 2) {
                Crossover(pairs[i], pairs[i + 1]);
                Mutate(pairs[i]);
                population.Add(pairs[i]);
            }
        }

        public void CreateRandomPopulation(Func<IGeneticOptimizeable> randGen) {
            population.Clear();
            for (var i = 0; i < populationCount; i++) {
                population.Add(randGen());
            }
        }

        public void Evolve(double[] newFitnesses) {
            for (var i = 0; i < newFitnesses.Length; i++) population[i].fitness = newFitnesses[i];
            ReproduceAll(GetPairs());
        }

        public void Mutate(IGeneticOptimizeable gen) {
            var w = gen.optimizeableValues;
            for (var i = 0; i < w.Count; i++)
                if (rnd.NextDouble() < mutateProbability) w[i] += rnd.NextDouble() * 2 - 1;
            gen.optimizeableValues = w;
        }

        public void Crossover(IGeneticOptimizeable mom, IGeneticOptimizeable dad) {
            var momW = mom.optimizeableValues;
            var dadW = dad.optimizeableValues;
            var n = rnd.Next(momW.Count);
            for (var i = 0; i < momW.Count; i++) {
                if (i < n)
                    momW[i] = dadW[i];
                else
                    dadW[i] = momW[i];
            }
            mom.optimizeableValues = momW;
            dad.optimizeableValues = dadW;
        }

        public void RandomizePopulation(double min = -1.0, double max = 1.0) {
            for (var i = 0; i < populationCount; i++)
                Randomize(population[i], min, max);
        }

        private static void Randomize(IGeneticOptimizeable gen, double min, double max) {
            var w = gen.optimizeableValues;
            for (var i = 0; i < w.Count; i++) {
                w[i] = rnd.NextDouble() * (max - min) + min;
            }
            gen.optimizeableValues = w;
        }
    }

    public interface IGeneticOptimizeable {
        List<double> optimizeableValues { get; set; }

        double fitness { get; set; }

        IGeneticOptimizeable Reproduce();
    }
}
