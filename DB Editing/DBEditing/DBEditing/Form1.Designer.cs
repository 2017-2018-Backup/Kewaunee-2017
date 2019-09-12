namespace DBEditing
{
    partial class Form1
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
            this.label1 = new System.Windows.Forms.Label();
            this.btnDownload = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnUploadExisting = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.btnBrowseExisting = new System.Windows.Forms.Button();
            this.txtExisting = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnUploadNew = new System.Windows.Forms.Button();
            this.btnBrowseNew = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.txtNew = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(130, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Download Product Master";
            // 
            // btnDownload
            // 
            this.btnDownload.Location = new System.Drawing.Point(160, 13);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(75, 23);
            this.btnDownload.TabIndex = 1;
            this.btnDownload.Text = "Download";
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnUploadExisting);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.btnBrowseExisting);
            this.groupBox1.Controls.Add(this.txtExisting);
            this.groupBox1.Location = new System.Drawing.Point(15, 43);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(258, 93);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Update Existing";
            // 
            // btnUploadExisting
            // 
            this.btnUploadExisting.Location = new System.Drawing.Point(93, 61);
            this.btnUploadExisting.Name = "btnUploadExisting";
            this.btnUploadExisting.Size = new System.Drawing.Size(75, 23);
            this.btnUploadExisting.TabIndex = 5;
            this.btnUploadExisting.Text = "Upload";
            this.btnUploadExisting.UseVisualStyleBackColor = true;
            this.btnUploadExisting.Click += new System.EventHandler(this.btnUploadExisting_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 34);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Upload";
            // 
            // btnBrowseExisting
            // 
            this.btnBrowseExisting.Location = new System.Drawing.Point(207, 29);
            this.btnBrowseExisting.Name = "btnBrowseExisting";
            this.btnBrowseExisting.Size = new System.Drawing.Size(40, 23);
            this.btnBrowseExisting.TabIndex = 5;
            this.btnBrowseExisting.Text = "...";
            this.btnBrowseExisting.UseVisualStyleBackColor = true;
            this.btnBrowseExisting.Click += new System.EventHandler(this.btnBrowseExisting_Click);
            // 
            // txtExisting
            // 
            this.txtExisting.Location = new System.Drawing.Point(53, 31);
            this.txtExisting.Name = "txtExisting";
            this.txtExisting.ReadOnly = true;
            this.txtExisting.Size = new System.Drawing.Size(153, 20);
            this.txtExisting.TabIndex = 5;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnUploadNew);
            this.groupBox2.Controls.Add(this.btnBrowseNew);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.txtNew);
            this.groupBox2.Enabled = false;
            this.groupBox2.Location = new System.Drawing.Point(15, 183);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(258, 90);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Insert New Records";
            // 
            // btnUploadNew
            // 
            this.btnUploadNew.Location = new System.Drawing.Point(93, 57);
            this.btnUploadNew.Name = "btnUploadNew";
            this.btnUploadNew.Size = new System.Drawing.Size(75, 23);
            this.btnUploadNew.TabIndex = 6;
            this.btnUploadNew.Text = "Upload";
            this.btnUploadNew.UseVisualStyleBackColor = true;
            this.btnUploadNew.Click += new System.EventHandler(this.btnUploadNew_Click);
            // 
            // btnBrowseNew
            // 
            this.btnBrowseNew.Location = new System.Drawing.Point(207, 29);
            this.btnBrowseNew.Name = "btnBrowseNew";
            this.btnBrowseNew.Size = new System.Drawing.Size(40, 23);
            this.btnBrowseNew.TabIndex = 6;
            this.btnBrowseNew.Text = "...";
            this.btnBrowseNew.UseVisualStyleBackColor = true;
            this.btnBrowseNew.Click += new System.EventHandler(this.btnBrowseNew_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 34);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Upload";
            // 
            // txtNew
            // 
            this.txtNew.Location = new System.Drawing.Point(53, 31);
            this.txtNew.Name = "txtNew";
            this.txtNew.ReadOnly = true;
            this.txtNew.Size = new System.Drawing.Size(153, 20);
            this.txtNew.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 276);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(229, 26);
            this.label2.TabIndex = 4;
            this.label2.Text = "Notes: \r\nFor New Insert don\'t enter values for ProductID";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 146);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(206, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "Do not add or change headers or columns";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(287, 309);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnDownload);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Kewaunee";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TextBox txtExisting;
        private System.Windows.Forms.TextBox txtNew;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnBrowseExisting;
        private System.Windows.Forms.Button btnBrowseNew;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnUploadExisting;
        private System.Windows.Forms.Button btnUploadNew;
        private System.Windows.Forms.Label label5;
    }
}

