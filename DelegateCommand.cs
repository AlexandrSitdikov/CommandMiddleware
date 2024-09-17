namespace CommandMiddleware
{
	using System;
	using System.Reflection;

	internal class DelegateCommand : Command
    {
        private readonly Delegate @delegate;

        public DelegateCommand(string name, Delegate @delegate) : base(name)
        {
            this.@delegate = @delegate;
        }

        protected override MethodInfo MethodInfo => @delegate.Method;

        public override object? Invoke(IServiceProvider container, object[] args)
        {
            return @delegate.DynamicInvoke(args);
        }
    }
}