using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using NeonNet3.Data;

namespace NeonNet3
{
    public partial class HomeScreen : UserControl
    {
        private string _loggedInUsername;
        private Button _activeBtn;  // tracks which nav button is currently active
        public event Action<Database.FeedPost> OnOpenPost;
        private Control _lastScreen;
        private Panel _profileDropdown;


        public HomeScreen(string username = "User")
        {
            _loggedInUsername = username;
            InitializeComponent();
            SetupScreen();
        }

        private void SetupScreen()
        {
            this.BackColor = Color.FromArgb(5, 11, 22);

            lblUsername.Text = _loggedInUsername;

            LoadProfilePicture();

            lblLogo.Cursor = Cursors.Hand;
            lblLogo.Click += LblLogo_Click;

            StyleNav(btnHome, "Home", 79);
            StyleNav(btnExplore, "Explore", 129);
            StyleNav(btnMessages, "Messages", 179);
            StyleNav(btnNotifications, "Notifications", 229);
            StyleNav(btnProfile, "Profile", 279);

            btnHome.Click          += BtnHome_Click;
            btnExplore.Click       += BtnExplore_Click;
            btnMessages.Click      += BtnMessages_Click;
            btnNotifications.Click += BtnNotifications_Click;
            btnProfile.Click       += BtnProfile_Click;

            btnCreatePost.Click  += BtnCreatePost_Click;
            btnCreateTopic.Click += BtnCreateTopic_Click;

            // Make profile picture + username in top bar clickable
            picProfile.Cursor  = Cursors.Hand;
            lblUsername.Cursor = Cursors.Hand;
            picProfile.Click += ShowProfileMenu;
            lblUsername.Click += ShowProfileMenu;


            txtSearch.Enter   += TxtSearch_Enter;
            txtSearch.Leave   += TxtSearch_Leave;
            txtSearch.KeyDown += TxtSearch_KeyDown;

            pnlTopBar.Paint    += PnlTopBar_Paint;
            pnlSidebar.Paint   += PnlSidebar_Paint;
            pnlContent.Paint   += PnlContent_Paint;
            pnlDiscover.Paint  += PnlDiscover_Paint;
            pnlDiscover.Resize += PnlDiscover_Resize;

            this.Resize += HomeScreen_Resize;
            this.Paint  += HomeScreen_Paint;

            picProfile.Paint += PicProfile_Paint;

            HomeScreen_Resize(this, EventArgs.Empty);

            // Load algorithm feed once the control is fully ready
            this.Load += (s, e) => { SetActiveNav(btnHome); LblLogo_Click(this, EventArgs.Empty); };

            // ── Custom Profile Dropdown ─────────────────────────────
            _profileDropdown = new Panel();
            _profileDropdown.Size = new Size(180, 90);
            _profileDropdown.BackColor = Color.FromArgb(12, 28, 48);
            _profileDropdown.Visible = false;

            this.Controls.Add(_profileDropdown);
            _profileDropdown.BringToFront();

            // Buttons inside dropdown
            Button btnView = new Button();
            Button btnSignOut = new Button();

            StyleDropdownButton(btnView, "View Account", 0);
            StyleDropdownButton(btnSignOut, "Sign Out", 45);

            btnView.Click += (s, e) =>
            {
                HideDropdown();
                SetActiveNav(btnProfile);
                BtnProfile_Click(s, e);
            };

            btnSignOut.Click += (s, e) =>
            {
                HideDropdown();
                HandleSignOut();
            };

            _profileDropdown.Controls.Add(btnView);
            _profileDropdown.Controls.Add(btnSignOut);

            // Paint (neon border)
            _profileDropdown.Paint += ProfileDropdown_Paint;

            // Click outside to close
            this.Click += (s, e) => HideDropdown();

        }
        private void HandleSignOut()
        {
            var login = new LoginScreen();
            login.Dock = DockStyle.Fill;

            var mainForm = this.FindForm() as Form1;

            if (mainForm != null)
            {
                ScreenTransition.TransitionScreens(mainForm.pnlMain, this, login);
            }
        }

        private void ShowProfileMenu(object sender, EventArgs e)
        {
            if (_profileDropdown.Visible)
            {
                HideDropdown();
                return;
            }

            int x = picProfile.Right - _profileDropdown.Width;
            int y = picProfile.Bottom + 4;

            // Keep inside window
            if (x + _profileDropdown.Width > this.Width)
                x = this.Width - _profileDropdown.Width - 10;

            _profileDropdown.Location = new Point(x, y);
            _profileDropdown.Visible = true;
            _profileDropdown.BringToFront();
        }

        private void StyleDropdownButton(Button btn, string text, int y)
        {
            btn.Text = text;
            btn.ForeColor = Color.FromArgb(0, 234, 255);
            btn.BackColor = Color.FromArgb(12, 28, 48);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(20, 45, 70);
            btn.Font = new Font("Microsoft Sans Serif", 9F);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(10, 0, 0, 0);
            btn.Size = new Size(180, 45);
            btn.Location = new Point(0, y);
            btn.Cursor = Cursors.Hand;
        }

        private void HideDropdown()
        {
            _profileDropdown.Visible = false;
        }

        private void ProfileDropdown_Paint(object sender, PaintEventArgs e)
        {
            DrawNeonRoundedBorder(e.Graphics, _profileDropdown.ClientRectangle, 6);
        }


        // ── Active nav highlight ──────────────────────────────────────
        private void SetActiveNav(Button btn)
        {
            Color neon    = Color.FromArgb(0, 234, 255);
            Color inactive = Color.FromArgb(0, 180, 200);

            // Reset all buttons to inactive style
            foreach (Button b in new[] { btnHome, btnExplore, btnMessages, btnNotifications, btnProfile })
            {
                b.ForeColor   = inactive;
                b.Font        = new Font("Microsoft Sans Serif", 9.5F, FontStyle.Regular);
                b.BackColor   = Color.Transparent;
            }

            // Highlight active button
            btn.ForeColor = Color.White;
            btn.Font      = new Font("Microsoft Sans Serif", 9.5F, FontStyle.Bold);
            btn.BackColor = Color.FromArgb(0, 40, 60);
            _activeBtn    = btn;
        }

        // ── profile picture ───────────────────────────────────────────
        private void LoadProfilePicture()
        {
            try
            {
                string imagePath = Database.GetProfileImagePath(_loggedInUsername);
                if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                {
                    using (var original = new Bitmap(imagePath))
                        picProfile.Image = CropToCircle(new Bitmap(original), picProfile.Width);
                }
            }
            catch { }
        }

        private Bitmap CropToCircle(Bitmap source, int size)
        {
            Bitmap result = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.Clear(Color.Transparent);
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddEllipse(0, 0, size, size);
                    g.SetClip(path);
                    g.DrawImage(source, 0, 0, size, size);
                }
            }
            return result;
        }

        public void RefreshProfilePicture()
        {
            LoadProfilePicture();
            picProfile.Invalidate();
        }

        // ── nav styling ───────────────────────────────────────────────
        private void StyleNav(Button btn, string text, int yPos)
        {
            Color neon = Color.FromArgb(0, 234, 255);
            Color navHover = Color.FromArgb(12, 28, 48);

            btn.Text = text;
            btn.ForeColor = neon;
            btn.BackColor = Color.Transparent;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = navHover;
            btn.Font = new Font("Microsoft Sans Serif", 9.5F);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(14, 0, 0, 0);
            btn.Size = new Size(160, 42);
            btn.Location = new Point(0, yPos);
            btn.Cursor = Cursors.Hand;
        }

        // ── logo / home navigation ────────────────────────────────────
        private void LblLogo_Click(object sender, EventArgs e)
        {
            SetActiveNav(btnHome);
            pnlContent.Controls.Clear();
            FeedScreen feed = new FeedScreen(_loggedInUsername);
            feed.Dock = DockStyle.Fill;
            feed.OnNavigateToProfile += HandleExploreNavigation;
            feed.OnOpenPost += OpenPostViewer;
            pnlContent.Controls.Add(feed);
        }

        // ── search ────────────────────────────────────────────────────
        private void TxtSearch_Enter(object sender, EventArgs e)
        {
            if (txtSearch.Text == "Search the Grid...")
            {
                txtSearch.Text = "";
                txtSearch.ForeColor = Color.FromArgb(0, 234, 255);
            }
        }

        private void TxtSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = "Search the Grid...";
                txtSearch.ForeColor = Color.FromArgb(0, 120, 140);
            }
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string searchText = txtSearch.Text.Trim();
                if (string.IsNullOrWhiteSpace(searchText) || searchText == "Search the Grid...") return;

                pnlContent.Controls.Clear();
                SearchResultsScreen results = new SearchResultsScreen(searchText, _loggedInUsername);
                results.Dock = DockStyle.Fill;
                results.OnNavigateToProfile += HandleExploreNavigation;
                results.OnOpenPost += OpenPostViewer;
                pnlContent.Controls.Add(results);

                e.SuppressKeyPress = true;
            }
        }

        // ── Shared navigation handler (Explore + Search) ──────────────────────
        private void HandleExploreNavigation(string target)
        {
            // Topic searches pass "__topic__TopicName"
            if (target.StartsWith("__topic__"))
            {
                string topic = target.Substring("__topic__".Length);
                pnlContent.Controls.Clear();
                SearchResultsScreen results = new SearchResultsScreen(topic, _loggedInUsername);
                results.Dock = DockStyle.Fill;
                results.OnNavigateToProfile += HandleExploreNavigation;
                results.OnOpenPost += OpenPostViewer;
                pnlContent.Controls.Add(results);
            }
            else
            {
                pnlContent.Controls.Clear();
                ProfileScreen profile = new ProfileScreen(target, _loggedInUsername);
                profile.Dock = DockStyle.Fill;
                pnlContent.Controls.Add(profile);
            }
        }

        // ── nav button handlers ───────────────────────────────────────
        private void BtnHome_Click(object sender, EventArgs e) { SetActiveNav(btnHome); LblLogo_Click(sender, e); }

        private void BtnExplore_Click(object sender, EventArgs e)
        {
            SetActiveNav(btnExplore);
            pnlContent.Controls.Clear();
            ExploreScreen explore = new ExploreScreen(_loggedInUsername);
            explore.Dock = DockStyle.Fill;
            explore.OnNavigateToProfile += HandleExploreNavigation;
            pnlContent.Controls.Add(explore);
        }

        private void BtnMessages_Click(object sender, EventArgs e)
        {
            SetActiveNav(btnMessages);
            pnlContent.Controls.Clear();
            MessagesScreen messages = new MessagesScreen(_loggedInUsername);
            messages.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(messages);
        }

        private void BtnNotifications_Click(object sender, EventArgs e)
        {
            SetActiveNav(btnNotifications);
            pnlContent.Controls.Clear();
            NotificationsScreen notifications = new NotificationsScreen(_loggedInUsername);
            notifications.Dock = DockStyle.Fill;
            notifications.OnNavigateToMessages += () =>
            {
                SetActiveNav(btnMessages);
                pnlContent.Controls.Clear();
                MessagesScreen messages = new MessagesScreen(_loggedInUsername);
                messages.Dock = DockStyle.Fill;
                pnlContent.Controls.Add(messages);
            };
            pnlContent.Controls.Add(notifications);
        }

        private void BtnProfile_Click(object sender, EventArgs e)
        {
            SetActiveNav(btnProfile);
            pnlContent.Controls.Clear();
            ProfileScreen profile = new ProfileScreen(_loggedInUsername, _loggedInUsername);
            profile.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(profile);
        }

        public void NavigateToMessages(string targetUser = null)
        {
            pnlContent.Controls.Clear();
            MessagesScreen messages = new MessagesScreen(_loggedInUsername);
            messages.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(messages);
        }

        private void BtnCreatePost_Click(object sender, EventArgs e)
        {
            pnlContent.Controls.Clear();

            CreatePostScreen screen = new CreatePostScreen(_loggedInUsername);
            screen.Dock = DockStyle.Fill;

            screen.OnPostCreated += () =>
            {
                pnlContent.Controls.Clear();
                ProfileScreen profile = new ProfileScreen(_loggedInUsername);
                profile.Dock = DockStyle.Fill;
                pnlContent.Controls.Add(profile);
            };

            pnlContent.Controls.Add(screen);
        }

        private void BtnCreateTopic_Click(object sender, EventArgs e)
        {
            pnlContent.Controls.Clear();

            CreateTopicScreen screen = new CreateTopicScreen(_loggedInUsername);
            screen.Dock = DockStyle.Fill;

            screen.OnTopicCreated += () =>
            {
                // After creating a topic → go to Create Post and refresh topics
                pnlContent.Controls.Clear();

                CreatePostScreen postScreen = new CreatePostScreen(_loggedInUsername);
                postScreen.Dock = DockStyle.Fill;

                // Make sure topics reload INCLUDING the new one
              

                pnlContent.Controls.Add(postScreen);
            };

            pnlContent.Controls.Add(screen);
        }

        // ── resize ────────────────────────────────────────────────────
        private void HomeScreen_Resize(object sender, EventArgs e)
        {
            const int PADDING = 14;
            const int MARGIN = 10;
            const int TOP_H = 56;
            const int SIDE_W = 160;
            const int ACT_W = 200;

            pnlTopBar.Location = new Point(0, 0);
            pnlTopBar.Size = new Size(this.Width, TOP_H);

            pnlSidebar.Location = new Point(0, TOP_H);
            pnlSidebar.Size = new Size(SIDE_W, this.Height - TOP_H);

            pnlActions.Location = new Point(this.Width - ACT_W - PADDING, TOP_H + PADDING);

            int contentX = SIDE_W + MARGIN + PADDING;
            int contentY = TOP_H + PADDING;
            int contentW = this.Width - SIDE_W - ACT_W - MARGIN * 2 - PADDING * 2;
            int contentH = this.Height - TOP_H - PADDING * 2;

            pnlContent.Location = new Point(contentX, contentY);
            pnlContent.Size = new Size(Math.Max(contentW, 100), Math.Max(contentH, 100));

            pnlDiscover.Size = new Size(pnlContent.Width - 24, pnlContent.Height - 24);

            txtSearch.Left = (this.Width - txtSearch.Width) / 2;
            txtSearch.Top = (TOP_H - txtSearch.Height) / 2;

            int rightEdge = this.Width - PADDING;
            picProfile.Left = rightEdge - picProfile.Width;
            lblUsername.Left = picProfile.Left - lblUsername.Width - 6;
            picProfile.Top = (TOP_H - picProfile.Height) / 2;
            lblUsername.Top = (TOP_H - lblUsername.Height) / 2;

            lblLogo.Left = PADDING;
            lblLogo.Top = (TOP_H - lblLogo.Height) / 2;
        }

        private void PnlDiscover_Resize(object sender, EventArgs e)
        {
            lblDiscover.Left = (pnlDiscover.Width - lblDiscover.Width) / 2;
            lblDiscover.Top = (pnlDiscover.Height - 60) / 2;
            lblDiscoverSub.Left = (pnlDiscover.Width - lblDiscoverSub.Width) / 2;
            lblDiscoverSub.Top = lblDiscover.Bottom + 8;
        }

        // ── painting ──────────────────────────────────────────────────
        private void HomeScreen_Paint(object sender, PaintEventArgs e)
        {
            const int PAD = 14;
            Color gridColor = Color.FromArgb(18, 36, 52);
            using (Pen gridPen = new Pen(gridColor, 1))
            {
                for (int x = PAD; x < this.Width - PAD; x += 40)
                    e.Graphics.DrawLine(gridPen, x, PAD, x, this.Height - PAD);
                for (int y = PAD; y < this.Height - PAD; y += 40)
                    e.Graphics.DrawLine(gridPen, PAD, y, this.Width - PAD, y);
            }
        }

        private void PnlTopBar_Paint(object sender, PaintEventArgs e)
        {
            Color neon = Color.FromArgb(0, 234, 255);
            using (Pen p = new Pen(neon, 1))
                e.Graphics.DrawLine(p, 0, pnlTopBar.Height - 1, pnlTopBar.Width, pnlTopBar.Height - 1);
        }

        private void PnlSidebar_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Color neon = Color.FromArgb(0, 234, 255);
            using (Pen p = new Pen(neon, 1))
                e.Graphics.DrawLine(p, pnlSidebar.Width - 1, 0, pnlSidebar.Width - 1, pnlSidebar.Height);
        }

        private void PnlContent_Paint(object sender, PaintEventArgs e)
        {
            DrawNeonRoundedBorder(e.Graphics, pnlContent.ClientRectangle, 6);
        }

        private void PnlDiscover_Paint(object sender, PaintEventArgs e)
        {
            DrawNeonRoundedBorder(e.Graphics, pnlDiscover.ClientRectangle, 6);
        }

        private void PicProfile_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (SolidBrush bg = new SolidBrush(Color.FromArgb(5, 11, 22)))
                e.Graphics.FillRectangle(bg, picProfile.ClientRectangle);

            if (picProfile.Image != null)
            {
                Rectangle circle = new Rectangle(2, 2, picProfile.Width - 4, picProfile.Height - 4);
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddEllipse(circle);
                    e.Graphics.SetClip(path);
                    e.Graphics.DrawImage(picProfile.Image, circle);
                    e.Graphics.ResetClip();
                }
            }

            Rectangle borderRect = new Rectangle(1, 1, picProfile.Width - 3, picProfile.Height - 3);
            using (Pen p = new Pen(Color.FromArgb(0, 234, 255), 1.5f))
                e.Graphics.DrawEllipse(p, borderRect);
        }

        private void DrawNeonRoundedBorder(Graphics g, Rectangle rect, int radius)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Color neon = Color.FromArgb(0, 234, 255);
            int glowSize = 6;

            Rectangle borderRect = new Rectangle(
                glowSize / 2, glowSize / 2,
                rect.Width - glowSize, rect.Height - glowSize);

            using (Pen borderPen = new Pen(neon, 1.2f))
                DrawRoundedRect(g, borderPen, borderRect, radius);
        }

        private void DrawRoundedRect(Graphics g, Pen pen, Rectangle bounds, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            g.DrawPath(pen, path);
        }

        private void OpenPostViewer(Database.FeedPost post)
        {
            // Save current screen BEFORE switching
            if (pnlContent.Controls.Count > 0)
                _lastScreen = pnlContent.Controls[0];

            pnlContent.Controls.Clear();

            PostViewScreen view = new PostViewScreen(
            post.Id, 
            post.Username,
            post.Title,
            post.Content,
            post.CreatedAt,
            _loggedInUsername
        );

            view.Dock = DockStyle.Fill;

            view.OnBack += () =>
            {
                pnlContent.Controls.Clear();

                if (_lastScreen != null)
                    pnlContent.Controls.Add(_lastScreen);
            };

            pnlContent.Controls.Add(view);
        }
    }
}