using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.UserProfile;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Recipe11_3
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            string language = GlobalizationPreferences.Languages.First();
            _cultureInfo = new CultureInfo(language);

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (sender, e) => UpdateClockText();
        }

        private const string TASK_NAME_USERPRESENT = "TileSchedulerTask_UserPresent";
        private const string TASK_NAME_TIMER = "TileSchedulerTask_Timer";

        private readonly CultureInfo _cultureInfo;
        private readonly DispatcherTimer _timer;

        private void UpdateClockText()
        {
            Clock.Text = DateTime.Now.ToString(_cultureInfo.DateTimeFormat.FullDateTimePattern);
        }

        private static async void CreateClockTask()
        {
            BackgroundAccessStatus result = await BackgroundExecutionManager.RequestAccessAsync();
            if (result == BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity
                || result == BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity)
            {
                TileSchedulerTask.CreateSchedule();

                EnsureUserPresentTask();
                EnsureTimerTask();
            }
        }

        private static void EnsureTimerTask()
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == TASK_NAME_TIMER)
                {
                    return;
                }

                var builder = new BackgroundTaskBuilder();
                builder.Name = TASK_NAME_TIMER;
                builder.TaskEntryPoint = (typeof(TileSchedulerTask)).FullName;
                builder.SetTrigger(new TimeTrigger(180, false));
                builder.Register();
            }
        }

        private static void EnsureUserPresentTask()
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == TASK_NAME_USERPRESENT)
                {
                    return;
                }

                var builder = new BackgroundTaskBuilder();
                builder.Name = TASK_NAME_USERPRESENT;
                builder.TaskEntryPoint = (typeof(TileSchedulerTask)).FullName;
                builder.SetTrigger(new SystemTrigger(SystemTriggerType.UserPresent, false));
                builder.Register();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _timer.Start();
            UpdateClockText();
            CreateClockTask();
        }
    }

    public class TileSchedulerTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            CreateSchedule();
            deferral.Complete();
        }

        public static void CreateSchedule()
        {
            var tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
            var plannedUpdated = tileUpdater.GetScheduledTileNotifications();

            DateTime now = DateTime.Now;
            DateTime planTill = now.AddHours(4);

            DateTime updateTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0).AddMinutes(1);
            if (plannedUpdated.Count > 0)
            {
                updateTime = plannedUpdated
                    .Select(x => x.DeliveryTime.DateTime)
                    .Union(new[] { updateTime })
                    .Max();
            }
            XmlDocument documentNow = GetTilenotificationXML(now);

            tileUpdater.Update(new TileNotification(documentNow)
            {
                ExpirationTime = now.AddMinutes(1)
            });

            for (var startPlanning = updateTime; startPlanning < planTill; startPlanning = startPlanning.AddMinutes(1))
            {
                Debug.WriteLine(startPlanning);
                Debug.WriteLine(planTill);

                try
                {
                    XmlDocument document = GetTilenotificationXML(startPlanning);

                    var scheduleNotification = new ScheduledTileNotification(document, new DateTimeOffset(startPlanning))
                    {
                        ExpirationTime = startPlanning.AddMinutes(1)
                    };

                    tileUpdater.AddToSchedule(scheduleNotification);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error: " + ex.Message);
                }
            }
        }

        private static XmlDocument GetTilenotificationXML(DateTime dateTime)
        {
            string language = GlobalizationPreferences.Languages.First();
            var cultureInfo = new CultureInfo(language);

            string shortDate = dateTime.ToString(cultureInfo.DateTimeFormat.ShortDatePattern);
            string longDate = dateTime.ToString(cultureInfo.DateTimeFormat.LongDatePattern);

            var document = XElement.Parse(string.Format(@"<tile>
    <visual>
        <binding template=""TileSquareText02"">
            <text id=""1"">{0}</text>
            <text id=""2"">{1}</text>
        </binding>
        <binding template=""TileWideText01"">
            <text id=""1"">{0}</text>
            <text id=""2"">{1}</text>
            <text id=""3""></text>
            <text id=""4""></text>
        </binding>
    </visual>
</tile>", shortDate, longDate));

            return document.ToXmlDocument();
        }

    }

    public static class DocumentExtensions
    {
         public static XmlDocument ToXmlDocument(this XElement xDocument)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xDocument.ToString());
            return xmlDocument;
        }
    }
}
