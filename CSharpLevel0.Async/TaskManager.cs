using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSharpLevel0.Async
{
    /// <summary>
    /// Object->Task->Task<TResult>,官方文档：https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task?view=netframework-4.8
    /// </summary>
    public class TaskManager
    {

        public static void Manage()
        {
            //Create();
            //WaitCommon();
            //WaitAny();
            //WaitAll();
            Flatten();
        }

        public static async Task Create()
        {
            await Task.Run(() => {
                // Just loop.
                int ctr = 0;
                for (ctr = 0; ctr <= 1000000; ctr++)
                { }
                Console.WriteLine("Finished {0} loop iterations",
                                  ctr);
            });
        }

        private static void WaitCommon()
        {
            Task taskA = Task.Run(() => Thread.Sleep(2000));
            try
            {
                //var ct = new CancellationToken();
                //taskA.Wait(1000, ct);       // Wait for 1 second.
                taskA.Wait(TimeSpan.FromSeconds(1));
                bool completed = taskA.IsCompleted;
                Console.WriteLine("Task A completed: {0}, Status: {1}",
                                 completed, taskA.Status);
                if (!completed)
                    Console.WriteLine("Timed out before task A completed.");
            }
            catch (AggregateException)
            {
                Console.WriteLine("Exception in taskA.");
            }
        }

        private static void WaitAny()
        {
            var tasks = new Task[3];
            var rnd = new Random();
            for (int ctr = 0; ctr <= 2; ctr++)
                tasks[ctr] = Task.Run(() => Thread.Sleep(rnd.Next(500, 3000)));

            try
            {
                int index = Task.WaitAny(tasks);
                Console.WriteLine("Task #{0} completed first.\n", tasks[index].Id);
                Console.WriteLine("Status of all tasks:");
                foreach (var t in tasks)
                    Console.WriteLine("   Task #{0}: {1}", t.Id, t.Status);
            }
            catch (AggregateException)
            {
                Console.WriteLine("An exception occurred.");
            }
        }

        private static void WaitAll()
        {
            // Wait for all tasks to complete.
            Task[] tasks = new Task[10];
            for (int i = 0; i < 10; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    Thread.Sleep(2000);
                });
            }
            try
            {
                Task.WaitAll(tasks);
            }
            catch (AggregateException ae)
            {
                Console.WriteLine("One or more exceptions occurred: ");
                foreach (var ex in ae.Flatten().InnerExceptions)
                    Console.WriteLine("   {0}", ex.Message);
            }

            Console.WriteLine("Status of completed tasks:");
            foreach (var t in tasks)
                Console.WriteLine("   Task #{0}: {1}", t.Id, t.Status);
        }

        private static void Flatten()
        {
            //var child2 = new Task(() => { throw new CustomException("Attached child2 faulted."); }, TaskCreationOptions.AttachedToParent);
            //var child1 = new Task(() =>
            //{
            //    child2.Start();
            //    throw new CustomException("Attached child1 faulted.");
            //}, TaskCreationOptions.AttachedToParent);
            //var task1 = Task.Run(() => { child1.Start(); });//Task.Run默认：TaskCreationOptions.DenyChildAttached，异常不会抛出至AggregateException

            var task1 = Task.Factory.StartNew(() =>
            {
                var child1 = Task.Factory.StartNew(() =>
                {
                    var child2 = Task.Factory.StartNew(() =>
                    {
                        // This exception is nested inside three AggregateExceptions.
                        throw new CustomException("Attached child2 faulted.");
                    }, TaskCreationOptions.AttachedToParent);

                    // This exception is nested inside two AggregateExceptions.
                    throw new CustomException("Attached child1 faulted.");
                }, TaskCreationOptions.AttachedToParent);
            });

            try
            {
                task1.Wait();
            }
            catch (AggregateException ae)
            {
                foreach (var e in ae.Flatten().InnerExceptions)
                {
                    if (e is CustomException)
                    {
                        Console.WriteLine(e.Message);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
    public class CustomException : Exception
    {
        public CustomException(String message) : base(message)
        { }
    }
}
