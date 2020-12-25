using System.Threading;

namespace SpicyReader
{
    class Program
    {
        static void Main(string[] args)
        {
            SpicyReader reader = new SpicyReader();
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            reader.OpenPcapNGFile(@"C:\2222222222222.pcapng", false, cancellationTokenSource.Token);
        }
    }
}
