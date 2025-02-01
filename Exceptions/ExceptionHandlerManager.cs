namespace CommandMiddleware.Exceptions
{
    public static class ExceptionHandlerManager
    {
        private static List<IExceptionHandler> handlers = new List<IExceptionHandler>();

        public static void Register(IExceptionHandler handler)
        {
            handlers.Add(handler);
        }

        public static void Handle(ref Exception exception)
        {
            foreach (var handler in handlers)
            {
                handler.Handle(ref exception);
            }
        }
    }
}