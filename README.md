# Freezium

Freezium, Windows için geliştirilmiş akıllı proxy yönlendirmesi ve izleme listesi yönetim aracıdır. Yerel PAC (Proxy Auto-Configuration) servisi sayesinde yalnızca hedef site trafiğini işler ve genel internet hızınızı etkilemeden çalışır.

---

## Özellikler

- **Akıllı Yönlendirme:** Yalnızca hedef domain trafiğini proxy üzerinden geçirir; diğer tüm ağ bağlantıları doğrudan (DIRECT) sağlanır.
- **İzleme Listesi Yönetimi:** İzleme listesi, takip ve favori durumlarını yerel LiteDB veritabanında saklar ve senkronize eder.
- **Otomatik Oturum Yönetimi:** Uygulama başlatıldığında ağ ayarlarını yapılandırır, kapatıldığında sistem ayarlarını otomatik olarak temizler.
- **Sistem Tepsi (Tray) Entegrasyonu:** Arka planda sessizce çalışabilir ve sistem tepsisinden hızlıca yönetilebilir.

---

## Kurulum ve Çalıştırma

### Kurulum Dosyası İle (Önerilen)

1. [Releases](https://github.com/Aezhec/Freezium-v2/releases/tag/v1.0.0) sayfasından `setup.exe` dosyasını indirin.
2. `setup.exe` dosyasını çalıştırarak kurulumu tamamlayın.
3. Masaüstü veya Başlat menüsündeki **Freezium** kısayolundan uygulamayı başlatın.

> **Not (Windows SmartScreen):** İmzalanmamış kurulum dosyalarında Windows mavi uyarı ekranı gösterebilir. Bu durumda *"Daha fazla bilgi -> Yine de çalıştır"* seçeneği ile devam edebilirsiniz.

### Kaynak Koddan Derleme

```bash
# Visual Studio ile Freezium.sln dosyasını açın
# Çözümü derleyin (Ctrl + Shift + B)
# Uygulamayı çalıştırın (F5)
```

---

## Kullanım

1. **Proxy'yi Başlat** butonuna tıklayın.
2. İlk çalıştırmada yerel SSL sertifika onayı penceresi çıkarsa onay verin.
3. Uygulama aktif durumdayken hedef siteye erişim sağlayın.
4. Kullanımı sonlandırmak için **Proxy'yi Durdur** butonuna tıklayın.

---

## Sistem Gereksinimleri

- **İşletim Sistemi:** Windows 10 / Windows 11 (64-bit)
- **Çalışma Zamanı:** .NET Framework 4.8
