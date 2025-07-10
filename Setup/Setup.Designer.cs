namespace Setup
{
    partial class Setup
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Setup));
            progressBar1 = new ProgressBar();
            button1 = new Button();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            label1 = new Label();
            label6 = new Label();
            label7 = new Label();
            label8 = new Label();
            label9 = new Label();
            label10 = new Label();
            checkBox1 = new CheckBox();
            label11 = new Label();
            logTextBox = new TextBox();
            button2 = new Button();
            SuspendLayout();
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(18, 385);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(374, 23);
            progressBar1.TabIndex = 2;
            // 
            // button1
            // 
            button1.Enabled = false;
            button1.Location = new Point(12, 429);
            button1.Name = "button1";
            button1.Size = new Size(380, 88);
            button1.TabIndex = 3;
            button1.Text = "Установить";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label2.AutoSize = true;
            label2.FlatStyle = FlatStyle.Flat;
            label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label2.Location = new Point(18, 83);
            label2.Name = "label2";
            label2.Size = new Size(121, 15);
            label2.TabIndex = 6;
            label2.Text = "Актуальная версия:";
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label3.AutoSize = true;
            label3.FlatStyle = FlatStyle.System;
            label3.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label3.Location = new Point(20, 98);
            label3.Name = "label3";
            label3.Size = new Size(118, 15);
            label3.TabIndex = 7;
            label3.Text = "Название выпуска:";
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label4.AutoSize = true;
            label4.FlatStyle = FlatStyle.System;
            label4.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label4.Location = new Point(20, 149);
            label4.Name = "label4";
            label4.Size = new Size(74, 15);
            label4.TabIndex = 8;
            label4.Text = "Что нового:";
            // 
            // label5
            // 
            label5.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label5.AutoSize = true;
            label5.FlatStyle = FlatStyle.System;
            label5.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label5.Location = new Point(18, 354);
            label5.Name = "label5";
            label5.Size = new Size(91, 15);
            label5.TabIndex = 9;
            label5.Text = "Размер файла:";
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label1.Location = new Point(18, 16);
            label1.Name = "label1";
            label1.Size = new Size(374, 272);
            label1.TabIndex = 5;
            label1.Text = resources.GetString("label1.Text");
            label1.Click += label1_Click;
            // 
            // label6
            // 
            label6.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label6.AutoSize = true;
            label6.FlatStyle = FlatStyle.System;
            label6.Location = new Point(18, 411);
            label6.Name = "label6";
            label6.Size = new Size(34, 15);
            label6.TabIndex = 10;
            label6.Text = "0 Мб";
            // 
            // label7
            // 
            label7.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label7.AutoSize = true;
            label7.FlatStyle = FlatStyle.System;
            label7.Location = new Point(109, 354);
            label7.Name = "label7";
            label7.Size = new Size(34, 15);
            label7.TabIndex = 11;
            label7.Text = "0 МБ";
            // 
            // label8
            // 
            label8.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label8.AutoEllipsis = true;
            label8.FlatStyle = FlatStyle.System;
            label8.Location = new Point(18, 164);
            label8.MaximumSize = new Size(374, 176);
            label8.Name = "label8";
            label8.Size = new Size(374, 176);
            label8.TabIndex = 12;
            // 
            // label9
            // 
            label9.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label9.FlatStyle = FlatStyle.System;
            label9.Location = new Point(18, 113);
            label9.Name = "label9";
            label9.Size = new Size(374, 241);
            label9.TabIndex = 13;
            // 
            // label10
            // 
            label10.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label10.AutoSize = true;
            label10.FlatStyle = FlatStyle.Flat;
            label10.Location = new Point(142, 83);
            label10.Name = "label10";
            label10.Size = new Size(40, 15);
            label10.TabIndex = 14;
            label10.Text = "0.0.0.0";
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(12, 523);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(219, 19);
            checkBox1.TabIndex = 15;
            checkBox1.Text = "Принудительно установить пакеты";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            label11.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label11.AutoEllipsis = true;
            label11.FlatStyle = FlatStyle.System;
            label11.Location = new Point(12, 545);
            label11.MaximumSize = new Size(374, 176);
            label11.Name = "label11";
            label11.Size = new Size(374, 47);
            label11.TabIndex = 16;
            label11.Text = " (Активируйте, если у Вас отключены некоторые компоненты и службы Windows, или просто не устаанавливается по причине невозможности установить пакеты зависимостей.)";
            // 
            // logTextBox
            // 
            logTextBox.Location = new Point(12, 602);
            logTextBox.Multiline = true;
            logTextBox.Name = "logTextBox";
            logTextBox.ReadOnly = true;
            logTextBox.ScrollBars = ScrollBars.Vertical;
            logTextBox.Size = new Size(380, 123);
            logTextBox.TabIndex = 17;
            // 
            // button2
            // 
            button2.Location = new Point(237, 519);
            button2.Name = "button2";
            button2.Size = new Size(155, 27);
            button2.TabIndex = 18;
            button2.Text = "Тех поддержка";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // Setup
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(404, 734);
            Controls.Add(button2);
            Controls.Add(logTextBox);
            Controls.Add(label11);
            Controls.Add(checkBox1);
            Controls.Add(label10);
            Controls.Add(label9);
            Controls.Add(label8);
            Controls.Add(label7);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(button1);
            Controls.Add(progressBar1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Setup";
            Text = "Установщик VK M";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private ProgressBar progressBar1;
        private Button button1;
        protected internal Label label2;
        protected internal Label label3;
        protected internal Label label4;
        protected internal Label label5;
        protected internal Label label1;
        protected internal Label label6;
        protected internal Label label7;
        protected internal Label label8;
        protected internal Label label9;
        protected internal Label label10;
        private CheckBox checkBox1;
        protected internal Label label11;
        private System.Windows.Forms.TextBox logTextBox;
        private Button button2;
    }
}
