using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.IO;

namespace ASMES.SunucuTarafi
{
    
    // Asenkron Soketli Mesajla�ma Sunucusu
    
    public class ASMESSunucusu
    {
        // PUBLIC �ZELL�KLER 

        
        // Dinlenen Port
        
        public int Port
        {
            get { return port; }
        }
        private int port;

        // OLAYLAR 

        
        // Sunucuya yeni bir istemci ba�lant���nda tetiklenir.
        
        public event dgYeniIstemciBaglandi YeniIstemciBaglandi;
        
        // Sunucuya ba�l� bir istemci ba�lant�s� kapat�ld���nda tetiklenir.
        
        public event dgIstemciBaglantisiKapatildi IstemciBaglantisiKapatildi;
        
        // Bir istemciden yeni bir mesaj al�nd���nda tetiklenir.
        
        public event dgIstemcidenYeniMesajAlindi IstemcidenYeniMesajAlindi;

        // PRIVATE ALANLAR 

        
        // En son ba�lant� sa�lanan istemci ID'sini �ektim.
        
        private long sonIstemciID = 0;
        
        // O anda ba�lant� kurulmu� olan istemcilerin listesini olu�turdum.
        
        private SortedList<long, Istemci> istemciler;
        
        // Sunucunun �al��ma durumunu kontrol ettim.
        
        private volatile bool calisiyor;
        
        // Senkronizasyonda kullan�lacak nesne
        
        private object objSenk = new object();
        
        // Ba�lant� dinleyen nesne
        
        private BaglantiDinleyici baglantiDinleyici;

        // PUBLIC FONKSYONLAR 
        
        
        // Bir ASMES sunucusu olu�turdum
        
        /// <param name="port">Dinlenecek port</param>
        public ASMESSunucusu(int port)
        {
            this.port = port;
            this.istemciler = new SortedList<long, Istemci>();
            this.baglantiDinleyici = new BaglantiDinleyici(this, port);
        }

        
        // Sunucunun �al��mas�n� ba�lat�r
        
        public void Baslat()
        {
            //E�er zaten �al���yorsa i�lem yapmadan ��k
            if (calisiyor)
            {
                return;
            }
            //Dinleyiciyi ba�lat
            if (!baglantiDinleyici.Baslat())
            {
                return;
            }
            //�al���yor bayra��n� i�aretle
            calisiyor = true;
        }

        
        // Sunucunun �al��mas�n� durdurur
        
        public void Durdur()
        {
            //Dinleyiciyi durdur
            baglantiDinleyici.Durdur();
            //T�m istemcileri durdur
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
            //�stemcileri temizle
            istemciler.Clear();
            //�al���yor bayra��ndaki i�areti kald�r
            calisiyor = false;
        }

        
        // Bir istemciye bir mesaj yollar
        
        /// <param name="mesaj">Yollanacak mesaj</param>
        // <returns>��lemin ba�ar� durumu</returns>
        public bool MesajYolla(IIstemci istemci, string mesaj)
        {
            return istemci.MesajYolla(mesaj);
        }

        // PRIVATE FONKSYONLAR 

        
        // Yeni bir istemci ba�land���nda buraya g�nderilir.
        
        /// <param name="istemciSoketi">Yeni ba�lanan istemci soketi</param>
        private void yeniIstemciSoketiBaglandi(Socket istemciSoketi)
        {
            //Yeni ba�lanan istemciyi listeye ekle
            Istemci istemci = null;
            lock (objSenk)
            {
                istemci = new Istemci(this, istemciSoketi, ++sonIstemciID);
                istemciler.Add(istemci.IstemciID, istemci);
            }
            //�stemciyi �al��maya ba�lat
            istemci.Baslat();
            //YeniIstemciBaglandi olay�n� tetikle
            if (YeniIstemciBaglandi != null)
            {
                YeniIstemciBaglandi(new IstemciBaglantiArgumanlari(istemci));
            }
        }

        
        // Bir Istemci nesnesi bir mesaj ald���nda buraya iletir
        
        /// <param name="istemci">Paketi alan Istemci nesnesi</param>
        /// <param name="mesaj">Mesaj nesnesi</param>
        private void yeniIstemciMesajiAlindi(Istemci istemci, string mesaj)
        {
            if (IstemcidenYeniMesajAlindi != null)
            {
                IstemcidenYeniMesajAlindi(new IstemcidenMesajAlmaArgumanlari(istemci, mesaj));
            }
        }

        
        //Bir Istemci nesnesiyle ili�kili ba�lant� kapat�ld���nda, buras� �a��r�l�r
        
        /// <param name="istemci">Kapat�lan istemci ba�lant�s�</param>
        private void istemciBaglantisiKapatildi(Istemci istemci)
        {
            //IstemciBaglantisiKapatildi olay�n� tetikle
            if (IstemciBaglantisiKapatildi != null)
            {
                IstemciBaglantisiKapatildi(new IstemciBaglantiArgumanlari(istemci));
            }
            //Kapanan istemciyi listeden ��kar
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
        
        
        // Ayr� bir thread olarak �al���p gelen soket ba�lant�lar�n� kabul ederek 
        // ASMESSunucusu mod�l�ne ileten s�n�f.
        
        private class BaglantiDinleyici
        {
            // Gelen ba�lant�lar�n aktar�laca�� mod�l 
            private ASMESSunucusu sunucu;
            // Gelen ba�lant�lar� dinlemek i�in sunucu soketi 
            private TcpListener dinleyiciSoket;
            //Dinlenen portun numaras� 
            private int port;
            // thread'in �al��mas�n� kontrol eden bayrak 
            private volatile bool calisiyor = false;
            // �al��an thread'e referans 
            private volatile Thread thread;

            
            // Kurucu fonksyon.
            
            /// <param name="port">Dinlenecek port no</param>
            public BaglantiDinleyici(ASMESSunucusu sunucu, int port)
            {
                this.sunucu = sunucu;
                this.port = port;
            }

            
            // Dinlemeyi ba�lat�r
            
            // <returns>i�lemin ba�ar� durumu</returns>
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
            
            // <returns>i�lemin ba�ar� durumu</returns>
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

           
            // Port dinleme i�lemini ba�lat�r ve ba�lant�y� a��k hale getirir
            
            // <returns>��lemin ba�ar� durumu</returns>
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

            
            // Port dinleme i�lemini kapat�r
            
            // <returns>��lemin ba�ar� durumu</returns>
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

            
            //Ayr� bir thread olarak �al���p gelen soket ba�lant�lar�n� kabul ederek 
            // ASMESSunucusu mod�l�ne ileten fonksyon.
            
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
                            //ba�lant�y� s�f�rla
                            baglantiyiKes();
                            //1 saniye bekle
                            try { Thread.Sleep(1000); }
                            catch (Exception) { }
                            //yeniden ba�lant� kur
                            baglan();
                        }
                    }
                }
            }
        }

        

        
        // Sunucuya ba�lant� kuran bir istemciyi temsil eder.
        
        private class Istemci : IIstemci
        {
            // Sabitler 

            private const byte BASLANGIC_BYTE = (byte)60;
            private const byte BITIS_BYTE = (byte)62;

            // Public �zellikler 

            
            // �stemciyi temsil eden tekil ID de�eri
            
            public long IstemciID
            {
                get { return istemciID; }
            }

            
            // �stemci ile ba�lant�n�n do�ru �ekilde kurulu olup olmad���n� verir. True ise mesaj yollan�p al�nabilir.
            
            public bool BaglantiVar
            {
                get { return calisiyor; }
            }

            // OLAYLAR

            
            // Sunucu ile olan ba�lant� kapand���nda tetiklenir
            
            public event dgBaglantiKapatildi BaglantiKapatildi;
            
            // Sunucudan yeni bir mesaj al�nd���nda tetiklenir
            
            public event dgYeniMesajAlindi YeniMesajAlindi;

            // Private Alanlar 

            
            // �stemci ile ileti�imde kullan�lan soket ba�lant�s�
            
            private Socket soket;
            
            //Sunucuya referans
            
            private ASMESSunucusu sunucu;
            
            // �stemciyi temsil eden tekil ID de�eri
            
            private long istemciID;
            
            // Veri transfer etmek i�in kullan�lan ak�� nesnesi
           
            private NetworkStream agAkisi;
            
            // Veri transfer etmek i�in kullan�lan ak�� nesnesi
            
            private BinaryReader binaryOkuyucu;
            
            // Veri transfer etmek i�in kullan�lan ak�� nesnesi
            
            private BinaryWriter binaryYazici;
            
            // �stemci ile ileti�im devam ediyorsa true, aksi halde false
            
            private volatile bool calisiyor = false;
            
            // �al��an thread'e referans
            
            private Thread thread;

            // Public Fonksyonlar 

            
            // Bir istemci nesnesi olu�turur
            
            /// <param name="sunucu">Sunucuya referans</param>
            /// <param name="istemciSoketi">�stemci ile ileti�imde kullan�lan soket ba�lant�s�</param>
            /// <param name="istemciID">�stemciyi temsil eden tekil ID de�eri</param>
            public Istemci(ASMESSunucusu sunucu, Socket istemciSoketi, long istemciID)
            {
                this.sunucu = sunucu;
                this.soket = istemciSoketi;
                this.istemciID = istemciID;
            }

            
            // �stemci ile mesaj al��veri�ini ba�lat�r
            
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

            
            // �stemci ile mesaj al��veri�ini durdurur
            
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

            
            // �stemci ile olan ba�lant�y� keser
            
            public void BaglantiyiKapat()
            {
                this.Durdur();
            }

            
            // �stemciye bir mesaj yollamak i�indir
            
            /// <param name="mesaj">Yollanacak mesaj</param>
            // <returns>��lemin ba�ar� durumu</returns>
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

            
            // �stemciden mesajlar� dinleyen fonksyon
            
            private void tCalis()
            {
                //Her d�ng�de bir mesaj okunup sunucuya iletilir
                while (calisiyor)
                {
                    try
                    {
                        //Ba�lang�� Byte'�n� oku
                        byte b = binaryOkuyucu.ReadByte();
                        if (b != BASLANGIC_BYTE)
                        {
                            //Ba�lang�� byte'� de�il, bu karakteri atla!
                            continue;
                        }
                        //Mesaj� oku
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
                        //Hata olu�tu, ba�lant�y� kes!
                        break;
                    }
                }
                //D�ng�den ��k�ld���na g�re istemciyle ba�lant� kapat�lm�� ya da bir hata olu�mu� demektir
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

            // Olaylar� tetikleyen i� fonksyonlar 

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
