﻿using System;
using BenchmarkDotNet.Running;

namespace Benchmarker
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

            Console.ReadKey();
        }
    }
}
