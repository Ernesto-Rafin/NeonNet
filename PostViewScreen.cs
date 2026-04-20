using System;
using System.Drawing;
using System.Windows.Forms;

namespace NeonNet3
{
    public class PostViewScreen : UserControl
    {
        private int _postId;
        private string _title;
        private string _content;
        private string _username;
        private DateTime _createdAt;
        private string _loggedInUser;
        private bool _isOwner;
        private TextBox txtComment;
        private Button btnComment;
        private Panel pnlComments;

        public event Action OnBack;

        public PostViewScreen(int postId, string author, string title, string content, DateTime createdAt, string loggedInUser)
        {
            _postId = postId;
            _username = author;
            _title = title;
            _content = content;
            _createdAt = createdAt;
            _loggedInUser = loggedInUser;
            _isOwner = (_username == _loggedInUser);

            SetupUI();
        }

        private void SetupUI()
        {
            this.BackColor = Color.FromArgb(8, 18, 32);

            Color neon = Color.FromArgb(0, 234, 255);

            // ── Back Button ──────────────────────────────────────────
            Button btnBack = new Button();
            btnBack.Text = "← Back";
            btnBack.ForeColor = neon;
            btnBack.BackColor = Color.Transparent;
            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.FlatAppearance.BorderSize = 1;
            btnBack.FlatAppearance.BorderColor = neon;
            btnBack.Size = new Size(80, 30);
            btnBack.Location = new Point(20, 20);
            btnBack.Click += (s, e) => OnBack?.Invoke();

            // ── Edit Button ──────────────────────────────────────────
            Button btnEdit = new Button();
            btnEdit.Text = "Edit";
            btnEdit.ForeColor = Color.White;
            btnEdit.BackColor = Color.Transparent;
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.FlatAppearance.BorderSize = 1;
            btnEdit.FlatAppearance.BorderColor = neon;
            btnEdit.Size = new Size(60, 30);
            btnEdit.Location = new Point(120, 20);
            btnEdit.Visible = _isOwner;

            // ── Delete Button ────────────────────────────────────────
            Button btnDelete = new Button();
            btnDelete.Text = "Delete";
            btnDelete.ForeColor = Color.Red;
            btnDelete.BackColor = Color.Transparent;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.FlatAppearance.BorderSize = 1;
            btnDelete.FlatAppearance.BorderColor = Color.Red;
            btnDelete.Size = new Size(60, 30);
            btnDelete.Location = new Point(200, 20);
            btnDelete.Visible = _isOwner;

            // ── Title ────────────────────────────────────────────────
            Label lblTitle = new Label();
            lblTitle.Text = _title;
            lblTitle.ForeColor = neon;
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.AutoSize = false;
            lblTitle.Width = 600;
            lblTitle.Location = new Point(20, 70);

            // ── Meta ─────────────────────────────────────────────────
            Label lblMeta = new Label();
            lblMeta.Text = $"@{_username} • {_createdAt}";
            lblMeta.ForeColor = Color.Gray;
            lblMeta.Font = new Font("Segoe UI", 9F);
            lblMeta.AutoSize = true;
            lblMeta.Location = new Point(20, 110);

            // ── Content ──────────────────────────────────────────────
            TextBox txtContent = new TextBox();
            txtContent.Text = _content;
            txtContent.Multiline = true;
            txtContent.ReadOnly = true;
            txtContent.ScrollBars = ScrollBars.Vertical;
            txtContent.BackColor = Color.FromArgb(12, 28, 48);
            txtContent.ForeColor = Color.White;
            txtContent.BorderStyle = BorderStyle.None;
            txtContent.Font = new Font("Segoe UI", 11F);
            txtContent.Location = new Point(20, 140);
            txtContent.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtContent.Width = this.Width - txtContent.Left - 20;
            txtContent.Height = this.Height - txtContent.Top - 200;

            // ── Comment Input ─────────────────────────────────────────
            txtComment = new TextBox();
            txtComment.Text = "Write a comment...";
            txtComment.ForeColor = Color.Gray;
            txtComment.BackColor = Color.FromArgb(12, 28, 48);
            txtComment.BorderStyle = BorderStyle.None;
            txtComment.Font = new Font("Segoe UI", 10F);
            txtComment.Location = new Point(20, this.Height - 100);
            txtComment.Width = 400;
            txtComment.Height = 30;

            txtComment.GotFocus += (s, e) =>
            {
                if (txtComment.Text == "Write a comment...")
                {
                    txtComment.Text = "";
                    txtComment.ForeColor = Color.White;
                }
            };
            txtComment.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtComment.Text))
                {
                    txtComment.Text = "Write a comment...";
                    txtComment.ForeColor = Color.Gray;
                }
            };

            // ── Comment Button ────────────────────────────────────────
            btnComment = new Button();
            btnComment.Text = "Post";
            btnComment.FlatStyle = FlatStyle.Flat;
            btnComment.FlatAppearance.BorderSize = 0;
            btnComment.BackColor = Color.FromArgb(0, 234, 255);
            btnComment.ForeColor = Color.Black;
            btnComment.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnComment.Cursor = Cursors.Hand;
            btnComment.Location = new Point(430, this.Height - 100);
            btnComment.Size = new Size(80, 32);
            btnComment.TextAlign = ContentAlignment.MiddleCenter;

            // ── Comments Panel ────────────────────────────────────────
            pnlComments = new Panel();
            pnlComments.Location = new Point(20, this.Height - 260);
            pnlComments.Size = new Size(this.Width - 40, 150);
            pnlComments.AutoScroll = true;
            pnlComments.BackColor = Color.FromArgb(6, 10, 22);
            pnlComments.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            // ── Post Comment Click ────────────────────────────────────
            btnComment.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtComment.Text) || txtComment.Text == "Write a comment...")
                    return;

                if (txtComment.Text.Length > 2000)
                {
                    MessageBox.Show("Comment too long (max 2000 characters)");
                    return;
                }

                int newId = NeonNet3.Data.Database.AddComment(_postId, _loggedInUser, txtComment.Text);
                AddCommentToUI(newId, _loggedInUser, txtComment.Text, DateTime.Now);

                txtComment.Text = "Write a comment...";
                txtComment.ForeColor = Color.Gray;
            };

            // ── Load Existing Comments ────────────────────────────────
            var comments = NeonNet3.Data.Database.GetComments(_postId);
            foreach (var c in comments)
                AddCommentToUI(c.Id, c.Username, c.Content, c.CreatedAt);

            // ── Edit Post Logic ───────────────────────────────────────
            bool editing = false;
            btnEdit.Click += (s, e) =>
            {
                if (!_isOwner) return;
                editing = !editing;
                txtContent.ReadOnly = !editing;
                txtContent.BackColor = editing ? Color.FromArgb(20, 40, 60) : Color.FromArgb(6, 10, 22);
                btnEdit.Text = editing ? "Save" : "Edit";
                if (!editing)
                {
                    _content = txtContent.Text;
                    NeonNet3.Data.Database.UpdatePost(_postId, _content);
                }
            };

            // ── Delete Post Logic ─────────────────────────────────────
            btnDelete.Click += (s, e) =>
            {
                if (!_isOwner) return;
                var confirm = MessageBox.Show("Delete this post?", "Confirm", MessageBoxButtons.YesNo);
                if (confirm == DialogResult.Yes)
                {
                    NeonNet3.Data.Database.DeletePost(_postId);
                    OnBack?.Invoke();
                }
            };

            // ── Resize Logic ──────────────────────────────────────────
            this.Resize += (s, e) =>
            {
                int margin = 20;
                int commentBoxHeight = 30;
                int commentsHeight = 150;
                int spacing = 10;

                int commentTop = this.Height - margin - commentBoxHeight;
                int commentsTop = commentTop - spacing - commentsHeight;

                txtContent.Width = this.Width - txtContent.Left - margin;
                txtContent.Height = commentsTop - txtContent.Top - spacing;

                txtComment.Location = new Point(20, commentTop);
                txtComment.Width = this.Width - 120;

                btnComment.Location = new Point(txtComment.Right + 10, commentTop);
                btnComment.Size = new Size(80, 32);

                pnlComments.Location = new Point(20, commentsTop);
                pnlComments.Size = new Size(this.Width - 40, commentsHeight);
            };

            // ── Add Controls ──────────────────────────────────────────
            this.Controls.Add(btnBack);
            this.Controls.Add(btnEdit);
            this.Controls.Add(btnDelete);
            this.Controls.Add(lblTitle);
            this.Controls.Add(lblMeta);
            this.Controls.Add(txtContent);
            this.Controls.Add(txtComment);
            this.Controls.Add(btnComment);
            this.Controls.Add(pnlComments);
            this.PerformLayout();
            this.OnResize(EventArgs.Empty);
        }

        private void AddCommentToUI(int commentId, string username, string text, DateTime time)
        {
            Image pfp = null;
            try
            {
                string path = NeonNet3.Data.Database.GetProfileImagePath(username);
                if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
                {
                    using (var original = new Bitmap(path))
                        pfp = new Bitmap(original);
                }
            }
            catch { }

            CommentControl comment = new CommentControl(commentId, username, text, time, _loggedInUser, pfp);
            comment.Width = pnlComments.Width - 25;

            int y = 0;
            foreach (Control c in pnlComments.Controls)
                y = Math.Max(y, c.Bottom + 8);

            comment.Location = new Point(0, y);
            pnlComments.Controls.Add(comment);
        }
    }
}