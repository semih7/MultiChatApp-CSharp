using System;
using System.Collections.Generic;
using System.Text;

namespace ASMES.SunucuTarafi
{
    
    // Sunucuya ba�l� olan bir istemciyi temsil eder
    
    public interface IIstemci
    {
        
        // �stemciyi temsil eden tekil ID de�eri
        
        long IstemciID { get; }

        
        // �stemci ile ba�lant�n�n do�ru �ekilde kurulu olup olmad���n� verir. True ise mesaj yollan�p al�nabilir.
        
        bool BaglantiVar { get; }
        
        
        // �stemciye bir mesaj yollamak i�indir
        
        /// <param name="mesaj">Yollanacak mesaj</param>
        // <returns>��lemin ba�ar� durumu</returns>
        bool MesajYolla(string mesaj);

        
        // �stemci ile olan ba�lant�y� kapat�r
        
        void BaglantiyiKapat();
    }
}
