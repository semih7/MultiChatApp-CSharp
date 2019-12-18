using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
//�lgili namespace bildirimleri
using HIK.ASMES;
using HIK.ASMES.SunucuTarafi;

namespace ChatSunucusu
{
    public partial class frmAna : Form
    {
        /// <summary>
        /// ASMES k�t�phanesindeki sunucu nesnesi
        /// </summary>
        private ASMESSunucusu sunucu;
        /// <summary>
        /// Sunucuya ba�l� olan kullan�c�lar� saklayan liste
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
                //Kullan�c� aray�z�n� ayarla
                txtPortNo.ReadOnly = true;
                btnBaslat.Enabled = false;
                btnDurdur.Enabled = true;
            }
            else
            {
                MessageBox.Show("Sunucu ba�lat�lamad�!", "Hata!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDurdur_Click(object sender, EventArgs e)
        {
            //Sunucuyu durdur
            durdur();
            //Kullan�c� aray�z�n� ayarla
            txtPortNo.ReadOnly = false;
            btnBaslat.Enabled = true;
            btnDurdur.Enabled = false;
        }

        /// <summary>
        /// ASMES sunucusunu ba�lat�r
        /// </summary>
        /// <returns>��lemin ba�ar� durumu</returns>
        private bool baslat()
        {
            //Port numaras�n� TextBox'dan al
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

            //Kullan�c� listesini temizle
            kullanicilar.Clear();

            //Sunucuyu olu�tur, olaylar�na kaydol ve ba�lat
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
            //Form kapat�l�rken, sunucu �al���yorsa durdural�m.
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

        // CHAT MANTI�ININ �ALI�TI�I KODLAR ///////////////////////////////////

        /// <summary>
        /// Bir istemci ba�lant�s� kapat�ld���nda ilgili olay bu fonksyonu �a��r�r
        /// </summary>
        /// <param name="e">Kapanan istemciyle ilgili bilgiler</param>
        private void istemciKapandi(IstemciBaglantiArgumanlari e)
        {
            komut_cikis(e.Istemci);
        }

        /// <summary>
        /// Bir istemciden mesaj al�nd���nda ilgili olay bu fonksyonu �a��r�r
        /// </summary>
        /// <param name="e">Mesaj ve �stemci parametreleri</param>
        private void mesajAlindi(IstemcidenMesajAlmaArgumanlari e)
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

            //Mesaj� 'Son Gelen 10 Mesaj' listesine en ba�a ekle
            lstSon10Mesaj.Items.Insert(0, "[" + e.Istemci.IstemciID.ToString("0000") + "] " + e.Mesaj);
            //Listedeki mesaj say�s� 10'u ge�mi�se sondan sil.
            if (lstSon10Mesaj.Items.Count > 10)
            {
                lstSon10Mesaj.Items.RemoveAt(10);
            }
        }

        /// <summary>
        /// giris komutunu uygulayan fonksyon
        /// </summary>
        /// <param name="istemci">Gir�i yapan istemci</param>
        /// <param name="nick">Se�ilen nick</param>
        private void komut_giris(IIstemci istemci, string nick)
        {
            //E�zamanl� eri�imlere kar�� koleksiyonu kilitleyelim
            lock (kullanicilar)
            {
                Kullanici kullanici = null;
                //T�m kullan�c�lar� tara, 
                //ayn� nickli ba�kas� varsa giri� ba�ar�s�zd�r
                foreach (Kullanici kul in kullanicilar)
                {
                    if (kul.Nick == nick)
                    {
                        kullanici = kul;
                        break;
                    }
                }
                //Nick kullan�mdaysa istemciye uygun d�n�� mesaj�n� verip ��k
                if (kullanici != null)
                {
                    istemci.MesajYolla("komut=giris&sonuc=basarisiz");
                    return;
                }
                //T�m kullan�c�lar� tara,
                //ayn� istemci zaten listede varsa sadece nickini g�ncelle
                foreach (Kullanici kul in kullanicilar)
                {
                    if (kul.Istemci == istemci)
                    {
                        kullanici = kul;
                        break;
                    }
                }
                //�stemci listede varsa sadece nickini g�ncelle
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
            //Kullan�c�ya i�lemin ba�ar�l� oldu�u bilgisini g�nder
            istemci.MesajYolla("komut=giris&sonuc=basarili");
            //T�m kullan�c�lara bu kullan�c�n�n giri� yapt��� bilgisini g�nder
            tumKullanicilaraMesajYolla("komut=kullanicigiris&nick=" + nick);
            //Bu kullan�c�ya mevcut kullan�c� listesini g�nder
            kullaniciListesiniGonder(istemci);
            //Kullan�c� listesini ekranda g�sterelim
            kullaniciListesiniYenile();
        }

        /// <summary>
        /// ozelmesaj komutunu uygulayan fonksyon
        /// </summary>
        /// <param name="istemci">Mesaj� g�nderen istemci</param>
        /// <param name="nick">Mesaj�n iletilece�i nick</param>
        /// <param name="mesaj">G�nderilen mesaj</param>
        private void komut_ozelmesaj(IIstemci istemci, string nick, string mesaj)
        {
            //Kullan�c�lar� saklamak i�in de�i�kenler
            Kullanici gonderenKullanici = null, hedefKullanici = null;
            //E�zamanl� eri�imlere kar�� koleksiyonu kilitleyelim
            lock (kullanicilar)
            {
                //T�m kullan�c�lar� tara, 
                //mesaj� g�nderen ve mesaj�n hedefinde olan kullan�c�y� bul
                foreach (Kullanici kul in kullanicilar)
                {
                    //G�nderen kullan�c�y� Istemci nesnesine g�re ay�rt ediyoruz
                    if (kul.Istemci == istemci)
                    {
                        gonderenKullanici = kul;
                    }
                    //Hedef kullan�c�y� nick'e g�re ay�rt ediyoruz
                    if (kul.Nick == nick)
                    {
                        hedefKullanici = kul;
                    }
                    //E�er kullan�c�lar� bulduysak d�ng�y� devam ettirmeye gerek yok
                    if (gonderenKullanici != null && hedefKullanici != null)
                    {
                        break;
                    }
                }
            }
            //Kullan�c�lar bulunamad�ysa fonksyonu sonland�ral�m
            if (gonderenKullanici == null || hedefKullanici == null)
            {
                return;
            }
            //Hedef kullan�c�ya istenilen mesaj� g�nderelim
            hedefKullanici.Istemci.MesajYolla("komut=ozelmesaj&nick=" + gonderenKullanici.Nick + "&mesaj=" + mesaj);
        }

        /// <summary>
        /// toplumesaj komutunu uygulayan fonksyon
        /// </summary>
        /// <param name="istemci">Mesaj� g�nderen istemci</param>
        /// <param name="mesaj">G�nderilen mesaj</param>
        private void komut_toplumesaj(IIstemci istemci, string mesaj)
        {
            //Kullan�c�lar� saklamak i�in de�i�kenler
            Kullanici gonderenKullanici = null;
            //E�zamanl� eri�imlere kar�� koleksiyonu kilitleyelim
            lock (kullanicilar)
            {
                //T�m kullan�c�lar� tara, 
                //mesaj� g�nderen kullan�c�y� bul
                foreach (Kullanici kul in kullanicilar)
                {
                    //G�nderen kullan�c�y� Istemci nesnesine g�re ay�rt ediyoruz
                    if (kul.Istemci == istemci)
                    {
                        gonderenKullanici = kul;
                        break;
                    }
                }
            }
            //G�nderen kullan�c� bulunamad�ysa fonksyonu sonland�ral�m
            if (gonderenKullanici == null)
            {
                return;
            }
            //T�m kullan�c�lara istenilen mesaj� g�nderelim
            tumKullanicilaraMesajYolla("komut=toplumesaj&nick=" + gonderenKullanici.Nick + "&mesaj=" + mesaj);
        }

        /// <summary>
        /// cikis komutunu uygulayan fonksyon
        /// </summary>
        /// <param name="istemci">��k�� yapan istemci</param>
        private void komut_cikis(IIstemci istemci)
        {
            Kullanici kullanici = null;
            //E�zamanl� eri�imlere kar�� koleksiyonu kilitleyelim
            lock (kullanicilar)
            {
                //T�m kullan�c�lar� tara, istemci nesnesini bul
                foreach (Kullanici kul in kullanicilar)
                {
                    if (kul.Istemci == istemci)
                    {
                        kullanici = kul;
                        break;
                    }
                }
                //�stemci listede varsa listeden ��kar
                if (kullanici != null)
                {
                    kullanicilar.Remove(kullanici);
                }
                //Listede yoksa devam etmeye gerek yok, fonksyondan ��k
                else
                {
                    return;
                }
            }
            //T�m kullan�c�lara bu kullan�c�n�n giri� yapt��� bilgisini g�nder
            tumKullanicilaraMesajYolla("komut=kullanicicikis&nick=" + kullanici.Nick);
            //Kullan�c� listesini ekranda g�sterelim
            kullaniciListesiniYenile();
        }

        /// <summary>
        /// Bir istemciye t�m kullan�c�lar�n listesini g�nderir
        /// </summary>
        /// <param name="istemci"></param>
        private void kullaniciListesiniGonder(IIstemci istemci)
        {
            //Kullan�c� listesini "," ile ay�rarak birle�tir
            StringBuilder nickler = new StringBuilder();
            //E�zamanl� eri�imlere kar�� koleksiyonu kilitleyelim
            lock (kullanicilar)
            {
                //T�m kullan�c�lar� tara, nickleri birle�tir
                foreach (Kullanici kul in kullanicilar)
                {
                    nickler.Append("," + kul.Nick);
                }
                //�lk kullan�c�n�n ba��na konulan "," metnini kald�r
                if (nickler.Length >= 1)
                {
                    nickler.Remove(0, 1);
                }
            }
            //Kullan�c�ya listeyi g�nder
            istemci.MesajYolla("komut=kullanicilistesi&liste=" + nickler.ToString());
        }

        /// <summary>
        /// kullan�c�lar listesindeki kullan�c�lar�n nick'lerini ekranda g�sterir.
        /// </summary>
        private void kullaniciListesiniYenile()
        {
            //E�zamanl� eri�imlere kar�� koleksiyonu kilitleyelim
            lock (kullanicilar)
            {
                StringBuilder nickler = new StringBuilder();
                //T�m kullan�c�lar� tara, nickleri birle�tir
                foreach (Kullanici kul in kullanicilar)
                {
                    nickler.Append(", " + kul.Nick);
                }
                //�lk kullan�c�n�n ba��na konulan ", " metnini kald�r
                if (nickler.Length >= 2)
                {
                    nickler.Remove(0, 2);
                }
                //Nickleri g�ster
                txtOnlineKullanicilar.Text = nickler.ToString();
            }
        }

        /// <summary>
        /// kullan�c�lar listesindeki t�m kullan�c�lara istenilen bir mesaj� iletir
        /// </summary>
        /// <param name="mesaj"></param>
        private void tumKullanicilaraMesajYolla(string mesaj)
        {
            Kullanici[] kullaniciDizisi = null;
            //E�zamanl� eri�imlere kar�� koleksiyonu kilitleyelim
            lock (kullanicilar)
            {
                //Listedeki t�m kullan�c�lar� bir diziye atal�m
                kullaniciDizisi = kullanicilar.ToArray();
            }
            //T�m kullan�c�lara istenilen mesaj� g�nderelim
            foreach (Kullanici kul in kullaniciDizisi)
            {
                kul.Istemci.MesajYolla(mesaj);
            }
        }

        // D��ER YARARLI FONKSYONLAR //////////////////////////////////////////

        private NameValueCollection mesajCoz(string mesaj)
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

        // ALT SINIFLAR ///////////////////////////////////////////////////////

        private class Kullanici
        {
            /// <summary>
            /// ASMES k�t�phanesindeki Istemci nesnesine referans
            /// </summary>
            public IIstemci Istemci
            {
                get { return istemci; }
                set { istemci = value; }
            }
            private IIstemci istemci;

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
            /// Yeni bir Kullan�c� nesnesi olu�turur.
            /// </summary>
            /// <param name="istemci">ASMES k�t�phanesindeki Istemci nesnesine referans</param>
            /// <param name="nick">Kullan�c�n�n Nick'i</param>
            public Kullanici(IIstemci istemci, string nick)
            {
                this.istemci = istemci;
                this.nick = nick;
            }
        }
    }
}