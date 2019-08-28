using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSharpLevel0.Async
{
    /// <summary>
    /// Parallel Programming in .NET 官方文档：https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/
    /// Object->Task->Task<TResult>,Task类官方文档：https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task?view=netframework-4.8
    /// Task-based asynchronous programming：官方文档：https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-based-asynchronous-programming
    /// Task support：waiting, cancellation, continuations, robust exception handling, detailed status, custom scheduling, and more
    /// </summary>
    public class TaskManager
    {

        public static void Manage()
        {
            //Create();
            //CreateImplicitly();
            //WaitCommon();
            //WaitAny();
            //WaitAll();
            //Cancel();
            //Continuation();
            //Exception();
            Handle();
            //Flatten();
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
            //var task1 = new Task(() => { Console.WriteLine("sdf"); }, TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness);
            //var task2 = Task.Factory.StartNew((obj) => { Console.WriteLine("factory"); }, "state");
        }

        public static async Task CreateImplicitly()
        {
            Parallel.Invoke(() => { Console.WriteLine("1"); }, () => { Console.WriteLine("2"); }, () => { Console.WriteLine("3"); }, () => { Console.WriteLine("4"); }, () => { Console.WriteLine("5"); });
        }

        #region Wait

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

        #endregion

        #region Cancel

        private static async void Cancel()
        {
            var tokenSource2 = new CancellationTokenSource();
            CancellationToken ct = tokenSource2.Token;

            var task = Task.Run(() =>
            {
                // Were we already canceled?
                ct.ThrowIfCancellationRequested();

                bool moreToDo = true;
                while (moreToDo)
                {
                    // Poll on this property if you have to do
                    // other cleanup before throwing.
                    if (ct.IsCancellationRequested)
                    {
                        // Clean up here, then...
                        ct.ThrowIfCancellationRequested();
                    }

                }
            }, tokenSource2.Token); // Pass same token to Task.Run.

            tokenSource2.Cancel();

            // Just continue on this thread, or await with try-catch:
            try
            {
                await task;
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine($"{nameof(OperationCanceledException)} thrown with message: {e.Message}");
            }
            finally
            {
                tokenSource2.Dispose();
            }

        }

        #endregion

        #region Continuation

        private static async void Continuation()
        {
            var t = Task.Run(() => {
                DateTime dat = DateTime.Now;
                if (dat == DateTime.MinValue)
                    throw new ArgumentException("The clock is not working.");

                if (dat.Hour > 17)
                    return "evening";
                else if (dat.Hour > 12)
                    return "afternoon";
                else
                    return "morning";
            });
            var c = t.ContinueWith((antecedent) => {
                if (t.Status == TaskStatus.RanToCompletion)
                {
                    Console.WriteLine("Good {0}!",
                                      antecedent.Result);
                    Console.WriteLine("And how are you this fine {0}?",
                                   antecedent.Result);
                }
                else if (t.Status == TaskStatus.Faulted)
                {
                    Console.WriteLine(t.Exception.GetBaseException().Message);
                }
            });
        }

        #endregion

        #region Exception

        private static void Exception()
        {
            //var task1 = Task.Run(() =>
            //{
            //    throw new CustomException("task1 faulted.");
            //}).ContinueWith(t => {
            //    Console.WriteLine("{0}: {1}",
            //                      t.Exception.InnerException.GetType().Name,
            //                      t.Exception.InnerException.Message);
            //}, TaskContinuationOptions.OnlyOnFaulted);
            //Thread.Sleep(500);

            // Assume this is a user-entered String.
            String path = @"C:\";
            List<Task> tasks = new List<Task>();

            tasks.Add(Task.Run(() => {
                // This should throw an UnauthorizedAccessException.
                return Directory.GetFiles(path, "*.txt",
                                          SearchOption.AllDirectories);
            }));

            tasks.Add(Task.Run(() => {
                if (path == @"C:\")
                    throw new ArgumentException("The system root is not a valid path.");
                return new String[] { ".txt", ".dll", ".exe", ".bin", ".dat" };
            }));

            tasks.Add(Task.Run(() => {
                throw new NotImplementedException("This operation has not been implemented.");
            }));

            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException ae)
            {
                throw ae.Flatten();
            }
        }

        private static void Handle()
        {
            var task1 = Task.Run(() => { throw new CustomException("This exception is expected!"); });

            try
            {
                task1.Wait();
            }
            catch (AggregateException ae)
            {
                // Call the Handle method to handle the custom exception,
                // otherwise rethrow the exception.
                ae.Handle(ex => {
                    if (ex is CustomException)
                        Console.WriteLine(ex.Message);
                    return ex is CustomException;
                });
            }
        }

        /// <summary>
        /// 演示需要，应该尽量避免AttachedToParent
        /// </summary>
        private static void Flatten()
        {
            //var child2 = new Task(() => { throw new CustomException("Attached child2 faulted."); }, TaskCreationOptions.AttachedToParent);
            //var child1 = new Task(() =>
            //{
            //    child2.Start();
            //    throw new CustomException("Attached child1 faulted.");
            //}, TaskCreationOptions.AttachedToParent);
            //var task1 = Task.Run(() => { child1.Start(); });//Task.Run默认：TaskCreationOptions.DenyChildAttached 


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
                task1.Wait();//调用Wait方法才能捕获到异常
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

        #endregion

    }

    //public enum TaskStatus
    //{
    //    //
    //    // 摘要:
    //    //     该任务已初始化，但尚未被计划。
    //    Created = 0,
    //    //
    //    // 摘要:
    //    //     该任务正在等待 .NET Framework 基础结构在内部将其激活并进行计划。
    //    WaitingForActivation = 1,
    //    //
    //    // 摘要:
    //    //     该任务已被计划执行，但尚未开始执行。
    //    WaitingToRun = 2,
    //    //
    //    // 摘要:
    //    //     该任务正在运行，但尚未完成。
    //    Running = 3,
    //    //
    //    // 摘要:
    //    //     该任务已完成执行，正在隐式等待附加的子任务完成。
    //    WaitingForChildrenToComplete = 4,
    //    //
    //    // 摘要:
    //    //     已成功完成执行的任务。
    //    RanToCompletion = 5,
    //    //
    //    // 摘要:
    //    //     该任务已通过对其自身的 CancellationToken 引发 OperationCanceledException 对取消进行了确认，此时该标记处于已发送信号状态；或者在该任务开始执行之前，已向该任务的
    //    //     CancellationToken 发出了信号。 有关详细信息，请参阅任务取消。
    //    Canceled = 6,
    //    //
    //    // 摘要:
    //    //     由于未处理异常的原因而完成的任务。
    //    Faulted = 7
    //}

    public class CustomException : Exception
    {
        public CustomException(String message) : base(message)
        { }
    }
}
