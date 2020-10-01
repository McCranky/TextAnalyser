using System;

namespace TextAnalyser
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Nothing to analyse.");
                Console.WriteLine("Run me with arguments: text [-wf] [-cf]");
                Console.WriteLine("\t-wf (word frequencies)");
                Console.WriteLine("\t-cf (character frequencies)");
                return;
            }

            Analyser analyser = new Analyser(args[0]);
            analyser.PrintAnalyses(TryGetSwitch("-cf", ref args), TryGetSwitch("-wf", ref args));
        }

        private static bool TryGetSwitch(string wantedSwitch, ref string [] args)
        {
            foreach (var item in args)
            {
                if (item == wantedSwitch)
                {
                    return true;
                }
            }
            return false;
        }
    }
}