using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using NeonNet3.Data;

namespace NeonNet3
{
    public class CreatePostScreen : UserControl
    {
        private string _username;

        TextBox txtTitle;
        TextBox txtContent;
        TextBox txtTopicSearch;
        ListBox lstTopicSuggestions;

        public event Action OnPostCreated;

        public CreatePostScreen(string username)
        {
            _username = username;
            SetupUI();
        }

        private void SetupUI()
        {
            this.BackColor = Color.FromArgb(8, 18, 32);

            Color neon = Color.FromArgb(0, 234, 255);
            Color panelBg = Color.FromArgb(10, 20, 40);
            Color navHover = Color.FromArgb(12, 28, 48);
            Color textBoxBg = Color.FromArgb(6, 10, 22);
            Color dimCyan = Color.FromArgb(40, 100, 140);

            Panel container = new Panel();
            container.BackColor = panelBg;
            container.Padding = new Padding(20);
            container.Margin = new Padding(20);
            container.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Paint += (s, e) => DrawBorder(e.Graphics, container.ClientRectangle, neon);
            this.Controls.Add(container);

            int innerPadding = 20;

            // ── Title Label ───────────────────────────────────────────
            Label lblTitle = new Label();
            lblTitle.Text = "⚡ Create New Transmission";
            lblTitle.ForeColor = neon;
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.AutoSize = true;
            container.Controls.Add(lblTitle);

            // ── Post Title TextBox ────────────────────────────────────
            txtTitle = new TextBox();
            txtTitle.Text = "Topic Title";
            txtTitle.BackColor = textBoxBg;
            txtTitle.ForeColor = neon;
            txtTitle.BorderStyle = BorderStyle.None;
            txtTitle.Font = new Font("Segoe UI", 10F);
            txtTitle.Enter += (s, e) =>
            {
                if (txtTitle.Text == "Topic Title") txtTitle.Text = "";
            };
            txtTitle.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtTitle.Text)) txtTitle.Text = "Topic Title";
            };
            container.Controls.Add(txtTitle);

            // ── Content TextBox ───────────────────────────────────────
            txtContent = new TextBox();
            txtContent.Multiline = true;
            txtContent.BackColor = textBoxBg;
            txtContent.ForeColor = neon;
            txtContent.BorderStyle = BorderStyle.None;
            txtContent.Font = new Font("Segoe UI", 10F);
            txtContent.Text = "Broadcast your thoughts to the Grid...";
            txtContent.Enter += (s, e) =>
            {
                if (txtContent.Text.Contains("Broadcast")) txtContent.Text = "";
            };
            txtContent.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtContent.Text))
                    txtContent.Text = "Broadcast your thoughts to the Grid...";
            };
            container.Controls.Add(txtContent);

            // ── Topic Search Label ────────────────────────────────────
            Label lblTopic = new Label();
            lblTopic.Text = "Topic";
            lblTopic.ForeColor = Color.LightGray;
            lblTopic.Font = new Font("Segoe UI", 9F);
            lblTopic.AutoSize = true;
            container.Controls.Add(lblTopic);

            // ── Topic Search TextBox ──────────────────────────────────
            txtTopicSearch = new TextBox();
            txtTopicSearch.Text = _username;
            txtTopicSearch.BackColor = textBoxBg;
            txtTopicSearch.ForeColor = neon;
            txtTopicSearch.BorderStyle = BorderStyle.None;
            txtTopicSearch.Font = new Font("Segoe UI", 10F);
            txtTopicSearch.TextChanged += TxtTopicSearch_TextChanged;
            txtTopicSearch.Enter += (s, e) =>
            {
                if (txtTopicSearch.Text == _username)
                    txtTopicSearch.SelectAll();
            };
            container.Controls.Add(txtTopicSearch);

            // ── Topic Suggestions ListBox ─────────────────────────────
            lstTopicSuggestions = new ListBox();
            lstTopicSuggestions.BackColor = Color.FromArgb(10, 24, 40);
            lstTopicSuggestions.ForeColor = neon;
            lstTopicSuggestions.BorderStyle = BorderStyle.FixedSingle;
            lstTopicSuggestions.Font = new Font("Segoe UI", 9.5F);
            lstTopicSuggestions.Visible = false;
            lstTopicSuggestions.Height = 120;
            lstTopicSuggestions.Click += (s, e) =>
            {
                if (lstTopicSuggestions.SelectedItem != null)
                {
                    txtTopicSearch.Text = lstTopicSuggestions.SelectedItem.ToString();
                    lstTopicSuggestions.Visible = false;
                }
            };
            // Bring to front so it overlaps other controls
            container.Controls.Add(lstTopicSuggestions);
            lstTopicSuggestions.BringToFront();

            // ── Buttons ───────────────────────────────────────────────
            Button btnCancel = StyleButton("Cancel", navHover);
            btnCancel.Click += (s, e) => this.Parent?.Controls.Remove(this);
            container.Controls.Add(btnCancel);

            Button btnPost = StyleButton("Publish ⚡", navHover);
            btnPost.Click += BtnPost_Click;
            container.Controls.Add(btnPost);

            // ── Layout ────────────────────────────────────────────────
            Action layoutControls = () =>
            {
                container.Width = this.Width - 40;
                container.Height = Math.Min(this.Height - 40, 520);
                container.Left = 20;
                container.Top = 20;

                int innerWidth = container.Width - innerPadding * 2;

                lblTitle.Location = new Point(innerPadding, innerPadding);

                txtTitle.Location = new Point(innerPadding, lblTitle.Bottom + 10);
                txtTitle.Width = innerWidth;
                txtTitle.Height = 30;

                txtContent.Location = new Point(innerPadding, txtTitle.Bottom + 10);
                txtContent.Width = innerWidth;
                txtContent.Height = 180;

                lblTopic.Location = new Point(innerPadding, txtContent.Bottom + 12);

                txtTopicSearch.Location = new Point(innerPadding, lblTopic.Bottom + 4);
                txtTopicSearch.Width = 220;
                txtTopicSearch.Height = 28;

                lstTopicSuggestions.Location = new Point(innerPadding, txtTopicSearch.Bottom + 2);
                lstTopicSuggestions.Width = 220;

                int bottomPadding = 20;
                btnPost.Left = container.Width - btnPost.Width - innerPadding;
                btnPost.Top = container.Height - btnPost.Height - bottomPadding;

                btnCancel.Left = btnPost.Left - btnCancel.Width - 10;
                btnCancel.Top = btnPost.Top;
            };

            this.Resize += (s, e) => layoutControls();
            layoutControls();

            // Load initial suggestions (followed topics first)
            LoadTopicSuggestions("");
        }

        private void TxtTopicSearch_TextChanged(object sender, EventArgs e)
        {
            string query = txtTopicSearch.Text.Trim();
            LoadTopicSuggestions(query);
            lstTopicSuggestions.Visible = lstTopicSuggestions.Items.Count > 0;
        }

        private void LoadTopicSuggestions(string query)
        {
            lstTopicSuggestions.Items.Clear();

            // Followed topics first
            var followedTopics = Database.GetFollowedTopics(_username);
            foreach (var topic in followedTopics)
            {
                if (string.IsNullOrEmpty(query) ||
                    topic.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    lstTopicSuggestions.Items.Add(topic);
                }
            }

            // Then all other topics not already added
            var allTopics = Database.GetTopics();
            foreach (var topic in allTopics)
            {
                if (!lstTopicSuggestions.Items.Contains(topic))
                {
                    if (string.IsNullOrEmpty(query) ||
                        topic.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        lstTopicSuggestions.Items.Add(topic);
                    }
                }
            }

            // Always include username as an option
            if (!lstTopicSuggestions.Items.Contains(_username))
            {
                if (string.IsNullOrEmpty(query) ||
                    _username.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    lstTopicSuggestions.Items.Insert(0, _username);
                }
            }
        }

        private Button StyleButton(string text, Color hover)
        {
            Color neon = Color.FromArgb(0, 234, 255);
            Button btn = new Button();
            btn.Text = text;
            btn.Size = new Size(90, 32);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = neon;
            btn.FlatAppearance.MouseOverBackColor = hover;
            btn.FlatAppearance.MouseDownBackColor = hover;
            btn.BackColor = Color.Transparent;
            btn.ForeColor = neon;
            btn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            return btn;
        }

        private void BtnPost_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text) || txtTitle.Text == "Topic Title")
            {
                NeonMessageBox.Show("Please enter a title.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtContent.Text) || txtContent.Text.Contains("Broadcast"))
            {
                NeonMessageBox.Show("Please enter some content.");
                return;
            }

            string topic = string.IsNullOrWhiteSpace(txtTopicSearch.Text)
                ? _username
                : txtTopicSearch.Text.Trim();

            string title = txtTitle.Text.Trim();
            string content = txtContent.Text.Trim();

            // ── Moderation ────────────────────────────────────────────
            var modResult = ModerationService.CheckContent(title, content);
            if (!modResult.IsClean)
            {
                string warning = ModerationService.FlagUser(_username, modResult.Reason);
                NeonMessageBox.Show(warning);
                return;
            }

            var topicMod = ModerationService.CheckTopicName(topic);
            if (!topicMod.IsClean)
            {
                string warning = ModerationService.FlagUser(_username, topicMod.Reason);
                NeonMessageBox.Show(warning);
                return;
            }

            Database.CreatePost(_username, title, content, topic);
            OnPostCreated?.Invoke();
            NeonMessageBox.Show("Post created!");
        }

        private void DrawBorder(Graphics g, Rectangle rect, Color borderColor)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (Pen p = new Pen(borderColor, 2))
                g.DrawRectangle(p, rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2);
        }
    }
}