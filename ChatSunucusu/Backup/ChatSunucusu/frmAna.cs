using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
//Ýlgili namespace bildirimleri
using HIK.ASMES;
using HIK.ASMES.SunucuTarafi;

namespace ChatSunucusu
{
    public partial class frmAna : Form
    {
        /// <summary>
        /// ASMES kütüphanesindeki sunucu nesnesi
        /// </summary>
        private ASMESSunucusu sunucu;
        /// <summary>
        /// Sunucuya baðlý olan kullanýcýlarý saklayan liste
        /// </summary>
        private List<Kullanici> kullanicilar;

        public frmAna()
        {
            kullanicilar = new List<Kullanici>();
            InitializeComponent();
        }

        private void btnBaslat_Click(object sender, EventArgs e)
        {
            if (baslat())
            {
                //Kullanýcý arayüzünü ayarla
                txtPortNo.ReadOnly = true;
                btnBaslat.Enabled = false;
                btnDurdur.Enabled = true;
            }
            else
            {
                MessageBox.Show("Sunucu baþlatýlamadý!", "Hata!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDurdur_Click(object sender, EventArgs e)
        {
            //Sunucuyu durdur
            durdur();
            //Kullanýcý arayüzünü ayarla
            txtPortNo.ReadOnly = false;
            btnBaslat.Enabled = true;
            btnDurdur.Enabled = false;
        }

        /// <summary>
        /// ASMES sunucusunu baþlatýr
        /// </summary>
        /// <returns>Ýþlemin baþarý durumu</returns>
        private bool baslat()
        {
            //Port numarasýný TextBox'dan al
            int port = 0;
            try
            {
                port = Convert.ToInt32(txtPortNo.Text);
                if (port <= 0)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            //Kullanýcý listesini temizle
            kullanicilar.Clear();

            //Sunucuyu oluþtur, olaylarýna kaydol ve baþlat
            sunucu = new ASMESSunucusu(port);
            sunucu.IstemcidenYeniMesajAlindi += new dgIstemcidenYeniMesajAlindi(sunucu_IstemcidenYeniMesajAlindi);
            sunucu.IstemciBaglantisiKapatildi += new dgIstemciBaglantisiKapatildi(sunucu_IstemciBaglantisiKapatildi);
            sunucu.Baslat();

            return true;
        }

        /// <summary>
        /// ASMES sunucusunu durdurur
        /// </summary>
        private void durdur()
        {
            sunucu.Durdur();
            sunucu = null;
        }

        private void frmAna_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Form kapatýlýrken, sunucu çalýþýyorsa durduralým.
            if (sunucu != null)
            {
                sunucu.Durdur();
            }
        }

        private void sunucu_IstemciBaglantisiKapatildi(IstemciBaglantiArgumanlari e)
        {
            Invoke(new dgIstemciBaglantisiKapatildi(istemciKapandi), e);
        }

        private void sunucu_IstemcidenYeniMesajAlindi(IstemcidenMesajAlmaArgumanlari e)
        {
            Invoke(new dgIstemcidenYeniMesajAlindi(mesajAlindi), e);
        }

        // CHAT MANTIÐININ ÇALIÞTIÐI KODLAR ///////////////////////////////////

        /// <summary>
        /// Bir istemci baðlantýsý kapatýldýðýnda ilgili olay bu fonksyonu çaðýrýr
        /// </summary>
        /// <param name="e">Kapanan istemciyle ilgili bilgiler</param>
        private void istemciKapandi(IstemciBaglantiArgumanlari e)
        {
            komut_cikis(e.Istemci);
        }

        /// <summary>
        /// Bir istemciden mesaj alýndýðýnda ilgili olay bu fonksyonu çaðýrýr
        /// </summary>
        /// <param name="e">Mesaj ve Ýstemci parametreleri</param>
        private void mesajAlindi(IstemcidenMesajAlmaArgumanlari e)
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
                    case "giris":
                        //parametreler: nick
                        komut_giris(e.Istemci, parametreler["nick"]);
                        break;
                    case "ozelmesaj":
                        //parametreler: nick, mesaj
                        komut_ozelmesaj(e.Istemci, parametreler["nick"], parametreler["mesaj"]);
                        break;
                    case "toplumesaj":
                        //parametreler: mesaj
                        komut_toplumesaj(e.Istemci, parametreler["mesaj"]);
                        break;
                    case "cikis":
                        //parametreler: YOK
                        komut_cikis(e.Istemci);
                        break;
                }
            }
            catch (Exception)
            {

            }

            //Mesajý 'Son Gelen 10 Mesaj' listesine en baþa ekle
            lstSon10Mesaj.Items.Insert(0, "[" + e.Istemci.IstemciID.ToString("0000") + "] " + e.Mesaj);
            //Listedeki mesaj sayýsý 10'u geçmiþse sondan sil.
            if (lstSon10Mesaj.Items.Count > 10)
            {
                lstSon10Mesaj.Items.RemoveAt(10);
            }
        }

        /// <summary>
        /// giris komutunu uygulayan fonksyon
        /// </summary>
        /// <param name="istemci">Girþi yapan istemci</param>
        /// <param name="nick">Seçilen nick</param>
        private void komut_giris(IIstemci istemci, string nick)
        {
            //Eþzamanlý eriþimlere karþý koleksiyonu kilitleyelim
            lock (kullanicilar)
            {
                Kullanici kullanici = null;
                //Tüm kullanýcýlarý tara, 
                //ayný nickli baþkasý varsa giriþ baþarýsýzdýr
                foreach (Kullanici kul in kullanicilar)
                {
                    if (kul.Nick == nick)
                    {
                        kullanici = kul;
                        break;
                    }
                }
                //Nick kullanýmdaysa istemciye uygun dönüþ mesajýný verip çýk
                if (kullanici != null)
                {
                    istemci.MesajYolla("komut=giris&sonuc=basarisiz");
                    return;
                }
                //Tüm kullanýcýlarý tara,
                //ayný istemci zaten listede varsa sadece nickini güncelle
                foreach (Kullanici kul in kullanicilar)
                {
                    if (kul.Istemci == istemci)
                    {
                        kullanici = kul;
                        break;
                    }
                }
                //Ýstemci listede varsa sadece nickini güncelle
                if (kullanici != null)
                {
                    kullanici.Nick = nick;
                }
                //Listede yoksa listeye ekle
                else
                {
                    kullanicilar.Add(new Kullanici(istemci, nick));
                }
            }
            //Kullanýcýya iþlemin baþarýlý olduðu bilgisini gönder
            istemci.MesajYolla("komut=giris&sonuc=basarili");
            //Tüm kullanýcýlara bu kullanýcýnýn giriþ yaptýðý bilgisini gönder
            tumKullanicilaraMesajYolla("komut=kullanicigiris&nick=" + nick);
            //Bu kullanýcýya mevcut kullanýcý listesini gönder
            kullaniciListesiniGonder(istemci);
            //Kullanýcý listesini ekranda gösterelim
            kullaniciListesiniYenile();
        }

        /// <summary>
        /// ozelmesaj komutunu uygulayan fonksyon
        /// </summary>
        /// <param name="istemci">Mesajý gönderen istemci</param>
        /// <param name="nick">Mesajýn iletileceði nick</param>
        /// <param name="mesaj">Gönderilen mesaj</param>
        private void komut_ozelmesaj(IIstemci istemci, string nick, string mesaj)
        {
            //Kullanýcýlarý saklamak için deðiþkenler
            Kullanici gonderenKullanici = null, hedefKullanici = null;
            //Eþzamanlý eriþimlere karþý koleksiyonu kilitleyelim
            lock (kullanicilar)
            {
                //Tüm kullanýcýlarý tara, 
                //mesajý gönderen ve mesajýn hedefinde olan kullanýcýyý bul
                foreach (Kullanici kul in kullanicilar)
                {
                    //Gönderen kullanýcýyý Istemci nesnesine göre ayýrt ediyoruz
                    if (kul.Istemci == istemci)
                    {
                        gonderenKullanici = kul;
                    }
                    //Hedef kullanýcýyý nick'e göre ayýrt ediyoruz
                    if (kul.Nick == nick)
                    {
                        hedefKullanici = kul;
                    }
                    //Eðer kullanýcýlarý bulduysak döngüyü devam ettirmeye gerek yok
                    if (gonderenKullanici != null && hedefKullanici != null)
                    {
                        break;
                    }
                }
            }
            //Kullanýcýlar bulunamadýysa fonksyonu sonlandýralým
            if (gonderenKullanici == null || hedefKullanici == null)
            {
                return;
            }
            //Hedef kullanýcýya istenilen mesajý gönderelim
            hedefKullanici.Istemci.MesajYolla("komut=ozelmesaj&nick=" + gonderenKullanici.Nick + "&mesaj=" + mesaj);
        }

        /// <summary>
        /// toplumesaj komutunu uygulayan fonksyon
        /// </summary>
        /// <param name="istemci">Mesajý gönderen istemci</param>
        /// <param name="mesaj">Gönderilen mesaj</param>
        private void komut_toplumesaj(IIstemci istemci, string mesaj)
        {
            //Kullanýcýlarý saklamak için deðiþkenler
            Kullanici gonderenKullanici = null;
            //Eþzamanlý eriþimlere karþý koleksiyonu kilitleyelim
            lock (kullanicilar)
            {
                //Tüm kullanýcýlarý tara, 
                //mesajý gönderen kullanýcýyý bul
                foreach (Kullanici kul in kullanicilar)
                {
                    //Gönderen kullanýcýyý Istemci nesnesine göre ayýrt ediyoruz
                    if (kul.Istemci == istemci)
                    {
                        gonderenKullanici = kul;
                        break;
                    }
                }
            }
            //Gönderen kullanýcý bulunamadýysa fonksyonu sonlandýralým
            if (gonderenKullanici == null)
            {
                return;
            }
            //Tüm kullanýcýlara istenilen mesajý gönderelim
            tumKullanicilaraMesajYolla("komut=toplumesaj&nick=" + gonderenKullanici.Nick + "&mesaj=" + mesaj);
        }

        /// <summary>
        /// cikis komutunu uygulayan fonksyon
        /// </summary>
        /// <param name="istemci">Çýkýþ yapan istemci</param>
        private void komut_cikis(IIstemci istemci)
        {
            Kullanici kullanici = null;
            //Eþzamanlý eriþimlere karþý koleksiyonu kilitleyelim
            lock (kullanicilar)
            {
                //Tüm kullanýcýlarý tara, istemci nesnesini bul
                foreach (Kullanici kul in kullanicilar)
                {
                    if (kul.Istemci == istemci)
                    {
                        kullanici = kul;
                        break;
                    }
                }
                //Ýstemci listede varsa listeden çýkar
                if (kullanici != null)
                {
                    kullanicilar.Remove(kullanici);
                }
                //Listede yoksa devam etmeye gerek yok, fonksyondan çýk
                else
                {
                    return;
                }
            }
            //Tüm kullanýcýlara bu kullanýcýnýn giriþ yaptýðý bilgisini gönder
            tumKullanicilaraMesajYolla("komut=kullanicicikis&nick=" + kullanici.Nick);
            //Kullanýcý listesini ekranda gösterelim
            kullaniciListesiniYenile();
        }

        /// <summary>
        /// Bir istemciye tüm kullanýcýlarýn listesini gönderir
        /// </summary>
        /// <param name="istemci"></param>
        private void kullaniciListesiniGonder(IIstemci istemci)
        {
            //Kullanýcý listesini "," ile ayýrarak birleþtir
            StringBuilder nickler = new StringBuilder();
            //Eþzamanlý eriþimlere karþý koleksiyonu kilitleyelim
            lock (kullanicilar)
            {
                //Tüm kullanýcýlarý tara, nickleri birleþtir
                foreach (Kullanici kul in kullanicilar)
                {
                    nickler.Append("," + kul.Nick);
                }
                //Ýlk kullanýcýnýn baþýna konulan "," metnini kaldýr
                if (nickler.Length >= 1)
                {
                    nickler.Remove(0, 1);
                }
            }
            //Kullanýcýya listeyi gönder
            istemci.MesajYolla("komut=kullanicilistesi&liste=" + nickler.ToString());
        }

        /// <summary>
        /// kullanýcýlar listesindeki kullanýcýlarýn nick'lerini ekranda gösterir.
        /// </summary>
        private void kullaniciListesiniYenile()
        {
            //Eþzamanlý eriþimlere karþý koleksiyonu kilitleyelim
            lock (kullanicilar)
            {
                StringBuilder nickler = new StringBuilder();
                //Tüm kullanýcýlarý tara, nickleri birleþtir
                foreach (Kullanici kul in kullanicilar)
                {
                    nickler.Append(", " + kul.Nick);
                }
                //Ýlk kullanýcýnýn baþýna konulan ", " metnini kaldýr
                if (nickler.Length >= 2)
                {
                    nickler.Remove(0, 2);
                }
                //Nickleri göster
                txtOnlineKullanicilar.Text = nickler.ToString();
            }
        }

        /// <summary>
        /// kullanýcýlar listesindeki tüm kullanýcýlara istenilen bir mesajý iletir
        /// </summary>
        /// <param name="mesaj"></param>
        private void tumKullanicilaraMesajYolla(string mesaj)
        {
            Kullanici[] kullaniciDizisi = null;
            //Eþzamanlý eriþimlere karþý koleksiyonu kilitleyelim
            lock (kullanicilar)
            {
                //Listedeki tüm kullanýcýlarý bir diziye atalým
                kullaniciDizisi = kullanicilar.ToArray();
            }
            //Tüm kullanýcýlara istenilen mesajý gönderelim
            foreach (Kullanici kul in kullaniciDizisi)
            {
                kul.Istemci.MesajYolla(mesaj);
            }
        }

        // DÝÐER YARARLI FONKSYONLAR //////////////////////////////////////////

        private NameValueCollection mesajCoz(string mesaj)
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

        // ALT SINIFLAR ///////////////////////////////////////////////////////

        private class Kullanici
        {
            /// <summary>
            /// ASMES kütüphanesindeki Istemci nesnesine referans
            /// </summary>
            public IIstemci Istemci
            {
                get { return istemci; }
                set { istemci = value; }
            }
            private IIstemci istemci;

            /// <summary>
            /// Kullanýcýnýn Nick'i
            /// </summary>
            public string Nick
            {
                get { return nick; }
                set { nick = value; }
            }
            private string nick;

            /// <summary>
            /// Yeni bir Kullanýcý nesnesi oluþturur.
            /// </summary>
            /// <param name="istemci">ASMES kütüphanesindeki Istemci nesnesine referans</param>
            /// <param name="nick">Kullanýcýnýn Nick'i</param>
            public Kullanici(IIstemci istemci, string nick)
            {
                this.istemci = istemci;
                this.nick = nick;
            }
        }
    }
}