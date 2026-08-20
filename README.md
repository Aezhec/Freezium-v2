# Freezium

Freezium, anime sitesi için akıllı proxy yönlendirmesi ve özellik yönetim aracıdır. Windows yerleşik PAC (Proxy Auto-Configuration) desteği sayesinde **herhangi bir tarayıcı eklentisine ihtiyaç duymadan, tek tıkla çalışır ve internet hızınızı %100 korur**.

---

## Özellikler

- **Tek Tıkla Çalışma:** Ekstra tarayıcı ayarı veya SwitchyOmega gibi eklenti kurulumu gerektirmez.
- **Sıfır Hız Kaybı:** Yalnızca hedef anime sitesinin trafiği filtrelenir; YouTube, oyunlar, indirmeler ve diğer tüm siteler doğrudan (DIRECT) bağlanır.
- **Otomatik Temizleme:** Uygulama durdurulduğunda veya kapatıldığında sistem ayarları otomatik olarak eski haline getirilir.
- **İzleme Listesi & Premium Entegrasyonu:** İzleme listesi, takip ve favori durumlarını yerel veritabanında yönetir.

---

## Kurulum ve Kullanım

### 1. Projeyi Derleme

```bash
# Visual Studio ile Freezium.sln dosyasını açın
# Çözümü Derleyin (Ctrl + Shift + B)
# Uygulamayı Başlatın (F5)
```

### 2. Kullanım (Tek Tıkla)

1. Freezium uygulamasında **Proxy'yi Başlat** butonuna tıklayın.
2. İlk çalıştırmada SSL sertifika onayı istenirse **Evet/Onayla** deyin.
3. Tarayıcınızdan doğrudan anime sitesine girin.
4. İşiniz bittiğinde **Proxy'yi Durdur** butonuna basmanız yeterlidir.

> [!NOTE]
> Artık tarayıcınıza SwitchyOmega vb. eklentiler kurmanıza ya da Windows Proxy ayarlarına elle port yazmanıza gerek yoktur. Freezium arka planda Windows PAC yapılandırmasını otomatik olarak yönetir.

---

## Sorun Giderme

### Sertifika Uyarısı
- İlk açılışta çıkan kök sertifika yükleme onayına "Evet" verildiğinden emin olun.

### Port Çakışması
- 8888 portunun başka bir hata ayıklama veya proxy aracı tarafından kullanılmadığından emin olun.

---

## Sistem Gereksinimleri

- Windows 10 / 11
- .NET Framework 4.8+
- Visual Studio 2019 / 2022 / 2026
