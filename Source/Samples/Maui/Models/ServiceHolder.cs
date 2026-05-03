namespace MauiSample.Models
{
    public class ServiceHolder(MyService service)
    {
        public MyService Service { get; set; } = service;
    }
}
