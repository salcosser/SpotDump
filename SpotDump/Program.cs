using SpotDump.ClientApi;

namespace SpotDump {
    internal class Program {
        static void Main(string[] args) {
            Do_Main();
        }

        protected static void Do_Main() {
            Console.WriteLine("Hello, World! Time to do the thing!\n press any key to start...");
            Console.ReadLine();

            var baseClient = new SpotDumpClient();

            var rp = Task.Run(() => baseClient.GetGeneresAsync());
            rp.Wait();
            if (rp.Result != null) {
                Console.WriteLine(string.Join(", ", rp.Result));
            } else {
                Console.WriteLine("Something broke...");
            }

            Console.WriteLine("End");
            Console.ReadLine();
        }
    }
}
