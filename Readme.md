# DataVision - Enterprise Management, Analytics & AI Auditing Platform

DataVision, işletmelerin büyük ölçekli satış, müşteri, sipariş ve ürün verilerini yönetmesini, gelişmiş analitik panelleriyle görselleştirmesini ve yapay zeka servisleriyle (Google Gemini & Hugging Face Toxic-BERT) entegre bir şekilde denetlemesini sağlayan kurumsal düzeyde bir ASP.NET Core MVC web uygulamasıdır.

## 📊 Veri Seti Kapsamı

Proje altyapısı ve analitik modülleri şu büyüklükteki kurumsal veri seti üzerinde çalışacak şekilde optimize edilmiştir:
- **500.000+** Sipariş (Order)
- **1.500+** Müşteri (Customer)
- **18.000+** Ürün (Product)
- **78** Şehir
- **13** Ülke

---

## 🚀 Temel Özellikler & Modüller

### 1. Kapsamlı Yönetim Modülleri (CRUD)
- **Ürün & Kategori Yönetimi:** Ürün kataloglama, stok takibi, menşei yönetimi ve kategori bazlı filtreleme.
- **Müşteri Yönetimi:** Demografik bilgiler, adres yönetimi ve müşteri detay sayfaları.
- **Sipariş Yönetimi:** Ödeme yöntemleri, sipariş durumları (Tamamlandı, İptal Edildi vb.) ve miktar takipleri.
- **Müşteri Mesajları & AI Toksisite Analizi:** Müşteri iletişim mesajlarının kaydedilmesi ve yapay zeka ile otomatik denetlenmesi.

### 2. Yapay Zeka ve Makine Öğrenmesi Entegrasyonları
- **Google Gemini API Entegrasyonu (`CustomerAiService`):** Müşteri sipariş geçmişi ve harcama alışkanlıkları analiz edilerek otomatik kurumsal HTML tabanlı yapay zeka analiz raporları üretilir.
- **Hugging Face Toxic-BERT Entegrasyonu (`MessageAiService`):** Müşteri mesajları arka planda İngilizceye çevrilerek Toxic-BERT modeli üzerinden taranır; *Toksik, Hakaret, Tehdit* veya *Temiz / Normal* olarak otomatik etiketlenir.
- **ML.NET RFM Segmentasyonu (`LoyaltyModule`):** Müşterilerin sadakat analizleri ve RFM skorlamaları ML.NET SDCA regresyon pipeline'ları ile gerçekleştirilir.

### 3. Gelişmiş Analitik ve Raporlama
- **Dashboard & İstatistikler:** Anlık KPI metrikleri, grafiksel göstergeler ve özet raporlar.
- **Talep ve Satış Tahminleme (Forecasting):** Gelecek dönem satış ve stok trend tahminlemeleri.

---

## 🛠️ Mimari ve Teknolojiler

Bu proje, modern .NET ekosistemi standartlarına ve kurumsal yazılım mimarisi prensiplerine uygun olarak geliştirilmiştir:

- **Backend:** .NET 8.0, ASP.NET Core MVC, C#
- **ORM / Veritabanı:** Entity Framework Core (EF Core), MSSQL / PostgreSQL, LINQ optimizasyonları ve `.AsNoTracking()` performans iyileştirmeleri.
- **Yapay Zeka & ML:** Google Gemini API, Hugging Face Inference API (Toxic-BERT), ML.NET.
- **Frontend & UI:** Bootstrap 5, Material Design Icons (`zmdi`), Custom Modal Pop-up mimarisi, AJAX, Chart.js.
- **Mimari Desenler:** Repository Pattern yaklaşımı, Service-Oriented Architecture, ViewComponents (Modüler arayüz bileşenleri), ViewModels.

---

## ⚙️ Kurulum ve Çalıştırma

Projeyi yerel ortamınızda çalıştırmak için aşağıdaki adımları izleyebilirsiniz:

1. **Repoyu Klonlayın:**
   ```bash
   git clone [https://github.com/EnessCode/DataVision.git](https://github.com/EnessCode/DataVision.git)
   ```
2. **Veritabanı Bağlantısını Ayarlayın:**
appsettings.json dosyası içerisindeki ConnectionStrings alanını kendi veritabanı sunucunuza göre güncelleyin.

3. **API Anahtarlarını Tanımlayın:**
appsettings.json dosyasına Google Gemini ve Hugging Face API anahtarlarınızı ekleyin:
```bash
"HuggingFace": {
  "ApiKey": "hf_SizinTokeniniz"
},
"GoogleGemini": {
  "ApiKey": "SizinGeminiKeyiniz"
}
```

4. **Migration İşlemleri:**
Visual Studio Paket Yöneticisi Konsolu (Package Manager Console) üzerinden veritabanını oluşturun:
```bash
Add-Migration InitialCreate
Update-Database
```

5. **Projeyi Çalıştırın:**
Visual Studio üzerinden F5 tuşuna basarak veya terminalden dotnet run komutu ile projeyi başlatın.

---

## 📄 Lisans
Bu proje MIT lisansı altında lisanslanmıştır.