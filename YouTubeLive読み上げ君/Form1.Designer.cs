namespace YouTubeLive読み上げ君
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            listChat = new ListBox();
            btnGetChannelId = new Button();
            txtChannelName = new TextBox();
            listBoxChannels = new ListBox();
            comboBox1 = new ComboBox();
            SuspendLayout();
            // 
            // listChat
            // 
            listChat.Dock = DockStyle.Right;
            listChat.FormattingEnabled = true;
            listChat.ItemHeight = 15;
            listChat.Location = new Point(270, 0);
            listChat.Name = "listChat";
            listChat.Size = new Size(514, 561);
            listChat.TabIndex = 0;
            // 
            // btnGetChannelId
            // 
            btnGetChannelId.Location = new Point(189, 12);
            btnGetChannelId.Name = "btnGetChannelId";
            btnGetChannelId.Size = new Size(75, 23);
            btnGetChannelId.TabIndex = 1;
            btnGetChannelId.Text = "チャンネルID取得";
            btnGetChannelId.UseVisualStyleBackColor = true;
            btnGetChannelId.Click += btnGetChannelId_Click;
            // 
            // txtChannelName
            // 
            txtChannelName.Location = new Point(12, 12);
            txtChannelName.Name = "txtChannelName";
            txtChannelName.Size = new Size(171, 23);
            txtChannelName.TabIndex = 2;
            // 
            // listBoxChannels
            // 
            listBoxChannels.FormattingEnabled = true;
            listBoxChannels.ItemHeight = 15;
            listBoxChannels.Location = new Point(12, 41);
            listBoxChannels.Name = "listBoxChannels";
            listBoxChannels.Size = new Size(252, 124);
            listBoxChannels.TabIndex = 3;
            listBoxChannels.SelectedIndexChanged += listBoxChannels_SelectedIndexChanged;
            // 
            // comboBox1
            // 
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(12, 171);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(252, 23);
            comboBox1.TabIndex = 4;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 561);
            Controls.Add(comboBox1);
            Controls.Add(listBoxChannels);
            Controls.Add(txtChannelName);
            Controls.Add(btnGetChannelId);
            Controls.Add(listChat);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Form1";
            Text = "YouTube Live 読み上げ君";
            ResumeLayout(false);
            PerformLayout();
        }

        private ListBox listChat;
        private Button btnGetChannelId;
        private TextBox txtChannelName;
        private ListBox listBoxChannels;
        private ComboBox comboBox1;
    }
}
