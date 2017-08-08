using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Recipe5_6
{
    class Program
    {
        /// <summary>
        /// 避免使用捕获的同步上下文
        /// </summary>
        /// <param name="args"></param>
        [STAThread]
        static void Main(string[] args)
        {
            var app = new Application();
            var win = new Window();
            var panel = new StackPanel();
            var button = new Button();

            _label = new Label();
            _label.FontSize = 32;
            _label.Height = 200;

            button.Height = 100;
            button.FontSize = 32;
            button.Content = new TextBlock { Text = "开始异步操作" };
            button.Click += Click;

            panel.Children.Add(_label);
            panel.Children.Add(button);

            win.Content = panel;
            app.Run(win);

            Console.ReadLine();
        }

        private static Label _label;

        static async void Click(object sender, EventArgs e)
        {
            _label.Content = new TextBlock { Text = "计算中......" };
            TimeSpan resultWithContent = await Test();
            TimeSpan resultNoContent = await TestNoContent();
            //TimeSpan resultNoContent = await TestNoContent().ConfigureAwait(false);

            var sb = new StringBuilder();
            sb.AppendLine($"With the content: {resultWithContent}");
            sb.AppendLine($"Without the content: {resultNoContent}");
            sb.AppendLine($"Ratio: {resultWithContent.TotalMilliseconds / resultNoContent.TotalMilliseconds:0.00}");

            _label.Content = new TextBlock { Text = sb.ToString() };
        }

        static async Task<TimeSpan> Test()
        {
            const int iterationsNumber = 100000;
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < iterationsNumber; i++)
            {
                var t = Task.Run(() => { });
                await t;
            }
            sw.Stop();
            return sw.Elapsed;
        }

        static async Task<TimeSpan> TestNoContent()
        {
            const int interationsNumber = 100000;
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < interationsNumber; i++)
            {
                var t = Task.Run(() => { });
                // 将continueOnCapturedContext指定为false，不使用捕获的同步上下文运行后续操作代码
                await t.ConfigureAwait(continueOnCapturedContext: false);
            }
            sw.Stop();
            return sw.Elapsed;
        }
    }
}

