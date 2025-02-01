namespace CommandMiddleware
{
    using System;
    using System.Reflection;

    internal class DelegateCommand : Command
    {
        private readonly Delegate @delegate;

        public DelegateCommand(string name, Delegate @delegate, bool? allowAnonymous = null, bool? rawResult = null) : base(name, allowAnonymous, rawResult)
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