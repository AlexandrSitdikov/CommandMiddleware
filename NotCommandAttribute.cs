namespace CommandMiddleware
{
    using System;

    [AttributeUsage(AttributeTargets.Method)]
    public class NotCommandAttribute : Attribute
    {
    }
}