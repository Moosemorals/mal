

using System;
using System.Collections.Generic;
using System.IO;

namespace uk.osric.mal {

    public class Readline {

        private const string ESC = "\u001b";
        private const string CSI = ESC + "\u005B";

        private const string CursorRight = CSI + "1C";
        private const string CursorLeft = CSI + "1D";
        private const string Delete = CSI + "1P";
        private const string Backspace = CursorLeft + Delete;
        private const string ClearLine = CSI + "2K";

        private static string SetCursorX(int n) => $"{CSI}{n}G";

        private readonly List<string> history = new();


        public string? WaitForInput(string Prompt, bool basic) {
            if (basic) {
                Console.Out.Write(Prompt);
                return Console.In.ReadLine();
            } else {
                return WaitForInput(Prompt);
            }
        }

        public string WaitForInput(string Prompt) {
            TextWriter Out = Console.Out;
            string display = "";

            int cursorX = 0;
            int cursorY = 0;

            while (true) {
                // Clear the line, draw the prompt and the user inpit
                Out.Write(ClearLine + SetCursorX(1) + Prompt + display);
                // And move the cursor into position
                Out.Write(SetCursorX(Prompt.Length + cursorX + 1));

                // Wait for (and react to) user key
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.Key) {
                    case ConsoleKey.UpArrow:
                        if (cursorY  + 1 < history.Count) {


                        }
                        break;
                    case ConsoleKey.LeftArrow:
                        if (cursorX > 0) {
                            cursorX -= 1;
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        if (cursorX + 1 < display.Length) {
                            cursorX += 1;
                        }
                        break;
                    case ConsoleKey.Backspace:
                        if (cursorX - 1 > 0) {
                            cursorX -= 1;
                            display = display.Remove(cursorX, 1);
                        }
                        break;
                    case ConsoleKey.Delete:
                        if (display.Length > 0) {
                            display = display.Remove(cursorX, 1);
                        }
                        break;
                    case ConsoleKey.Enter:
                        Out.WriteLine();
                        history.Add(display);
                        return display;
                    default:
                        display = display.Insert(cursorX, Char.ToString(key.KeyChar));
                        cursorX += 1;
                        break;
                }
            }
        }
    }
}