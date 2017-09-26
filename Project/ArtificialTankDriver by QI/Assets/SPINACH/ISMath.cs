using System;
using UnityEngine;
using Random = System.Random;

public class ISMath {

	private static readonly Random RANDOM = new Random();
	
	public static double GaussianRandomDistributed {
		get {
			var u1 = RANDOM.NextDouble();
			var u2 = RANDOM.NextDouble();

			var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
			                      Math.Sin(2.0 * Math.PI * u2);
			return randStdNormal / 12.5663706144 + 0.5;
		}
	}

	public static double GaussianDistribution(ISGaussianDistributionInfo info, double x, double y) {
		return GaussianDistribution(info, x) * GaussianDistribution(info, y) / 2d;
	}
	
	public static double GaussianDistribution(ISGaussianDistributionInfo info, double x) {
		return GaussianDistribution(x, info.s, info.o, info.u);
	}
	
	public static double GaussianDistribution(double x, double s, double o, double u) {
		var sqro = o * o;
		return 1d / Math.Sqrt(6.283185307 * sqro) * Math.Exp(-(Math.Pow(s * x - s * u, 2d) / (2d * sqro)));
	}
	
	public static bool ContainsIn(float value, float a, float b) {
		return value >= a && value <= b;
	}

	public static Vector2 RotateVector(Vector2 v, float angle) {
		return new Vector2(v.x * Mathf.Cos(angle) - v.y * Mathf.Sin(angle), v.x * Mathf.Sin(angle) + v.y * Mathf.Cos(angle));
	}

	public static Vector3 RotateVectorXZ(Vector3 v, float angle) {
		return new Vector3(v.x * Mathf.Cos(angle) - v.z * Mathf.Sin(angle), 0,
			v.x * Mathf.Sin(angle) + v.z * Mathf.Cos(angle));
	}

	public static Vector3 ChangeLengthTo(Vector3 v, float s) {
		var f = s / v.magnitude;
		return v * f;
	}
	
	public static bool Zero(float v)
	{
		return Mathf.Approximately(v, 0.0f);
	}

	public static float Reciprocal(float v)
	{
		return Zero(v) ? 0.0f : 1.0f / v;
	}

	public static float[] SoftmaxFliter(float[] input) {
		var sum = 0f;
		for (var i = 0; i < input.Length; i++) sum += (float)Math.Exp(input[i]);
		for (var i = 0; i < input.Length; i++) input[i] = (float)Math.Exp(input[i]) / sum;
		return input;
	}
	
	public static float Clamp(float v, ISRange range) {
		return Mathf.Clamp(v, range.min, range.max);
	}

	public static Vector3 ClampLength(Vector3 v, ISRange range) {
		var l = v.magnitude;
		if (l < range.min) return ChangeLengthTo(v, range.min);
		if (l > range.max) return ChangeLengthTo(v, range.max);
		return v;
	}

	public static float Map(float v, ISRange valueRange, ISRange targetedRange) {
		v = Clamp(v, valueRange);
		var f = targetedRange.length / valueRange.length;
		return targetedRange.min + f * v;
	}

	public static float MapToZeroAndOne(float min, float max, float value) {
		return value / (max - min);
	}

	public static float Random(ISRange range) {
		return UnityEngine.Random.Range(range.min, range.max);
	}

	public static float GaussianRandom(ISRange range) {
		return GaussianRandom(range.min, range.max);
	}
	
	public static float GaussianRandom(float min, float max) {
		return min + (max - min) * (float)GaussianRandomDistributed;
	}

	public static float GaussianRandomDescending(float min, float max) {
		var offseted = GaussianRandomDistributed + 0.5;
		if (offseted > 1d) offseted = 1 - (offseted - 1d);
		return min + (max - min) * (float)offseted;
	}
	
	public enum ISMath_BoundCompareResult {
		Contains,
		Larger,
		Lower
	}

	public static ISMath_BoundCompareResult BoundCompare(float value, float a, float b) {
		if (value > b) return ISMath_BoundCompareResult.Larger;
		if (value < a) return ISMath_BoundCompareResult.Lower;

		return ISMath_BoundCompareResult.Contains;
	}

	public class WaveNumberGenerater {
		public float numberMax;
		public float numberMin;
		public float translation;
		public float cycleOffseter;

		public WaveNumberGenerater(float min, float max, float offset) {
			numberMax = max;
			numberMin = min;
			cycleOffseter = offset;
			translation = UnityEngine.Random.Range(0, 2 * Mathf.PI / Mathf.Abs(cycleOffseter));
		}

		public float NumberForTime(float time) {
			return (numberMax - numberMin) / 2 * Mathf.Sin(time * cycleOffseter + translation) +
			       (numberMax + numberMin) / 2f;
		}
	}
}

[Serializable]
public class ISGaussianDistributionInfo {
	public double s, o, u;
}

[Serializable]
public class ISPerlinDistribution {

	public float scale;
	public static readonly ISRange valueRange = new ISRange(0f,1f); 
	
	private int m_seed;
	public int seed {
		get { return m_seed; }
		set {
			if (value == 0) {
				s_perm = new byte[s_perm_ORIGINAL.Length];
				s_perm_ORIGINAL.CopyTo(s_perm, 0);
			}
			else {
				s_perm = new byte[512];
				var random = new Random(value);
				random.NextBytes(s_perm);
				m_seed = value;
			}
		}
	}
	
	public ISPerlinDistribution() {
		s_perm = new byte[s_perm_ORIGINAL.Length];
		s_perm_ORIGINAL.CopyTo(s_perm, 0);
	}

	public float ValueAt(float x) {
		return Generate(x * scale);
	}

	public float ValueAt(float x, float y) {
		return Generate(x * scale, y * scale);
	}
	
	public float ValueAt(float x, float y, float z) {
		return Generate(x * scale, y * scale, z * scale);
	}
	
	internal float Generate(float x) {
		var i0 = FastFloor(x);
		var i1 = i0 + 1;
		var x0 = x - i0;
		var x1 = x0 - 1.0f;

		float n0, n1;

		var t0 = 1.0f - x0 * x0;
		t0 *= t0;
		n0 = t0 * t0 * Grad(s_perm[i0 & 0xff], x0);

		var t1 = 1.0f - x1 * x1;
		t1 *= t1;
		n1 = t1 * t1 * Grad(s_perm[i1 & 0xff], x1);
		return 0.395f * (n0 + n1);
	}

	internal float Generate(float x, float y) {
		const float f2 = 0.366025403f; // f2 = 0.5*(sqrt(3.0)-1.0)
		const float g2 = 0.211324865f; // g2 = (3.0-Math.sqrt(3.0))/6.0

		float n0, n1, n2;

		var s = (x + y) * f2;
		var xs = x + s;
		var ys = y + s;
		var i = FastFloor(xs);
		var j = FastFloor(ys);

		var t = (i + j) * g2;
		var X0 = i - t;
		var Y0 = j - t;
		var x0 = x - X0;
		var y0 = y - Y0;

		int i1, j1;
		if (x0 > y0) {
			i1 = 1;
			j1 = 0;
		}
		else {
			i1 = 0;
			j1 = 1;
		}

		var x1 = x0 - i1 + g2;
		var y1 = y0 - j1 + g2;
		var x2 = x0 - 1.0f + 2.0f * g2;
		var y2 = y0 - 1.0f + 2.0f * g2;

		var ii = i % 256;
		var jj = j % 256;

		var t0 = 0.5f - x0 * x0 - y0 * y0;
		if (t0 < 0.0f) n0 = 0.0f;
		else {
			t0 *= t0;
			n0 = t0 * t0 * Grad(s_perm[ii + s_perm[jj]], x0, y0);
		}

		var t1 = 0.5f - x1 * x1 - y1 * y1;
		if (t1 < 0.0f) n1 = 0.0f;
		else {
			t1 *= t1;
			n1 = t1 * t1 * Grad(s_perm[ii + i1 + s_perm[jj + j1]], x1, y1);
		}

		var t2 = 0.5f - x2 * x2 - y2 * y2;
		if (t2 < 0.0f) n2 = 0.0f;
		else {
			t2 *= t2;
			n2 = t2 * t2 * Grad(s_perm[ii + 1 + s_perm[jj + 1]], x2, y2);
		}

		return 20.0f * (n0 + n1 + n2) + 0.5f;
	}
	
	internal float Generate(float x, float y, float z)
        {
            const float F3 = 0.333333333f;
            const float G3 = 0.166666667f;

            float n0, n1, n2, n3;

            
            var s = (x + y + z) * F3;
            var xs = x + s;
            var ys = y + s;
            var zs = z + s;
            var i = FastFloor(xs);
            var j = FastFloor(ys);
            var k = FastFloor(zs);

            var t = (float)(i + j + k) * G3;
            var X0 = i - t; 
            var Y0 = j - t;
            var Z0 = k - t;
            var x0 = x - X0; 
            var y0 = y - Y0;
            var z0 = z - Z0;

            
            int i1, j1, k1; 
            int i2, j2, k2;

            
            if (x0 >= y0)
            {
                if (y0 >= z0)
                { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0; } 
                else if (x0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1; } 
                else { i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1; }
            }
            else
            { 
                if (y0 < z0) { i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1; }
                else if (x0 < z0) { i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1; }
                else { i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0; }
            }

            var x1 = x0 - i1 + G3; 
            var y1 = y0 - j1 + G3;
            var z1 = z0 - k1 + G3;
            var x2 = x0 - i2 + 2.0f * G3;
            var y2 = y0 - j2 + 2.0f * G3;
            var z2 = z0 - k2 + 2.0f * G3;
            var x3 = x0 - 1.0f + 3.0f * G3;
            var y3 = y0 - 1.0f + 3.0f * G3;
            var z3 = z0 - 1.0f + 3.0f * G3;

            var ii = Mod(i, 256);
            var jj = Mod(j, 256);
            var kk = Mod(k, 256);

            var t0 = 0.6f - x0 * x0 - y0 * y0 - z0 * z0;
            if (t0 < 0.0f) n0 = 0.0f;
            else
            {
                t0 *= t0;
                n0 = t0 * t0 * Grad(s_perm[ii + s_perm[jj + s_perm[kk]]], x0, y0, z0);
            }

            var t1 = 0.6f - x1 * x1 - y1 * y1 - z1 * z1;
            if (t1 < 0.0f) n1 = 0.0f;
            else
            {
                t1 *= t1;
                n1 = t1 * t1 * Grad(s_perm[ii + i1 + s_perm[jj + j1 + s_perm[kk + k1]]], x1, y1, z1);
            }

            var t2 = 0.6f - x2 * x2 - y2 * y2 - z2 * z2;
            if (t2 < 0.0f) n2 = 0.0f;
            else
            {
                t2 *= t2;
                n2 = t2 * t2 * Grad(s_perm[ii + i2 + s_perm[jj + j2 + s_perm[kk + k2]]], x2, y2, z2);
            }

            var t3 = 0.6f - x3 * x3 - y3 * y3 - z3 * z3;
            if (t3 < 0.0f) n3 = 0.0f;
            else
            {
                t3 *= t3;
                n3 = t3 * t3 * Grad(s_perm[ii + 1 + s_perm[jj + 1 + s_perm[kk + 1]]], x3, y3, z3);
            }

            return 16.0f * (n0 + n1 + n2 + n3) + 0.5f;
        }

	private byte[] s_perm;

	private static readonly byte[] s_perm_ORIGINAL = {
		151, 160, 137, 91, 90, 15,
		131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23,
		190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33,
		88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134, 139, 48, 27, 166,
		77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244,
		102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196,
		135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123,
		5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42,
		223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9,
		129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 218, 246, 97, 228,
		251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107,
		49, 192, 214, 31, 181, 199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254,
		138, 236, 205, 93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180,
		151, 160, 137, 91, 90, 15,
		131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23,
		190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33,
		88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134, 139, 48, 27, 166,
		77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244,
		102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196,
		135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123,
		5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42,
		223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9,
		129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 218, 246, 97, 228,
		251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107,
		49, 192, 214, 31, 181, 199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254,
		138, 236, 205, 93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180
	};

	private static int FastFloor(float x) {
		return x > 0 ? (int) x : (int) x - 1;
	}

	private static float Grad(int hash, float x) {
		var h = hash & 15;
		var Grad = 1.0f + (h & 7);
		if ((h & 8) != 0) Grad = -Grad;
		return Grad * x;
	}

	private static float Grad(int hash, float x, float y) {
		var h = hash & 7;
		var u = h < 4 ? x : y;
		var v = h < 4 ? y : x;
		return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -2.0f * v : 2.0f * v);
	}
	
	private static float Grad(int hash, float x, float y, float z)
	{
		var h = hash & 15;     
		var u = h < 8 ? x : y; 
		var v = h < 4 ? y : h == 12 || h == 14 ? x : z; 
		return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -v : v);
	}

	private static float Grad(int hash, float x, float y, float z, float t)
	{
		var h = hash & 31;     
		var u = h < 24 ? x : y;
		var v = h < 16 ? y : z;
		var w = h < 8 ? z : t;
		return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -v : v) + ((h & 4) != 0 ? -w : w);
	}
	
	private static int Mod(int x, int m)
	{
		var a = x % m;
		return a < 0 ? a + m : a;
	}
}

[Serializable]
public struct ISRange {
	public bool Equals(ISRange other) {
		return min.Equals(other.min) && max.Equals(other.max);
	}

	public override bool Equals(object obj) {
		if (ReferenceEquals(null, obj)) return false;
		return obj is ISRange && Equals((ISRange) obj);
	}

	public override int GetHashCode() {
		unchecked {
			return (min.GetHashCode() * 397) ^ max.GetHashCode();
		}
	}

	public float min, max;

	public float sum => min + max;

	public float length => max - min;

	public ISRange(float min, float max) {
		this.min = min;
		this.max = max;
	}

	public float Lerp(float v) {
		return min + v * length;
	}

	public float Evaluate(AnimationCurve c, float v) {
		return min + c.Evaluate(v) * length;
	}

	public static bool operator >(ISRange a, float b) {
		return a.max > b;
	}

	public static bool operator >(float a, ISRange b) {
		return a > b.max;
	}

	public static bool operator <(ISRange a, float b) {
		return a.min < b;
	}

	public static bool operator <(float a, ISRange b) {
		return a < b.min;
	}

	public static bool operator ==(float a, ISRange b) {
		return a >= b.min && a <= b.max;
	}

	public static bool operator !=(float a, ISRange b) {
		return a < b.min || a > b.max;
	}

	public override string ToString() {
		return $"{min} ~ {max}";
	}
}