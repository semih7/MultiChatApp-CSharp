using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace ASMES.IstemciTarafi       //Asenkron soket message library (ASMES)
{
    
    // Asenkron Soketli Mesajlaþma Ýstemcisi tanýmladým.
    
    public class ASMESIstemcisi
    {
        // SABÝTLER

        private const byte BASLANGIC_BYTE = (byte)60; //baþlangýç byte atamasýný yaptým.
        private const byte BITIS_BYTE = (byte)62; //bitiþ byte atamasýný yaptým.

        // PUBLIC ÖZELLÝKLER 

        
        // ASMES Sunucusunun IP adresini ayarladým.
        
        public string ServerIpAdresi
        {
            get { return serverIpAdresi; }      //Server ip adresini döndürdüm.
        }
        private string serverIpAdresi;
        
        // ASMES sunucusunun port numarasýný ayarladým.
        
        public int ServerPort
        {
            get { return serverPort; }      //Server Port numarasýný döndürdüm.
            set { serverPort = value; }     //Server port numarasýný bir deðer olarak setledim.
        }
        private int serverPort;

        // OLAYLAR 

        
        // Sunucu ile olan baðlantý kapandýðýnda çalýþýyor.
        
        public event dgBaglantiKapatildi BaglantiKapatildi;     
        
        // Sunucudan yeni bir mesaj alýndýðýnda çalýþýyor.
        
        public event dgYeniMesajAlindi YeniMesajAlindi;

        // PRIVATE ALANLAR 

        
        // Sunucuya baðlantýyý saðlayan soket nesnesini ayarladým.
        
        private Socket istemciBaglantisi;
        
        // Sunucuyla iletiþimi saðlayan temel akýþ nesnesini ayarladým.
        
        private NetworkStream agAkisi;
        
        // Yazýlan veriyi transfer etmek için kullanýlan akýþ nesnesini ayarladým.
        
        private BinaryWriter binaryYazici;
        
        //Alýnan veriyi transfer etmek için kullanýlan akýþ nesnesini ayarladým.
        
        private BinaryReader binaryOkuyucu;
        
        // Çalýþan thread'e referans verdim.
        
        private Thread thread;
        
        // Ýstemci ile iletiþim devam ediyorsa true, etmiyorsa false
        
        private volatile bool calisiyor = false;

        // PUBLIC FONKSYONLAR

        
        // Bir ASMES Ýstemcisi oluþturdum.
        
        /// <param name="serverIpAdresi">ASMES Sunucusunun IP adresi</param>
        /// <param name="serverPort">ASMES sunucusunun port numarasý</param>
        public ASMESIstemcisi(string serverIpAdresi, int serverPort)
        {
            this.serverIpAdresi = serverIpAdresi;   //Ýstemci ip adresini sunucudan çektim.
            this.serverPort = serverPort;           //Ýstemci port numarasýný sunucudan çektim.
        }

        
        // Sunucuyla baðlantý kurdum.     
        //baðlantý saðlandýysa true, aksi halde false
        public bool Baglan()
        {
            try
            {
                //sunucuya baðlanma kodlarý
                istemciBaglantisi = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(serverIpAdresi), serverPort);
                istemciBaglantisi.Connect(ipep);
                agAkisi = new NetworkStream(istemciBaglantisi);
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

        
        // Sunucuyla olan baðlantýyý kapatýr.
        
        public void BaglantiyiKes()
        {
            try
            {
                calisiyor = false;
                istemciBaglantisi.Close();
                thread.Join();
            }
            catch (Exception)
            {

            }
        }

        
        // Sunucuya bir mesaj yollamak için fonksiyon.
        
        /// <param name="mesaj">Yollanacak mesaj</param>
        // Ýþlemin baþarý durumu
        public bool MesajYolla(string mesaj)
        {
            try
            {
                //Mesajý byte dizisine çevirdim.
                byte[] bMesaj = System.Text.Encoding.ASCII.GetBytes(mesaj);
                //Karþý tarafa gönderilecek byte dizisini oluþturdum.
                byte[] b = new byte[bMesaj.Length + 2];
                Array.Copy(bMesaj, 0, b, 1, bMesaj.Length);
                b[0] = BASLANGIC_BYTE;
                b[b.Length - 1] = BITIS_BYTE;
                //Mesajý sokete yazdým.
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

        
        // Sunucudan mesajlarý dinleyen fonksiyon
        
        private void tCalis()
        {
            //Her döngüde bir mesaj okundu.
            while (calisiyor)
            {
                try
                {
                    //Baþlangýç Byte'ýný oku
                    byte b = binaryOkuyucu.ReadByte();
                    if (b != BASLANGIC_BYTE)
                    {
                        //Hatalý paket, baðlantýyý kes!
                        break;
                    }
                    //Mesajý oku
                    List<byte> bList = new List<byte>();
                    while ((b = binaryOkuyucu.ReadByte()) != BITIS_BYTE)
                    {
                        bList.Add(b);
                    }
                    string mesaj = System.Text.Encoding.ASCII.GetString(bList.ToArray());
                    //Yeni mesaj baþarýyla alýndý
                    yeniMesajAlindiTetikle(mesaj);
                }
                catch (Exception)
                {
                    //Hata oluþtu, baðlantýyý kes!
                    break;
                }
            }
            //Döngüden çýkýldýðýna göre baðlantý kapatýlmýþ demektir
            calisiyor = false;
            baglantiKapatildiTetikle();
        }

        private void baglantiKapatildiTetikle()
        {
            if (BaglantiKapatildi != null)
            {
                BaglantiKapatildi();        //Baðlantýyý kapattým.
            }
        }

        private void yeniMesajAlindiTetikle(string mesaj)
        {
            if (YeniMesajAlindi != null)
            {
                YeniMesajAlindi(new MesajAlmaArgumanlari(mesaj));       //Yeni mesaj geldiðinde çalýþan fonksiyon.
            }
        }
    }
}