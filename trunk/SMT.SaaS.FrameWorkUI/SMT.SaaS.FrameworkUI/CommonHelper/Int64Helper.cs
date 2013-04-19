// (c) Copyright Vitor de Souza (sincorde.com)
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

# region Using Directives

using System.Globalization;

# endregion

namespace System
{
	public static class Int64Helper
	{
		# region Declarations

		private const long _Zero = 0;
		private const long _MinusOne = -1;

		# endregion

		# region ValidateRange

		public static void ValidateRange(this long predicate, long minValue, long maxValue)
		{
			if (predicate < minValue || predicate > maxValue)
			{
				throw new OverflowException("The value is outside of range.");
			}
		}


		# endregion

		# region IsMinusOne

		public static bool IsNotMinusOne(this long predicate)
		{
			return !predicate.IsMinusOne();
		}

		public static bool IsMinusOne(this long predicate)
		{
			return predicate.Equals(Int64Helper._MinusOne);
		} 

		# endregion

		# region ToInt64

		public static long ToInt64(this object predicate)
		{
			return predicate.ToInt64(null, null);
		}

		public static long ToInt64OrMinusOne(this object predicate)
		{
			return predicate.ToInt64OrMinusOne(null, null);
		}

		public static long ToInt64OrMinimum(this object predicate)
		{
			return predicate.ToInt64OrMinimum(null, null);
		}

		public static long ToInt64OrMaximum(this object predicate)
		{
			return predicate.ToInt64OrMaximum(null, null);
		}

		public static long ToInt64OrZero(this object predicate)
		{
			return predicate.ToInt64OrZero(null, null);
		}

		public static long? ToInt64OrNull(this object predicate)
		{
			return predicate.ToInt64OrNull(null, null);
		}

		public static long ToInt64OrDefault(this object predicate, long defaultValue)
		{
			return predicate.ToInt64OrDefault(defaultValue, null, null);
		}

		public static long ToInt64(this object predicate,
			IFormatProvider provider, NumberStyles? numberStyles)
		{
			return predicate.Convert<long>(provider, numberStyles, DateTimeStyles.None, null);
		}

		public static long ToInt64OrMinusOne(this object predicate,
			IFormatProvider provider, NumberStyles? numberStyles)
		{
			return predicate.ToInt64OrDefault(Int64Helper._MinusOne, provider, numberStyles);
		}

		public static long ToInt64OrMinimum(this object predicate,
			IFormatProvider provider, NumberStyles? numberStyles)
		{
			return predicate.ToInt64OrDefault(long.MinValue, provider, numberStyles);
		}

		public static long ToInt64OrMaximum(this object predicate,
			IFormatProvider provider, NumberStyles? numberStyles)
		{
			return predicate.ToInt64OrDefault(long.MaxValue, provider, numberStyles);
		}

		public static long ToInt64OrZero(this object predicate,
			IFormatProvider provider, NumberStyles? numberStyles)
		{
			return predicate.ToInt64OrDefault(Int64Helper._Zero, provider, numberStyles);
		}

		public static long? ToInt64OrNull(this object predicate,
			IFormatProvider provider, NumberStyles? numberStyles)
		{
			return predicate.ConvertOrDefault<long?>(provider, numberStyles, DateTimeStyles.None, null);
		}

		public static long ToInt64OrDefault(this object predicate, long defaultValue,
			IFormatProvider provider, NumberStyles? numberStyles)
		{
			return predicate.ConvertOrDefault<long>(defaultValue,
				provider, numberStyles, DateTimeStyles.None, null);
		}

		# endregion
	}
}
