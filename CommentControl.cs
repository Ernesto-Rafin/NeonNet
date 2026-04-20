using System;
using System.Drawing;
using System.Windows.Forms;

namespace NeonNet3
{
    public class CommentControl : UserControl
    {
        public int CommentId { get; private set; }

        public CommentControl(int commentId, string username, string text, DateTime time, string currentUser, Image pfp = null)
        {
            CommentId = commentId;
            this.Width = 500;
            this.BackColor = Color.Transparent;

            Color neon = Color.FromArgb(0, 234, 255);

            // ── Profile Picture ──────────────────────────────────────
            PictureBox pic = new PictureBox();
            pic.Size = new Size(40, 40);
            pic.Location = new Point(0, 5);
            pic.SizeMode = PictureBoxSizeMode.StretchImage;
            pic.Image = pfp ?? SystemIcons.Information.ToBitmap();
            pic.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddEllipse(0, 0, pic.Width - 1, pic.Height - 1);
                    pic.Region = new Region(path);
                }
            };

            // ── Username + Timestamp ─────────────────────────────────
            Label lblMeta = new Label();
            lblMeta.Text = $"{username} • {time:g}";
            lblMeta.ForeColor = Color.Gray;
            lblMeta.Font = new Font("Segoe UI", 8F);
            lblMeta.AutoSize = false;
            lblMeta.Width = 300;
            lblMeta.Location = new Point(50, 5);

            // ── Comment Bubble ───────────────────────────────────────
            Panel bubble = new Panel();
            bubble.BackColor = Color.FromArgb(12, 28, 48);
            bubble.Location = new Point(50, 25);
            bubble.AutoSize = true;
            bubble.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            bubble.Padding = new Padding(8);

            Label lblText = new Label();
            lblText.Text = text;
            lblText.ForeColor = Color.White;
            lblText.Font = new Font("Segoe UI", 10F);
            lblText.AutoSize = true;
            lblText.MaximumSize = new Size(500, 0);
            bubble.Controls.Add(lblText);

            this.Controls.Add(pic);
            this.Controls.Add(lblMeta);
            this.Controls.Add(bubble);

            // ── Edit / Delete (own comments only) ────────────────────
            if (username == currentUser)
            {
                Button btnEdit = new Button();
                btnEdit.Text = "✏";
                btnEdit.Size = new Size(24, 20);
                btnEdit.FlatStyle = FlatStyle.Flat;
                btnEdit.FlatAppearance.BorderSize = 0;
                btnEdit.ForeColor = Color.Gray;
                btnEdit.BackColor = Color.Transparent;
                btnEdit.Cursor = Cursors.Hand;

                Button btnDelete = new Button();
                btnDelete.Text = "🗑";
                btnDelete.Size = new Size(24, 20);
                btnDelete.FlatStyle = FlatStyle.Flat;
                btnDelete.FlatAppearance.BorderSize = 0;
                btnDelete.ForeColor = Color.Red;
                btnDelete.BackColor = Color.Transparent;
                btnDelete.Cursor = Cursors.Hand;

                // ── Position buttons on the right ────────────────────
                this.Resize += (s, e) =>
                {
                    btnDelete.Location = new Point(this.Width - 35, 2);
                    btnEdit.Location = new Point(this.Width - 65, 2);
                };
                this.OnResize(EventArgs.Empty);

                // ── Edit logic ───────────────────────────────────────
                btnEdit.Click += (s, e) =>
                {
                    bool isEditing = btnEdit.Tag?.ToString() == "editing";

                    if (!isEditing)
                    {
                        // Switch to edit mode
                        TextBox txtEdit = new TextBox();
                        txtEdit.Text = lblText.Text;
                        txtEdit.BackColor = Color.FromArgb(20, 40, 60);
                        txtEdit.ForeColor = Color.White;
                        txtEdit.BorderStyle = BorderStyle.None;
                        txtEdit.Font = new Font("Segoe UI", 10F);
                        txtEdit.Width = lblText.MaximumSize.Width;
                        txtEdit.Multiline = true;
                        txtEdit.Name = "txtEdit";

                        bubble.Controls.Remove(lblText);
                        bubble.Controls.Add(txtEdit);

                        btnEdit.Text = "💾";
                        btnEdit.Tag = "editing";
                    }
                    else
                    {
                        // Save edit
                        TextBox txtEdit = bubble.Controls["txtEdit"] as TextBox;
                        if (txtEdit != null && !string.IsNullOrWhiteSpace(txtEdit.Text))
                        {
                            string newText = txtEdit.Text.Trim();
                            Data.Database.EditComment(CommentId, newText);
                            lblText.Text = newText;
                            bubble.Controls.Remove(txtEdit);
                            bubble.Controls.Add(lblText);
                        }
                        btnEdit.Text = "✏";
                        btnEdit.Tag = null;
                    }
                };

                // ── Delete logic ─────────────────────────────────────
                btnDelete.Click += (s, e) =>
                {
                    var confirm = MessageBox.Show(
                        "Delete this comment?", "Confirm",
                        MessageBoxButtons.YesNo);

                    if (confirm == DialogResult.Yes)
                    {
                        Data.Database.DeleteComment(CommentId);
                        this.Parent?.Controls.Remove(this);
                        this.Dispose();
                    }
                };

                this.Controls.Add(btnEdit);
                this.Controls.Add(btnDelete);
            }

            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }
    }
}