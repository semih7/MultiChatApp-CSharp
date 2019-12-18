using System;
using System.Collections.Generic;
using System.Text;

namespace ASMES.SunucuTarafi
{
    
    // Sunucuya baðlý olan bir istemciyi temsil eder
    
    public interface IIstemci
    {
        
        // Ýstemciyi temsil eden tekil ID deðeri
        
        long IstemciID { get; }

        
        // Ýstemci ile baðlantýnýn doðru þekilde kurulu olup olmadýðýný verir. True ise mesaj yollanýp alýnabilir.
        
        bool BaglantiVar { get; }
        
        
        // Ýstemciye bir mesaj yollamak içindir
        
        /// <param name="mesaj">Yollanacak mesaj</param>
        // <returns>Ýþlemin baþarý durumu</returns>
        bool MesajYolla(string mesaj);

        
        // Ýstemci ile olan baðlantýyý kapatýr
        
        void BaglantiyiKapat();
    }
}
