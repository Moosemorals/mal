using System;

namespace Mal
{
    class Program
    {

        private MalType Read(string input)
        {
            return Reader.ReadStr(input);
        }

        private MalType Eval(MalType input)
        {
            return input;
        }

        private string Print(MalType input)
        {
            return Printer.PrStr(input);
        }

        private string Rep(string input)
        {
            return Print(Eval(Read(input)));
        }

        static void Main(string[] args)
        {
            Program p = new Program();
            while (true)
            {
                Console.Write("user> ");
                string? input = Console.ReadLine();
                if (input == null) {
                    break;
                }
                try {
                    string output = p.Rep(input);
                    Console.WriteLine(output);
                } catch (MalError ex) {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}
