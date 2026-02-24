namespace source
{
    partial class Form1
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
            comboBox1 = new ComboBox();
            ClearBtn = new Button();
            listBox1 = new ListBox();
            SendBtn = new Button();
            SuspendLayout();
            // 
            // ModelLabel
            // 
            ModelLabel.AutoSize = true;
            ModelLabel.Font = new Font("Verdana", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            ModelLabel.ForeColor = Color.White;
            ModelLabel.Location = new Point(12, 12);
            ModelLabel.Name = "ModelLabel";
            ModelLabel.Size = new Size(77, 18);
            ModelLabel.TabIndex = 0;
            ModelLabel.Text = "Модель";
            // 
            // comboBox1
            // 
            comboBox1.Font = new Font("Verdana", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(105, 11);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(121, 22);
            comboBox1.TabIndex = 1;
            // 
            // ClearBtn
            // 
            ClearBtn.Font = new Font("Verdana", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            ClearBtn.Location = new Point(304, 10);
            ClearBtn.Name = "ClearBtn";
            ClearBtn.Size = new Size(88, 23);
            ClearBtn.TabIndex = 3;
            ClearBtn.Text = "Очитстить";
            ClearBtn.UseVisualStyleBackColor = true;
            // 
            // listBox1
            // 
            listBox1.Font = new Font("Verdana", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            listBox1.FormattingEnabled = true;
            listBox1.Location = new Point(12, 66);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(776, 368);
            listBox1.TabIndex = 4;
            // 
            // SendBtn
            // 
            SendBtn.Font = new Font("Verdana", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            SendBtn.Location = new Point(691, 10);
            SendBtn.Name = "SendBtn";
            SendBtn.Size = new Size(88, 23);
            SendBtn.TabIndex = 5;
            SendBtn.Text = "Отправить";
            SendBtn.UseVisualStyleBackColor = true;
            SendBtn.Click += SendBtn_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            ClientSize = new Size(800, 450);
            Controls.Add(SendBtn);
            Controls.Add(listBox1);
            Controls.Add(ClearBtn);
            Controls.Add(comboBox1);
            Controls.Add(ModelLabel);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Another one";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label ModelLabel;
        private ComboBox comboBox1;
        private Button ClearBtn;
        private ListBox listBox1;
        private Button SendBtn;
    }
}
