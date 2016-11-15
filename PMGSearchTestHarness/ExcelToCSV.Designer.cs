namespace PMGSearchTestHarness
{
    partial class ExcelToCSV
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtPath = new System.Windows.Forms.TextBox();
            this.btnGenerateCSV = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtCSV = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(103, 12);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(389, 20);
            this.txtPath.TabIndex = 0;
            // 
            // btnGenerateCSV
            // 
            this.btnGenerateCSV.Location = new System.Drawing.Point(498, 12);
            this.btnGenerateCSV.Name = "btnGenerateCSV";
            this.btnGenerateCSV.Size = new System.Drawing.Size(132, 23);
            this.btnGenerateCSV.TabIndex = 1;
            this.btnGenerateCSV.Text = "Generate CSV";
            this.btnGenerateCSV.UseVisualStyleBackColor = true;
            this.btnGenerateCSV.Click += new System.EventHandler(this.btnGenerateCSV_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Path to Excel File";
            // 
            // txtCSV
            // 
            this.txtCSV.Location = new System.Drawing.Point(103, 38);
            this.txtCSV.Multiline = true;
            this.txtCSV.Name = "txtCSV";
            this.txtCSV.Size = new System.Drawing.Size(527, 289);
            this.txtCSV.TabIndex = 3;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(103, 333);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(132, 23);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // ExcelToCSV
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(639, 367);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txtCSV);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnGenerateCSV);
            this.Controls.Add(this.txtPath);
            this.Name = "ExcelToCSV";
            this.Text = "ExcelToCSV";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Button btnGenerateCSV;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtCSV;
        private System.Windows.Forms.Button btnOK;
    }
}