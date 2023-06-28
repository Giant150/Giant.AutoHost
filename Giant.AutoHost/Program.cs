namespace Giant.AutoHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices(services =>
                {
                    services.AddHttpClient();
                    services.AddHostedService<Worker>();
                })
                .Build();

            host.Run();
        }
    }
}