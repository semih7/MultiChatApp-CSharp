namespace ChatSunucusu
{
    partial class frmAna
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
            this.pnlAna = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.txtOnlineKullanicilar = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lstSon10Mesaj = new System.Windows.Forms.ListBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtPortNo = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnDurdur = new System.Windows.Forms.Button();
            this.btnBaslat = new System.Windows.Forms.Button();
            this.pnlAna.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlAna
            // 
            this.pnlAna.BackColor = System.Drawing.Color.Honeydew;
            this.pnlAna.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlAna.Controls.Add(this.label3);
            this.pnlAna.Controls.Add(this.txtOnlineKullanicilar);
            this.pnlAna.Controls.Add(this.label1);
            this.pnlAna.Controls.Add(this.lstSon10Mesaj);
            this.pnlAna.Controls.Add(this.groupBox1);
            this.pnlAna.Location = new System.Drawing.Point(2, 2);
            this.pnlAna.Name = "pnlAna";
            this.pnlAna.Size = new System.Drawing.Size(459, 366);
            this.pnlAna.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.PaleGreen;
            this.label3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label3.Location = new System.Drawing.Point(9, 270);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(439, 24);
            this.label3.TabIndex = 4;
            this.label3.Text = "Online olan kullanýcýlar";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtOnlineKullanicilar
            // 
            this.txtOnlineKullanicilar.BackColor = System.Drawing.Color.WhiteSmoke;
            this.txtOnlineKullanicilar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtOnlineKullanicilar.Location = new System.Drawing.Point(9, 297);
            this.txtOnlineKullanicilar.Multiline = true;
            this.txtOnlineKullanicilar.Name = "txtOnlineKullanicilar";
            this.txtOnlineKullanicilar.ReadOnly = true;
            this.txtOnlineKullanicilar.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtOnlineKullanicilar.Size = new System.Drawing.Size(439, 56);
            this.txtOnlineKullanicilar.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.PaleGreen;
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label1.Location = new System.Drawing.Point(9, 78);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(439, 24);
            this.label1.TabIndex = 2;
            this.label1.Text = "Son Gelen 10 Mesaj";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lstSon10Mesaj
            // 
            this.lstSon10Mesaj.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstSon10Mesaj.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstSon10Mesaj.FormattingEnabled = true;
            this.lstSon10Mesaj.HorizontalScrollbar = true;
            this.lstSon10Mesaj.ItemHeight = 14;
            this.lstSon10Mesaj.Location = new System.Drawing.Point(9, 105);
            this.lstSon10Mesaj.Name = "lstSon10Mesaj";
            this.lstSon10Mesaj.Size = new System.Drawing.Size(439, 156);
            this.lstSon10Mesaj.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtPortNo);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.btnDurdur);
            this.groupBox1.Controls.Add(this.btnBaslat);
            this.groupBox1.Location = new System.Drawing.Point(9, 9);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(439, 62);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Sunucu Durumu";
            // 
            // txtPortNo
            // 
            this.txtPortNo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPortNo.Location = new System.Drawing.Point(131, 24);
            this.txtPortNo.Name = "txtPortNo";
            this.txtPortNo.Size = new System.Drawing.Size(69, 23);
            this.txtPortNo.TabIndex = 4;
            this.txtPortNo.Text = "10048";
            this.txtPortNo.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.PaleGreen;
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label2.Location = new System.Drawing.Point(11, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(120, 23);
            this.label2.TabIndex = 3;
            this.label2.Text = "Dinleme Portu:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnDurdur
            // 
            this.btnDurdur.BackColor = System.Drawing.Color.Crimson;
            this.btnDurdur.Enabled = false;
            this.btnDurdur.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnDurdur.ForeColor = System.Drawing.Color.White;
            this.btnDurdur.Location = new System.Drawing.Point(323, 20);
            this.btnDurdur.Name = "btnDurdur";
            this.btnDurdur.Size = new System.Drawing.Size(105, 29);
            this.btnDurdur.TabIndex = 1;
            this.btnDurdur.Text = "Durdur";
            this.btnDurdur.UseVisualStyleBackColor = false;
            this.btnDurdur.Click += new System.EventHandler(this.btnDurdur_Click);
            // 
            // btnBaslat
            // 
            this.btnBaslat.BackColor = System.Drawing.Color.ForestGreen;
            this.btnBaslat.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnBaslat.ForeColor = System.Drawing.Color.White;
            this.btnBaslat.Location = new System.Drawing.Point(212, 20);
            this.btnBaslat.Name = "btnBaslat";
            this.btnBaslat.Size = new System.Drawing.Size(105, 29);
            this.btnBaslat.TabIndex = 0;
            this.btnBaslat.Text = "Baþlat";
            this.btnBaslat.UseVisualStyleBackColor = false;
            this.btnBaslat.Click += new System.EventHandler(this.btnBaslat_Click);
            // 
            // frmAna
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(463, 370);
            this.Controls.Add(this.pnlAna);
            this.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "frmAna";
            this.Text = "ASMES Chat Sunucusu";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmAna_FormClosing);
            this.pnlAna.ResumeLayout(false);
            this.pnlAna.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlAna;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnDurdur;
        private System.Windows.Forms.Button btnBaslat;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox lstSon10Mesaj;
        private System.Windows.Forms.TextBox txtPortNo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtOnlineKullanicilar;
        private System.Windows.Forms.Label label3;
    }
}

