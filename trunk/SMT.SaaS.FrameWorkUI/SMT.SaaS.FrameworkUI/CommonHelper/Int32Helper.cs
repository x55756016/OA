// (c) Copyright Vitor de Souza (sincorde.com)
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

# region Using Directives

using System.Globalization;

# endregion

namespace System
{
	public static class Int32Helper
	{
		# region Declarations

		private const int _Zero = 0;
		private const int _MinusOne = -1;

		# endregion

		# region ValidateRange

		public static void ValidateRange(this int predicate, int minValue, int maxValue)
		{
			if (predicate < minValue || predicate > maxValue)
			{
				throw new OverflowException("The value is outside of range.");
			}
		}

		# endregion

		# region IsMinusOne

		public static bool IsNotMinusOne(this int predicate)
		{
			return !predicate.IsMinusOne();
		}

		public static bool IsMinusOne(this int predicate)
		{
			return predicate.Equals(Int32Helper._MinusOne);
		} 

		# endregion

		# region ToInt32

		public static int ToInt32(this object predicate)
		{
			return predicate.ToInt32(null, null);
		}

		public static int ToInt32OrMinusOne(this object predicate)
		{
			return predicate.ToInt32OrMinusOne(null, null);
		}

		public static int ToInt32OrMinimum(this object predicate)
		{
			return predicate.ToInt32OrMinimum(null, null);
		}

		public static int ToInt32OrMaximum(this object predicate)
		{
			return predicate.ToInt32OrMaximum(null, null);
		}

		public static int ToInt32OrZero(this object predicate)
		{
			return predicate.ToInt32OrZero(null, null);
		}

		public static int? ToInt32OrNull(this object predicate)
		{
			return predicate.ToInt32OrNull(null, null);
		}

		public static int ToInt32OrDefault(this object predicate, int defaultValue)
		{
			return predicate.ToInt32OrDefault(defaultValue, null, null);
		}

		public static int ToInt32(this object predicate,
			IFormatProvider provider, NumberStyles? numberStyles)
		{
			return predicate.Convert<int>(provider, numberStyles, DateTimeStyles.None, null);
		}

		public static int ToInt32OrMinusOne(this object predicate,
			IFormatProvider provider, NumberStyles? numberStyles)
		{
			return predicate.ToInt32OrDefault(Int32Helper._MinusOne, provider, numberStyles);
		}

		public static int ToInt32OrMinimum(this object predicate,
			IFormatProvider provider, NumberStyles? numberStyles)
		{
			return predicate.ToInt32OrDefault(int.MinValue, provider, numberStyles);
		}

		public static int ToInt32OrMaximum(this object predicate,
			IFormatProvider provider, NumberStyles? numberStyles)
		{
			return predicate.ToInt32OrDefault(int.MaxValue, provider, numberStyles);
		}

		public static int ToInt32OrZero(this object predicate,
			IFormatProvider provider, NumberStyles? numberStyles)
		{
			return predicate.ToInt32OrDefault(Int32Helper._Zero, provider, numberStyles);
		}

		public static int? ToInt32OrNull(this object predicate,
			IFormatProvider provider, NumberStyles? numberStyles)
		{
			return predicate.ConvertOrDefault<int?>(provider, numberStyles, DateTimeStyles.None, null);
		}

		public static int ToInt32OrDefault(this object predicate, int defaultValue,
			IFormatProvider provider, NumberStyles? numberStyles)
		{
			return predicate.ConvertOrDefault<int>(defaultValue,
				provider, numberStyles, DateTimeStyles.None, null);
		}

		# endregion

		# region Normalize

		public static int Normalize(this int number, int range)
		{
			return ((number % range) + range) % range;
		} 

		# endregion

		# region Normalize

		public static int Normalize(this int number, int minimum, int maximum)
		{
			number = Math.Min(Math.Max(number, minimum), maximum);
			return number;
		}

		# endregion
	}
}
