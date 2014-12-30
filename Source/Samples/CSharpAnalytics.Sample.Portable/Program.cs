using System;

namespace CSharpAnalytics.Sample.Portable
{
    class Program
    {
        static void Main(string[] args)
        {
            var m = new ManualMeasurement(() => true);
            m.DebugWriter = Console.WriteLine;
            m.Start(new MeasurementConfiguration("UA-319000-8", "console app", "1.0"), "run");

            Console.WriteLine("Sample app using portable library");
            while (true)
            {
                Console.WriteLine("[s] to track screen");
                Console.WriteLine("[e] to track event");
                Console.WriteLine("[q] to quit");
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.KeyChar)
                {
                    case 's':
                        m.Client.TrackScreenView("My Text Screen");
                        break;
                    case 'e':
                        m.Client.TrackEvent("My Action", "My Custom Category", "My Label");
                        break;
                    case 'q':
                        return;
                }
                Console.WriteLine();
            }
        }
    }
}
