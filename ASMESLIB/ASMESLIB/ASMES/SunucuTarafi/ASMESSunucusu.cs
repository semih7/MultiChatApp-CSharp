using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.IO;

namespace ASMES.SunucuTarafi
{
    
    // Asenkron Soketli Mesajlaþma Sunucusu
    
    public class ASMESSunucusu
    {
        // PUBLIC ÖZELLÝKLER 

        
        // Dinlenen Port
        
        public int Port
        {
            get { return port; }
        }
        private int port;

        // OLAYLAR 

        
        // Sunucuya yeni bir istemci baðlantýðýnda tetiklenir.
        
        public event dgYeniIstemciBaglandi YeniIstemciBaglandi;
        
        // Sunucuya baðlý bir istemci baðlantýsý kapatýldýðýnda tetiklenir.
        
        public event dgIstemciBaglantisiKapatildi IstemciBaglantisiKapatildi;
        
        // Bir istemciden yeni bir mesaj alýndýðýnda tetiklenir.
        
        public event dgIstemcidenYeniMesajAlindi IstemcidenYeniMesajAlindi;

        // PRIVATE ALANLAR 

        
        // En son baðlantý saðlanan istemci ID'sini çektim.
        
        private long sonIstemciID = 0;
        
        // O anda baðlantý kurulmuþ olan istemcilerin listesini oluþturdum.
        
        private SortedList<long, Istemci> istemciler;
        
        // Sunucunun çalýþma durumunu kontrol ettim.
        
        private volatile bool calisiyor;
        
        // Senkronizasyonda kullanýlacak nesne
        
        private object objSenk = new object();
        
        // Baðlantý dinleyen nesne
        
        private BaglantiDinleyici baglantiDinleyici;

        // PUBLIC FONKSYONLAR 
        
        
        // Bir ASMES sunucusu oluþturdum
        
        /// <param name="port">Dinlenecek port</param>
        public ASMESSunucusu(int port)
        {
            this.port = port;
            this.istemciler = new SortedList<long, Istemci>();
            this.baglantiDinleyici = new BaglantiDinleyici(this, port);
        }

        
        // Sunucunun çalýþmasýný baþlatýr
        
        public void Baslat()
        {
            //Eðer zaten çalýþýyorsa iþlem yapmadan çýk
            if (calisiyor)
            {
                return;
            }
            //Dinleyiciyi baþlat
            if (!baglantiDinleyici.Baslat())
            {
                return;
            }
            //Çalýþýyor bayraðýný iþaretle
            calisiyor = true;
        }

        
        // Sunucunun çalýþmasýný durdurur
        
        public void Durdur()
        {
            //Dinleyiciyi durdur
            baglantiDinleyici.Durdur();
            //Tüm istemcileri durdur
            calisiyor = false;
            try
            {
                IList<Istemci> istemciListesi = istemciler.Values;
                foreach (Istemci ist in istemciListesi)
                {
                    ist.Durdur();
                }
            }
            catch (Exception)
            {

            }
            //Ýstemcileri temizle
            istemciler.Clear();
            //Çalýþýyor bayraðýndaki iþareti kaldýr
            calisiyor = false;
        }

        
        // Bir istemciye bir mesaj yollar
        
        /// <param name="mesaj">Yollanacak mesaj</param>
        // <returns>Ýþlemin baþarý durumu</returns>
        public bool MesajYolla(IIstemci istemci, string mesaj)
        {
            return istemci.MesajYolla(mesaj);
        }

        // PRIVATE FONKSYONLAR 

        
        // Yeni bir istemci baðlandýðýnda buraya gönderilir.
        
        /// <param name="istemciSoketi">Yeni baðlanan istemci soketi</param>
        private void yeniIstemciSoketiBaglandi(Socket istemciSoketi)
        {
            //Yeni baðlanan istemciyi listeye ekle
            Istemci istemci = null;
            lock (objSenk)
            {
                istemci = new Istemci(this, istemciSoketi, ++sonIstemciID);
                istemciler.Add(istemci.IstemciID, istemci);
            }
            //Ýstemciyi çalýþmaya baþlat
            istemci.Baslat();
            //YeniIstemciBaglandi olayýný tetikle
            if (YeniIstemciBaglandi != null)
            {
                YeniIstemciBaglandi(new IstemciBaglantiArgumanlari(istemci));
            }
        }

        
        // Bir Istemci nesnesi bir mesaj aldýðýnda buraya iletir
        
        /// <param name="istemci">Paketi alan Istemci nesnesi</param>
        /// <param name="mesaj">Mesaj nesnesi</param>
        private void yeniIstemciMesajiAlindi(Istemci istemci, string mesaj)
        {
            if (IstemcidenYeniMesajAlindi != null)
            {
                IstemcidenYeniMesajAlindi(new IstemcidenMesajAlmaArgumanlari(istemci, mesaj));
            }
        }

        
        //Bir Istemci nesnesiyle iliþkili baðlantý kapatýldýðýnda, burasý çaðýrýlýr
        
        /// <param name="istemci">Kapatýlan istemci baðlantýsý</param>
        private void istemciBaglantisiKapatildi(Istemci istemci)
        {
            //IstemciBaglantisiKapatildi olayýný tetikle
            if (IstemciBaglantisiKapatildi != null)
            {
                IstemciBaglantisiKapatildi(new IstemciBaglantiArgumanlari(istemci));
            }
            //Kapanan istemciyi listeden çýkar
            if (calisiyor)
            {
                lock (objSenk)
                {
                    if (istemciler.ContainsKey(istemci.IstemciID))
                    {
                        istemciler.Remove(istemci.IstemciID);
                    }
                }
            }
        }

        // ALT SINIFLAR 
        
        
        // Ayrý bir thread olarak çalýþýp gelen soket baðlantýlarýný kabul ederek 
        // ASMESSunucusu modülüne ileten sýnýf.
        
        private class BaglantiDinleyici
        {
            // Gelen baðlantýlarýn aktarýlacaðý modül 
            private ASMESSunucusu sunucu;
            // Gelen baðlantýlarý dinlemek için sunucu soketi 
            private TcpListener dinleyiciSoket;
            //Dinlenen portun numarasý 
            private int port;
            // thread'in çalýþmasýný kontrol eden bayrak 
            private volatile bool calisiyor = false;
            // çalýþan thread'e referans 
            private volatile Thread thread;

            
            // Kurucu fonksyon.
            
            /// <param name="port">Dinlenecek port no</param>
            public BaglantiDinleyici(ASMESSunucusu sunucu, int port)
            {
                this.sunucu = sunucu;
                this.port = port;
            }

            
            // Dinlemeyi baþlatýr
            
            // <returns>iþlemin baþarý durumu</returns>
            public bool Baslat()
            {
                if (baglan())
                {
                    calisiyor = true;
                    thread = new Thread(new ThreadStart(tDinle));
                    thread.Start();
                    return true;
                }
                else
                {
                    return false;
                }
            }

            
            // dinlemeyi durdurur
            
            // <returns>iþlemin baþarý durumu</returns>
            public bool Durdur()
            {
                try
                {
                    calisiyor = false;
                    baglantiyiKes();
                    thread.Join();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

           
            // Port dinleme iþlemini baþlatýr ve baðlantýyý açýk hale getirir
            
            // <returns>Ýþlemin baþarý durumu</returns>
            private bool baglan()
            {
                try
                {
                    dinleyiciSoket = new TcpListener(System.Net.IPAddress.Any, port);
                    dinleyiciSoket.Start();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            
            // Port dinleme iþlemini kapatýr
            
            // <returns>Ýþlemin baþarý durumu</returns>
            private bool baglantiyiKes()
            {
                try
                {
                    dinleyiciSoket.Stop();
                    dinleyiciSoket = null;
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            
            //Ayrý bir thread olarak çalýþýp gelen soket baðlantýlarýný kabul ederek 
            // ASMESSunucusu modülüne ileten fonksyon.
            
            public void tDinle()
            {
                Socket istemciSoketi;
                while (calisiyor)
                {
                    try
                    {
                        istemciSoketi = dinleyiciSoket.AcceptSocket();
                        if (istemciSoketi.Connected)
                        {
                            try { sunucu.yeniIstemciSoketiBaglandi(istemciSoketi); }
                            catch (Exception) { }
                        }
                    }
                    catch (Exception)
                    {
                        if (calisiyor)
                        {
                            //baðlantýyý sýfýrla
                            baglantiyiKes();
                            //1 saniye bekle
                            try { Thread.Sleep(1000); }
                            catch (Exception) { }
                            //yeniden baðlantý kur
                            baglan();
                        }
                    }
                }
            }
        }

        

        
        // Sunucuya baðlantý kuran bir istemciyi temsil eder.
        
        private class Istemci : IIstemci
        {
            // Sabitler 

            private const byte BASLANGIC_BYTE = (byte)60;
            private const byte BITIS_BYTE = (byte)62;

            // Public Özellikler 

            
            // Ýstemciyi temsil eden tekil ID deðeri
            
            public long IstemciID
            {
                get { return istemciID; }
            }

            
            // Ýstemci ile baðlantýnýn doðru þekilde kurulu olup olmadýðýný verir. True ise mesaj yollanýp alýnabilir.
            
            public bool BaglantiVar
            {
                get { return calisiyor; }
            }

            // OLAYLAR

            
            // Sunucu ile olan baðlantý kapandýðýnda tetiklenir
            
            public event dgBaglantiKapatildi BaglantiKapatildi;
            
            // Sunucudan yeni bir mesaj alýndýðýnda tetiklenir
            
            public event dgYeniMesajAlindi YeniMesajAlindi;

            // Private Alanlar 

            
            // Ýstemci ile iletiþimde kullanýlan soket baðlantýsý
            
            private Socket soket;
            
            //Sunucuya referans
            
            private ASMESSunucusu sunucu;
            
            // Ýstemciyi temsil eden tekil ID deðeri
            
            private long istemciID;
            
            // Veri transfer etmek için kullanýlan akýþ nesnesi
           
            private NetworkStream agAkisi;
            
            // Veri transfer etmek için kullanýlan akýþ nesnesi
            
            private BinaryReader binaryOkuyucu;
            
            // Veri transfer etmek için kullanýlan akýþ nesnesi
            
            private BinaryWriter binaryYazici;
            
            // Ýstemci ile iletiþim devam ediyorsa true, aksi halde false
            
            private volatile bool calisiyor = false;
            
            // Çalýþan thread'e referans
            
            private Thread thread;

            // Public Fonksyonlar 

            
            // Bir istemci nesnesi oluþturur
            
            /// <param name="sunucu">Sunucuya referans</param>
            /// <param name="istemciSoketi">Ýstemci ile iletiþimde kullanýlan soket baðlantýsý</param>
            /// <param name="istemciID">Ýstemciyi temsil eden tekil ID deðeri</param>
            public Istemci(ASMESSunucusu sunucu, Socket istemciSoketi, long istemciID)
            {
                this.sunucu = sunucu;
                this.soket = istemciSoketi;
                this.istemciID = istemciID;
            }

            
            // Ýstemci ile mesaj alýþveriþini baþlatýr
            
            // <returns></returns>
            public bool Baslat()
            {
                try
                {
                    agAkisi = new NetworkStream(soket);
                    binaryOkuyucu = new BinaryReader(agAkisi, Encoding.ASCII);
                    binaryYazici = new BinaryWriter(agAkisi, Encoding.ASCII);
                    thread = new Thread(new ThreadStart(tCalis));
                    calisiyor = true;
                    thread.Start();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            
            // Ýstemci ile mesaj alýþveriþini durdurur
            
            public void Durdur()
            {
                try
                {
                    calisiyor = false;
                    soket.Close();
                    thread.Abort();
                    thread.Join();
                }
                catch (Exception)
                {

                }
            }

            
            // Ýstemci ile olan baðlantýyý keser
            
            public void BaglantiyiKapat()
            {
                this.Durdur();
            }

            
            // Ýstemciye bir mesaj yollamak içindir
            
            /// <param name="mesaj">Yollanacak mesaj</param>
            // <returns>Ýþlemin baþarý durumu</returns>
            public bool MesajYolla(string mesaj)
            {
                try
                {
                    byte[] bMesaj = System.Text.Encoding.ASCII.GetBytes(mesaj);
                    byte[] b = new byte[bMesaj.Length + 2];
                    Array.Copy(bMesaj, 0, b, 1, bMesaj.Length);
                    b[0] = BASLANGIC_BYTE;
                    b[b.Length - 1] = BITIS_BYTE;
                    binaryYazici.Write(b);
                    agAkisi.Flush();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            // PRIVATE FONKSYONLAR 

            
            // Ýstemciden mesajlarý dinleyen fonksyon
            
            private void tCalis()
            {
                //Her döngüde bir mesaj okunup sunucuya iletilir
                while (calisiyor)
                {
                    try
                    {
                        //Baþlangýç Byte'ýný oku
                        byte b = binaryOkuyucu.ReadByte();
                        if (b != BASLANGIC_BYTE)
                        {
                            //Baþlangýç byte'ý deðil, bu karakteri atla!
                            continue;
                        }
                        //Mesajý oku
                        List<byte> bList = new List<byte>();
                        while ((b = binaryOkuyucu.ReadByte()) != BITIS_BYTE)
                        {
                            bList.Add(b);
                        }
                        string mesaj = System.Text.Encoding.ASCII.GetString(bList.ToArray());
                        //Okunan paketi sunucuya ilet
                        sunucu.yeniIstemciMesajiAlindi(this, mesaj);
                        yeniMesajAlindiTetikle(mesaj);
                    }
                    catch (Exception)
                    {
                        //Hata oluþtu, baðlantýyý kes!
                        break;
                    }
                }
                //Döngüden çýkýldýðýna göre istemciyle baðlantý kapatýlmýþ ya da bir hata oluþmuþ demektir
                calisiyor = false;
                try
                {
                    if (soket.Connected)
                    {
                        soket.Close();
                    }
                }
                catch (Exception)
                {

                }
                sunucu.istemciBaglantisiKapatildi(this);
                baglantiKapatildiTetikle();
            }

            // Olaylarý tetikleyen iç fonksyonlar 

            private void baglantiKapatildiTetikle()
            {
                if (BaglantiKapatildi != null)
                {
                    BaglantiKapatildi();
                }
            }

            private void yeniMesajAlindiTetikle(string mesaj)
            {
                if (YeniMesajAlindi != null)
                {
                    YeniMesajAlindi(new MesajAlmaArgumanlari(mesaj));
                }
            }
        }
    }
}
