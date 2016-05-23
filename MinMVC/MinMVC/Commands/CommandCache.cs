using System.Collections.Generic;

namespace MinMVC
{
	public class CommandCache<T> : ICommandCache where T : class, IBaseCommand, new()
	{
		readonly Queue<IBaseCommand> _cache = new Queue<IBaseCommand>();
		readonly HashSet<IBaseCommand> _retained = new HashSet<IBaseCommand>();
		readonly IContext _context;

		public CommandCache(IContext context, bool init)
		{
			_context = context;

			if(init) {
				IBaseCommand command = Create();
				_cache.Enqueue(command);
			}
		}

		public void Execute()
		{
			var command = (ICommand)Get();
			command.Execute();
			Finish(command);
		}

		public void Execute<TParam>(TParam param)
		{
			var command = (ICommand<TParam>)Get();
			command.Execute(param);
			Finish(command);
		}

		public void Execute<TParam0, TParam1>(TParam0 param0, TParam1 param1)
		{
			var command = (ICommand<TParam0, TParam1>)Get();
			command.Execute(param0, param1);
			Finish(command);
		}

		public void Retain(IBaseCommand command)
		{
			_retained.Add(command);
		}

		public void Finish(IBaseCommand command)
		{
			if(!command.isRetained) {
				_retained.Remove(command);
				_cache.Enqueue(command);
			}
		}

		public void CleanUp()
		{
			_retained.Each(command => command.Cancel());
			_retained.Clear();
			_cache.Clear();
		}

		IBaseCommand Get()
		{
			return _cache.Count > 0 ? _cache.Dequeue() : Create();
		}

		IBaseCommand Create()
		{
			if(!_context.Has<T>()) {
				_context.Register<T>(true);
			}

			IBaseCommand command = _context.Get<T>();
			command.cache = this;

			return command;
		}
	}
}
