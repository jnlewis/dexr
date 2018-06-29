using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace DEXR.UnitTest
{
    class UnitTest
    {
        static void Main(string[] args)
        {
            RunSimulationTest();
        }

        private static void RunSimulationTest()
        {
            try
            {
                SimulationTest sim = new SimulationTest();
                sim.Run();
            }
            catch (Exception ex)
            {
                Log("Error:" + ex.Message);
            }

            Console.ReadLine();
        }

        private static void RunStressTest()
        {
            Stopwatch st = new Stopwatch();
            st.Start();

            try
            {
                StressTest test = new StressTest(null);
                test.RunEmitTransactions();
            }
            catch (Exception ex)
            {
                Log("Error:" + ex.Message);
            }

            st.Stop();

            Log("Test Run Completed.");
            Log("Elapsed: " + st.Elapsed.ToString());

            Console.ReadLine();
        }
        
        private static void Log(string message)
        {
            Console.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + message);
        }

        private static void OutputToFile(string fileName, string message)
        {
            System.IO.File.WriteAllText(fileName, message);
        }
    }
}
