namespace CommandMiddleware.Exceptions
{
    public interface IExceptionHandler
    {
        void Handle(ref Exception exception);
    }
}
