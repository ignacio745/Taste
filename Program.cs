﻿using System;

namespace Taste
{

    class Taste
    {

        public static void Main(string[] arg)
        {
            if (arg.Length == 1)
            {
                Scanner scanner = new Scanner(arg[0]);
                Parser parser = new Parser(scanner);
                parser.tab = new SymbolTable(parser);
                parser.gen = new CodeGenerator();
                parser.Parse();
                if (parser.errors.count == 0)
                {
                    parser.gen.Decode();
                    parser.gen.Interpret();
                }
            }
            else
                Console.WriteLine("-- No source file specified");
        }

    }

} // end namespace