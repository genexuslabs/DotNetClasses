using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeneXus.Application;
using GeneXus.Notifications.ProgressIndicator;

namespace GeneXus.Notifications
{
    public class GXWebProgressIndicator
    {
        private static string ID = "GX_PROGRESS_BAR";
        private GXWebNotification notification;
        private GXWebProgressIndicatorInfo info;
        private bool running;

        public GXWebProgressIndicator(IGxContext gxContext)
        {
            notification = new GXWebNotification(gxContext);
            info = new GXWebProgressIndicatorInfo();
        }

        public void Show()
        {
            running = true;
            info.Action = "0";
            UpdateProgress();
        }

        private void UpdateProgress()
        {
            if (running)
            {
                GXWebNotificationInfo notif = new GXWebNotificationInfo();
                notif.Id = GXWebProgressIndicator.ID;
                notif.GroupName = GXWebProgressIndicator.ID;
                notif.Message = info;
                notification.Notify(notif);
            }
        }

        public void ShowWithTitle(string title)
        {
            running = true;
            Title = title;
            info.Action = "1";
            UpdateProgress();
        }

        public void ShowWithTitleAndDescription(string title, string desc)
        {
            running = true;
            Title = title;
            Description = desc;
            info.Action = "2";
            UpdateProgress();
        }

        public void Hide()
        {
            info.Action = "3";
            UpdateProgress();
            running = false;
        }

        public string Class
        {
            get { return info.Class; }
            set
            {
                info.Class = value;
                UpdateProgress();
            }
        }

        public string Title
        {
            get { return info.Title; }
            set
            {
                info.Title = value;
                UpdateProgress();
            }
        }


        public string Description
        {
            get { return info.Description; }
            set
            {
                info.Description = value;
                UpdateProgress();
            }
        }

        public short Type
        {
            get { return info.Type; }
            set { info.Type = value; }
        }

        public int Value
        {
            get { return info.Value; }
            set
            {
                info.Value = value;
                UpdateProgress();
            }
        }

        public int MaxValue
        {
            get { return info.MaxValue; }
            set { info.MaxValue = value; }
        }
    }
}
