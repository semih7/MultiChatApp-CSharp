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
    public partial class frmAna : Form
    {
        /// <summary>
        /// ASMES k�t�phanesini kullanarak ASMES sunucusuna ba�l� olan istemci nesnesi
        /// </summary>
        private ASMESIstemcisi istemci;

        /// <summary>
        /// Kullan�c�n�n Nick'i
        /// </summary>
        public string Nick
        {
            get { return nick; }
            set { nick = value; }
        }
        private string nick;

        /// <summary>
        /// Rastgele say� �retmede kullan�lacak bir nesne
        /// </summary>
        private Random rnd = new Random();

        /// <summary>
        /// �zel sohbet yap�lan formlar
        /// </summary>
        private SortedList<string, frmOzelSohbet> ozelSohbetFormlari;

        /// <summary>
        /// Chat ortam�nda bulunan kullan�c� listesi
        /// </summary>
        private List<string> kullanicilar;

        public frmAna()
        {
            ozelSohbetFormlari = new SortedList<string, frmOzelSohbet>();
            kullanicilar = new List<string>();
            InitializeComponent();
        }

        private void frmAna_Shown(object sender, EventArgs e)
        {
            //Giri� formunu g�ster
            frmGiris girisFormu = new frmGiris();
            girisFormu.ShowDialog();
            //E�er giri� formundan sunucuya do�ru ba�lan�ld�ysa sistemi ba�lat
            if (girisFormu.GirisYapildi)
            {
                //ASMESIstemcisi referans�n� al
                istemci = girisFormu.Istemci;
                nick = girisFormu.Nick;
                //Olaylara kaydol
                istemci.YeniMesajAlindi += new dgYeniMesajAlindi(istemci_YeniMesajAlindi);
                //Sunucuya giri� mesaj� g�nder
                Text = "Chat �stemcisi - Ba�lan�yor...";
                istemci.MesajYolla("komut=giris&nick=" + nick);
                //textbox'a odaklan
                txtTopluMesaj.Focus();
            }
            //Aksi halde formu kapat
            else
            {
                Close();
            }
        }

        void istemci_YeniMesajAlindi(MesajAlmaArgumanlari e)
        {
            Invoke(new dgYeniMesajAlindi(mesajAlindi), e);
        }

        private void frmAna_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Form kapat�l�rken sunucuya olan ba�lant�y� keselim
            if (istemci != null)
            {
                istemci.MesajYolla("komut=cikis");
                istemci.BaglantiyiKes();
            }
        }

        // CHAT MANTI�ININ �ALI�TI�I KODLAR ///////////////////////////////////

        /// <summary>
        /// Sunucudan bir mesaj al�nd���nda buraya gelir
        /// </summary>
        /// <param name="e">Al�nan mesajla ilgili bilgiler</param>
        private void mesajAlindi(MesajAlmaArgumanlari e)
        {
            //Gelen mesaj� & ve = i�aretlerine g�re ayr��t�r
            NameValueCollection parametreler = mesajCoz(e.Mesaj);
            //Ayr��t�rma ba�ar�s�zsa ��k
            if (parametreler == null || parametreler.Count < 1)
            {
                return;
            }
            //Ayr��t�rma sonucunda komuta g�re gerekli i�lemleri yap
            try
            {
                switch (parametreler["komut"])
                {
                    case "giris": //Yollad���m�z giris mesaj�na kar��l�k gelen mesaj
                        komut_giris(parametreler["sonuc"]);
                        break;
                    case "ozelmesaj": //Bir ki�iden bize gelen �zel mesaj
                        komut_ozelmesaj(parametreler["nick"], parametreler["mesaj"]);
                        break;
                    case "toplumesaj": //Bir ki�iden t�m gruba gelen mesaj
                        komut_toplumesaj(parametreler["nick"], parametreler["mesaj"]);
                        break;
                    case "kullanicigiris": //Bir ki�i girdi�inde bize gelen bilgi
                        komut_kullanicigiris(parametreler["nick"]);
                        break;
                    case "kullanicicikis": //Bir ki�i ��kt���nda bize gelen bilgi
                        komut_kullanicicikis(parametreler["nick"]);
                        break;
                    case "kullanicilistesi": //T�m kullan�c�lar�n listesi
                        komut_kullanicilistesi(parametreler["liste"]);
                        break;
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// giris komutunu uygulayan fonksyon
        /// </summary>
        /// <param name="sonuc">giri� sonucu</param>
        private void komut_giris(string sonuc)
        {
            //giri� ba�ar�l�ysa gerekli kontrolleri aktif yap
            if (sonuc == "basarili")
            {
                gbKullanicilar.Enabled = true;
                gbMesajlar.Enabled = true;
                lblNick.Text = nick;
                Text = "Chat �stemcisi - Ba�l�";
            }
            //giri� ba�ar�s�zsa (nick kullan�mdaysa) sonuna 1-9 aras� rastgele bir say� ekleyip yeniden giri� yap
            else
            {
                int rs = rnd.Next(1, 9);
                nick += rs.ToString();
                istemci.MesajYolla("komut=giris&nick=" + nick);
            }
        }

        /// <summary>
        /// ozelmesaj komutunu uygulayan fonksyon
        /// </summary>
        /// <param name="nick">mesaj� g�nderen nick</param>
        /// <param name="mesaj">mesaj i�eri�i</param>
        private void komut_ozelmesaj(string nick, string mesaj)
        {
            //E�er bu nick'li kullan�c�yla bir sohbet penceresi a��ksa o pencereye referans alal�m.
            frmOzelSohbet sohbetFormu = null;
            //E�zamanl� eri�imlere kar�� koleksiyonu kilitleyelim
            lock (ozelSohbetFormlari)
            {
                if (ozelSohbetFormlari.ContainsKey(nick))
                {
                    sohbetFormu = ozelSohbetFormlari[nick];
                }
            }
            //Bu ki�iyle bir sohbet penceresi a��k de�ilse �nce sohbet penceresini olu�turup a�al�m
            if(sohbetFormu == null)
            {
                sohbetFormu = new frmOzelSohbet(this, nick);
                lock (ozelSohbetFormlari)
                {
                    ozelSohbetFormlari.Add(nick, sohbetFormu);
                }
                sohbetFormu.Show();
            }
            //Mesaj� bu pencereye y�nlendirelim
            sohbetFormu.MesajAl(mesaj);
        }

        /// <summary>
        /// toplumesaj komutunu uygulayan fonksyon
        /// </summary>
        /// <param name="nick">Mesaj� g�nderen kullan�c�n�n nick'i</param>
        /// <param name="mesaj">G�nderilen mesaj</param>
        private void komut_toplumesaj(string nick, string mesaj)
        {
            //gelen mesaj� sohbet alan�na ekle
            string mesajlar = txtTopluMesajlar.Text;
            mesajlar += "\r\n" + nick + ": " + mesaj;
            txtTopluMesajlar.Text = mesajlar;
        }

        /// <summary>
        /// kullanicigiris komutunu uygulayan fonksyon
        /// </summary>
        /// <param name="nick"></param>
        private void komut_kullanicigiris(string nick)
        {
            //E�er kullan�c� 'kullan�c�lar' listesinde yoksa listeye ekle
            lock (kullanicilar)
            {
                if (!kullanicilar.Contains(nick))
                {
                    kullanicilar.Add(nick);
                }
                kullanicilar.Sort();
            }
            //Ekrandaki listeyi g�ncelle
            kullaniciListesiniGuncelle();
            //E�er bu kullan�c�yle bir sohbet penceresi a��ksa, pencereye bilgi g�nder
            frmOzelSohbet sohbetFormu = null;
            lock (ozelSohbetFormlari)
            {
                if (ozelSohbetFormlari.ContainsKey(nick))
                {
                    sohbetFormu = ozelSohbetFormlari[nick];
                }
            }
            if (sohbetFormu != null)
            {
                sohbetFormu.KullaniciGirdi();
            }
        }

        /// <summary>
        /// kullanicicikis komutunu uygulayan fonksyon
        /// </summary>
        /// <param name="nick"></param>
        private void komut_kullanicicikis(string nick)
        {
            //E�er kullan�c� 'kullan�c�lar' listesinde varsa listeden sil
            lock (kullanicilar)
            {
                if (kullanicilar.Contains(nick))
                {
                    kullanicilar.Remove(nick);
                }
            }
            //Ekrandaki listeyi g�ncelle
            kullaniciListesiniGuncelle();
            //E�er bu kullan�c�yle bir sohbet penceresi a��ksa, pencereye bilgi g�nder
            frmOzelSohbet sohbetFormu = null;
            lock (ozelSohbetFormlari)
            {
                if (ozelSohbetFormlari.ContainsKey(nick))
                {
                    sohbetFormu = ozelSohbetFormlari[nick];
                }
            }
            if (sohbetFormu != null)
            {
                sohbetFormu.KullaniciCikti();
            }
        }

        /// <summary>
        /// kullanicilistesi komutunu uygulayan fonksyon
        /// </summary>
        /// <param name="liste">Sistemdeki kullan�c�lar�n , ile ay�r�lm�� listesi</param>
        private void komut_kullanicilistesi(string liste)
        {
            //T�m kullan�c�lar� temizle ve gelen listeye g�re yeniden olu�tur
            try
            {
                //Gelen mesaj� , ile ay�r
                string[] kullaniciDizisi = liste.Split(',');
                lock (kullanicilar)
                {
                    //Mevcut listeyi temizle
                    kullanicilar.Clear();
                    //Gelen listeyi ekle
                    kullanicilar.AddRange(kullaniciDizisi);
                }
            }
            catch (Exception)
            {
                
            }
            //Ekrandaki listeyi g�ncelle
            kullaniciListesiniGuncelle();
        }

        /// <summary>
        /// �zel Sohbet formlar� vas�tas�yla sunucuya bir mesaj yollamak i�indir.
        /// </summary>
        /// <param name="nick">Kar�� taraf�n nick'i</param>
        /// <param name="mesaj">Mesaj i�eri�i</param>
        public void OzelMesajYolla(string nick, string mesaj)
        {
            istemci.MesajYolla("komut=ozelmesaj&nick=" + nick + "&mesaj=" + mesaj);
        }

        /// <summary>
        /// Bir �zel Sohbet formu kapand���nda buras� �a��r�l�r.
        /// </summary>
        /// <param name="nick">Hangi kullan�c� ile yap�lan sohbetin kapat�ld���n� belirtir</param>
        public void OzelSohbetFormuKapandi(string nick)
        {
            lock (ozelSohbetFormlari)
            {
                if (ozelSohbetFormlari.ContainsKey(nick))
                {
                    ozelSohbetFormlari.Remove(nick);
                }
            }
        }

        /// <summary>
        /// lstKullanicilar ListBox'unda kullanicilar listesini g�sterir.
        /// </summary>
        private void kullaniciListesiniGuncelle()
        {
            //Kullan�c�lar� bir diziye al
            string[] kullaniciDizisi = null;
            lock (kullanicilar)
            {
                kullaniciDizisi = kullanicilar.ToArray();
            }
            //Listeyi temizle
            lstKullanicilar.Items.Clear();
            //T�m kullan�c�lar� listeye ekle
            foreach (string kul in kullaniciDizisi)
            {
                lstKullanicilar.Items.Add(kul);
            }
        }

        /// <summary>
        /// ListBox'daki bir kullan�c�ya �ift t�kland���nda yeni bir �zel sohbet penceresi a�mak i�in kullan�l�r.
        /// </summary>
        private void lstKullanicilar_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //Se�ilen eleman varsa..
            if (lstKullanicilar.SelectedItems.Count > 0 && lstKullanicilar.SelectedItem != null)
            {
                //Se�ilen kullan�c�n�n nick'inin al
                string secilenNick = lstKullanicilar.SelectedItem as string;
                if (secilenNick != null)
                {
                    lock (ozelSohbetFormlari)
                    {
                        //E�er bu kullan�c� ile bir sohbet formu zaten a��ksa formu aktif yap, 
                        //de�ilse yeni bir �zel sohbet formu a� ve ozelSohbetFormlari listesine ekle
                        if (ozelSohbetFormlari.ContainsKey(secilenNick))
                        {
                            ozelSohbetFormlari[secilenNick].Activate();
                        }
                        else
                        {
                            frmOzelSohbet sohbetFormu = new frmOzelSohbet(this, secilenNick);
                            ozelSohbetFormlari.Add(secilenNick, sohbetFormu);
                            sohbetFormu.Show();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Toplu sohbet penceresindeki alana mesaj yazarken enter'a bas�ld���nda
        /// mesaj� sunucuya yollamak i�indir.
        /// </summary>
        private void txtTopluMesaj_KeyPress(object sender, KeyPressEventArgs e)
        {
            //Enter'a bas�lm��sa ve textbox'da bir metin varsa sunucuya yolla
            if (e.KeyChar == (char)13 && txtTopluMesaj.Text.Length > 0)
            {
                //Mesaj� kontrol et, uygunsa yolla
                if (mesajGonderimeUygun(txtTopluMesaj.Text))
                {
                    //mesaj� sunucuya yolla
                    istemci.MesajYolla("komut=toplumesaj&mesaj=" + txtTopluMesaj.Text);
                    //tu�a bas�lmay� iptal et ( bas�lan enter tu�unu dikkate alma )
                    e.Handled = true;
                    //yaz� alan�n� bir sonraki mesaj i�in bo�alt                
                    txtTopluMesaj.Text = "";
                }
                else
                {
                    //Mesaj uygun de�ilse uyar� g�ster
                    MessageBox.Show("G�ndermek istedi�iniz mesajda uygun olmayan karakterler var. Mesaj i�erisinde �u karakterler olamaz: < > & =", "Dikkat!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
        }

        // D��ER YARARLI FONKSYONLAR //////////////////////////////////////////

        public NameValueCollection mesajCoz(string mesaj)
        {
            try
            {
                //& i�aretine g�re b�l ve diziye at
                string[] parametreler = mesaj.Split('&');
                //d�n�� de�eri i�in bir NameValueCollection olu�tur
                NameValueCollection nvcParametreler = new NameValueCollection(parametreler.Length);
                //b�l�nen her parametreyi = i�aretine g�re yeniden b�l ve anahtar/de�er �iftleri �ret
                foreach (string parametre in parametreler)
                {
                    string[] esitlik = parametre.Split('=');
                    nvcParametreler.Add(esitlik[0], esitlik[1]);
                }
                //olu�turulan koleksiyonu d�nder
                return nvcParametreler;
            }
            catch (Exception)
            {
                return null;
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
    }
}