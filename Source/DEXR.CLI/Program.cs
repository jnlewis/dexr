using System;
using System.Collections.Generic;
using DEXR.Core;
using Newtonsoft.Json;

namespace DEXR.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to DEXR.");
            Console.WriteLine("Enter `help` for list of commands.");

            while (true)
            {
                Console.Write("> ");
                var input = Console.ReadLine();
                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                Commands.Run(input);
            }
        }
    }
}
