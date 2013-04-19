// (c) Copyright Vitor de Souza (sincorde.com)
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

# region Using Directives

using System.Linq;
using System.Globalization;
using System.Reflection;
using System.Collections.Generic;

# endregion

namespace System
{
	public static class ConvertHelper
	{
		# region Convert

        public static object Convert(this object predicate, Type type)
        {
            try
            {
                MethodInfo methodConvert = typeof(ConvertHelper).GetMethods(System.Reflection.BindingFlags.Static
                    | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod).Where(method
                    =>
                    {
                        return method.Name == "Convert" && method.IsGenericMethod;
                    }).FirstOrDefault();

                object value = methodConvert.MakeGenericMethod(type).Invoke(null, new object[] { predicate });
                return value;
            }
            catch 
            {
                return default(object);
            }

        }
		public static T Convert<T>(this object predicate)
		{
			return predicate.Convert<T>(null, null, DateTimeStyles.None, null);
		}

		public static T ConvertOrDefault<T>(this object predicate, T defaultValue)
		{
			return predicate.ConvertOrDefault<T>(defaultValue,
				null, null, DateTimeStyles.None, null);
		}

		internal static T Convert<T>(this object predicate,
			IFormatProvider provider, NumberStyles? numberStyles,
			DateTimeStyles datetimeStyles, params string[] dateTimeExpectedformats)
		{
			var convertType = typeof(T);

			var result = predicate.ConvertOrNull(convertType, provider,
				numberStyles, datetimeStyles, dateTimeExpectedformats);

			if (result.IsNull())
			{
                //throw new ConvertException(string.Format(
                //    "Couldn't convert to a valid value of Type {0}", convertType.Name));
                return default(T);
			}

			return (T)result;
		}


		internal static T ConvertOrDefault<T>(this object predicate,
			IFormatProvider provider, NumberStyles? numberStyles,
			DateTimeStyles datetimeStyles, params string[] dateTimeExpectedformats)
		{
			return predicate.ConvertOrDefault<T>(default(T),
				provider, numberStyles, datetimeStyles, dateTimeExpectedformats);
		}

		internal static T ConvertOrDefault<T>(this object predicate, T defaultValue,
			IFormatProvider provider, NumberStyles? numberStyles,
			DateTimeStyles datetimeStyles, params string[] dateTimeExpectedformats)
		{
			return (T)predicate.ConvertOrDefault(typeof(T), defaultValue,
				provider, numberStyles, datetimeStyles, dateTimeExpectedformats);
		}

		internal static object ConvertOrNull(this object predicate,
			Type convertType,
			IFormatProvider provider, NumberStyles? numberStyles,
			DateTimeStyles datetimeStyles, params string[] dateTimeExpectedformats)
		{
			return predicate.ConvertOrDefault(convertType, null, provider,
				numberStyles, datetimeStyles, dateTimeExpectedformats);
		}

		# region Main entrance

		private static object ConvertOrDefault(this object predicate,
			Type convertType, object defaultValue,
			IFormatProvider provider, NumberStyles? numberStyles,
			DateTimeStyles dateTimeStyles, params string[] dateTimeExpectedFormats)
		{
			# region Initializations

			if (predicate.IsNull()) // || predicate == DBNull.Value)
			{
				return defaultValue;
			}

			convertType = Nullable.GetUnderlyingType(convertType) ?? convertType;

			# endregion

			# region TryParse

			if (convertType.IsDateTime())
			{
				if (provider.IsNull())
				{
					provider = DateTimeFormatInfo.CurrentInfo;
				}

				DateTime dateTime;

				if (dateTimeExpectedFormats.IsNull())
				{
					if (DateTime.TryParse(predicate.ToString(), provider, dateTimeStyles, out dateTime))
					{
						return dateTime;
					}
				}
				else
				{
					if (DateTime.TryParseExact(predicate.ToString(),
						dateTimeExpectedFormats, provider, dateTimeStyles, out dateTime))
					{
						return dateTime;
					}
				}
			}
			else if (convertType.IsInteger())
			{
				if (provider.IsNull())
				{
					provider = NumberFormatInfo.CurrentInfo;
				}

				if (numberStyles.IsNull())
				{
					numberStyles = NumberStyles.Integer;
				}

				if (convertType.IsSByte())
				{
					sbyte sbyteValue;

					if (sbyte.TryParse(predicate.ToString(), numberStyles.Value, provider, out sbyteValue))
					{
						return sbyteValue;
					}
				}
				else if (convertType.IsByte())
				{
					byte byteValue;

					if (byte.TryParse(predicate.ToString(), numberStyles.Value, provider, out byteValue))
					{
						return byteValue;
					}
				}
				else if (convertType.IsUInt16())
				{
					ushort ushortValue;

					if (ushort.TryParse(predicate.ToString(), numberStyles.Value, provider, out ushortValue))
					{
						return ushortValue;
					}
				}
				else if (convertType.IsInt16())
				{
					short shortValue;

					if (short.TryParse(predicate.ToString(), numberStyles.Value, provider, out shortValue))
					{
						return shortValue;
					}
				}
				else if (convertType.IsUInt32())
				{
					uint uintValue;

					if (uint.TryParse(predicate.ToString(), numberStyles.Value, provider, out uintValue))
					{
						return uintValue;
					}
				}
				else if (convertType.IsInt32())
				{
					int intValue;

					if (int.TryParse(predicate.ToString(), numberStyles.Value, provider, out intValue))
					{
						return intValue;
					}
				}
				else if (convertType.IsUInt64())
				{
					ulong ulongValue;

					if (ulong.TryParse(predicate.ToString(), numberStyles.Value, provider, out ulongValue))
					{
						return ulongValue;
					}
				}
				else if (convertType.IsInt64())
				{
					long longValue;

					if (long.TryParse(predicate.ToString(), numberStyles.Value, provider, out longValue))
					{
						return longValue;
					}
				}

			}
			else if (convertType.IsFractionary())
			{
				if (provider.IsNull())
				{
					provider = NumberFormatInfo.CurrentInfo;
				}

				if (numberStyles.IsNull())
				{
					numberStyles = NumberStyles.Float | NumberStyles.AllowThousands;
				}

				if (convertType.IsSingle())
				{
					float floatValue;

					if (float.TryParse(predicate.ToString(), numberStyles.Value, provider, out floatValue))
					{
						return floatValue;
					}
				}
				else if (convertType.IsDouble())
				{
					double doubleValue;

					if (double.TryParse(predicate.ToString(), numberStyles.Value, provider, out doubleValue))
					{
						return doubleValue;
					}
				}
				else if (convertType.IsDecimal())
				{
					decimal decimalValue;

					if (decimal.TryParse(predicate.ToString(), numberStyles.Value, provider, out decimalValue))
					{
						return decimalValue;
					}
				}
			}

			# endregion

			# region Enum

			if (convertType.IsEnum)
			{
				var predicateType = predicate.GetType();

				if (predicateType.IsNumeric())
				{
					var value = System.Convert.ToInt32(predicate);

					return Enum.IsDefined(convertType, value) ? value : defaultValue;
				}
				else
				{
					var name = predicate.ToString();

					if (name.Contains(','))
					{
						throw new NotImplementedException(
							"Multiple values translation was not implemented.");
					}

					try
					{
						var value = Enum.Parse(convertType, name, true);

						if (Enum.IsDefined(convertType, value))
						{
							return value;	
						}
					}
					catch
					{
					}

					var fieldInfo = convertType.GetFields()
						.FirstOrDefault(field => field.HasFlag(name));

					return fieldInfo.IsNull() ? defaultValue : Enum.Parse(convertType, fieldInfo.Name, true);
				}
			}

			# endregion

			# region IConvertible

			if (predicate is IConvertible)
			{
				try
				{
					return System.Convert.ChangeType(predicate, convertType, null);
				}
				catch
				{
				}
			}

			# endregion

			# region Cast

			/*
			try
			{
				foreach (var methodInfo in type.GetMethods()
				.Where(m => m.Name.Equals("op_Implicit") || m.Name.Equals("op_Explicit")))
				{
					Console.WriteLine();
					Console.WriteLine(methodInfo.Name);
					Console.WriteLine("\tresult type:" + methodInfo.ReturnType.Name);

					foreach (var param in methodInfo.GetParameters())
					{
						Console.Write("\t" + param.Name);
						Console.Write(", " + param.ParameterType.Name);

					}

				}
			}
			catch
			{
			} 
			*/

			# endregion

			return defaultValue;
		}  

		# endregion

		# endregion
	}
}
