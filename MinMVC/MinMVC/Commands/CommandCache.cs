using System.Collections.Generic;

namespace MinMVC
{
	public class CommandCache<T> : ICommandCache where T : class, IBaseCommand, new()
	{
		readonly Queue<IBaseCommand> cache = new Queue<IBaseCommand>();
		readonly IList<IBaseCommand> retained = new List<IBaseCommand>();
		readonly IContext context;

		public CommandCache (IContext con)
		{
			context = con;
		}

		public void Execute ()
		{
			var command = (ICommand)Get();
			command.Execute();
			Finish(command);
		}

		public void Execute<TParam> (TParam param)
		{
			var command = (ICommand<TParam>)Get();
			command.Execute(param);
			Finish(command);
		}

		public void Execute<TParam0, TParam1> (TParam0 param0, TParam1 param1)
		{
			var command = (ICommand<TParam0, TParam1>)Get();
			command.Execute(param0, param1);
			Finish(command);
		}

		public void Retain (IBaseCommand command)
		{
			retained.Add(command);
		}

		public void Finish (IBaseCommand command)
		{
			if (!command.isRetained) {
				retained.Remove(command);
				cache.Enqueue(command);
			}
		}

		public void CleanUp ()
		{
			retained.For(command => command.Cancel());
			retained.Clear();
			cache.Clear();
		}

		IBaseCommand Get ()
		{
			return cache.Count > 0 ? cache.Dequeue() : Create();
		}

		IBaseCommand Create ()
		{
			if (!context.Has<T>()) {
				context.Register<T>(true);
			}

			IBaseCommand command = context.Get<T>();
			command.cache = this;

			return command;
		}
	}
}
