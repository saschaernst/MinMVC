using System;

namespace MinMVC
{
	public class NotRegistered : Exception
	{
		public NotRegistered (string message) : base(message)
		{
		}
	}

	public class AlreadyRegistered : Exception
	{
		public AlreadyRegistered (string message) : base(message)
		{
		}
	}

	public class CannotRegisterInterfaceAsValue : Exception
	{
		public CannotRegisterInterfaceAsValue (string message) : base(message)
		{
		}
	}
}
