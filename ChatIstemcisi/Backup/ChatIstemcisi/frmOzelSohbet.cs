using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ChatIstemcisi
{
    public partial class frmOzelSohbet : Form
    {
        /// <summary>
        /// Ana forma referans
        /// </summary>
        private frmAna anaForm;

        /// <summary>
        /// Karþý tarafýn nicki
        /// </summary>
        private string nick;

        public frmOzelSohbet(frmAna anaForm, string nick)
        {
            InitializeComponent();

            this.anaForm = anaForm;
            this.nick = nick;

            baslikYaz();
        }

        /// <summary>
        /// Konuþulan kiþi bir mesaj göndermiþse bu mesaj buraya gelir
        /// </summary>
        /// <param name="mesaj">Gelen mesaj</param>
        public void MesajAl(string mesaj)
        {
            string mesajlar = txtOzelMesajlar.Text;
            mesajlar += "\r\n" + nick + ": " + mesaj;
            txtOzelMesajlar.Text = mesajlar;
        }

        /// <summary>
        /// Sohbet yapýlan kullanýcý yeniden girdiyse buraya bilgi gelir
        /// </summary>
        public void KullaniciGirdi()
        {
            gbMesajlar.Enabled = true;
            txtOzelMesajlar.Text += "\r\n" + nick + " oturumunu açtý.";
        }

        /// <summary>
        /// Sohbet yapýlan kullanýcý programýný kapattýysa buraya bilgi gelir
        /// </summary>
        public void KullaniciCikti()
        {
            gbMesajlar.Enabled = false;
            txtOzelMesajlar.Text += "\r\n" + nick + " oturumunu kapattý.";
        }

        /// <summary>
        /// Formun baþlýðýný yazar
        /// </summary>
        private void baslikYaz()
        {
            Text = "Özel Sohbet | " + anaForm.Nick + " - " + nick;
        }

        private void frmOzelSohbet_Shown(object sender, EventArgs e)
        {
            //Formun açýlýþýnda mesaj yazma kutusuna odaklan
            txtOzelMesaj.Focus();
        }

        private void txtOzelMesaj_KeyPress(object sender, KeyPressEventArgs e)
        {
            //Enter'a basýlmýþsa ve textbox'da bir metin varsa karþý tarafa yolla
            if (e.KeyChar == (char)13 && txtOzelMesaj.Text.Length > 0)
            {
                if (mesajGonderimeUygun(txtOzelMesaj.Text))
                {
                    //mesajý karþý tarafa yolla
                    anaForm.OzelMesajYolla(nick, txtOzelMesaj.Text);
                    //tuþa basýlmayý iptal et
                    e.Handled = true;
                    //sohbet alanýna gönderilen mesajý ekle
                    string mesajlar = txtOzelMesajlar.Text;
                    mesajlar += "\r\n" + anaForm.Nick + ": " + txtOzelMesaj.Text;
                    txtOzelMesajlar.Text = mesajlar;
                    //yazý alanýný bir sonraki mesaj için boþalt                
                    txtOzelMesaj.Text = "";
                }
                else
                {
                    MessageBox.Show("Göndermek istediðiniz mesajda uygun olmayan karakterler var. Mesaj içerisinde þu karakterler olamaz: < > & =", "Dikkat!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
        }

        /// <summary>
        /// Mesajýn gönderime uygunluðunu denetler
        /// </summary>
        /// <param name="mesaj">Gönderilecek mesaj</param>
        /// <returns>uygunsa true, aksi halde false</returns>
        private bool mesajGonderimeUygun(string mesaj)
        {
            //Eðer aþaðýdaki karakterlerden birisi varsa mesaj uygun deðildir
            if (mesaj.IndexOfAny(new char[] { '<', '>', '&', '=' }) >= 0)
            {
                return false;
            }
            return true;
        }

        private void frmOzelSohbet_FormClosed(object sender, FormClosedEventArgs e)
        {
            anaForm.OzelSohbetFormuKapandi(nick);
        }
    }
}