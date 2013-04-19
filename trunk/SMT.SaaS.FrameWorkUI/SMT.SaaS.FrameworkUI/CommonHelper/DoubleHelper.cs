// (c) Copyright Vitor de Souza (sincorde.com)
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

# region Using Directives

using System.Globalization;

# endregion

namespace System
{
	public static class DoubleHelper
	{
		# region Declarations

		private const double _Zero = 0;

		# endregion

		# region ValidateRange

		public static void ValidateRange(this double predicate, double minValue, double maxValue)
		{
			if (predicate < minValue || predicate > maxValue)
			{
				throw new OverflowException("The value is outside of range.");
			}
		} 

		# endregion

		# region ToDouble

		public static double ToDouble(this object predicate)
		{
			return predicate.ToDouble(null, null);
		}

		public static double ToDoubleOrMinimum(this object predicate)
		{
			return predicate.ToDoubleOrMinimum(null, null);
		}

		public static double ToDoubleOrMaximum(this object predicate)
		{
			return predicate.ToDoubleOrMaximum(null, null);
		}

		public static double ToDoubleOrZero(this object predicate)
		{
			return predicate.ToDoubleOrZero(null, null);
		}

		public static double? ToDoubleOrNull(this object predicate)
		{
			return predicate.ToDoubleOrNull(null, null);
		}

		public static double ToDoubleOrDefault(this object predicate, double defaultValue)
		{
			return predicate.ToDoubleOrDefault(defaultValue, null, null);
		}

		public static double ToDouble(this object predicate,
			IFormatProvider provider, NumberStyles? numberStyles)
		{
			return predicate.Convert<double>(provider, numberStyles, DateTimeStyles.None, null);
		}

		public static double ToDoubleOrMinimum(this object predicate,
			IFormatProvider provider, NumberStyles? numberStyles)
		{
			return predicate.ToDoubleOrDefault(double.MinValue, provider, numberStyles);
		}

		public static double ToDoubleOrMaximum(this object predicate,
			IFormatProvider provider, NumberStyles? numberStyles)
		{
			return predicate.ToDoubleOrDefault(double.MaxValue, provider, numberStyles);
		}

		public static double ToDoubleOrZero(this object predicate,
			IFormatProvider provider, NumberStyles? numberStyles)
		{
			return predicate.ToDoubleOrDefault(DoubleHelper._Zero, provider, numberStyles);
		}

		public static double? ToDoubleOrNull(this object predicate,
			IFormatProvider provider, NumberStyles? numberStyles)
		{
			return predicate.ConvertOrDefault<double?>(provider, numberStyles, DateTimeStyles.None, null);
		}

		public static double ToDoubleOrDefault(this object predicate, double defaultValue,
			IFormatProvider provider, NumberStyles? numberStyles)
		{
			return predicate.ConvertOrDefault<double>(defaultValue,
				provider, numberStyles, DateTimeStyles.None, null);
		}

		# endregion

		# region Normalize

		public static double Normalize(this double number, double range)
		{
			return ((number % range) + range) % range;
		} 

		# endregion

		# region Normalize

		public static double Normalize(this double number, double minimum, double maximum)
		{
			number = Math.Min(Math.Max(number, minimum), maximum);
			return number;
		}

		# endregion
	}
}
