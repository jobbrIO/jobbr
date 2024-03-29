﻿using System;
using System.Threading;

namespace Sandbox.JobRunner.Jobs
{
    public class ProgressJob
    {
        public void Run()
        {
            const int iterations = 15;

            for (var i = 0; i < iterations; i++)
            {
                Thread.Sleep(500);

                Console.WriteLine("##jobbr[progress percent='{0:0.00}']", (double)(i + 1) / iterations * 100);
            }
        }
    }
}
