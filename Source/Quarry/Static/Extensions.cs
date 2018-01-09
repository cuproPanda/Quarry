using System;

using UnityEngine;

namespace Quarry {

	public static class Extensions {

		public static string ToStringDecimal(this float num) {
			string result;
			if (Math.Abs(num) < 1f) {
				result = Math.Round(num, 2).ToString("0.##");
			}
			else {
				result = Math.Round(num, 1).ToString("0.#");
			}
			return result;
		}


		public static int RoundToAsInt(this float num, int factor) {
			return (int)(Math.Round(num / (double)factor, 0) * factor);
		}


		public static Rect LeftThird(this Rect rect) {
			return new Rect(rect.x, rect.y, rect.width / 3f, rect.height);
		}


		public static Rect MiddleThird(this Rect rect) {
			return new Rect(rect.x + rect.width / 3f, rect.y, rect.width / 3f, rect.height);
		}


		public static Rect RightThird(this Rect rect) {
			return new Rect(rect.x + rect.width / 1.5f, rect.y, rect.width / 3f, rect.height);
		}
	}
}
