namespace H3WebServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting HTTP listener...");

            var httpServer = new Server();
            httpServer.Start();
            while(true)
            {

            }
        }
    }
}