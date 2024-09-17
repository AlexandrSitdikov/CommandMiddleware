namespace CommandMiddleware
{
	using System.Data;
	using System.Net;
	using System.Net.Http.Headers;
	using System.Reflection;
	using System.Text.Json;
	using System.Threading.Tasks;

	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.Primitives;

	internal static class CommandMiddleware
    {
        private static object errorObj = new
        {
            success = false
        };

		public static Func<object?, string> Serializer { get; set; } = obj => JsonSerializer.Serialize(obj);

		public static Func<string, Type, object?> DeSerializer { get; set; } = (json, type) => JsonSerializer.Deserialize(json, type);

		public static async Task InvokeAsync(HttpContext context, Func<Task> next)
        {
            var r = context.Request;
            if (r.Path.StartsWithSegments("/api")) // TODO: в настройку
            {
                var path = r.Path.Value?.Substring(5).Split('/'); // "/api/".Length

                var actionName = path.FirstOrDefault(); // /api/commandName
                if (!string.IsNullOrEmpty(actionName) && CommandManager.TryGetCommand(actionName, out var commandInfo))
                {
                    try
                    {
                        Validate(context, commandInfo);

                        var parValues = await GetParametersValues(r, commandInfo);

                        var result = commandInfo.Invoke(context.RequestServices, parValues);
                        if (result is Task task)
                        {
                            await task;

                            result = task.GetType().GetProperty("Result")?.GetValue(task);
                        }

                        if (!commandInfo.RawResult)
                        {
                            result = new { result, success = true };
                        }

                        context.Response.StatusCode = (int)HttpStatusCode.OK;
						context.Response.ContentType = "application/json; charset=utf-8";
						await context.Response.WriteAsync(Serializer(result));
                    }
                    catch (UnauthorizedAccessException)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    }
                    catch (Exception ex)
                    {
                        while (ex?.GetType() == typeof(TargetInvocationException) && ex.InnerException != null)
                        {
                            ex = ex.InnerException;
                        }


                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                        if (ex != null)
                        {
                            //ExceptionHandlerManager.Handle(ref ex);
                            var type = ex.GetType().Name;

                            await context.Response.WriteAsync(Serializer(new
                            {
                                ex.Message,
                                ex.StackTrace,
                                Type = type.EndsWith("Exception") ? type.Substring(0, type.Length - 9) : type,
                                success = false
                            }));
                        }
                        else
                        {
                            await context.Response.WriteAsync(Serializer(errorObj));
                        }
                    }

                    return;
                }
            }

            await next();
        }

        private static async Task<object?[]> GetParametersValues(HttpRequest r, Command commandInfo)
        {
            var parameters = commandInfo.GetParameters();
            var parValues = new object?[parameters.Length];
            if (parameters.Length > 0)
            {
                byte i = 0;

                if (r.Method != "GET" && r.HasJsonContentType())
                {
                    try
                    {
                        using (var reader = new StreamReader(r.Body))
                        {
                            parValues[i++] = DeSerializer(reader.ReadToEnd(), parameters[0].ParameterType);
                        }
                    }
                    catch (System.Text.Json.JsonException ex)
                    {
                        if (!(ex.LineNumber == 0 &&
                            ex.BytePositionInLine == 0 &&
                            parameters[0].ParameterType.IsGenericType &&
                            parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                        {
                            throw;
                        }
                    }
                }

                for (; i < parameters.Length; i++)
                {
                    if (r.Query.TryGetValue(parameters[i].Name!, out var val))
                    {
                        // TODO: Перенести в регистрацию комманд
                        var type = parameters[i].ParameterType;
                        if (!type.TryGetItemType(out var itemType))
                        {
                            parValues[i] = ChangeType(val[0], type);
                        }
                        else
                        {
                            var arr = Array.CreateInstance(itemType!, val.Count);
                            for (var j = 0; j < arr.Length; j++)
                            {
                                arr.SetValue(ChangeType(val[j], itemType!), j);
                            }

                            parValues[i] = arr;
                        }
                    }
                }
            }

            return parValues;
        }

        private static int GetStatusCode(Exception ex)
        {
            switch (ex)
            {
                case UnauthorizedAccessException: return (int)HttpStatusCode.Unauthorized;
                default: return (int)HttpStatusCode.InternalServerError;
            }
        }

        private static void Validate(HttpContext context, Command commandInfo)
        {
            if (!commandInfo.AllowAnonymous && (context.User.Identity?.IsAuthenticated != true))
            {
                throw new UnauthorizedAccessException();
            }
        }

        private static bool TryGetItemType(this Type type, out Type? itemType)
        {
            if (type == typeof(string))
            {
                itemType = null;
                return false;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                itemType = type.GetGenericArguments()[0];
            }
            else
            {
                itemType = type
                    .GetInterfaces()
                    .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    .Select(x => x.GetGenericArguments()[0])
                    .FirstOrDefault();
            }

            return itemType != null;
        }

        private static object? ChangeType(string? val, Type type)
        {
            if (string.IsNullOrEmpty(val))
            {
                return null;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = type.GetGenericArguments()[0];
            }

            if (!type.IsPrimitive && type.IsClass && type != typeof(string))
            {
                return DeSerializer(val, type);
            }
            else
            {
                return Convert.ChangeType(val, type);
            }
        }

		private static bool HasJsonContentType(this HttpRequest request)
		{
			if (request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			if (!MediaTypeHeaderValue.TryParse(request.ContentType, out var mt))
			{
				return false;
			}

			if (mt.MediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}


            var subType = new StringSegment(mt.MediaType);
            subType = subType.Subsegment(subType.IndexOf('/') + 1);

			var startOfSuffix = subType.LastIndexOf('+');
			if (startOfSuffix != -1)
			{
				if (subType.Subsegment(startOfSuffix + 1).Equals("json", StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}
	}
}