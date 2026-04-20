using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using NeonNet3.Data;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NeonNet3
{
    // ── Custom scrollbar colors via Windows messages ──────────────────
    public class NeonScrollPanel : FlowLayoutPanel
    {
        private const int WM_NCPAINT = 0x0085;
        private const int WS_EX_CLIENTEDGE = 0x00000200;
        private const int GWL_EXSTYLE = -20;
        private const int SB_VERT = 1;
        private Control _lastScreen;

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("user32.dll")]
        private static extern bool GetScrollInfo(IntPtr hwnd, int fnBar, ref SCROLLINFO lpsi);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        [StructLayout(LayoutKind.Sequential)]
        private struct SCROLLINFO
        {
            public uint cbSize;
            public uint fMask;
            public int nMin;
            public int nMax;
            public uint nPage;
            public int nPos;
            public int nTrackPos;
        }

        private const uint SIF_ALL = 0x17;
        private const int SM_CXVSCROLL = 2;

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_NCPAINT)
            {
                DrawCustomScrollbar();
            }
        }

        private void DrawCustomScrollbar()
        {
            SCROLLINFO si = new SCROLLINFO();
            si.cbSize = (uint)Marshal.SizeOf(si);
            si.fMask = SIF_ALL;

            if (!GetScrollInfo(this.Handle, SB_VERT, ref si))
                return;

            IntPtr hdc = GetWindowDC(this.Handle);
            if (hdc == IntPtr.Zero) return;

            try
            {
                using (Graphics g = Graphics.FromHdc(hdc))
                {
                    int sbWidth = GetSystemMetrics(SM_CXVSCROLL);
                    int totalHeight = this.Height;
                    int sbX = this.Width;

                    // Track background
                    Rectangle track = new Rectangle(sbX, 0, sbWidth, totalHeight);
                    using (SolidBrush trackBrush = new SolidBrush(Color.FromArgb(6, 14, 26)))
                        g.FillRectangle(trackBrush, track);

                    // Thumb
                    int range = si.nMax - si.nMin;
                    if (range > 0)
                    {
                        float thumbRatio = (float)si.nPage / (range + si.nPage);
                        int thumbHeight = Math.Max(20, (int)(totalHeight * thumbRatio));
                        float posRatio = (float)(si.nPos - si.nMin) / range;
                        int thumbY = (int)((totalHeight - thumbHeight) * posRatio);

                        Rectangle thumb = new Rectangle(sbX + 2, thumbY + 2, sbWidth - 4, thumbHeight - 4);
                        using (GraphicsPath gp = RoundedRect(thumb, 4))
                        using (SolidBrush thumbBrush = new SolidBrush(Color.FromArgb(0, 180, 210)))
                            g.FillPath(thumbBrush, gp);

                        // Thumb glow edge
                        using (GraphicsPath gp = RoundedRect(thumb, 4))
                        using (Pen glowPen = new Pen(Color.FromArgb(0, 234, 255), 1))
                            g.DrawPath(glowPen, gp);
                    }

                    // Border between content and scrollbar
                    using (Pen borderPen = new Pen(Color.FromArgb(0, 60, 80), 1))
                        g.DrawLine(borderPen, sbX, 0, sbX, totalHeight);
                }
            }
            finally
            {
                ReleaseDC(this.Handle, hdc);
            }
        }

        private GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(r.X, r.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(r.Right - radius * 2, r.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(r.Right - radius * 2, r.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(r.X, r.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // Remove the sunken border so our custom paint is clean
                cp.ExStyle &= ~WS_EX_CLIENTEDGE;
                return cp;
            }
        }
    }

    public partial class ProfileScreen : UserControl
    {
        private string _username;
        private string _loggedInUsername;
        private PictureBox picProfile;
        private Panel profilePanel;
        private NeonScrollPanel pnlPostsFeed;
        private Label lblFollowersNum;
        private Label lblFollowingNum;
        private Label lblTagPronouns;
        private Button btnFollow;

        public ProfileScreen(string username, string loggedInUsername = null)
        {
            InitializeComponent();
            _username = username;
            _loggedInUsername = loggedInUsername ?? username;
            SetupScreen();
        }

        private void SetupScreen()
        {
            this.BackColor = Color.FromArgb(8, 18, 32);

            Color neon = Color.FromArgb(0, 234, 255);
            Color panelBg = Color.FromArgb(6, 14, 26);
            Color navHover = Color.FromArgb(12, 28, 48);

            // ── PROFILE PANEL ────────────────────────────────────────
            profilePanel = new Panel();
            profilePanel.BackColor = panelBg;
            profilePanel.Dock = DockStyle.Top;
            profilePanel.Height = 200;
            profilePanel.Padding = new Padding(30);
            profilePanel.Paint += ProfilePanel_Paint;

            // ── PROFILE PICTURE ──────────────────────────────────────
            picProfile = new PictureBox();
            picProfile.Size = new Size(90, 90);
            picProfile.Location = new Point(30, 35);
            picProfile.SizeMode = PictureBoxSizeMode.Zoom;
            picProfile.Cursor = _loggedInUsername == _username ? Cursors.Hand : Cursors.Default;
            if (_loggedInUsername == _username)
                picProfile.Click += PicProfile_Click;

            Bitmap bmp = new Bitmap(90, 90);
            using (Graphics g = Graphics.FromImage(bmp))
                g.Clear(Color.FromArgb(0, 30, 45));
            picProfile.Image = bmp;

            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(0, 0, picProfile.Width, picProfile.Height);
            picProfile.Region = new Region(path);

            // ── DISPLAY NAME ─────────────────────────────────────────
            Label lblUser = new Label();
            lblUser.Text = _username;
            lblUser.ForeColor = neon;
            lblUser.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblUser.AutoSize = true;
            lblUser.Location = new Point(150, 35);

            // ── PRONOUNS ─────────────────────────────────────────────
            string pronouns = Database.GetPronouns(_username);
            lblTagPronouns = new Label();
            lblTagPronouns.Text = "@" + _username + (string.IsNullOrEmpty(pronouns) ? " | (add pronouns)" : " | " + pronouns);
            lblTagPronouns.ForeColor = Color.LightGray;
            lblTagPronouns.Font = new Font("Segoe UI", 11F);
            lblTagPronouns.AutoSize = true;
            lblTagPronouns.Location = new Point(152, 70);
            if (_loggedInUsername == _username)
            {
                lblTagPronouns.Cursor = Cursors.Hand;
                lblTagPronouns.Click += LblTagPronouns_Click;
            }

            // ── STATS ────────────────────────────────────────────────
            int statsY = 120;

            Label lblPostsNum = new Label();
            lblPostsNum.Text = Database.GetPostCount(_username).ToString();
            lblPostsNum.ForeColor = neon;
            lblPostsNum.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblPostsNum.Location = new Point(150, statsY);
            lblPostsNum.AutoSize = true;

            Label lblPosts = new Label();
            lblPosts.Text = "Posts";
            lblPosts.ForeColor = Color.White;
            lblPosts.Font = new Font("Segoe UI", 10F);
            lblPosts.Location = new Point(175, statsY + 4);
            lblPosts.AutoSize = true;

            lblFollowersNum = new Label();
            lblFollowersNum.Text = Database.GetFollowersCount(_username).ToString();
            lblFollowersNum.ForeColor = neon;
            lblFollowersNum.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblFollowersNum.Location = new Point(240, statsY);
            lblFollowersNum.AutoSize = true;

            Label lblFollowers = new Label();
            lblFollowers.Text = "Followers";
            lblFollowers.ForeColor = Color.White;
            lblFollowers.Font = new Font("Segoe UI", 10F);
            lblFollowers.Location = new Point(265, statsY + 4);
            lblFollowers.AutoSize = true;

            lblFollowingNum = new Label();
            lblFollowingNum.Text = Database.GetFollowingCount(_username).ToString();
            lblFollowingNum.ForeColor = neon;
            lblFollowingNum.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblFollowingNum.Location = new Point(360, statsY);
            lblFollowingNum.AutoSize = true;

            Label lblFollowing = new Label();
            lblFollowing.Text = "Following";
            lblFollowing.ForeColor = Color.White;
            lblFollowing.Font = new Font("Segoe UI", 10F);
            lblFollowing.Location = new Point(385, statsY + 4);
            lblFollowing.AutoSize = true;

            // ── SETTINGS BUTTON (own profile only) ───────────────────
            if (_loggedInUsername == _username)
            {
                Button btnSettings = new Button();
                btnSettings.Size = new Size(32, 32);
                btnSettings.Location = new Point(profilePanel.Width - 50, 30);
                btnSettings.FlatStyle = FlatStyle.Flat;
                btnSettings.FlatAppearance.BorderSize = 0;
                btnSettings.BackColor = Color.Transparent;
                btnSettings.Cursor = Cursors.Hand;
                btnSettings.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                btnSettings.Text = "⚙";
                btnSettings.Font = new Font("Segoe UI", 14F);
                btnSettings.ForeColor = neon;
                btnSettings.Click += BtnSettings_Click;
                profilePanel.Controls.Add(btnSettings);
            }

            // ── FOLLOW / MESSAGE BUTTONS (other profiles only) ────────
            if (_loggedInUsername != _username)
            {
                bool isFollowing = Database.IsFollowing(_loggedInUsername, _username);

                btnFollow = new Button();
                btnFollow.Text = isFollowing ? "Unfollow" : "Follow";
                btnFollow.ForeColor = neon;
                btnFollow.BackColor = Color.Transparent;
                btnFollow.FlatStyle = FlatStyle.Flat;
                btnFollow.FlatAppearance.BorderColor = neon;
                btnFollow.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                btnFollow.Size = new Size(90, 30);
                btnFollow.Location = new Point(150, 155);
                btnFollow.Cursor = Cursors.Hand;
                btnFollow.Click += BtnFollow_Click;
                profilePanel.Controls.Add(btnFollow);

                Button btnMessage = new Button();
                btnMessage.Text = "Message";
                btnMessage.ForeColor = neon;
                btnMessage.BackColor = Color.Transparent;
                btnMessage.FlatStyle = FlatStyle.Flat;
                btnMessage.FlatAppearance.BorderColor = neon;
                btnMessage.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                btnMessage.Size = new Size(90, 30);
                btnMessage.Location = new Point(250, 155);
                btnMessage.Cursor = Cursors.Hand;
                btnMessage.Click += (s, e) =>
                {
                    var homeScreen = this.Parent?.Parent as HomeScreen;
                    if (homeScreen != null)
                        homeScreen.NavigateToMessages(_username);
                };
                profilePanel.Controls.Add(btnMessage);
            }

            profilePanel.Controls.Add(picProfile);
            profilePanel.Controls.Add(lblUser);
            profilePanel.Controls.Add(lblTagPronouns);
            profilePanel.Controls.Add(lblPostsNum);
            profilePanel.Controls.Add(lblPosts);
            profilePanel.Controls.Add(lblFollowersNum);
            profilePanel.Controls.Add(lblFollowers);
            profilePanel.Controls.Add(lblFollowingNum);
            profilePanel.Controls.Add(lblFollowing);

            this.Controls.Add(profilePanel);

            // ── TAB BAR ──────────────────────────────────────────────
            Panel pnlTabs = new Panel();
            pnlTabs.Dock = DockStyle.Top;
            pnlTabs.Height = 50;
            pnlTabs.BackColor = Color.Transparent;

            Color tabBg = Color.FromArgb(6, 14, 26);

            Button btnPosts = new Button();
            btnPosts.Text = "Posts";
            btnPosts.ForeColor = neon;
            btnPosts.BackColor = tabBg;
            btnPosts.FlatStyle = FlatStyle.Flat;
            btnPosts.UseVisualStyleBackColor = false;
            btnPosts.FlatAppearance.BorderSize = 1;
            btnPosts.FlatAppearance.BorderColor = neon;
            btnPosts.FlatAppearance.MouseOverBackColor = navHover;
            btnPosts.FlatAppearance.MouseDownBackColor = navHover;
            btnPosts.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnPosts.Size = new Size(120, 36);
            btnPosts.Location = new Point(0, 7);
            btnPosts.Cursor = Cursors.Hand;
            btnPosts.Click += (s, e) => LoadUserPosts();

            Button btnReplies = new Button();
            btnReplies.Text = "Replies";
            btnReplies.ForeColor = neon;
            btnReplies.BackColor = tabBg;
            btnReplies.FlatStyle = FlatStyle.Flat;
            btnReplies.UseVisualStyleBackColor = false;
            btnReplies.FlatAppearance.BorderSize = 1;
            btnReplies.FlatAppearance.BorderColor = neon;
            btnReplies.FlatAppearance.MouseOverBackColor = navHover;
            btnReplies.FlatAppearance.MouseDownBackColor = navHover;
            btnReplies.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnReplies.Size = new Size(120, 36);
            btnReplies.Location = new Point(130, 7);
            btnReplies.Cursor = Cursors.Hand;
            btnReplies.Click += (s, e) => ShowComingSoon("Replies");

            Button btnMedia = new Button();
            btnMedia.Text = "Media";
            btnMedia.ForeColor = neon;
            btnMedia.BackColor = tabBg;
            btnMedia.FlatStyle = FlatStyle.Flat;
            btnMedia.UseVisualStyleBackColor = false;
            btnMedia.FlatAppearance.BorderSize = 1;
            btnMedia.FlatAppearance.BorderColor = neon;
            btnMedia.FlatAppearance.MouseOverBackColor = navHover;
            btnMedia.FlatAppearance.MouseDownBackColor = navHover;
            btnMedia.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnMedia.Size = new Size(120, 36);
            btnMedia.Location = new Point(260, 7);
            btnMedia.Cursor = Cursors.Hand;
            btnMedia.Click += (s, e) => ShowComingSoon("Media");

            Button btnTopics = new Button();
            btnTopics.Text = "Topics";
            btnTopics.ForeColor = neon;
            btnTopics.BackColor = tabBg;
            btnTopics.FlatStyle = FlatStyle.Flat;
            btnTopics.UseVisualStyleBackColor = false;
            btnTopics.FlatAppearance.BorderSize = 1;
            btnTopics.FlatAppearance.BorderColor = neon;
            btnTopics.FlatAppearance.MouseOverBackColor = navHover;
            btnTopics.FlatAppearance.MouseDownBackColor = navHover;
            btnTopics.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnTopics.Size = new Size(120, 36);
            btnTopics.Location = new Point(390, 7);
            btnTopics.Cursor = Cursors.Hand;
            btnTopics.Click += (s, e) => LoadTopicsView();

            pnlTabs.Controls.Add(btnPosts);
            pnlTabs.Controls.Add(btnReplies);
            pnlTabs.Controls.Add(btnMedia);
            pnlTabs.Controls.Add(btnTopics);

            this.Controls.Add(pnlTabs);
            pnlTabs.BringToFront();

            // ── POSTS FEED (custom neon scrollbar panel) ──────────────
            pnlPostsFeed = new NeonScrollPanel();
            pnlPostsFeed.Dock = DockStyle.Fill;
            pnlPostsFeed.BackColor = Color.FromArgb(10, 20, 35);
            pnlPostsFeed.AutoScroll = true;
            pnlPostsFeed.FlowDirection = FlowDirection.TopDown;
            pnlPostsFeed.WrapContents = false;
            pnlPostsFeed.Padding = new Padding(10);

            this.Controls.Add(pnlPostsFeed);
            pnlPostsFeed.BringToFront();

            LoadUserPosts();
            LoadProfileImage();
        }

        // ── TOPICS TAB VIEW ──────────────────────────────────────────
        private void LoadTopicsView()
        {
            pnlPostsFeed.Controls.Clear();

            Color neon = Color.FromArgb(0, 234, 255);
            Color panelBg = Color.FromArgb(6, 14, 26);
            Color navHover = Color.FromArgb(12, 28, 48);

            // ── Followed topics section ───────────────────────────────
            Label lblFollowedHeader = new Label();
            lblFollowedHeader.Text = "YOUR TOPICS";
            lblFollowedHeader.ForeColor = neon;
            lblFollowedHeader.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblFollowedHeader.AutoSize = true;
            lblFollowedHeader.Margin = new Padding(10, 15, 0, 5);
            pnlPostsFeed.Controls.Add(lblFollowedHeader);

            var followedTopics = Database.GetFollowedTopics(_loggedInUsername);

            if (followedTopics.Count == 0)
            {
                Label lblNone = new Label();
                lblNone.Text = "You haven't followed any topics yet.";
                lblNone.ForeColor = Color.FromArgb(0, 130, 160);
                lblNone.Font = new Font("Segoe UI", 10F);
                lblNone.AutoSize = true;
                lblNone.Margin = new Padding(10, 5, 0, 10);
                pnlPostsFeed.Controls.Add(lblNone);
            }
            else
            {
                foreach (string topic in followedTopics)
                {
                    Panel row = BuildTopicRow(topic, true, neon, navHover);
                    pnlPostsFeed.Controls.Add(row);
                }
            }

            // ── Divider ───────────────────────────────────────────────
            Panel divider = new Panel();
            divider.Width = pnlPostsFeed.Width - 40;
            divider.Height = 1;
            divider.BackColor = Color.FromArgb(0, 60, 80);
            divider.Margin = new Padding(10, 10, 10, 10);
            pnlPostsFeed.Controls.Add(divider);

            // ── Browse all topics section ─────────────────────────────
            Label lblBrowseHeader = new Label();
            lblBrowseHeader.Text = "BROWSE TOPICS";
            lblBrowseHeader.ForeColor = neon;
            lblBrowseHeader.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblBrowseHeader.AutoSize = true;
            lblBrowseHeader.Margin = new Padding(10, 5, 0, 5);
            pnlPostsFeed.Controls.Add(lblBrowseHeader);

            string[] allTopics = { "Technology", "Gaming", "Music", "Sports", "Art", "Science", "News", "Film", "Food", "Travel" };

            foreach (string topic in allTopics)
            {
                bool alreadyFollowing = followedTopics.Contains(topic);
                Panel row = BuildTopicRow(topic, alreadyFollowing, neon, navHover);
                pnlPostsFeed.Controls.Add(row);
            }
        }

        private Panel BuildTopicRow(string topic, bool isFollowing, Color neon, Color navHover)
        {
            Panel row = new Panel();
            row.Width = pnlPostsFeed.Width - 40;
            row.Height = 48;
            row.BackColor = Color.FromArgb(10, 24, 40);
            row.Margin = new Padding(10, 4, 10, 4);

            row.Paint += (s, pe) =>
            {
                using (Pen p = new Pen(Color.FromArgb(0, 50, 70), 1))
                    pe.Graphics.DrawRectangle(p, 0, 0, row.Width - 1, row.Height - 1);
            };

            Label lblHash = new Label();
            lblHash.Text = "#";
            lblHash.ForeColor = neon;
            lblHash.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblHash.AutoSize = true;
            lblHash.Location = new Point(12, 12);
            row.Controls.Add(lblHash);

            Label lblName = new Label();
            lblName.Text = topic;
            lblName.ForeColor = Color.White;
            lblName.Font = new Font("Segoe UI", 11F);
            lblName.AutoSize = true;
            lblName.Location = new Point(30, 14);
            row.Controls.Add(lblName);

            Button btnToggle = new Button();
            btnToggle.Text = isFollowing ? "Unfollow" : "+ Follow";
            btnToggle.ForeColor = isFollowing ? Color.FromArgb(0, 160, 180) : neon;
            btnToggle.BackColor = Color.Transparent;
            btnToggle.FlatStyle = FlatStyle.Flat;
            btnToggle.FlatAppearance.BorderColor = isFollowing ? Color.FromArgb(0, 120, 140) : neon;
            btnToggle.FlatAppearance.MouseOverBackColor = navHover;
            btnToggle.FlatAppearance.MouseDownBackColor = navHover;
            btnToggle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            btnToggle.Size = new Size(80, 28);
            btnToggle.Location = new Point(row.Width - 100, 10);
            btnToggle.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnToggle.Cursor = Cursors.Hand;

            string topicName = topic;
            btnToggle.Click += (s, pe) =>
            {
                if (Database.IsFollowingTopic(_loggedInUsername, topicName))
                {
                    Database.UnfollowTopic(_loggedInUsername, topicName);
                    btnToggle.Text = "+ Follow";
                    btnToggle.ForeColor = neon;
                    btnToggle.FlatAppearance.BorderColor = neon;
                }
                else
                {
                    Database.FollowTopic(_loggedInUsername, topicName);
                    btnToggle.Text = "Unfollow";
                    btnToggle.ForeColor = Color.FromArgb(0, 160, 180);
                    btnToggle.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 140);
                }
                // Reload the full view so "Your Topics" section stays in sync
                LoadTopicsView();
            };

            row.Controls.Add(btnToggle);
            return row;
        }

        // ── Settings ─────────────────────────────────────────────────
        private void BtnSettings_Click(object sender, EventArgs e)
        {
            Color neon = Color.FromArgb(0, 234, 255);

            Panel pnlSettings = new Panel();
            pnlSettings.BackColor = Color.FromArgb(6, 14, 26);
            pnlSettings.Size = new Size(300, 330);
            pnlSettings.Location = new Point(this.Width - 320, 50);
            pnlSettings.BringToFront();
            pnlSettings.Paint += (s, pe) =>
            {
                using (Pen p = new Pen(neon, 1))
                    pe.Graphics.DrawRectangle(p, 0, 0, pnlSettings.Width - 1, pnlSettings.Height - 1);
            };

            Label lblSettingsTitle = new Label();
            lblSettingsTitle.Text = "SETTINGS";
            lblSettingsTitle.ForeColor = neon;
            lblSettingsTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblSettingsTitle.AutoSize = true;
            lblSettingsTitle.Location = new Point(15, 15);
            pnlSettings.Controls.Add(lblSettingsTitle);

            Button btnEditPronouns = new Button();
            btnEditPronouns.Text = "Edit Pronouns";
            btnEditPronouns.ForeColor = neon;
            btnEditPronouns.BackColor = Color.Transparent;
            btnEditPronouns.FlatStyle = FlatStyle.Flat;
            btnEditPronouns.FlatAppearance.BorderColor = neon;
            btnEditPronouns.Font = new Font("Segoe UI", 9F);
            btnEditPronouns.Size = new Size(260, 35);
            btnEditPronouns.Location = new Point(15, 50);
            btnEditPronouns.Cursor = Cursors.Hand;
            btnEditPronouns.Click += (s, pe) =>
            {
                pnlSettings.Dispose();
                LblTagPronouns_Click(null, null);
            };
            pnlSettings.Controls.Add(btnEditPronouns);

            Button btnChangePhoto = new Button();
            btnChangePhoto.Text = "Change Profile Photo";
            btnChangePhoto.ForeColor = neon;
            btnChangePhoto.BackColor = Color.Transparent;
            btnChangePhoto.FlatStyle = FlatStyle.Flat;
            btnChangePhoto.FlatAppearance.BorderColor = neon;
            btnChangePhoto.Font = new Font("Segoe UI", 9F);
            btnChangePhoto.Size = new Size(260, 35);
            btnChangePhoto.Location = new Point(15, 95);
            btnChangePhoto.Cursor = Cursors.Hand;
            btnChangePhoto.Click += (s, pe) =>
            {
                pnlSettings.Dispose();
                UploadProfileImage();
            };
            pnlSettings.Controls.Add(btnChangePhoto);

            // ── EDIT USERNAME ─────────────────────────────
            Button btnEditUsername = new Button();
            btnEditUsername.Text = "Change Username";
            btnEditUsername.ForeColor = neon;
            btnEditUsername.BackColor = Color.Transparent;
            btnEditUsername.FlatStyle = FlatStyle.Flat;
            btnEditUsername.FlatAppearance.BorderColor = neon;
            btnEditUsername.Font = new Font("Segoe UI", 9F);
            btnEditUsername.Size = new Size(260, 35);
            btnEditUsername.Location = new Point(15, 140);
            btnEditUsername.Cursor = Cursors.Hand;
            btnEditUsername.Click += (s, pe) =>
            {
                pnlSettings.Dispose();
                ShowChangeUsername();
            };
            pnlSettings.Controls.Add(btnEditUsername);

            // ── EDIT CONTACT ─────────────────────────────
            Button btnEditContact = new Button();
            btnEditContact.Text = "Change Email / Phone";
            btnEditContact.ForeColor = neon;
            btnEditContact.BackColor = Color.Transparent;
            btnEditContact.FlatStyle = FlatStyle.Flat;
            btnEditContact.FlatAppearance.BorderColor = neon;
            btnEditContact.Font = new Font("Segoe UI", 9F);
            btnEditContact.Size = new Size(260, 35);
            btnEditContact.Location = new Point(15, 185);
            btnEditContact.Cursor = Cursors.Hand;
            btnEditContact.Click += (s, pe) =>
            {
                pnlSettings.Dispose();
                ShowChangeContact();
            };
            pnlSettings.Controls.Add(btnEditContact);

            // ── DELETE ACCOUNT ───────────────────────────
            Button btnDelete = new Button();
            btnDelete.Text = "Delete Account";
            btnDelete.ForeColor = Color.Red;
            btnDelete.BackColor = Color.Transparent;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.FlatAppearance.BorderColor = Color.Red;
            btnDelete.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnDelete.Size = new Size(260, 35);
            btnDelete.Location = new Point(15, 230);
            btnDelete.Cursor = Cursors.Hand;
            btnDelete.Click += (s, pe) =>
            {
                pnlSettings.Dispose();
                ConfirmDeleteAccount();
            };
            pnlSettings.Controls.Add(btnDelete);

            // ── CLOSE BUTTON ───────────────────────────
            Button btnClose = new Button();
            btnClose.Text = "Close";
            btnClose.ForeColor = neon;
            btnClose.BackColor = Color.Transparent;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderColor = neon;
            btnClose.Font = new Font("Segoe UI", 9F);
            btnClose.Size = new Size(260, 35);
            btnClose.Location = new Point(15, 275);
            btnClose.Cursor = Cursors.Hand;
            btnClose.Click += (s, pe) => pnlSettings.Dispose();
            pnlSettings.Controls.Add(btnClose);

            this.Controls.Add(pnlSettings);
            pnlSettings.BringToFront();
        }

        // ── Pronouns ─────────────────────────────────────────────────
        private void LblTagPronouns_Click(object sender, EventArgs e)
        {
            Color neon = Color.FromArgb(0, 234, 255);
            string current = Database.GetPronouns(_username);

            Panel pnlPronouns = new Panel();
            pnlPronouns.BackColor = Color.FromArgb(6, 14, 26);
            pnlPronouns.Size = new Size(300, 130);
            pnlPronouns.Location = new Point(150, 65);
            pnlPronouns.BringToFront();
            pnlPronouns.Paint += (s, pe) =>
            {
                using (Pen p = new Pen(neon, 1))
                    pe.Graphics.DrawRectangle(p, 0, 0, pnlPronouns.Width - 1, pnlPronouns.Height - 1);
            };

            Label lblPrompt = new Label();
            lblPrompt.Text = "Enter your pronouns:";
            lblPrompt.ForeColor = neon;
            lblPrompt.Font = new Font("Segoe UI", 9F);
            lblPrompt.AutoSize = true;
            lblPrompt.Location = new Point(15, 15);
            pnlPronouns.Controls.Add(lblPrompt);

            TextBox txtPronouns = new TextBox();
            txtPronouns.BackColor = Color.FromArgb(8, 18, 32);
            txtPronouns.ForeColor = neon;
            txtPronouns.Font = new Font("Segoe UI", 10F);
            txtPronouns.BorderStyle = BorderStyle.FixedSingle;
            txtPronouns.Text = current;
            txtPronouns.Size = new Size(265, 30);
            txtPronouns.Location = new Point(15, 40);
            pnlPronouns.Controls.Add(txtPronouns);

            Button btnSave = new Button();
            btnSave.Text = "Save";
            btnSave.ForeColor = neon;
            btnSave.BackColor = Color.Transparent;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderColor = neon;
            btnSave.Size = new Size(120, 30);
            btnSave.Location = new Point(15, 85);
            btnSave.Cursor = Cursors.Hand;
            btnSave.Click += (s, pe) =>
            {
                Database.SavePronouns(_username, txtPronouns.Text.Trim());
                lblTagPronouns.Text = "@" + _username + (string.IsNullOrEmpty(txtPronouns.Text.Trim()) ? " | (add pronouns)" : " | " + txtPronouns.Text.Trim());
                pnlPronouns.Dispose();
            };
            pnlPronouns.Controls.Add(btnSave);

            Button btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.ForeColor = neon;
            btnCancel.BackColor = Color.Transparent;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderColor = neon;
            btnCancel.Size = new Size(120, 30);
            btnCancel.Location = new Point(145, 85);
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.Click += (s, pe) => pnlPronouns.Dispose();
            pnlPronouns.Controls.Add(btnCancel);

            this.Controls.Add(pnlPronouns);
            pnlPronouns.BringToFront();
        }

        private void ShowChangeUsername()
        {
            Color neon = Color.FromArgb(0, 234, 255);

            Panel pnl = new Panel();
            pnl.BackColor = Color.FromArgb(6, 14, 26);
            pnl.Size = new Size(300, 150);
            pnl.Location = new Point(150, 70);
            pnl.BringToFront();

            pnl.Paint += (s, pe) =>
            {
                using (Pen p = new Pen(neon, 1))
                    pe.Graphics.DrawRectangle(p, 0, 0, pnl.Width - 1, pnl.Height - 1);
            };

            Label lbl = new Label();
            lbl.Text = "New Username:";
            lbl.ForeColor = neon;
            lbl.Font = new Font("Segoe UI", 9F);
            lbl.AutoSize = true;
            lbl.Location = new Point(15, 15);
            pnl.Controls.Add(lbl);

            TextBox txt = new TextBox();
            txt.Text = _username;
            txt.BackColor = Color.FromArgb(8, 18, 32);
            txt.ForeColor = neon;
            txt.BorderStyle = BorderStyle.FixedSingle;
            txt.Font = new Font("Segoe UI", 10F);
            txt.Size = new Size(260, 30);
            txt.Location = new Point(15, 40);
            pnl.Controls.Add(txt);

            Button btnSave = new Button();
            btnSave.Text = "Save";
            btnSave.ForeColor = neon;
            btnSave.BackColor = Color.Transparent;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderColor = neon;
            btnSave.Size = new Size(120, 30);
            btnSave.Location = new Point(15, 90);

            btnSave.Click += (s, e) =>
            {
                string newUsername = txt.Text.Trim();

                if (!string.IsNullOrEmpty(newUsername))
                {
                    Database.UpdateUsername(_username, newUsername);

                    MessageBox.Show(
                        "Username successfully changed! Please log in again.",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    var mainForm = this.FindForm() as Form1;

                    if (mainForm != null)
                    {
                        var login = new LoginScreen();
                        login.Dock = DockStyle.Fill;

                        ScreenTransition.TransitionScreens(mainForm.pnlMain, this, login);
                    }
                }
            };


            Button btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.ForeColor = neon;
            btnCancel.BackColor = Color.Transparent;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderColor = neon;
            btnCancel.Size = new Size(120, 30);
            btnCancel.Location = new Point(145, 90);
            btnCancel.Click += (s, e) => pnl.Dispose();

            pnl.Controls.Add(btnSave);
            pnl.Controls.Add(btnCancel);

            this.Controls.Add(pnl);
            pnl.BringToFront();
        }


        private void ShowChangeContact()
        {
            Color neon = Color.FromArgb(0, 234, 255);

            Panel pnl = new Panel();
            pnl.BackColor = Color.FromArgb(6, 14, 26);
            pnl.Size = new Size(300, 150);
            pnl.Location = new Point(150, 70);
            pnl.BringToFront();

            // BORDER
            pnl.Paint += (s, pe) =>
            {
                using (Pen p = new Pen(neon, 1))
                    pe.Graphics.DrawRectangle(p, 0, 0, pnl.Width - 1, pnl.Height - 1);
            };

            Label lbl = new Label();
            lbl.Text = "New Email or Phone:";
            lbl.ForeColor = neon;
            lbl.Font = new Font("Segoe UI", 9F);
            lbl.AutoSize = true;
            lbl.Location = new Point(15, 15);
            pnl.Controls.Add(lbl);

            TextBox txt = new TextBox();
            txt.BackColor = Color.FromArgb(8, 18, 32);
            txt.ForeColor = neon;
            txt.BorderStyle = BorderStyle.FixedSingle;
            txt.Font = new Font("Segoe UI", 10F);
            txt.Size = new Size(260, 30);
            txt.Location = new Point(15, 40);
            pnl.Controls.Add(txt);

            Button btnSave = new Button();
            btnSave.Text = "Save";
            btnSave.ForeColor = neon;
            btnSave.BackColor = Color.Transparent;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderColor = neon;
            btnSave.Size = new Size(120, 30);
            btnSave.Location = new Point(15, 90);

            btnSave.Click += (s, e) =>
            {
                string value = txt.Text.Trim();

                if (!string.IsNullOrEmpty(value))
                {
                    Database.UpdateContact(_username, value);
                    MessageBox.Show("Contact updated!");
                    pnl.Dispose();
                }
            };

            Button btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.ForeColor = neon;
            btnCancel.BackColor = Color.Transparent;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderColor = neon;
            btnCancel.Size = new Size(120, 30);
            btnCancel.Location = new Point(145, 90);
            btnCancel.Click += (s, e) => pnl.Dispose();

            pnl.Controls.Add(btnSave);
            pnl.Controls.Add(btnCancel);

            this.Controls.Add(pnl);
            pnl.BringToFront();
        }


        private void ConfirmDeleteAccount()
        {
            var result = MessageBox.Show(
                "Are you sure you want to delete your account?\nThis cannot be undone.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                Database.DeleteUser(_loggedInUsername);

                var login = new LoginScreen();
                login.Dock = DockStyle.Fill;

                var mainForm = this.FindForm() as Form1;

                if (mainForm != null)
                {
                    ScreenTransition.TransitionScreens(mainForm.pnlMain, this, login);
                }
            }
        }

        // ── Follow ────────────────────────────────────────────────────
        private void BtnFollow_Click(object sender, EventArgs e)
        {
            bool isFollowing = Database.IsFollowing(_loggedInUsername, _username);

            if (isFollowing)
            {
                Database.UnfollowUser(_loggedInUsername, _username);
                btnFollow.Text = "Follow";
            }
            else
            {
                Database.FollowUser(_loggedInUsername, _username);
                btnFollow.Text = "Unfollow";
            }

            lblFollowersNum.Text = Database.GetFollowersCount(_username).ToString();
        }

        private void ShowComingSoon(string feature)
        {
            NeonMessageBox.Show($"{feature} coming soon!");
        }

        // ── Paint ────────────────────────────────────────────────────
        private void ProfilePanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Color neon = Color.FromArgb(0, 234, 255);

            Rectangle rect = new Rectangle(1, 1, profilePanel.Width - 2, profilePanel.Height - 2);
            using (Pen border = new Pen(neon, 2))
                g.DrawRectangle(border, rect);

            int glowSize = 6;
            Rectangle circle = new Rectangle(
                picProfile.Left - glowSize / 2,
                picProfile.Top - glowSize / 2,
                picProfile.Width + glowSize,
                picProfile.Height + glowSize);

            for (int i = 0; i < 6; i++)
            {
                using (Pen glow = new Pen(Color.FromArgb(40 - (i * 6), neon), 2 + i))
                    g.DrawEllipse(glow, circle);
            }
        }

        private void PicProfile_Click(object sender, EventArgs e)
        {
            UploadProfileImage();
        }

        private void UploadProfileImage()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string sourcePath = dialog.FileName;
                Image original;
                using (var temp = new Bitmap(sourcePath))
                    original = new Bitmap(temp);

                Image cropped = CropToSquare(original);
                string imagesFolder = Path.Combine(Application.StartupPath, "ProfileImages");
                if (!Directory.Exists(imagesFolder))
                    Directory.CreateDirectory(imagesFolder);

                string destinationPath = Path.Combine(imagesFolder, _username + ".png");
                cropped.Save(destinationPath, System.Drawing.Imaging.ImageFormat.Png);

                if (picProfile.Image != null)
                    picProfile.Image.Dispose();

                picProfile.Image = new Bitmap(cropped);
                SaveProfileImage(destinationPath);
                profilePanel.Invalidate();
            }
        }

        private void SaveProfileImage(string path)
        {
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE Users SET ProfileImagePath=@path WHERE Username=@user";
                cmd.Parameters.AddWithValue("@path", path);
                cmd.Parameters.AddWithValue("@user", _username);
                cmd.ExecuteNonQuery();
            }
        }

        private void LoadProfileImage()
        {
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT ProfileImagePath FROM Users WHERE Username=@user";
                cmd.Parameters.AddWithValue("@user", _username);
                var result = cmd.ExecuteScalar();
                if (result != null)
                {
                    string path = result.ToString();
                    if (File.Exists(path))
                    {
                        using (var temp = new Bitmap(path))
                            picProfile.Image = new Bitmap(temp);
                    }
                }
            }
        }

        private Image CropToSquare(Image img)
        {
            int size = Math.Min(img.Width, img.Height);
            int x = (img.Width - size) / 2;
            int y = (img.Height - size) / 2;
            Bitmap square = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(square))
            {
                g.DrawImage(img,
                    new Rectangle(0, 0, size, size),
                    new Rectangle(x, y, size, size),
                    GraphicsUnit.Pixel);
            }
            return square;
        }

        private void LoadUserPosts()
        {
            pnlPostsFeed.Controls.Clear();
            var posts = Database.GetUserPosts(_username);

            if (posts.Count == 0)
            {
                Label lblEmpty = new Label();
                lblEmpty.Text = "No posts yet.";
                lblEmpty.ForeColor = Color.FromArgb(0, 130, 160);
                lblEmpty.Font = new Font("Segoe UI", 10F);
                lblEmpty.AutoSize = true;
                lblEmpty.Margin = new Padding(15);
                pnlPostsFeed.Controls.Add(lblEmpty);
                return;
            }

            foreach (var post in posts)
            {
                Panel postPanel = new Panel();
                postPanel.Width = pnlPostsFeed.Width - 40;
                postPanel.Height = 100;
                postPanel.BackColor = Color.FromArgb(20, 40, 60);
                postPanel.Margin = new Padding(5, 5, 5, 5);

                postPanel.Paint += (s, e) =>
                {
                    using (Pen p = new Pen(Color.FromArgb(0, 60, 80), 1))
                        e.Graphics.DrawRectangle(p, 0, 0, postPanel.Width - 1, postPanel.Height - 1);
                };

                Label lblTitle = new Label();
                lblTitle.Text = post.Title;
                lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
                lblTitle.ForeColor = Color.FromArgb(0, 234, 255);
                lblTitle.AutoSize = false;
                lblTitle.Width = postPanel.Width - 20;
                lblTitle.Height = 30;
                lblTitle.Location = new Point(10, 10);

                Label lblContent = new Label();
                lblContent.Text = post.Content;
                lblContent.Font = new Font("Segoe UI", 10F);
                lblContent.ForeColor = Color.White;
                lblContent.AutoSize = false;
                lblContent.Width = postPanel.Width - 20;
                lblContent.Height = 40;
                lblContent.Location = new Point(10, 42);

                Label lblDate = new Label();
                lblDate.Text = post.CreatedAt.ToString("MMM dd, yyyy");
                lblDate.Font = new Font("Segoe UI", 8F);
                lblDate.ForeColor = Color.FromArgb(0, 130, 160);
                lblDate.AutoSize = true;
                lblDate.Location = new Point(10, 78);

                postPanel.Controls.Add(lblTitle);
                postPanel.Controls.Add(lblContent);
                postPanel.Controls.Add(lblDate);

                // MAKE POST CLICKABLE
                postPanel.Cursor = Cursors.Hand;

                postPanel.Click += (s, e) =>
                {
                    OpenPostView(post);
                };

                // ALSO MAKE CHILD CONTROLS CLICKABLE
                lblTitle.Click += (s, e) => OpenPostView(post);
                lblContent.Click += (s, e) => OpenPostView(post);
                lblDate.Click += (s, e) => OpenPostView(post);

                pnlPostsFeed.Controls.Add(postPanel);
            }
        }
        private void OpenPostView(Database.Post post)
        {
            // Save current screen reference (so we can return)
            var parent = this.Parent;
            if (parent == null) return;

            parent.SuspendLayout();

            // Hide current screen instead of removing it
            this.Hide();

            var postView = new PostViewScreen(
                post.Id,
                post.Username,
                post.Title,
                post.Content,
                post.CreatedAt,
                _loggedInUsername
            );

            postView.Dock = DockStyle.Fill;

            // BACK BUTTON LOGIC (consistent with HomeScreen)
            postView.OnBack += () =>
            {
                parent.Controls.Remove(postView);
                postView.Dispose();

                this.Show();
                this.BringToFront();
            };

            parent.Controls.Add(postView);
            postView.BringToFront();

            parent.ResumeLayout();
        }
        private void ReloadProfileScreen()
        {
            var parent = this.Parent;
            if (parent == null) return;

            var fresh = new ProfileScreen(_username, _username);
            fresh.Dock = DockStyle.Fill;

            parent.Controls.Clear();
            parent.Controls.Add(fresh);
        }

    }
}