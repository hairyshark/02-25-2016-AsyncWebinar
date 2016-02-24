﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using NUnit.Framework;

namespace AsyncDolls
{
    [TestFixture]
    public class AsyncScript
    {
        [Test]
        public async Task AsyncRecap()
        {
            Parallel.For(0, 1000, CpuBoundMethod);
            await Task.Run(() => CpuBoundMethod(10));

            // Asynchronous
            await IoBoundMethod(".\\IoBoundMethod.txt");
        }

        static void CpuBoundMethod(int i)
        {
            Console.WriteLine(i);
        }

        static async Task IoBoundMethod(string path)
        {
            using (var stream = new FileStream(path, FileMode.OpenOrCreate))
            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteLineAsync("Yehaa " + DateTime.Now);
                writer.Close();
                stream.Close();
            }
        }

        [Test]
        public async Task SequentialVsConcurrent()
        {
            var sequential = Enumerable.Range(0, 4).Select(t => Task.Delay(2500));

            Console.WriteLine(DateTime.Now + " : Starting sequential.");

            foreach (var task in sequential)
            {
                await task;
            }

            Console.WriteLine(DateTime.Now + " : Done sequential.");

            Console.WriteLine(DateTime.Now + " : Starting concurrent.");

            var concurrent = Enumerable.Range(0, 4).Select(t => Task.Delay(2500));
            await Task.WhenAll(concurrent);

            Console.WriteLine(DateTime.Now + " : Done concurrent.");
        }

        [Test]
        public async Task AsyncVoid()
        {
            try
            {
                AvoidAsyncVoid();

            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e);
            }
            await Task.Delay(100);
        }

        static async void AvoidAsyncVoid()
        {
            Console.WriteLine("Going inside async void.");
            await Task.Delay(10);
            Console.WriteLine("Going to throw soon");
            throw new InvalidOperationException("Gotcha!");
        }

        [Test]
        public async Task ConfigureAwait()
        {
            // ReSharper disable once PossibleNullReferenceException
            await Process.Start(new ProcessStartInfo(@".\configureawait.exe") { UseShellExecute = false });
        }

        [Test]
        public void DontMixBlockingAndAsync()
        {
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());

            Delay(15);
        }

        static void Delay(int milliseconds)
        {
            DelayAsync(milliseconds).Wait();
        }

        static async Task DelayAsync(int milliseconds)
        {
            await Task.Delay(milliseconds);
        }
    }
}