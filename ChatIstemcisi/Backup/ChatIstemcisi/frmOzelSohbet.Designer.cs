namespace ChatIstemcisi
{
    partial class frmOzelSohbet
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.gbMesajlar = new System.Windows.Forms.GroupBox();
            this.txtOzelMesaj = new System.Windows.Forms.TextBox();
            this.txtOzelMesajlar = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.gbMesajlar.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.AliceBlue;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.gbMesajlar);
            this.panel1.Location = new System.Drawing.Point(2, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(547, 626);
            this.panel1.TabIndex = 1;
            // 
            // gbMesajlar
            // 
            this.gbMesajlar.Controls.Add(this.txtOzelMesaj);
            this.gbMesajlar.Controls.Add(this.txtOzelMesajlar);
            this.gbMesajlar.Location = new System.Drawing.Point(9, 3);
            this.gbMesajlar.Name = "gbMesajlar";
            this.gbMesajlar.Size = new System.Drawing.Size(527, 610);
            this.gbMesajlar.TabIndex = 2;
            this.gbMesajlar.TabStop = false;
            this.gbMesajlar.Text = "Mesajlar";
            // 
            // txtOzelMesaj
            // 
            this.txtOzelMesaj.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtOzelMesaj.Location = new System.Drawing.Point(9, 577);
            this.txtOzelMesaj.Name = "txtOzelMesaj";
            this.txtOzelMesaj.Size = new System.Drawing.Size(509, 23);
            this.txtOzelMesaj.TabIndex = 1;
            this.txtOzelMesaj.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtOzelMesaj_KeyPress);
            // 
            // txtOzelMesajlar
            // 
            this.txtOzelMesajlar.BackColor = System.Drawing.Color.WhiteSmoke;
            this.txtOzelMesajlar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtOzelMesajlar.Location = new System.Drawing.Point(9, 22);
            this.txtOzelMesajlar.Multiline = true;
            this.txtOzelMesajlar.Name = "txtOzelMesajlar";
            this.txtOzelMesajlar.ReadOnly = true;
            this.txtOzelMesajlar.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOzelMesajlar.Size = new System.Drawing.Size(509, 549);
            this.txtOzelMesajlar.TabIndex = 0;
            // 
            // frmOzelSohbet
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(551, 633);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "frmOzelSohbet";
            this.Text = "Özel Sohbet";
            this.Shown += new System.EventHandler(this.frmOzelSohbet_Shown);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmOzelSohbet_FormClosed);
            this.panel1.ResumeLayout(false);
            this.gbMesajlar.ResumeLayout(false);
            this.gbMesajlar.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox gbMesajlar;
        private System.Windows.Forms.TextBox txtOzelMesaj;
        private System.Windows.Forms.TextBox txtOzelMesajlar;
    }
}