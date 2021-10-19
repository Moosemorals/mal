using System;

namespace step0_repl
{
    class Program
    {

        private string Read(string input)
        {
            return input;
        }

        private string Eval(string input)
        {
            return input;
        }

        private string Print(string input)
        {
            return input;
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
                Console.WriteLine("user> ");
                string input = Console.ReadLine();
                Console.WriteLine(p.Rep(input));
            }
        }
    }
}
