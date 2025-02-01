namespace CommandMiddleware
{
	using System;

	using Microsoft.AspNetCore.Builder;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.DependencyInjection.Extensions;

	public static class Extensions
	{
		public static IServiceCollection RegisterApiService<T>(this IServiceCollection container, string? prefix = null) =>
			container.RegisterApiService(typeof(T), prefix);

		public static IServiceCollection RegisterApiService(this IServiceCollection container, Type type, string? prefix = null)
		{
			container.TryAddScoped(type);

			CommandManager.RegisterApiService(type, prefix);

			return container;
		}

		public static void RegisterApiService<T>(string? prefix = null) =>
			CommandManager.RegisterApiService(typeof(T), prefix);

		public static void UseCommands(this IApplicationBuilder app) => 
			app.Use(CommandMiddleware.InvokeAsync);
	}
}