using System;

namespace SPINACH.AI {

	public interface IActivationFunction {
		double Compute(double x);
	}
	
	public class ThersholdFunction : IActivationFunction {
		public double Compute(double x) {
			return  x > 0.0 ? 1.0 : -1.0;
		}
	}
	
	public class SigmoidFunction : IActivationFunction {
		public double Compute(double x) {
			return  1.0 / (1.0 + Math.Exp(-x));
		}
	}
	
	public class TanhFunction : IActivationFunction {
		public double Compute(double x) {
			return  Math.Tanh(x);
		}
	}
}
