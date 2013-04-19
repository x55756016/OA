// (c) Copyright Vitor de Souza (sincorde.com)
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace System
{
	public static class EnumHelper
	{
		public static TEnum ToEnum<TEnum>(this object predicate)
		{
			return predicate.Convert<TEnum>();
		}
		
		public static TEnum? ToEnumOrNull<TEnum>(this object predicate) where TEnum : struct
		{
			return predicate.ConvertOrDefault<TEnum?>(null);
		}

		public static TEnum ToEnumOrDefault<TEnum>(this object predicate)
		{
			return predicate.ConvertOrDefault<TEnum>(default(TEnum));
		}

		public static TEnum ToEnumOrDefault<TEnum>(this object predicate, TEnum defaultValue)
		{
			return predicate.ConvertOrDefault<TEnum>(defaultValue);
		}

		public static bool Contains<TEnum>(this TEnum predicate, TEnum value) where TEnum : struct
		{
			var full = predicate.ToInt64();

			var contains = (full | value.ToInt64()) == full;
			return contains;
		}
	}
}
