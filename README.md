# KeyLogger

Bu proje, eğitim amaçlı bir keylogger uygulamasıdır.

## 🎥 Video Anlatım
[YouTube'da İzle](https://youtu.be/YbMjK89osHM)

## ⚠️ UYARI
Bu uygulama yalnızca eğitim ve kişisel test amaçlıdır. İzinsiz kullanımı yasa dışıdır ve etik değildir. Sorumlu bir şekilde kullanın.

## 🚀 Kurulum

### 1. Projeyi Klonlayın
```bash
git clone <repo-url>
cd KeyLogger
```

### 2. Email Ayarlarını Yapılandırın

`config.example.txt` dosyasını `config.txt` olarak kopyalayın:
```bash
copy config.example.txt config.txt
```

Ardından `config.txt` dosyasını düzenleyin:
```
EMAIL=your-email@gmail.com
PASSWORD=your-app-password
```

### 3. Gmail App Password Alın

1. Google hesabınıza gidin: https://myaccount.google.com/
2. "Güvenlik" → "2 Adımlı Doğrulama"yı açın
3. "Uygulama şifreleri" bölümünden yeni şifre oluşturun
4. 16 haneli şifreyi `config.txt` dosyasına yapıştırın

### 4. Çalıştırın

```bash
dotnet build
dotnet run
```

## 📝 Özellikler

- ✅ Tuş kayıtları TXT dosyasına kaydedilir
- ✅ Her 100 tuşta bir email gönderimi
- ✅ Türkçe karakter desteği (UTF8)
- ✅ Sistem bilgileri (IP, kullanıcı adı, tarih)
- ✅ Güvenli yapılandırma (config.txt)
- ✅ Temiz tuş kaydı (gereksiz sayılar/key kodları yok)
- ✅ Gmail App Password otomatik boşluk temizleme
- ✅ Gelişmiş hata mesajları ve debug log
- ✅ Visual Studio ve Terminal uyumluluğu

## 📂 Dosya Yapısı

```
KeyLogger/
├── Program.cs           # Ana program
├── config.txt          # Email ayarları (GİZLİ - GitHub'a yüklenmiyor)
├── config.example.txt  # Örnek konfigürasyon
├── .gitignore         # Git ignore kuralları
└── README.md          # Bu dosya
```

## 🔒 Güvenlik

- `config.txt` dosyası `.gitignore`'a eklenmiştir
- Hassas bilgileriniz GitHub'a yüklenmez
- Her kullanıcı kendi `config.txt` dosyasını oluşturmalıdır

## 📜 Lisans

Bu proje sadece eğitim amaçlıdır. Kullanımdan doğabilecek her türlü sorumluluk kullanıcıya aittir.

