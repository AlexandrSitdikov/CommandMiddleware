namespace CommandMiddleware
{
	using System;
	using System.Reflection;

	public abstract class Command
    {
        public Command(string name, bool? allowAnonymous = null, bool? rawResult = null)
        {
            this.Name = name;
            if (rawResult.HasValue)
            {
                this.RawResult = rawResult.Value;
            }

            if (allowAnonymous.HasValue)
            {
                this.AllowAnonymous = allowAnonymous.Value;
            }
        }

        public virtual string Name { get; }

        public virtual bool RawResult { get; }

        public virtual bool AllowAnonymous { get; }

        public Type DtoModel => IsAsync() ? MethodInfo.ReturnType.GetGenericArguments()[0] : MethodInfo.ReturnType;

        // TODO: ApplyInfo(MethodInfo)
        protected abstract MethodInfo MethodInfo { get; }

        public bool IsAsync() => typeof(Task).IsAssignableFrom(MethodInfo.ReturnType);

        public ParameterInfo[] GetParameters() => MethodInfo.GetParameters();

        public abstract object? Invoke(IServiceProvider container, object[] args);
    }
}