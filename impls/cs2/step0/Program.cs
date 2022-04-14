
using System;

namespace uk.osric.mal {
    public class Program {

        private string Read(string input) {
            return input;
        }

        private string Eval(string input) {
            return input;
        }

        private string Print(string input) {
            return input;
        }

        private string Rep(string input) {
            return Print(Eval(Read(input)));
        }

        public static void Main(string[] args) {
            Program mal = new Program();
            while (true) {
                Console.Write("user> ");
                string? input = Console.ReadLine();
                if (input != null) {
                    Console.WriteLine(mal.Rep(input));
                }
            }
        }
    }
}