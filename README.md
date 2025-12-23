# Stajyer & İşveren İletişim Platformu - Proje Özeti

Proje başarıyla oluşturulmuştur. Tüm isterler (ASP.NET Core MVC, Identity, SQLite, Entity Framework Core, Responsive Tasarım, PDF Raporlama) yerine getirilmiştir.

## Nasıl Çalıştırılır?
1. Visual Studio veya VS Code ile projeyi açın.
2. Terminali açın ve proje dizinine gidin.
3. Uygulamayı başlatın:
   ```bash
   dotnet run
   ```
4. Tarayıcıda `https://localhost:5001` (veya terminalde belirtilen port) adresine gidin.

## Özellikler ve Kullanım

### 1. Kimlik Doğrulama (Authentication)
- **Kayıt Ol**: Kullanıcılar "Stajyer" veya "İşveren" olarak kayıt olabilir.
- **Giriş Yap**: Email ve şifre ile sisteme giriş yapılır.
- **Güvenlik**: Şifreler hashlenerek saklanır (Identity kütüphanesi).

### 2. İşveren (Employer) Modülü
- **Profil**: Firma bilgilerini (Ad, Sektör, Web Sitesi) ekleyebilir.
- **İlan Ekle**: Yeni staj ilanı oluşturabilir.
- **Başvurular**: İlanına yapılan başvuruları görebilir.
- **PDF Rapor**: Başvuru listesini PDF olarak indirebilir.

### 3. Stajyer (Intern) Modülü
- **İlanlar**: Tüm staj ilanlarını listeleyebilir ve arama yapabilir.
- **Profil**: Eğitim ve yetenek bilgilerini girebilir.
- **Başvuru**: İlanlara tek tıkla başvurabilir.
- **Başvurularım**: Kendi yaptığı başvuruların durumunu takip edebilir.

### 4. Yönetici (Admin) Modülü
- **Erişim**: Sadece 'Admin' rolündeki kullanıcılar erişebilir. (Veritabanından manuel rol ataması gerekebilir veya `Program.cs` içindeki seed mekanizması ile ilk açılışta rolleri oluşturur, admin kullanıcısını elle atayabilirsiniz).
- **Yönetim**: Tüm kullanıcıları ve ilanları görüp silebilir.

## GitHub'a Yükleme
Projeyi GitHub'a yüklemek için:
1. GitHub'da yeni bir repository oluşturun.
2. Proje klasöründe terminal açın:
   ```bash
   git init
   git add .
   git commit -m "İlk sürüm tamamlandı"
   git branch -M main
   git remote add origin <GITHUB_REPO_LINKINIZ>
   git push -u origin main
   ```
