using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace ASMES.IstemciTarafi       //Asenkron soket message library (ASMES)
{
    
    // Asenkron Soketli Mesajla�ma �stemcisi tan�mlad�m.
    
    public class ASMESIstemcisi
    {
        // SAB�TLER

        private const byte BASLANGIC_BYTE = (byte)60; //ba�lang�� byte atamas�n� yapt�m.
        private const byte BITIS_BYTE = (byte)62; //biti� byte atamas�n� yapt�m.

        // PUBLIC �ZELL�KLER 

        
        // ASMES Sunucusunun IP adresini ayarlad�m.
        
        public string ServerIpAdresi
        {
            get { return serverIpAdresi; }      //Server ip adresini d�nd�rd�m.
        }
        private string serverIpAdresi;
        
        // ASMES sunucusunun port numaras�n� ayarlad�m.
        
        public int ServerPort
        {
            get { return serverPort; }      //Server Port numaras�n� d�nd�rd�m.
            set { serverPort = value; }     //Server port numaras�n� bir de�er olarak setledim.
        }
        private int serverPort;

        // OLAYLAR 

        
        // Sunucu ile olan ba�lant� kapand���nda �al���yor.
        
        public event dgBaglantiKapatildi BaglantiKapatildi;     
        
        // Sunucudan yeni bir mesaj al�nd���nda �al���yor.
        
        public event dgYeniMesajAlindi YeniMesajAlindi;

        // PRIVATE ALANLAR 

        
        // Sunucuya ba�lant�y� sa�layan soket nesnesini ayarlad�m.
        
        private Socket istemciBaglantisi;
        
        // Sunucuyla ileti�imi sa�layan temel ak�� nesnesini ayarlad�m.
        
        private NetworkStream agAkisi;
        
        // Yaz�lan veriyi transfer etmek i�in kullan�lan ak�� nesnesini ayarlad�m.
        
        private BinaryWriter binaryYazici;
        
        //Al�nan veriyi transfer etmek i�in kullan�lan ak�� nesnesini ayarlad�m.
        
        private BinaryReader binaryOkuyucu;
        
        // �al��an thread'e referans verdim.
        
        private Thread thread;
        
        // �stemci ile ileti�im devam ediyorsa true, etmiyorsa false
        
        private volatile bool calisiyor = false;

        // PUBLIC FONKSYONLAR

        
        // Bir ASMES �stemcisi olu�turdum.
        
        /// <param name="serverIpAdresi">ASMES Sunucusunun IP adresi</param>
        /// <param name="serverPort">ASMES sunucusunun port numaras�</param>
        public ASMESIstemcisi(string serverIpAdresi, int serverPort)
        {
            this.serverIpAdresi = serverIpAdresi;   //�stemci ip adresini sunucudan �ektim.
            this.serverPort = serverPort;           //�stemci port numaras�n� sunucudan �ektim.
        }

        
        // Sunucuyla ba�lant� kurdum.     
        //ba�lant� sa�land�ysa true, aksi halde false
        public bool Baglan()
        {
            try
            {
                //sunucuya ba�lanma kodlar�
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

        
        // Sunucuyla olan ba�lant�y� kapat�r.
        
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

        
        // Sunucuya bir mesaj yollamak i�in fonksiyon.
        
        /// <param name="mesaj">Yollanacak mesaj</param>
        // ��lemin ba�ar� durumu
        public bool MesajYolla(string mesaj)
        {
            try
            {
                //Mesaj� byte dizisine �evirdim.
                byte[] bMesaj = System.Text.Encoding.ASCII.GetBytes(mesaj);
                //Kar�� tarafa g�nderilecek byte dizisini olu�turdum.
                byte[] b = new byte[bMesaj.Length + 2];
                Array.Copy(bMesaj, 0, b, 1, bMesaj.Length);
                b[0] = BASLANGIC_BYTE;
                b[b.Length - 1] = BITIS_BYTE;
                //Mesaj� sokete yazd�m.
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

        
        // Sunucudan mesajlar� dinleyen fonksiyon
        
        private void tCalis()
        {
            //Her d�ng�de bir mesaj okundu.
            while (calisiyor)
            {
                try
                {
                    //Ba�lang�� Byte'�n� oku
                    byte b = binaryOkuyucu.ReadByte();
                    if (b != BASLANGIC_BYTE)
                    {
                        //Hatal� paket, ba�lant�y� kes!
                        break;
                    }
                    //Mesaj� oku
                    List<byte> bList = new List<byte>();
                    while ((b = binaryOkuyucu.ReadByte()) != BITIS_BYTE)
                    {
                        bList.Add(b);
                    }
                    string mesaj = System.Text.Encoding.ASCII.GetString(bList.ToArray());
                    //Yeni mesaj ba�ar�yla al�nd�
                    yeniMesajAlindiTetikle(mesaj);
                }
                catch (Exception)
                {
                    //Hata olu�tu, ba�lant�y� kes!
                    break;
                }
            }
            //D�ng�den ��k�ld���na g�re ba�lant� kapat�lm�� demektir
            calisiyor = false;
            baglantiKapatildiTetikle();
        }

        private void baglantiKapatildiTetikle()
        {
            if (BaglantiKapatildi != null)
            {
                BaglantiKapatildi();        //Ba�lant�y� kapatt�m.
            }
        }

        private void yeniMesajAlindiTetikle(string mesaj)
        {
            if (YeniMesajAlindi != null)
            {
                YeniMesajAlindi(new MesajAlmaArgumanlari(mesaj));       //Yeni mesaj geldi�inde �al��an fonksiyon.
            }
        }
    }
}