// (c) Copyright Vitor de Souza (sincorde.com)
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Linq;
using System.Reflection;

namespace System
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
	public sealed class FlagAttribute : Attribute
	{
		# region Constructor

		public FlagAttribute(object value)
		{
			this.Value = value;
		} 

		# endregion

		# region Value

		public object Value { get; private set; } 

		# endregion
	}
}
