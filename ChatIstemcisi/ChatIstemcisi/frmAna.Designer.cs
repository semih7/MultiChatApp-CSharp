namespace ChatIstemcisi
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAna));
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblNick = new System.Windows.Forms.Label();
            this.gbMesajlar = new System.Windows.Forms.GroupBox();
            this.txtTopluMesaj = new System.Windows.Forms.TextBox();
            this.txtTopluMesajlar = new System.Windows.Forms.TextBox();
            this.gbKullanicilar = new System.Windows.Forms.GroupBox();
            this.lstKullanicilar = new System.Windows.Forms.ListBox();
            this.panel1.SuspendLayout();
            this.gbMesajlar.SuspendLayout();
            this.gbKullanicilar.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.AliceBlue;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.lblNick);
            this.panel1.Controls.Add(this.gbMesajlar);
            this.panel1.Controls.Add(this.gbKullanicilar);
            this.panel1.Location = new System.Drawing.Point(2, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(740, 593);
            this.panel1.TabIndex = 0;
            // 
            // lblNick
            // 
            this.lblNick.BackColor = System.Drawing.Color.MidnightBlue;
            this.lblNick.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblNick.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblNick.ForeColor = System.Drawing.Color.White;
            this.lblNick.Location = new System.Drawing.Point(542, 10);
            this.lblNick.Name = "lblNick";
            this.lblNick.Size = new System.Drawing.Size(187, 23);
            this.lblNick.TabIndex = 2;
            this.lblNick.Text = "NICK";
            this.lblNick.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // gbMesajlar
            // 
            this.gbMesajlar.Controls.Add(this.txtTopluMesaj);
            this.gbMesajlar.Controls.Add(this.txtTopluMesajlar);
            this.gbMesajlar.Enabled = false;
            this.gbMesajlar.Location = new System.Drawing.Point(9, 3);
            this.gbMesajlar.Name = "gbMesajlar";
            this.gbMesajlar.Size = new System.Drawing.Size(527, 610);
            this.gbMesajlar.TabIndex = 1;
            this.gbMesajlar.TabStop = false;
            this.gbMesajlar.Text = "Mesajlar";
            // 
            // txtTopluMesaj
            // 
            this.txtTopluMesaj.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTopluMesaj.Location = new System.Drawing.Point(6, 364);
            this.txtTopluMesaj.Name = "txtTopluMesaj";
            this.txtTopluMesaj.Size = new System.Drawing.Size(509, 27);
            this.txtTopluMesaj.TabIndex = 1;
            this.txtTopluMesaj.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtTopluMesaj_KeyPress);
            // 
            // txtTopluMesajlar
            // 
            this.txtTopluMesajlar.BackColor = System.Drawing.Color.WhiteSmoke;
            this.txtTopluMesajlar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTopluMesajlar.Location = new System.Drawing.Point(9, 22);
            this.txtTopluMesajlar.Multiline = true;
            this.txtTopluMesajlar.Name = "txtTopluMesajlar";
            this.txtTopluMesajlar.ReadOnly = true;
            this.txtTopluMesajlar.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtTopluMesajlar.Size = new System.Drawing.Size(509, 336);
            this.txtTopluMesajlar.TabIndex = 0;
            // 
            // gbKullanicilar
            // 
            this.gbKullanicilar.Controls.Add(this.lstKullanicilar);
            this.gbKullanicilar.Enabled = false;
            this.gbKullanicilar.Location = new System.Drawing.Point(542, 36);
            this.gbKullanicilar.Name = "gbKullanicilar";
            this.gbKullanicilar.Size = new System.Drawing.Size(187, 577);
            this.gbKullanicilar.TabIndex = 0;
            this.gbKullanicilar.TabStop = false;
            this.gbKullanicilar.Text = "Kullanıcılar";
            // 
            // lstKullanicilar
            // 
            this.lstKullanicilar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstKullanicilar.FormattingEnabled = true;
            this.lstKullanicilar.ItemHeight = 18;
            this.lstKullanicilar.Location = new System.Drawing.Point(6, 25);
            this.lstKullanicilar.Name = "lstKullanicilar";
            this.lstKullanicilar.Size = new System.Drawing.Size(169, 326);
            this.lstKullanicilar.TabIndex = 0;
            this.lstKullanicilar.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstKullanicilar_MouseDoubleClick);
            // 
            // frmAna
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(744, 408);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "frmAna";
            this.Text = "Chat İstemcisi";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmAna_FormClosing);
            this.Shown += new System.EventHandler(this.frmAna_Shown);
            this.panel1.ResumeLayout(false);
            this.gbMesajlar.ResumeLayout(false);
            this.gbMesajlar.PerformLayout();
            this.gbKullanicilar.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox gbMesajlar;
        private System.Windows.Forms.GroupBox gbKullanicilar;
        private System.Windows.Forms.ListBox lstKullanicilar;
        private System.Windows.Forms.TextBox txtTopluMesaj;
        private System.Windows.Forms.TextBox txtTopluMesajlar;
        private System.Windows.Forms.Label lblNick;
    }
}

