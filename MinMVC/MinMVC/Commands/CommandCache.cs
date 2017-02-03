using System.Collections.Generic;

namespace MinMVC
{
	public class CommandCache<T> : ICommandCache where T : class, IBaseCommand, new()
	{
		readonly Queue<IBaseCommand> cache = new Queue<IBaseCommand>();
		readonly IList<IBaseCommand> retained = new List<IBaseCommand>();
		readonly ICommands commands;

		public CommandCache (ICommands coms)
		{
			commands = coms;
		}

		public void Execute ()
		{
			var command = Get<ICommand>();
			command.Execute();
			Finish(command);
		}

		public void Execute<TParam> (TParam param)
		{
			var command = Get<ICommand<TParam>>();
			command.Execute(param);
			Finish(command);
		}

		public void Execute<TParam0, TParam1> (TParam0 param0, TParam1 param1)
		{
			var command = Get<ICommand<TParam0, TParam1>>();
			command.Execute(param0, param1);
			Finish(command);
		}

		public TParam Request<TParam> ()
		{
			var command = Get<ICommand<TParam>>();
			var result = command.Request();
			Finish(command);

			return result;
		}

		public TParam0 Request<TParam0, TParam1> (TParam1 param1)
		{
			var command = Get<ICommand<TParam0, TParam1>>();
			var result = command.Request(param1);
			Finish(command);

			return result;
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
			for (int i = 0; i < retained.Count; i++) {
				retained[i].Cancel();
			}

			retained.Clear();
			cache.Clear();
		}

		TCom Get<TCom> ()
		{
			var command = cache.Count > 0 ? cache.Dequeue() : Create();

			return (TCom)command;
		}

		IBaseCommand Create ()
		{
			var command = commands.GetCommand<T>();
			command.cache = this;

			return command;
		}
	}
}
