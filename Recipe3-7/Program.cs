using System;
using System.ComponentModel;
using System.Threading;

namespace Recipe3_7
{
    class Program
    {
        /// <summary>
        /// 使用 BackgroundWorker 组件 
        /// 借助于该对象，可以将异步代码组织为一系列事件及事件处理器
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true; // 是否可以报告进度更新
            bw.WorkerSupportsCancellation = true; // 是否支持异步取消操作

            bw.DoWork += Worker_DoWork;
            bw.ProgressChanged += Worker_ProgressChanged;
            bw.RunWorkerCompleted += Worker_Completed;

            bw.RunWorkerAsync();

            Console.WriteLine("Press C to cancel work");
            do
            {
                if (Console.ReadKey(true).KeyChar == 'C')
                {
                    bw.CancelAsync();
                }
            } while (bw.IsBusy);
        }

        static void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Console.WriteLine($"DoWork 线程池的线程ID：{Thread.CurrentThread.ManagedThreadId}");
            var bw = (BackgroundWorker)sender;
            for (int i = 1; i <= 100; i++)
            {
                if (bw.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                if (i%10 == 0)
                {
                    // 触发BackgroundWorker的ProgressChanged事件
                    bw.ReportProgress(i);
                }
                Thread.Sleep(TimeSpan.FromSeconds(0.1));
            }

            e.Result = 42;
        }

        static void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Console.WriteLine($"已完成 {e.ProgressPercentage}%. Progress线程池Id：{Thread.CurrentThread.ManagedThreadId}");
        }

        static void Worker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine($"Completed线程池Id：{Thread.CurrentThread.ManagedThreadId}");
            if (e.Error != null)
            {
                Console.WriteLine($"发生异常：{e.Error.Message}");
            }
            else if (e.Cancelled)
            {
                Console.WriteLine($"操作已被取消.");
            } else
            {
                Console.WriteLine($"结果为：{e.Result}.");
            }
        }
    }
}
