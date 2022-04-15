
using System;
using System.Collections.Generic;
using System.IO;

namespace uk.osric.mal
{
    public class Program
    {

        private readonly Reader reader = new Reader();

        private MalType Read(string input)
        {
            return reader.ReadStr(input);
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

        public static void Main(string[] args)
        {
            Program mal = new Program();
            while (true)
            {
                Console.Write("user> ");
                string? input = Console.ReadLine();
                if (input != null)
                {
                    try
                    {
                        Console.WriteLine(mal.Rep(input));
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"There was a problem {ex.Message}");
                        FormatException(ex, Console.Error);
                    }
                }
            }
        }

        private static void FormatException(Exception ex, TextWriter output)
        {
            List<Exception> exceptions = new();

            Exception? inner = ex;
            while (inner != null)
            {
                exceptions.Add(inner);
                inner = ex.InnerException;
            }

            exceptions.Reverse();

            foreach (Exception e in exceptions) {
                output.WriteLine($"{e.GetType().FullName}: {ex.Message}");
                output.WriteLine(e.StackTrace);
            }
        }
    }
}