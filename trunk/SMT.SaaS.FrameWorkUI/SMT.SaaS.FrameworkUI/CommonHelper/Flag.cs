// (c) Copyright Vitor de Souza (sincorde.com)
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace System
{
	public static class Flag
	{
		# region Enum Methods

		public static IEnumerable<object> GetFlags(this Enum value)
		{
			var type = value.GetType();
			var flagged = type.GetCustomAttributes(typeof(FlagsAttribute), false).Any();

			if (flagged)
			{
				# if SILVERLIGHT || PocketPC

				var integerValue = value.ToInt32();

				var fields = type.GetFields().Skip(1);

				foreach (var field in fields)
				{
					var itemValue = field.GetValue(value).ToInt32();

					if (itemValue > 0 && (itemValue & integerValue) == itemValue)
					{
						yield return field.GetFlagOrNull();
					}
				}

				# else

				throw new NotImplementedException();

				//string result = null;

				//var integerValue = value.ToInt32();

				//foreach (Enum item in Enum.GetValues(type))
				//{
				//    var itemValue = Convert.ToInt32(item);

				//    if (itemValue > 0 && (itemValue & integerValue) == itemValue)
				//    {
				//        result += item.GetFlag(type, false) + ", ";
				//    }
				//}

				//return result.Length > 0 ? result.Substring(0, result.Length - 2) : result;

				# endif
			}
			else
			{
				var fieldName = value.ToString();

				var field = type.GetField(fieldName);

				yield return field.GetFlagOrNull();
			}
		}

		# endregion

		# region FieldInfo HasFlag

		public static bool HasFlag(this FieldInfo fieldInfo, object value)
		{
			var flag = fieldInfo.GetFlagOrNull();

			return flag != null && flag.Equals(value);
		}

		# endregion

		# region FieldInfo GetFlag

		//public static object GetFlagOrName(this FieldInfo fieldInfo)
		//{
		//    var attrib = fieldInfo.GetFlagOrNull();

		//    return attrib == null ? fieldInfo.Name : attrib.Value;
		//}
		public static object GetFlagOrNull(this FieldInfo fieldInfo)
		{
			var attrib = fieldInfo.GetCustomAttributes(typeof(FlagAttribute), false)
				.FirstOrDefault() as FlagAttribute;

			return attrib.IsNull() ? null : attrib.Value;
		}

		# endregion

		# region // String.ParseToEnum

		//public static TEnum ParseToEnum<TEnum>(this object predicate) where TEnum : struct
		//{
		//    var tEnum = predicate.ParseToEnumOrDefault<TEnum>();

		//    if (tEnum.Equals(default(TEnum)))
		//    {
		//        throw new NullReferenceException(
		//            "Cannot convert the string to the specified Enum type.");
		//    }

		//    return tEnum;
		//}
		//public static TEnum ParseToEnumOrDefault<TEnum>(this object predicate) where TEnum : struct
		//{
		//    return predicate.ParseToEnumOrDefault<TEnum>(default(TEnum));
		//}

		//public static TEnum ParseToEnumOrDefault<TEnum>(this object predicate, TEnum defaultValue) where TEnum : struct
		//{
		//    if (!typeof(TEnum).IsEnum)
		//    {
		//        throw new InvalidOperationException(
		//            "This method is intended to translate objects of System.Enum type.");
		//    }

		//    if (predicate == null)
		//    {
		//        return defaultValue;
		//    }

		//    var name = predicate.ToString();

		//    if (name.Contains(','))
		//    {
		//        throw new NotImplementedException(
		//            "Multiple values translation was not implemented.");
		//    }

		//    var type = typeof(TEnum);

		//    var fieldInfo = type.GetFields()
		//        .FirstOrDefault(field => field.Name.Equals(name,
		//            StringComparison.OrdinalIgnoreCase) || field.HasTranslation(name));

		//    return fieldInfo == null ? defaultValue : (TEnum)Enum.Parse(type, fieldInfo.Name, true);
		//}

		# endregion
	}
}
