namespace FileCompare
{
    partial class MainForm
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            txtPath1 = new TextBox();
            label1 = new Label();
            label2 = new Label();
            txtPath2 = new TextBox();
            btnRun = new Button();
            bgWorker = new System.ComponentModel.BackgroundWorker();
            panel1 = new Panel();
            progressBar1 = new ProgressBar();
            lblPro = new Label();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // txtPath1
            // 
            txtPath1.Location = new Point(54, 9);
            txtPath1.Margin = new Padding(4);
            txtPath1.Name = "txtPath1";
            txtPath1.Size = new Size(291, 23);
            txtPath1.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 13);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(40, 15);
            label1.TabIndex = 1;
            label1.Text = "Path1:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 44);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(40, 15);
            label2.TabIndex = 3;
            label2.Text = "Path2:";
            // 
            // txtPath2
            // 
            txtPath2.Location = new Point(54, 40);
            txtPath2.Margin = new Padding(4);
            txtPath2.Name = "txtPath2";
            txtPath2.Size = new Size(291, 23);
            txtPath2.TabIndex = 2;
            // 
            // btnRun
            // 
            btnRun.Location = new Point(353, 8);
            btnRun.Margin = new Padding(4);
            btnRun.Name = "btnRun";
            btnRun.Size = new Size(64, 55);
            btnRun.TabIndex = 6;
            btnRun.Text = "Run";
            btnRun.UseVisualStyleBackColor = true;
            btnRun.Click += btnRun_Click;
            // 
            // panel1
            // 
            panel1.BackColor = Color.PowderBlue;
            panel1.Controls.Add(progressBar1);
            panel1.Controls.Add(lblPro);
            panel1.Location = new Point(8, 5);
            panel1.Margin = new Padding(4);
            panel1.Name = "panel1";
            panel1.Size = new Size(411, 71);
            panel1.TabIndex = 7;
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(8, 28);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(393, 34);
            progressBar1.TabIndex = 1;
            // 
            // lblPro
            // 
            lblPro.AutoSize = true;
            lblPro.Font = new Font("MS UI Gothic", 18F, FontStyle.Bold, GraphicsUnit.Point, 128);
            lblPro.Location = new Point(1, 1);
            lblPro.Margin = new Padding(4, 0, 4, 0);
            lblPro.Name = "lblPro";
            lblPro.Size = new Size(95, 24);
            lblPro.TabIndex = 0;
            lblPro.Text = "process";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(426, 83);
            Controls.Add(panel1);
            Controls.Add(btnRun);
            Controls.Add(label2);
            Controls.Add(txtPath2);
            Controls.Add(label1);
            Controls.Add(txtPath1);
            Margin = new Padding(4);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "FileCompare";
            Load += MainForm_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox txtPath1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtPath2;
        private System.Windows.Forms.Button btnRun;
        private System.ComponentModel.BackgroundWorker bgWorker;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblPro;
        private ProgressBar progressBar1;
    }
}

