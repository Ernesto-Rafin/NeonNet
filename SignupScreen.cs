using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using NeonNet3.Data;

namespace NeonNet3
{
    public partial class SignupScreen : UserControl
    {
        public SignupScreen()
        {
            InitializeComponent();

            // ── Box aesthetics ────────────────────────────────────────
            pnlSignupBox.BorderStyle = BorderStyle.None;
            SetRoundedRegion(pnlSignupBox, 8);
            CenterRegisterBox();
            pnlSignupBox.Paint += pnlLoginBox_Paint;

            // ── Events ────────────────────────────────────────────────
            signupButton.Click += SignupButton_Click;
            returnLinkLabel.Click += ReturnLinkLabel_Click;

            passwordEntry.UseSystemPasswordChar = true;
            confirmPasswordEntry.UseSystemPasswordChar = true;

            usernameEntry.KeyPress += UsernameEntry_KeyPress;
            usernameEntry.TextChanged += UsernameEntry_TextChanged;

            passwordEntry.KeyPress += PasswordEntry_KeyPress;
            confirmPasswordEntry.KeyPress += PasswordEntry_KeyPress;

            // ── Terms link — show popup ───────────────────────────────
            lnkTerms.Click += (s, e) => ShowTermsPopup();
        }

        private void ShowTermsPopup()
        {
            Color neon = Color.FromArgb(0, 234, 255);

            Form termsForm = new Form();
            termsForm.Text = "Terms and Conditions";
            termsForm.Size = new Size(500, 400);
            termsForm.BackColor = Color.FromArgb(8, 18, 32);
            termsForm.ForeColor = neon;
            termsForm.StartPosition = FormStartPosition.CenterParent;
            termsForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            termsForm.MaximizeBox = false;
            termsForm.MinimizeBox = false;

            RichTextBox rtb = new RichTextBox();
            rtb.Dock = DockStyle.Fill;
            rtb.BackColor = Color.FromArgb(6, 14, 26);
            rtb.ForeColor = Color.White;
            rtb.Font = new Font("Segoe UI", 9.5F);
            rtb.ReadOnly = true;
            rtb.BorderStyle = BorderStyle.None;
            rtb.Padding = new Padding(10);
            rtb.Text =
                "NEONNET — TERMS AND CONDITIONS\n\n" +
                "1. ACCEPTANCE\n" +
                "By creating an account on NeonNet, you agree to these terms.\n\n" +
                "2. ACCOUNT RESPONSIBILITY\n" +
                "You are responsible for keeping your account credentials secure. " +
                "Do not share your password with anyone.\n\n" +
                "3. ACCEPTABLE USE\n" +
                "You agree not to post content that is harmful, offensive, or violates " +
                "the rights of others. NeonNet reserves the right to remove content " +
                "and suspend accounts that violate these rules.\n\n" +
                "4. PRIVACY\n" +
                "Your data is stored locally on this device. NeonNet does not share " +
                "your personal information with third parties.\n\n" +
                "5. CONTENT OWNERSHIP\n" +
                "You retain ownership of the content you post. By posting, you grant " +
                "NeonNet the right to display your content within the platform.\n\n" +
                "6. TERMINATION\n" +
                "NeonNet reserves the right to suspend or terminate accounts that " +
                "violate these terms.\n\n" +
                "7. CHANGES\n" +
                "These terms may be updated at any time. Continued use of NeonNet " +
                "after changes means you accept the new terms.\n\n" +
                "By checking the box, you confirm that you have read and agree to " +
                "these Terms and Conditions.";

            Button btnClose = new Button();
            btnClose.Text = "Close";
            btnClose.Dock = DockStyle.Bottom;
            btnClose.Height = 36;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderColor = neon;
            btnClose.ForeColor = neon;
            btnClose.BackColor = Color.Transparent;
            btnClose.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnClose.Click += (s, e) => termsForm.Close();

            termsForm.Controls.Add(rtb);
            termsForm.Controls.Add(btnClose);
            termsForm.ShowDialog();
        }

        private void SignupButton_Click(object sender, EventArgs e)
        {
            string emailOrPhone = emailEntry.Text.Trim();
            string username = usernameEntry.Text.Trim();
            string password = passwordEntry.Text;
            string confirmPassword = confirmPasswordEntry.Text;

            // ── Terms check ───────────────────────────────────────────
            if (!chkTerms.Checked)
            {
                NeonMessageBox.Show("You must agree to the Terms and Conditions to register.");
                return;
            }

            // ── Validation ────────────────────────────────────────────
            if (string.IsNullOrEmpty(emailOrPhone) || string.IsNullOrEmpty(username)
                || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                NeonMessageBox.Show("All fields are required.");
                return;
            }

            if (password != confirmPassword)
            {
                NeonMessageBox.Show("Passwords do not match.");
                return;
            }

            if (username.Contains("#") || username.Contains("&") || username.Contains("~"))
            {
                NeonMessageBox.Show("Username cannot contain '#', '&', or '~' characters.");
                return;
            }

            bool isEmail = System.Text.RegularExpressions.Regex.IsMatch(emailOrPhone,
                @"^[a-zA-Z0-9._%+-]+@(gmail\.com|yahoo\.com|outlook\.com|hotmail\.com|icloud\.com|live\.com|msn\.com)$");
            bool isPhone = System.Text.RegularExpressions.Regex.IsMatch(emailOrPhone,
                @"^\+?[\d\s\-]{7,15}$");
            if (!isEmail && !isPhone)
            {
                NeonMessageBox.Show("Please enter a valid email address or phone number.");
                return;
            }

            using (var conn = Database.GetConnection())
            {
                conn.Open();

                string checkQuery = @"
                    SELECT COUNT(*) FROM Users 
                    WHERE Username = @username OR Email = @email OR Phone = @phone;";

                using (var cmd = new System.Data.SQLite.SQLiteCommand(checkQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@email", emailOrPhone);
                    cmd.Parameters.AddWithValue("@phone", emailOrPhone);

                    long count = (long)cmd.ExecuteScalar();
                    if (count > 0)
                    {
                        NeonMessageBox.Show("Username or email/phone already exists.");
                        return;
                    }
                }

                if (username.Length < 3)
                {
                    NeonMessageBox.Show("Username must be at least 3 characters long.");
                    return;
                }

                if (username.Length > 20)
                {
                    NeonMessageBox.Show("Username cannot exceed 20 characters.");
                    return;
                }

                if (password.Length < 8)
                {
                    NeonMessageBox.Show("Password must be at least 8 characters long.");
                    return;
                }

                if (password.Length > 64)
                {
                    NeonMessageBox.Show("Password cannot exceed 64 characters.");
                    return;
                }

                bool hasLetter = password.Any(char.IsLetter);
                bool hasNumber = password.Any(char.IsDigit);
                bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

                if (!hasLetter || !hasNumber || !hasSpecial)
                {
                    NeonMessageBox.Show("Password must contain at least one letter, one number, and one special character.");
                    return;
                }

                string insertQuery = @"
                    INSERT INTO Users (Username, Email, Phone, PasswordHash)
                    VALUES (@username, @email, @phone, @password);";

                using (var cmd = new System.Data.SQLite.SQLiteCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);

                    if (emailOrPhone.Contains("@"))
                    {
                        cmd.Parameters.AddWithValue("@email", emailOrPhone);
                        cmd.Parameters.AddWithValue("@phone", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@email", DBNull.Value);
                        cmd.Parameters.AddWithValue("@phone", emailOrPhone);
                    }

                    cmd.Parameters.AddWithValue("@password", password);
                    cmd.ExecuteNonQuery();
                }
            }

            NeonMessageBox.Show("Account created successfully!");
            ReturnLinkLabel_Click(null, null);
        }

        private bool IsCommonSpecialChar(char c)
        {
            string allowed = "!@#$%^&*()-_.<>?";
            return allowed.Contains(c);
        }

        private void ReturnLinkLabel_Click(object sender, EventArgs e)
        {
            var login = new LoginScreen();
            login.Dock = DockStyle.Fill;

            var mainForm = this.FindForm() as Form1;
            if (mainForm != null)
            {
                ScreenTransition.TransitionScreens(mainForm.pnlMain, this, login);
            }
        }

        private void CenterRegisterBox()
        {
            pnlSignupBox.Left = (this.Width - pnlSignupBox.Width) / 2;
            pnlSignupBox.Top = (this.Height - pnlSignupBox.Height) / 2;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CenterRegisterBox();
            SetRoundedRegion(pnlSignupBox, 8);
        }

        private void SetRoundedRegion(Control control, int radius)
        {
            Rectangle bounds = new Rectangle(0, 0, control.Width, control.Height);
            int diameter = radius * 2;

            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            control.Region = new Region(path);
        }

        private void pnlLoginBox_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Color neon = Color.FromArgb(0, 234, 255);
            int radius = 8;
            int glowSize = 12;

            Rectangle borderRect = new Rectangle(
                glowSize / 2,
                glowSize / 2,
                pnlSignupBox.Width - glowSize,
                pnlSignupBox.Height - glowSize
            );

            for (int i = 0; i < glowSize; i += 3)
            {
                using (Pen glowPen = new Pen(Color.FromArgb(30 - i * 2, neon), 3))
                {
                    Rectangle glowRect = new Rectangle(
                        borderRect.X - i,
                        borderRect.Y - i,
                        borderRect.Width + i * 2,
                        borderRect.Height + i * 2
                    );
                    DrawRoundedRectangle(e.Graphics, glowPen, glowRect, radius);
                }
            }

            using (Pen borderPen = new Pen(neon, 1.5f))
                DrawRoundedRectangle(e.Graphics, borderPen, borderRect, radius);
        }

        private void DrawRoundedRectangle(Graphics g, Pen pen, Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            g.DrawPath(pen, path);
        }

        private void neonTextBoarder_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Color neon = Color.FromArgb(0, 234, 255);
            using (Pen p = new Pen(neon, 2))
                e.Graphics.DrawRectangle(p, 0, 0, panel1.Width - 1, panel1.Height - 1);
        }

        private void UsernameEntry_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && usernameEntry.Text.Length >= 20)
            {
                e.Handled = true;
                return;
            }
            if (char.IsControl(e.KeyChar))
                return;
            if (!char.IsLetterOrDigit(e.KeyChar) && e.KeyChar != '_')
                e.Handled = true;
        }

        private void UsernameEntry_TextChanged(object sender, EventArgs e)
        {
            string filtered = new string(usernameEntry.Text
                .Where(c => char.IsLetterOrDigit(c) || c == '_')
                .Take(20)
                .ToArray());

            if (usernameEntry.Text != filtered)
            {
                int cursor = usernameEntry.SelectionStart;
                usernameEntry.Text = filtered;
                usernameEntry.SelectionStart = Math.Min(cursor, filtered.Length);
            }
        }

        private void PasswordEntry_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox box = sender as TextBox;

            if (!char.IsControl(e.KeyChar) && box.Text.Length >= 64)
            {
                e.Handled = true;
                return;
            }
            if (char.IsControl(e.KeyChar))
                return;
            if (char.IsWhiteSpace(e.KeyChar))
            {
                e.Handled = true;
                return;
            }
            if (e.KeyChar == '/' || e.KeyChar == '\\')
            {
                e.Handled = true;
                return;
            }
            if (!char.IsLetterOrDigit(e.KeyChar) && !IsCommonSpecialChar(e.KeyChar))
                e.Handled = true;
        }
    }
}
