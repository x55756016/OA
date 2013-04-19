// (c) Copyright Vitor de Souza (sincorde.com)
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace System
{
	public static class TypeHelper
	{
		# region Instantiated Types

		public static readonly Type Byte = typeof(byte);
		public static readonly Type SByte = typeof(sbyte);
		public static readonly Type UInt16 = typeof(ushort);
		public static readonly Type Int16 = typeof(short);
		public static readonly Type UInt32 = typeof(uint);
		public static readonly Type Int32 = typeof(int);
		public static readonly Type UInt64 = typeof(ulong);
		public static readonly Type Int64 = typeof(long);
		//public static readonly Type BigInt = typeof(BigInt);
		public static readonly Type Single = typeof(float);
		public static readonly Type Double = typeof(double);
		public static readonly Type Decimal = typeof(decimal);
		public static readonly Type Boolean = typeof(bool); 
		public static readonly Type DateTime = typeof(DateTime);
		public static readonly Type Char = typeof(char);
		public static readonly Type String = typeof(string); 

		# endregion

		# region IsNullableValueType

		public static bool IsNullableValueType(this Type predicate)
		{
			return predicate.IsGenericTypeDefinition &&
				predicate.GetGenericTypeDefinition() == typeof(Nullable<>);
		} 

		# endregion

		# region IsNumeric

		public static bool IsNumeric(this Type predicate)
		{
			return predicate.In(
				TypeHelper.Byte,
				TypeHelper.SByte,
				TypeHelper.UInt16,
				TypeHelper.Int16,
				TypeHelper.UInt32,
				TypeHelper.Int32,
				TypeHelper.UInt64,
				TypeHelper.Int64,
				//TypeHelper.Bigint,
				TypeHelper.Single,
				TypeHelper.Double,
				TypeHelper.Decimal
				);
		} 

		# endregion

		# region IsInteger

		public static bool IsInteger(this Type predicate)
		{
			return predicate.In(
				TypeHelper.Byte,
				TypeHelper.SByte,
				TypeHelper.UInt16,
				TypeHelper.Int16,
				TypeHelper.UInt32,
				TypeHelper.Int32,
				TypeHelper.UInt64,
				TypeHelper.Int64
				//TypeHelper.Bigint
				);
		} 

		# endregion

		# region IsFractionary

		public static bool IsFractionary(this Type predicate)
		{
			return predicate.In(
				TypeHelper.Single,
				TypeHelper.Double,
				TypeHelper.Decimal
				);
		} 

		# endregion

		# region IsBoolean

		public static bool IsBoolean(this Type predicate)
		{
			return predicate.Equals(TypeHelper.Boolean);
		}

		# endregion

		# region IsSByte

		public static bool IsSByte(this Type predicate)
		{
			return predicate.Equals(TypeHelper.SByte);
		}

		# endregion

		# region IsByte

		public static bool IsByte(this Type predicate)
		{
			return predicate.Equals(TypeHelper.Byte);
		}

		# endregion

		# region IsUInt16

		public static bool IsUInt16(this Type predicate)
		{
			return predicate.Equals(TypeHelper.UInt16);
		}

		# endregion

		# region IsInt16

		public static bool IsInt16(this Type predicate)
		{
			return predicate.Equals(TypeHelper.Int16);
		}

		# endregion

		# region IsUInt32

		public static bool IsUInt32(this Type predicate)
		{
			return predicate.Equals(TypeHelper.UInt32);
		}

		# endregion

		# region IsInt32

		public static bool IsInt32(this Type predicate)
		{
			return predicate.Equals(TypeHelper.Int32);
		}

		# endregion

		# region IsUInt64

		public static bool IsUInt64(this Type predicate)
		{
			return predicate.Equals(TypeHelper.UInt64);
		}

		# endregion

		# region IsInt64

		public static bool IsInt64(this Type predicate)
		{
			return predicate.Equals(TypeHelper.Int64);
		}

		# endregion

		# region IsSingle

		public static bool IsSingle(this Type predicate)
		{
			return predicate.Equals(TypeHelper.Single);
		}

		# endregion

		# region IsDouble

		public static bool IsDouble(this Type predicate)
		{
			return predicate.Equals(TypeHelper.Double);
		}

		# endregion

		# region IsDecimal

		public static bool IsDecimal(this Type predicate)
		{
			return predicate.Equals(TypeHelper.Decimal);
		}

		# endregion

		# region IsChar

		public static bool IsChar(this Type predicate)
		{
			return predicate.Equals(TypeHelper.Char);
		}

		# endregion

		# region IsString

		public static bool IsString(this Type predicate)
		{
			return predicate.Equals(TypeHelper.String);
		}

		# endregion

		# region IsDateTime

		public static bool IsDateTime(this Type predicate)
		{
			return predicate.Equals(TypeHelper.DateTime);
		}

		# endregion
	}
}
