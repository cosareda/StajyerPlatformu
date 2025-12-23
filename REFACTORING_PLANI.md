# Refactoring Planı - Services Katmanı Ekleme

## ✅ GÜVENLİ YAKLAŞIM: Adım Adım Refactoring

### AŞAMA 1: Services Klasörü ve Interface'ler Oluşturma
**Risk:** YOK ❌ - Sadece yeni dosyalar ekleniyor
**Mevcut Kod:** Değişmiyor ✅

```
StajyerPlatformu/
├── Services/              ← YENİ (boş klasör)
│   ├── IInternService.cs  ← YENİ
│   └── InternService.cs    ← YENİ
├── Controllers/           ← DEĞİŞMEYECEK (şimdilik)
└── ...
```

### AŞAMA 2: Service Implementation
**Risk:** YOK ❌ - Sadece yeni kod ekleniyor
**Mevcut Kod:** Hala çalışıyor ✅

### AŞAMA 3: Controller'ları Yavaşça Güncelleme
**Risk:** DÜŞÜK ⚠️ - Her controller ayrı ayrı güncellenir
**Mevcut Kod:** Eski kod yedekte kalır

**Örnek:**
```csharp
// ESKİ (çalışmaya devam eder)
private readonly AppDbContext _context;

// YENİ (eklenir, eski kod silinmez hemen)
private readonly IInternService _internService;
```

### AŞAMA 4: Program.cs'e Service Registration
**Risk:** YOK ❌ - Sadece ekleme yapılır
```csharp
// EKLENECEK
builder.Services.AddScoped<IInternService, InternService>();
```

## 📋 DETAYLI ADIMLAR

### 1. Services Klasörü Oluştur
```bash
mkdir Services
```

### 2. Interface Oluştur
```csharp
// Services/IInternService.cs
public interface IInternService
{
    Task<InternProfile> GetProfileAsync(string userId);
    Task<List<InternshipPost>> SearchJobsAsync(string searchString, string city, string workType, string companyName);
    // ...
}
```

### 3. Service Implementation
```csharp
// Services/InternService.cs
public class InternService : IInternService
{
    private readonly AppDbContext _context;
    
    public InternService(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<InternProfile> GetProfileAsync(string userId)
    {
        return await _context.InternProfiles
            .Include(p => p.AppUser)
            .Include(p => p.Experiences)
            .FirstOrDefaultAsync(p => p.AppUserId == userId);
    }
    // ...
}
```

### 4. Program.cs'e Ekle
```csharp
// Program.cs
builder.Services.AddScoped<IInternService, InternService>();
```

### 5. Controller'ı Güncelle (YAVAŞÇA)
```csharp
// Controllers/InternController.cs
public class InternController : Controller
{
    private readonly IInternService _internService;  // YENİ
    private readonly UserManager<AppUser> _userManager;
    
    // ESKİ kod yorum satırına alınır (yedek)
    // private readonly AppDbContext _context;
    
    public InternController(IInternService internService, UserManager<AppUser> userManager)
    {
        _internService = internService;
        _userManager = userManager;
    }
    
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var profile = await _internService.GetProfileAsync(user.Id);  // YENİ
        
        // ESKİ kod:
        // var profile = await _context.InternProfiles...
        
        if (profile == null)
        {
            return RedirectToAction("CreateProfile");
        }
        
        return View(profile);
    }
}
```

## ⚠️ RİSK YÖNETİMİ

### 1. Git Kullan
```bash
git checkout -b feature/services-layer
# Her adımda commit yap
git commit -m "Add IInternService interface"
git commit -m "Add InternService implementation"
git commit -m "Update InternController to use service"
```

### 2. Test Et
- Her adımdan sonra projeyi çalıştır
- Tüm sayfaları test et
- Hata varsa geri al (git revert)

### 3. Yedekle
- Eski controller kodunu yorum satırına al
- Çalıştıktan sonra sil

## ✅ SONUÇ

**Proje Yapısı:**
- ✅ Bozulmaz
- ✅ Sadece geliştirilir
- ✅ Mevcut kod çalışmaya devam eder
- ✅ Adım adım yapılabilir
- ✅ Geri alınabilir (Git ile)

**Faydalar:**
- ✅ Kod daha temiz
- ✅ Test edilebilir
- ✅ Bakımı kolay
- ✅ Enterprise-level yapı



