﻿using System;
using System.IO;

namespace Snippets {

    public class Patched_HelloWorld {

        public static void SnippetMain(Pumpkin.Monitor monitor) {
            //Console.WriteLine("Hello world!");
            Pumpkin.Monitor.Console_WriteLine("Hello world!", monitor);
        }
    }
}