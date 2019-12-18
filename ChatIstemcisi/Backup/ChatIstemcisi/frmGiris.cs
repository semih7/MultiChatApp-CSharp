using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using HIK.ASMES;
using HIK.ASMES.IstemciTarafi;

namespace ChatIstemcisi
{
    public partial class frmGiris : Form
    {
        /// <summary>
        /// Bu Form kapatýldýðýnda bu deðiþken true ise baþarýlý bir þekilde giriþ yapýlmýþ demektir.
        /// </summary>
        public bool GirisYapildi
        {
            get { return girisYapildi; }
            set { girisYapildi = value; }
        }
        private bool girisYapildi = false;

        /// <summary>
        /// ASMES kütüphanesini kullanarak ASMES sunucusuna baðlý olan istemci nesnesi
        /// </summary>
        public ASMESIstemcisi Istemci
        {
            get { return istemci; }
            set { istemci = value; }
        }
        private ASMESIstemcisi istemci;

        /// <summary>
        /// Kullanýcýnýn seçtiði nick
        /// </summary>
        public string Nick
        {
            get { return nick; }
            set { nick = value; }
        }
        private string nick;

        public frmGiris()
        {
            InitializeComponent();
        }

        private void btnBaglan_Click(object sender, EventArgs e)
        {
            if (txtIP.Text.Length == 0 || txtPort.Text.Length == 0 || txtNick.Text.Length == 0)
            {
                MessageBox.Show("Lütfen formda boþ býraktýðýnýz alanlarý doldurun.", "Dikkat!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!nickUygun(txtNick.Text))
            {
                MessageBox.Show("Seçtiðiniz nickde uygun olmayan karakterler var. Nick içerisinde þu karakterler olamaz: < > & = ,", "Dikkat!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!baglan())
            {
                MessageBox.Show("Sunucuya baðlanýlamadý, IP/Port bilgilerini kontrol edin.", "Hata!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                girisYapildi = true;
                nick = txtNick.Text;
                Close();
            }
        }

        private bool baglan()
        {
            try
            {
                //Formdan IP ve PORT bilgilerini al
                string ip = txtIP.Text;
                int port = Convert.ToInt32(txtPort.Text);
                //Bir istemci nesnesi oluþtur ve baðlan
                istemci = new ASMESIstemcisi(ip, port);
                return istemci.Baglan();
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void frmGiris_Shown(object sender, EventArgs e)
        {
            txtNick.Focus();
        }


        /// <summary>
        /// Nickin  uygunluðunu denetler
        /// </summary>
        /// <param name="mesaj">Seçilen nick</param>
        /// <returns>uygunsa true, aksi halde false</returns>
        private bool nickUygun(string mesaj)
        {
            //Eðer aþaðýdaki karakterlerden birisi varsa nick uygun deðildir
            if (mesaj.IndexOfAny(new char[] { '<', '>', '&', '=', ',' }) >= 0)
            {
                return false;
            }
            return true;
        }
    }
}