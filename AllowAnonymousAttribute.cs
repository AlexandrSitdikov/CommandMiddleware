namespace CommandMiddleware
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AllowAnonymousAttribute : Attribute
    {
    }
}