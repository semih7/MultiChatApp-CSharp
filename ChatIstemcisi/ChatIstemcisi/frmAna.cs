using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using ASMES;
using ASMES.IstemciTarafi;

namespace ChatIstemcisi
{
    public partial class frmAna : Form
    {
        
        // ASMES kütüphanesini kullanarak ASMES sunucusuna baðlý olan istemci nesnesi
        
        private ASMESIstemcisi istemci;

        
        // Kullanýcýnýn Nick'i
        
        public string Nick
        {
            get { return nick; }
            set { nick = value; }
        }
        private string nick;

        
        // Rastgele sayý üretmede kullanýlacak bir nesne
        
        private Random rnd = new Random();

        
        // Özel sohbet yapýlan formlar
        
        private SortedList<string, frmOzelSohbet> ozelSohbetFormlari;

        
        // Chat ortamýnda bulunan kullanýcý listesi
        
        private List<string> kullanicilar;

        public frmAna()
        {
            ozelSohbetFormlari = new SortedList<string, frmOzelSohbet>();
            kullanicilar = new List<string>();
            InitializeComponent();
        }

        private void frmAna_Shown(object sender, EventArgs e)
        {
            //Giriþ formunu göster
            frmGiris girisFormu = new frmGiris();
            girisFormu.ShowDialog();
            //Eðer giriþ formundan sunucuya doðru baðlanýldýysa sistemi baþlat
            if (girisFormu.GirisYapildi)
            {
                //ASMESIstemcisi referansýný al
                istemci = girisFormu.Istemci;
                nick = girisFormu.Nick;
                //Olaylara kaydol
                istemci.YeniMesajAlindi += new dgYeniMesajAlindi(istemci_YeniMesajAlindi);
                //Sunucuya giriþ mesajý gönder
                Text = "Chat Ýstemcisi - Baðlanýyor...";
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
            //Form kapatýlýrken sunucuya olan baðlantýyý keselim
            if (istemci != null)
            {
                istemci.MesajYolla("komut=cikis");
                istemci.BaglantiyiKes();
            }
        }

        // CHAT MANTIÐININ ÇALIÞTIÐI KODLAR

        
        // Sunucudan bir mesaj alýndýðýnda buraya gelir
        
        /// <param name="e">Alýnan mesajla ilgili bilgiler</param>
        private void mesajAlindi(MesajAlmaArgumanlari e)
        {
            //Gelen mesajý & ve = iþaretlerine göre ayrýþtýr
            NameValueCollection parametreler = mesajCoz(e.Mesaj);
            //Ayrýþtýrma baþarýsýzsa çýk
            if (parametreler == null || parametreler.Count < 1)
            {
                return;
            }
            //Ayrýþtýrma sonucunda komuta göre gerekli iþlemleri yap
            try
            {
                switch (parametreler["komut"])
                {
                    case "giris": //Yolladýðýmýz giris mesajýna karþýlýk gelen mesaj
                        komut_giris(parametreler["sonuc"]);
                        break;
                    case "ozelmesaj": //Bir kiþiden bize gelen özel mesaj
                        komut_ozelmesaj(parametreler["nick"], parametreler["mesaj"]);
                        break;
                    case "toplumesaj": //Bir kiþiden tüm gruba gelen mesaj
                        komut_toplumesaj(parametreler["nick"], parametreler["mesaj"]);
                        break;
                    case "kullanicigiris": //Bir kiþi girdiðinde bize gelen bilgi
                        komut_kullanicigiris(parametreler["nick"]);
                        break;
                    case "kullanicicikis": //Bir kiþi çýktýðýnda bize gelen bilgi
                        komut_kullanicicikis(parametreler["nick"]);
                        break;
                    case "kullanicilistesi": //Tüm kullanýcýlarýn listesi
                        komut_kullanicilistesi(parametreler["liste"]);
                        break;
                }
            }
            catch (Exception)
            {

            }
        }

        
        // giris komutunu uygulayan fonksyon
        
        /// <param name="sonuc">giriþ sonucu</param>
        private void komut_giris(string sonuc)
        {
            //giriþ baþarýlýysa gerekli kontrolleri aktif yap
            if (sonuc == "basarili")
            {
                gbKullanicilar.Enabled = true;
                gbMesajlar.Enabled = true;
                lblNick.Text = nick;
                Text = "Chat Ýstemcisi - Baðlý";
            }
            //giriþ baþarýsýzsa (nick kullanýmdaysa) sonuna 1-9 arasý rastgele bir sayý ekleyip yeniden giriþ yap
            else
            {
                int rs = rnd.Next(1, 9);
                nick += rs.ToString();
                istemci.MesajYolla("komut=giris&nick=" + nick);
            }
        }

        
        // ozelmesaj komutunu uygulayan fonksyon
        
        /// <param name="nick">mesajý gönderen nick</param>
        /// <param name="mesaj">mesaj içeriði</param>
        private void komut_ozelmesaj(string nick, string mesaj)
        {
            //Eðer bu nick'li kullanýcýyla bir sohbet penceresi açýksa o pencereye referans alalým.
            frmOzelSohbet sohbetFormu = null;
            //Eþzamanlý eriþimlere karþý koleksiyonu kilitleyelim
            lock (ozelSohbetFormlari)
            {
                if (ozelSohbetFormlari.ContainsKey(nick))
                {
                    sohbetFormu = ozelSohbetFormlari[nick];
                }
            }
            //Bu kiþiyle bir sohbet penceresi açýk deðilse önce sohbet penceresini oluþturup açalým
            if(sohbetFormu == null)
            {
                sohbetFormu = new frmOzelSohbet(this, nick);
                lock (ozelSohbetFormlari)
                {
                    ozelSohbetFormlari.Add(nick, sohbetFormu);
                }
                sohbetFormu.Show();
            }
            //Mesajý bu pencereye yönlendirelim
            sohbetFormu.MesajAl(mesaj);
        }

        
        // toplumesaj komutunu uygulayan fonksyon
        
        /// <param name="nick">Mesajý gönderen kullanýcýnýn nick'i</param>
        /// <param name="mesaj">Gönderilen mesaj</param>
        private void komut_toplumesaj(string nick, string mesaj)
        {
            //gelen mesajý sohbet alanýna ekle
            string mesajlar = txtTopluMesajlar.Text;
            mesajlar += "\r\n" + nick + ": " + mesaj;
            txtTopluMesajlar.Text = mesajlar;
        }

        
        //kullanicigiris komutunu uygulayan fonksyon
        
        /// <param name="nick"></param>
        private void komut_kullanicigiris(string nick)
        {
            //Eðer kullanýcý 'kullanýcýlar' listesinde yoksa listeye ekle
            lock (kullanicilar)
            {
                if (!kullanicilar.Contains(nick))
                {
                    kullanicilar.Add(nick);
                }
                kullanicilar.Sort();
            }
            //Ekrandaki listeyi güncelle
            kullaniciListesiniGuncelle();
            //Eðer bu kullanýcýyle bir sohbet penceresi açýksa, pencereye bilgi gönder
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

        
        // kullanicicikis komutunu uygulayan fonksyon
        
        /// <param name="nick"></param>
        private void komut_kullanicicikis(string nick)
        {
            //Eðer kullanýcý 'kullanýcýlar' listesinde varsa listeden sil
            lock (kullanicilar)
            {
                if (kullanicilar.Contains(nick))
                {
                    kullanicilar.Remove(nick);
                }
            }
            //Ekrandaki listeyi güncelle
            kullaniciListesiniGuncelle();
            //Eðer bu kullanýcýyle bir sohbet penceresi açýksa, pencereye bilgi gönder
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

        
        // kullanicilistesi komutunu uygulayan fonksyon
        
        /// <param name="liste">Sistemdeki kullanýcýlarýn , ile ayýrýlmýþ listesi</param>
        private void komut_kullanicilistesi(string liste)
        {
            //Tüm kullanýcýlarý temizle ve gelen listeye göre yeniden oluþtur
            try
            {
                //Gelen mesajý , ile ayýr
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
            //Ekrandaki listeyi güncelle
            kullaniciListesiniGuncelle();
        }

        
        // Özel Sohbet formlarý vasýtasýyla sunucuya bir mesaj yollamak içindir.
        
        /// <param name="nick">Karþý tarafýn nick'i</param>
        /// <param name="mesaj">Mesaj içeriði</param>
        public void OzelMesajYolla(string nick, string mesaj)
        {
            istemci.MesajYolla("komut=ozelmesaj&nick=" + nick + "&mesaj=" + mesaj);
        }

        
        // Bir Özel Sohbet formu kapandýðýnda burasý çaðýrýlýr.
        
        /// <param name="nick">Hangi kullanýcý ile yapýlan sohbetin kapatýldýðýný belirtir</param>
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

        
        // lstKullanicilar ListBox'unda kullanicilar listesini gösterir.
        
        private void kullaniciListesiniGuncelle()
        {
            //Kullanýcýlarý bir diziye al
            string[] kullaniciDizisi = null;
            lock (kullanicilar)
            {
                kullaniciDizisi = kullanicilar.ToArray();
            }
            //Listeyi temizle
            lstKullanicilar.Items.Clear();
            //Tüm kullanýcýlarý listeye ekle
            foreach (string kul in kullaniciDizisi)
            {
                lstKullanicilar.Items.Add(kul);
            }
        }

        
        // ListBox'daki bir kullanýcýya çift týklandýðýnda yeni bir özel sohbet penceresi açmak için kullanýlýr.
        
        private void lstKullanicilar_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //Seçilen eleman varsa..
            if (lstKullanicilar.SelectedItems.Count > 0 && lstKullanicilar.SelectedItem != null)
            {
                //Seçilen kullanýcýnýn nick'inin al
                string secilenNick = lstKullanicilar.SelectedItem as string;
                if (secilenNick != null)
                {
                    lock (ozelSohbetFormlari)
                    {
                        //Eðer bu kullanýcý ile bir sohbet formu zaten açýksa formu aktif yap, 
                        //deðilse yeni bir özel sohbet formu aç ve ozelSohbetFormlari listesine ekle
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

       
        //Toplu sohbet penceresindeki alana mesaj yazarken enter'a basýldýðýnda
        // mesajý sunucuya yollamak içindir.
        
        private void txtTopluMesaj_KeyPress(object sender, KeyPressEventArgs e)
        {
            //Enter'a basýlmýþsa ve textbox'da bir metin varsa sunucuya yolla
            if (e.KeyChar == (char)13 && txtTopluMesaj.Text.Length > 0)
            {
                //Mesajý kontrol et, uygunsa yolla
                if (mesajGonderimeUygun(txtTopluMesaj.Text))
                {
                    //mesajý sunucuya yolla
                    istemci.MesajYolla("komut=toplumesaj&mesaj=" + txtTopluMesaj.Text);
                    //tuþa basýlmayý iptal et ( basýlan enter tuþunu dikkate alma )
                    e.Handled = true;
                    //yazý alanýný bir sonraki mesaj için boþalt                
                    txtTopluMesaj.Text = "";
                }
                else
                {
                    //Mesaj uygun deðilse uyarý göster
                    MessageBox.Show("Göndermek istediðiniz mesajda uygun olmayan karakterler var. Mesaj içerisinde þu karakterler olamaz: < > & =", "Dikkat!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
        }

        // DÝÐER YARARLI FONKSYONLAR 

        public NameValueCollection mesajCoz(string mesaj)
        {
            try
            {
                //& iþaretine göre böl ve diziye at
                string[] parametreler = mesaj.Split('&');
                //dönüþ deðeri için bir NameValueCollection oluþtur
                NameValueCollection nvcParametreler = new NameValueCollection(parametreler.Length);
                //bölünen her parametreyi = iþaretine göre yeniden böl ve anahtar/deðer çiftleri üret
                foreach (string parametre in parametreler)
                {
                    string[] esitlik = parametre.Split('=');
                    nvcParametreler.Add(esitlik[0], esitlik[1]);
                }
                //oluþturulan koleksiyonu dönder
                return nvcParametreler;
            }
            catch (Exception)
            {
                return null;
            }
        }

        
        // Mesajýn gönderime uygunluðunu denetler
        
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
    }
}