using System;
using System.Collections.Generic;

namespace MinMVC
{
	public class TimeSpeedo
	{
		readonly Stack<int> stack = new Stack<int>();
		readonly IDictionary<int, float> map = new Dictionary<int, float>();

		int currentId = 0;

		public int NextId { get { return currentId++; } }

		public Func<float> TimeProvider { private get; set; }

		public TimeSpeedo ()
		{
		}

		public TimeSpeedo (Func<float> provider)
		{
			TimeProvider = provider;
		}

		public int Start (bool stacked = true)
		{
			var id = NextId;

			if (stacked) {
				stack.Push(id);
			}

			map[id] = TimeProvider();

			return id;
		}

		public float GetResult (int id)
		{
			var value = map[id];
			var current = TimeProvider();

			return current - value;
		}

		public float Stop ()
		{
			var id = stack.Pop();

			return Stop(id);
		}

		public float Stop (int id)
		{
			var result = GetResult(id);
			map.Remove(id);

			return result;
		}
	}
}
