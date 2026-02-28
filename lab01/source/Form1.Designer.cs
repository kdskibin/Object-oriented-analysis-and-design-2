namespace source
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ModelLabel = new Label();
            ModelSelector_cb = new ComboBox();
            Clear_Btn = new Button();
            ConversationBox = new ListBox();
            Send_Btn = new Button();
            InputMessage_tb = new TextBox();
            SuspendLayout();
            // 
            // ModelLabel
            // 
            ModelLabel.AutoSize = true;
            ModelLabel.Font = new Font("Verdana", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            ModelLabel.ForeColor = Color.White;
            ModelLabel.Location = new Point(12, 12);
            ModelLabel.Name = "ModelLabel";
            ModelLabel.Size = new Size(99, 25);
            ModelLabel.TabIndex = 0;
            ModelLabel.Text = "Модель";
            // 
            // ModelSelector_cb
            // 
            ModelSelector_cb.Font = new Font("Arial", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            ModelSelector_cb.FormattingEnabled = true;
            ModelSelector_cb.Location = new Point(117, 11);
            ModelSelector_cb.Name = "ModelSelector_cb";
            ModelSelector_cb.Size = new Size(199, 25);
            ModelSelector_cb.TabIndex = 1;
            // 
            // Clear_Btn
            // 
            Clear_Btn.Font = new Font("Arial", 9F);
            Clear_Btn.Location = new Point(335, 11);
            Clear_Btn.Name = "Clear_Btn";
            Clear_Btn.Size = new Size(109, 22);
            Clear_Btn.TabIndex = 3;
            Clear_Btn.Text = "Очитстить";
            Clear_Btn.UseVisualStyleBackColor = true;
            Clear_Btn.Click += Clear_Btn_Click;
            // 
            // ConversationBox
            // 
            ConversationBox.Font = new Font("Arial", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 204);
            ConversationBox.FormattingEnabled = true;
            ConversationBox.HorizontalExtent = 8192;
            ConversationBox.HorizontalScrollbar = true;
            ConversationBox.Location = new Point(12, 112);
            ConversationBox.Name = "ConversationBox";
            ConversationBox.Size = new Size(776, 292);
            ConversationBox.TabIndex = 4;
            // 
            // Send_Btn
            // 
            Send_Btn.Font = new Font("Arial", 9F);
            Send_Btn.Location = new Point(669, 10);
            Send_Btn.Name = "Send_Btn";
            Send_Btn.Size = new Size(110, 23);
            Send_Btn.TabIndex = 5;
            Send_Btn.Text = "Отправить";
            Send_Btn.UseVisualStyleBackColor = true;
            Send_Btn.Click += SendBtn_Click;
            // 
            // InputMessage_tb
            // 
            InputMessage_tb.Font = new Font("Arial", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            InputMessage_tb.Location = new Point(12, 59);
            InputMessage_tb.Name = "InputMessage_tb";
            InputMessage_tb.PlaceholderText = "Ваше сообщение...";
            InputMessage_tb.Size = new Size(776, 25);
            InputMessage_tb.TabIndex = 6;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            ClientSize = new Size(800, 450);
            Controls.Add(InputMessage_tb);
            Controls.Add(Send_Btn);
            Controls.Add(ConversationBox);
            Controls.Add(Clear_Btn);
            Controls.Add(ModelSelector_cb);
            Controls.Add(ModelLabel);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Another one";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label ModelLabel;
        private ComboBox ModelSelector_cb;
        private Button Clear_Btn;
        private ListBox ConversationBox;
        private Button Send_Btn;
        private TextBox InputMessage_tb;
    }
}
