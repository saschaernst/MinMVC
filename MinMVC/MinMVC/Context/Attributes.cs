using System;

namespace MinMVC
{
	[AttributeUsage(AttributeTargets.Class)]
	public class InitAsync : Attribute
	{

	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class Inject : Attribute
	{

	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class Access : Attribute
	{

	}

	[AttributeUsage(AttributeTargets.Method)]
	public class PostInjection : Attribute
	{

	}

	[AttributeUsage(AttributeTargets.Method)]
	public class Cleanup : Attribute
	{

	}
}
