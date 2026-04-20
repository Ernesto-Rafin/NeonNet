using System;
using System.Drawing;
using System.Windows.Forms;
using NeonNet3.Data;

namespace NeonNet3
{
    public class CreateTopicScreen : UserControl
    {
        private string _username;
        TextBox txtTopicName;

        // Callback to notify that a topic was created
        public Action OnTopicCreated;

        public CreateTopicScreen(string username)
        {
            _username = username;
            SetupUI();
        }

        private void SetupUI()
        {
            this.BackColor = Color.FromArgb(8, 18, 32);

            Color neon = Color.FromArgb(0, 234, 255);

            Label lblTitle = new Label();
            lblTitle.Text = "Create Topic";
            lblTitle.ForeColor = neon;
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.Location = new Point(20, 20);
            lblTitle.AutoSize = true;

            txtTopicName = new TextBox();
            txtTopicName.Location = new Point(20, 70);
            txtTopicName.Size = new Size(300, 30);
            txtTopicName.BackColor = Color.FromArgb(6, 10, 22);
            txtTopicName.ForeColor = neon;
            txtTopicName.BorderStyle = BorderStyle.FixedSingle;
            txtTopicName.Font = new Font("Segoe UI", 10F);

            Button btnCreate = new Button();
            btnCreate.Text = "Create";
            btnCreate.Location = new Point(20, 110);
            btnCreate.BackColor = neon;
            btnCreate.ForeColor = Color.FromArgb(8, 18, 32);
            btnCreate.FlatStyle = FlatStyle.Flat;
            btnCreate.FlatAppearance.BorderSize = 0;
            btnCreate.Cursor = Cursors.Hand;

            // ===== CLICK =====
            btnCreate.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtTopicName.Text))
                {
                    NeonMessageBox.Show("Enter a topic name.");
                    return;
                }

                try
                {
                    // Save topic to database
                    Database.CreateTopic(txtTopicName.Text, _username);

                    // Notify HomeScreen (or whoever subscribed) that a topic was created
                    OnTopicCreated?.Invoke();

                    // Clear textbox and confirm creation
                    txtTopicName.Text = "";
                    NeonMessageBox.Show("Topic created!");
                }
                catch (Exception ex)
                {
                    NeonMessageBox.Show("Error creating topic: " + ex.Message);
                }
            };

            this.Controls.Add(lblTitle);
            this.Controls.Add(txtTopicName);
            this.Controls.Add(btnCreate);
        }
    }
}