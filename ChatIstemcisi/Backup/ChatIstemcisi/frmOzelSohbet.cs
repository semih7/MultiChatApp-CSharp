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
        /// Kar�� taraf�n nicki
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
        /// Konu�ulan ki�i bir mesaj g�ndermi�se bu mesaj buraya gelir
        /// </summary>
        /// <param name="mesaj">Gelen mesaj</param>
        public void MesajAl(string mesaj)
        {
            string mesajlar = txtOzelMesajlar.Text;
            mesajlar += "\r\n" + nick + ": " + mesaj;
            txtOzelMesajlar.Text = mesajlar;
        }

        /// <summary>
        /// Sohbet yap�lan kullan�c� yeniden girdiyse buraya bilgi gelir
        /// </summary>
        public void KullaniciGirdi()
        {
            gbMesajlar.Enabled = true;
            txtOzelMesajlar.Text += "\r\n" + nick + " oturumunu a�t�.";
        }

        /// <summary>
        /// Sohbet yap�lan kullan�c� program�n� kapatt�ysa buraya bilgi gelir
        /// </summary>
        public void KullaniciCikti()
        {
            gbMesajlar.Enabled = false;
            txtOzelMesajlar.Text += "\r\n" + nick + " oturumunu kapatt�.";
        }

        /// <summary>
        /// Formun ba�l���n� yazar
        /// </summary>
        private void baslikYaz()
        {
            Text = "�zel Sohbet | " + anaForm.Nick + " - " + nick;
        }

        private void frmOzelSohbet_Shown(object sender, EventArgs e)
        {
            //Formun a��l���nda mesaj yazma kutusuna odaklan
            txtOzelMesaj.Focus();
        }

        private void txtOzelMesaj_KeyPress(object sender, KeyPressEventArgs e)
        {
            //Enter'a bas�lm��sa ve textbox'da bir metin varsa kar�� tarafa yolla
            if (e.KeyChar == (char)13 && txtOzelMesaj.Text.Length > 0)
            {
                if (mesajGonderimeUygun(txtOzelMesaj.Text))
                {
                    //mesaj� kar�� tarafa yolla
                    anaForm.OzelMesajYolla(nick, txtOzelMesaj.Text);
                    //tu�a bas�lmay� iptal et
                    e.Handled = true;
                    //sohbet alan�na g�nderilen mesaj� ekle
                    string mesajlar = txtOzelMesajlar.Text;
                    mesajlar += "\r\n" + anaForm.Nick + ": " + txtOzelMesaj.Text;
                    txtOzelMesajlar.Text = mesajlar;
                    //yaz� alan�n� bir sonraki mesaj i�in bo�alt                
                    txtOzelMesaj.Text = "";
                }
                else
                {
                    MessageBox.Show("G�ndermek istedi�iniz mesajda uygun olmayan karakterler var. Mesaj i�erisinde �u karakterler olamaz: < > & =", "Dikkat!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
        }

        /// <summary>
        /// Mesaj�n g�nderime uygunlu�unu denetler
        /// </summary>
        /// <param name="mesaj">G�nderilecek mesaj</param>
        /// <returns>uygunsa true, aksi halde false</returns>
        private bool mesajGonderimeUygun(string mesaj)
        {
            //E�er a�a��daki karakterlerden birisi varsa mesaj uygun de�ildir
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