namespace CommandMiddleware
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.DependencyInjection.Extensions;

	public static class CommandManager
    {
        private static Dictionary<string, Command> commands = new();

        public static bool TryGetCommand(string name, out Command commandInfo) =>
            commands.TryGetValue(name.ToLower(), out commandInfo);

        public static IServiceCollection RegisterApiService<T>(this IServiceCollection container, string? prefix = null) =>
			container.RegisterApiService(typeof(T), prefix);

        public static IServiceCollection RegisterApiService(this IServiceCollection container, Type type, string? prefix = null)
        {
			container.TryAddScoped(type);

			RegisterApiService(type, prefix);

			return container;
        }

		public static void RegisterApiService<T>(string? prefix = null) =>
			CommandManager.RegisterApiService(typeof(T), prefix);

		public static void RegisterApiService(Type type, string? prefix = null)
		{
			foreach (var mtd in type.GetMethods()
				.Where(x => x.DeclaringType != typeof(object) && !x.IsDefined(typeof(NotCommandAttribute), false))
				.Reverse())
			{
				commands[(prefix + mtd.Name).ToLower()] = new ServiceCommand(mtd.Name, type, mtd);
			}
		}

		public static void Register(string name, Delegate @delegate)
        {
            commands[name.ToLower()] = new DelegateCommand(name, @delegate);
        }
    }
}