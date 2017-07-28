using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Recipe4_9
{
    /// <summary>
    /// 使用 TaskScheduler 配置任务的执行
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonSync_Click(object sender, RoutedEventArgs e)
        {
            ContentTextBlock.Text = string.Empty;
            try
            {
                // string result = TaskMethod(TaskScheduler.FromCurrentSynchronizationContext()).Result;
                // 同步调用，当前用户界面会被冻结，在任务执行完毕前无法响应任何操作
                // TaskMethod中的Task线程无法UI线程中的控件ContentTextBlock，导致发生异常
                string result = TaskMethod().Result;
                // 该赋值不会被执行到
                ContentTextBlock.Text = result;
            }
            catch (Exception ex)
            {
                // 出异常时显示异常消息
                ContentTextBlock.Text = ex.InnerException.Message;
            }
        }

        private void ButtonAsync_Click(object sender, RoutedEventArgs e)
        {
            ContentTextBlock.Text = string.Empty;
            Mouse.OverrideCursor = Cursors.Wait;
            // 异步执行Task，用户界面不会被冻结，任务执行期间界面仍然可以响应用户操作
            // 但是仍然会发生异常
            Task<string> task = TaskMethod();
            task.ContinueWith(
                t =>
                {
                    ContentTextBlock.Text = t.Exception.InnerException.Message;
                    Mouse.OverrideCursor = null;
                },
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void ButtonAsyncOK_Click(object sender, RoutedEventArgs e)
        {
            ContentTextBlock.Text = string.Empty;
            Mouse.OverrideCursor = Cursors.Wait;

            // 将UI线程任务调度程序(TaskScheduler.FromCurrentSynchronizationContext())提供给了任务
            Task<string> task = TaskMethod(TaskScheduler.FromCurrentSynchronizationContext());
            task.ContinueWith(
                t => Mouse.OverrideCursor = null,
                CancellationToken.None,
                TaskContinuationOptions.None,
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        private Task<string> TaskMethod()
        {
            return TaskMethod(TaskScheduler.Default);
        }

        private Task<string> TaskMethod(TaskScheduler scheduler)
        {
            Task delay = Task.Delay(TimeSpan.FromSeconds(5));
            return delay.ContinueWith(t =>
            {
                string str = $"Task is running on a thread id {Thread.CurrentThread.ManagedThreadId}. " +
                $"Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}";
                
                ContentTextBlock.Text = str;
                return str;
            }, scheduler);
        }
    }
}
