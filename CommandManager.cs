namespace CommandMiddleware
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static class CommandManager
	{
		private static Dictionary<string, Command> commands = new();

		public static bool TryGetCommand(string name, out Command? commandInfo) =>
			commands.TryGetValue(name.ToLower(), out commandInfo);

		public static void RegisterApiService(Type type, string? prefix = null)
		{
			foreach (var mtd in type.GetMethods()
				.Where(x => x.DeclaringType != typeof(object) && !x.IsDefined(typeof(NotCommandAttribute), false))
				.Reverse())
			{
				commands[(prefix + mtd.Name).ToLower()] = new ServiceCommand(mtd.Name, type, mtd);
			}
		}

		public static void Register(string name, Delegate @delegate, bool? allowAnonymous = null, bool? rawResult = null)
		{
			commands[name.ToLower()] = new DelegateCommand(name, @delegate, allowAnonymous, rawResult);
		}
	}
}