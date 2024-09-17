namespace CommandMiddleware
{
	using System;
	using System.Reflection;

	internal class ServiceCommand : Command
    {
        private readonly Type serviceType;

        public ServiceCommand(string name, Type serviceType, MethodInfo methodInfo, string? prefix = null) 
            : base(
                  name, 
                  allowAnonymous: methodInfo.IsDefined(typeof(AllowAnonymousAttribute), false) || serviceType.IsDefined(typeof(AllowAnonymousAttribute), false),
                  rawResult: methodInfo.IsDefined(typeof(RawResultAttribute), false))
        {
            this.MethodInfo = methodInfo;
            this.serviceType = serviceType;
            this.Prefix = prefix;
        }

        public string? Prefix { get; }

        protected override MethodInfo MethodInfo { get; }

        public override object? Invoke(IServiceProvider container, object[] args)
        {
            var service = container.GetService(serviceType);
            return MethodInfo.Invoke(service, args);
        }
    }
}