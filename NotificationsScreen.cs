using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using NeonNet3.Data;

namespace NeonNet3
{
    public partial class NotificationsScreen : UserControl
    {
        private string _username;
        private Panel pnlMain;
        private Label lblHeader;
        private FlowLayoutPanel flpNotifications;

        public event Action OnNavigateToMessages;

        public NotificationsScreen(string username)
        {
            _username = username;
            SetupScreen();
        }

        private void SetupScreen()
        {
            this.BackColor = Color.FromArgb(5, 11, 22);
            this.Dock = DockStyle.Fill;

            lblHeader = new Label();
            lblHeader.Text = "NOTIFICATIONS";
            lblHeader.ForeColor = Color.FromArgb(0, 234, 255);
            lblHeader.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblHeader.AutoSize = true;
            lblHeader.Location = new Point(20, 15);
            this.Controls.Add(lblHeader);

            pnlMain = new Panel();
            pnlMain.BackColor = Color.FromArgb(5, 11, 22);
            pnlMain.Location = new Point(20, 55);
            pnlMain.Size = new Size(this.Width - 40, this.Height - 75);
            pnlMain.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
            pnlMain.Paint += (s, e) =>
            {
                Color neon = Color.FromArgb(0, 234, 255);
                using (Pen p = new Pen(neon, 1))
                    e.Graphics.DrawRectangle(p, 0, 0, pnlMain.Width - 1, pnlMain.Height - 1);
            };
            this.Controls.Add(pnlMain);

            flpNotifications = new FlowLayoutPanel();
            flpNotifications.BackColor = Color.FromArgb(5, 11, 22);
            flpNotifications.Dock = DockStyle.Fill;
            flpNotifications.FlowDirection = FlowDirection.TopDown;
            flpNotifications.WrapContents = false;
            flpNotifications.AutoScroll = true;
            flpNotifications.Padding = new Padding(10);
            pnlMain.Controls.Add(flpNotifications);

            this.Load += (s, e) => LoadNotifications();
            flpNotifications.Resize += (s, e) => RefreshCardWidths();
        }

        private void LoadNotifications()
        {
            flpNotifications.Controls.Clear();

            List<Database.NotificationItem> notifications = Database.GetNotifications(_username);

            if (notifications.Count == 0)
            {
                Label lblEmpty = new Label();
                lblEmpty.Text = "No notifications yet.";
                lblEmpty.ForeColor = Color.FromArgb(0, 130, 160);
                lblEmpty.Font = new Font("Segoe UI", 10F);
                lblEmpty.AutoSize = true;
                lblEmpty.Margin = new Padding(15);
                flpNotifications.Controls.Add(lblEmpty);
                return;
            }

            foreach (var n in notifications)
            {
                Color accentColor;
                switch (n.Type)
                {
                    case "Message": accentColor = Color.FromArgb(0, 180, 220); break;
                    case "Post": accentColor = Color.FromArgb(0, 234, 255); break;
                    default: accentColor = Color.FromArgb(0, 150, 180); break;
                }

                string timeAgo = GetTimeAgo(n.CreatedAt);
                bool isMessage = n.Type == "Message";
                AddNotificationCard(n.Title, n.Message, timeAgo, accentColor, n.Message, isMessage);
            }
        }

        private void RefreshCardWidths()
        {
            foreach (Control c in flpNotifications.Controls)
            {
                if (c is Panel card)
                    card.Width = flpNotifications.Width - 30;
            }
        }

        private string GetTimeAgo(DateTime createdAt)
        {
            TimeSpan diff = DateTime.Now - createdAt;
            if (diff.TotalMinutes < 1) return "Just now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} minutes ago";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} hours ago";
            return $"{(int)diff.TotalDays} days ago";
        }

        private void AddNotificationCard(string title, string message, string time, Color accentColor, string popupMessage, bool isMessage)
        {
            Panel card = new Panel();
            card.BackColor = Color.FromArgb(8, 18, 32);
            card.Size = new Size(flpNotifications.Width - 30, 80);
            card.Margin = new Padding(5, 5, 5, 5);
            card.Cursor = Cursors.Hand;
            card.Paint += (s, e) =>
            {
                using (Pen p = new Pen(accentColor, 1))
                    e.Graphics.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1);
                using (SolidBrush accent = new SolidBrush(accentColor))
                    e.Graphics.FillRectangle(accent, 0, 0, 4, card.Height);
            };

            card.MouseEnter += (s, e) => { card.BackColor = Color.FromArgb(12, 28, 48); card.Invalidate(); };
            card.MouseLeave += (s, e) => { card.BackColor = Color.FromArgb(8, 18, 32); card.Invalidate(); };
            card.Click += (s, e) =>
            {
                NeonMessageBox.Show(popupMessage);
                if (isMessage) OnNavigateToMessages?.Invoke();
            };

            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.ForeColor = Color.FromArgb(0, 234, 255);
            lblTitle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblTitle.Location = new Point(15, 10);
            lblTitle.AutoSize = true;
            lblTitle.Cursor = Cursors.Hand;
            lblTitle.Click += (s, e) =>
            {
                NeonMessageBox.Show(popupMessage);
                if (isMessage) OnNavigateToMessages?.Invoke();
            };
            lblTitle.MouseEnter += (s, e) => { card.BackColor = Color.FromArgb(12, 28, 48); card.Invalidate(); };
            lblTitle.MouseLeave += (s, e) => { card.BackColor = Color.FromArgb(8, 18, 32); card.Invalidate(); };
            card.Controls.Add(lblTitle);

            Label lblMessage = new Label();
            lblMessage.Text = message;
            lblMessage.ForeColor = Color.FromArgb(180, 220, 230);
            lblMessage.Font = new Font("Segoe UI", 9F);
            lblMessage.Location = new Point(15, 32);
            lblMessage.AutoSize = true;
            lblMessage.Cursor = Cursors.Hand;
            lblMessage.Click += (s, e) =>
            {
                NeonMessageBox.Show(popupMessage);
                if (isMessage) OnNavigateToMessages?.Invoke();
            };
            lblMessage.MouseEnter += (s, e) => { card.BackColor = Color.FromArgb(12, 28, 48); card.Invalidate(); };
            lblMessage.MouseLeave += (s, e) => { card.BackColor = Color.FromArgb(8, 18, 32); card.Invalidate(); };
            card.Controls.Add(lblMessage);

            Label lblTime = new Label();
            lblTime.Text = time;
            lblTime.ForeColor = Color.FromArgb(0, 130, 160);
            lblTime.Font = new Font("Segoe UI", 8F);
            lblTime.Location = new Point(15, 55);
            lblTime.AutoSize = true;
            lblTime.Cursor = Cursors.Hand;
            lblTime.Click += (s, e) =>
            {
                NeonMessageBox.Show(popupMessage);
                if (isMessage) OnNavigateToMessages?.Invoke();
            };
            lblTime.MouseEnter += (s, e) => { card.BackColor = Color.FromArgb(12, 28, 48); card.Invalidate(); };
            lblTime.MouseLeave += (s, e) => { card.BackColor = Color.FromArgb(8, 18, 32); card.Invalidate(); };
            card.Controls.Add(lblTime);

            flpNotifications.Controls.Add(card);
        }
    }
}