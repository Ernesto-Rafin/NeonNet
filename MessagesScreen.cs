using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NeonNet3.Data;

namespace NeonNet3
{
    public partial class MessagesScreen : UserControl
    {
        private string _username;
        private string _selectedConversation = null;
        private Panel pnlConversations;
        private Panel pnlChat;
        private Panel pnlInputBar;
        private Panel pnlNewMessage;
        private ListBox lstConversations;
        private RichTextBox rtbMessages;
        private TextBox txtMessageInput;
        private TextBox txtReceiverInput;
        private Button btnSend;
        private Button btnNewMessage;
        private Label lblHeader;

        public MessagesScreen(string username)
        {
            _username = username;
            SetupScreen();
        }

        private void SetupScreen()
        {
            this.BackColor = Color.FromArgb(5, 11, 22);
            this.Dock = DockStyle.Fill;

            // ── Header ──────────────────────────────────────────────
            lblHeader = new Label();
            lblHeader.Text = "MESSAGES";
            lblHeader.ForeColor = Color.FromArgb(0, 234, 255);
            lblHeader.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblHeader.AutoSize = true;
            lblHeader.Location = new Point(20, 15);
            this.Controls.Add(lblHeader);

            // ── New Message button ───────────────────────────────────
            btnNewMessage = new Button();
            btnNewMessage.Text = "+ NEW";
            btnNewMessage.ForeColor = Color.FromArgb(0, 234, 255);
            btnNewMessage.BackColor = Color.Transparent;
            btnNewMessage.FlatStyle = FlatStyle.Flat;
            btnNewMessage.FlatAppearance.BorderColor = Color.FromArgb(0, 234, 255);
            btnNewMessage.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnNewMessage.Size = new Size(70, 28);
            btnNewMessage.Location = new Point(20, 50);
            btnNewMessage.Cursor = Cursors.Hand;
            btnNewMessage.Click += BtnNewMessage_Click;
            this.Controls.Add(btnNewMessage);

            // ── New message receiver input ────────────────────────────
            pnlNewMessage = new Panel();
            pnlNewMessage.BackColor = Color.FromArgb(8, 18, 32);
            pnlNewMessage.Location = new Point(20, 85);
            pnlNewMessage.Size = new Size(200, 45);
            pnlNewMessage.Visible = false;
            pnlNewMessage.Paint += (s, e) =>
            {
                Color neon = Color.FromArgb(0, 234, 255);
                using (var p = new System.Drawing.Pen(neon, 1))
                    e.Graphics.DrawRectangle(p, 0, 0, pnlNewMessage.Width - 1, pnlNewMessage.Height - 1);
            };
            this.Controls.Add(pnlNewMessage);

            txtReceiverInput = new TextBox();
            txtReceiverInput.BackColor = Color.FromArgb(8, 18, 32);
            txtReceiverInput.ForeColor = Color.FromArgb(0, 234, 255);
            txtReceiverInput.Font = new Font("Segoe UI", 9.5F);
            txtReceiverInput.BorderStyle = BorderStyle.None;
            txtReceiverInput.Location = new Point(8, 12);
            txtReceiverInput.Size = new Size(185, 20);
            txtReceiverInput.Text = "Enter username...";
            txtReceiverInput.Enter += (s, e) => { if (txtReceiverInput.Text == "Enter username...") txtReceiverInput.Text = ""; };
            txtReceiverInput.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(txtReceiverInput.Text)) txtReceiverInput.Text = "Enter username..."; };
            txtReceiverInput.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                    StartConversation(txtReceiverInput.Text.Trim());
            };
            pnlNewMessage.Controls.Add(txtReceiverInput);

            // ── Conversations panel (left side) ──────────────────────
            pnlConversations = new Panel();
            pnlConversations.BackColor = Color.FromArgb(8, 18, 32);
            pnlConversations.Location = new Point(20, 140);
            pnlConversations.Size = new Size(200, this.Height - 160);
            pnlConversations.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
            pnlConversations.Paint += (s, e) =>
            {
                Color neon = Color.FromArgb(0, 234, 255);
                using (var p = new System.Drawing.Pen(neon, 1))
                    e.Graphics.DrawRectangle(p, 0, 0, pnlConversations.Width - 1, pnlConversations.Height - 1);
            };
            this.Controls.Add(pnlConversations);

            lstConversations = new ListBox();
            lstConversations.BackColor = Color.FromArgb(8, 18, 32);
            lstConversations.ForeColor = Color.FromArgb(0, 234, 255);
            lstConversations.Font = new Font("Segoe UI", 9.5F);
            lstConversations.BorderStyle = BorderStyle.None;
            lstConversations.Dock = DockStyle.Fill;
            lstConversations.SelectedIndexChanged += LstConversations_SelectedIndexChanged;
            pnlConversations.Controls.Add(lstConversations);

            // ── Chat panel (right side) ──────────────────────────────
            pnlChat = new Panel();
            pnlChat.BackColor = Color.FromArgb(5, 11, 22);
            pnlChat.Location = new Point(235, 55);
            pnlChat.Size = new Size(this.Width - 260, this.Height - 80);
            pnlChat.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
            pnlChat.Paint += (s, e) =>
            {
                Color neon = Color.FromArgb(0, 234, 255);
                using (var p = new System.Drawing.Pen(neon, 1))
                    e.Graphics.DrawRectangle(p, 0, 0, pnlChat.Width - 1, pnlChat.Height - 1);
            };
            this.Controls.Add(pnlChat);

            // ── Bottom input bar ─────────────────────────────────────
            pnlInputBar = new Panel();
            pnlInputBar.BackColor = Color.FromArgb(8, 18, 32);
            pnlInputBar.Dock = DockStyle.Bottom;
            pnlInputBar.Height = 50;
            pnlChat.Controls.Add(pnlInputBar);

            // ── Messages display ─────────────────────────────────────
            rtbMessages = new RichTextBox();
            rtbMessages.BackColor = Color.FromArgb(5, 11, 22);
            rtbMessages.ForeColor = Color.FromArgb(0, 234, 255);
            rtbMessages.Font = new Font("Segoe UI", 9.5F);
            rtbMessages.BorderStyle = BorderStyle.None;
            rtbMessages.ReadOnly = true;
            rtbMessages.Dock = DockStyle.Fill;
            rtbMessages.Text = "Select a conversation to start messaging";
            pnlChat.Controls.Add(rtbMessages);

            // ── Message input ────────────────────────────────────────
            txtMessageInput = new TextBox();
            txtMessageInput.BackColor = Color.FromArgb(8, 18, 32);
            txtMessageInput.ForeColor = Color.FromArgb(0, 234, 255);
            txtMessageInput.Font = new Font("Segoe UI", 9.5F);
            txtMessageInput.BorderStyle = BorderStyle.FixedSingle;
            txtMessageInput.Location = new Point(10, 10);
            txtMessageInput.Size = new Size(pnlInputBar.Width - 100, 30);
            txtMessageInput.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            pnlInputBar.Controls.Add(txtMessageInput);

            // ── Send button ──────────────────────────────────────────
            btnSend = new Button();
            btnSend.Text = "SEND";
            btnSend.ForeColor = Color.FromArgb(0, 234, 255);
            btnSend.BackColor = Color.Transparent;
            btnSend.FlatStyle = FlatStyle.Flat;
            btnSend.FlatAppearance.BorderColor = Color.FromArgb(0, 234, 255);
            btnSend.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnSend.Size = new Size(70, 30);
            btnSend.Location = new Point(10, 10);
            btnSend.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnSend.Cursor = Cursors.Hand;
            btnSend.Click += BtnSend_Click;
            pnlInputBar.Controls.Add(btnSend);

            pnlInputBar.Layout += (s, e) =>
            {
                txtMessageInput.Width = pnlInputBar.Width - 100;
                btnSend.Left = pnlInputBar.Width - 85;
                btnSend.Top = 10;
                txtMessageInput.Top = 10;
            };

            // Load conversations
            LoadConversations();
        }

        private void LoadConversations()
        {
            lstConversations.Items.Clear();
            List<string> conversations = Database.GetConversations(_username);
            if (conversations.Count == 0)
                lstConversations.Items.Add("No conversations yet");
            else
                foreach (string c in conversations)
                    lstConversations.Items.Add(c);
        }

        private void LstConversations_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstConversations.SelectedItem == null) return;
            string selected = lstConversations.SelectedItem.ToString();
            if (selected == "No conversations yet") return;

            _selectedConversation = selected;
            LoadMessages(_selectedConversation);
        }

        private void LoadMessages(string otherUser)
        {
            rtbMessages.Clear();
            List<Database.MessageItem> messages = Database.GetMessages(_username, otherUser);
            if (messages.Count == 0)
            {
                rtbMessages.Text = $"No messages yet with {otherUser}. Say hello!";
                return;
            }
            foreach (var msg in messages)
            {
                string time = msg.SentAt.ToString("hh:mm tt");
                rtbMessages.AppendText($"[{time}] {msg.Sender}: {msg.Content}\n");
            }
            rtbMessages.ScrollToCaret();
        }

        private void BtnNewMessage_Click(object sender, EventArgs e)
        {
            pnlNewMessage.Visible = !pnlNewMessage.Visible;
            if (pnlNewMessage.Visible)
                txtReceiverInput.Focus();
        }

        private void StartConversation(string receiver)
        {
            if (string.IsNullOrWhiteSpace(receiver) || receiver == "Enter username...") return;

            if (!Database.UserExists(receiver))
            {
                NeonMessageBox.Show($"User '@{receiver}' does not exist.");
                return;
            }

            if (receiver == _username)
            {
                NeonMessageBox.Show("You cannot message yourself.");
                return;
            }

            _selectedConversation = receiver;
            pnlNewMessage.Visible = false;
            txtReceiverInput.Text = "Enter username...";

            if (!lstConversations.Items.Contains(receiver))
            {
                if (lstConversations.Items.Contains("No conversations yet"))
                    lstConversations.Items.Clear();
                lstConversations.Items.Insert(0, receiver);
            }

            lstConversations.SelectedItem = receiver;
            LoadMessages(receiver);
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMessageInput.Text)) return;

            if (_selectedConversation == null)
            {
                NeonMessageBox.Show("Please select or start a conversation first.");
                return;
            }

            string content = txtMessageInput.Text.Trim();
            Database.SendMessage(_username, _selectedConversation, content);
            txtMessageInput.Clear();
            LoadMessages(_selectedConversation);
        }
    }
}