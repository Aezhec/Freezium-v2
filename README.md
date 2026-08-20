# ❄️ Freezium v2

![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-blue?style=flat-square&logo=windows)
![Framework](https://img.shields.io/badge/.NET%20Framework-v4.8-purple?style=flat-square&logo=dotnet)
![Release](https://img.shields.io/badge/Release-v1.0.0-green?style=flat-square)
![License](https://img.shields.io/badge/License-MIT-orange?style=flat-square)

**Freezium**, Windows işletim sistemleri için geliştirilmiş, akıllı ağ yönlendirme teknolojisine (PAC) sahip bir proxy ve izleme listesi yönetim uygulamasıdır.

Genel internet trafiğinize (oyunlar, YouTube, indirmeler) dokunmadan, **yalnızca hedef domain trafiğini** güvenli şekilde işler ve internet hızınızda sıfır kayıp sağlar.

---

## ✨ Öne Çıkan Özellikler

| Özellik | Açıklama |
| :--- | :--- |
| ⚡ **Akıllı PAC Yönlendirmesi** | Ağ trafiğini analiz eder; sadece hedef site isteklerini proxy'ye alır, diğer tüm trafiği doğrudan (`DIRECT`) yönlendirir. |
| 💾 **Yerel Veritabanı Entegrasyonu** | Favoriler, takip listesi ve izleme geçmişini yerel **LiteDB** veritabanında güvenle saklar. |
| 🔄 **Otomatik Ağ Temizliği** | Uygulama durdurulduğunda veya kapatıldığında sistem proxy ayarlarını otomatik olarak ilk haline getirir. |
| 📌 **Sistem Tepsi (Tray) Desteği** | Arka planda sessizce çalışır, sistem tepsisinden tek tıkla kontrol edilebilir. |
| 🎨 **Modern Kullanıcı Arayüzü** | Sade, kullanışlı ve modern WPF arayüzü. |

---

## 📥 İndirme ve Kurulum

### 1. Hazır Kurulum Dosyası İle (Önerilen)

1. [**GitHub Releases**](https://github.com/Aezhec/Freezium-v2/releases/tag/v1.0.0) sayfasından en güncel **`setup.exe`** dosyasını indirin.
2. `setup.exe` dosyasını çalıştırın ve kurulum adımlarını takip edin.
3. Masaüstü veya Başlat menünüzde oluşan **Freezium** kısayolundan uygulamayı başlatın.

> 💡 **Windows SmartScreen Uyarısı Hakkında:**  
> İmzalanmamış kurulum dosyalarında Windows mavi uyarı ekranı gösterebilir. Bu durumda **"Daha fazla bilgi"** yazısına tıklayıp **"Yine de çalıştır"** butonunu seçerek kuruluma devam edebilirsiniz.

---

### 2. Kaynak Koddan Derleme (Geliştiriciler İçin)

Projenin kaynak kodlarını yerelde derlemek isterseniz:

```bash
# Repoyu klonlayın
git clone https://github.com/Aezhec/Freezium-v2.git

# Proje dizinine gidin
cd Freezium-v2
```

- `Freezium.sln` dosyasını **Visual Studio 2019 / 2022 / 2026** ile açın.
- `Ctrl + Shift + B` kısayolu ile projeyi derleyin.
- `F5` tuşu ile uygulamayı başlatın.

---

## 🚀 Kullanım Adımları

1. **Uygulamayı Açın:** Freezium arayüzünde bulunan **"Proxy'yi Başlat"** butonuna tıklayın.
2. **Sertifika Onayı (İlk Çalıştırma):** Yerel HTTPS trafiğinin işlenebilmesi için Windows sertifika onay penceresi çıkarsa **"Evet / Onayla"** seçeneğini seçin.
3. **Kullanım:** Uygulama aktif durumdayken hedef siteye erişebilirsiniz.
4. **Kapatma:** İşiniz bittiğinde **"Proxy'yi Durdur"** butonuna basarak ağ ayarlarını eski haline getirebilirsiniz.

---

## 🛠️ Sistem Gereksinimleri

- **İşletim Sistemi:** Windows 10 (1809+) / Windows 11 (64-bit)
- **Çalışma Zamanı:** [.NET Framework 4.8 Runtime](https://dotnet.microsoft.com/download/dotnet-framework/net48) (Setup dosyası otomatik kontrol eder)

---

## 🛠️ Kullanılan Teknolojiler

- **Dil / Çatı:** C# | .NET Framework 4.8 (WPF)
- **Proxy Motoru:** FiddlerCore
- **Veritabanı:** LiteDB
- **Paketleme & Yayın:** Costura.Fody & ClickOnce Publish

---

## 📜 Lisans

Bu proje **MIT Lisansı** altında lisanslanmıştır. Detaylar için [LICENSE](LICENSE) dosyasına göz atabilirsiniz.
