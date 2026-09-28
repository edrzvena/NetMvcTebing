# Architecture Guide — Tebing

Dokumen ini beda dari 2 dokumen lain di `docs/`:
- `PRD.md` = mau bikin fitur apa.
- `IMPLEMENTATION_PLAN.md` = urutan ngerjain & contoh kode tiap tahap.
- **`ARCHITECTURE.md` (ini) = kenapa foldernya dibagi kayak gitu, tiap folder fungsinya apa,
  dan gimana alur satu request jalan dari View sampai ke Database.**

Baca ini kalau lo lupa "ini kode harusnya ditaruh di mana" atau "kenapa gak taruh di sini aja".

---

## 1. Aturan Paling Penting: Arah Ketergantungan

Semua folder di project ini disusun berdasarkan **layer** (lapisan), dan lapisan itu cuma boleh
manggil lapisan di bawahnya, gak boleh kebalik:

```
Views (Razor)
   │  manggil
   ▼
Controllers
   │  manggil
   ▼
Services            (business logic)
   │  manggil
   ▼
Repositories        (akses data)
   │  manggil
   ▼
Data / AppDbContext (EF Core)
   │
   ▼
Database (SQL Server)
```

**Aturan:** panah cuma boleh ke bawah. `Repository` gak boleh manggil `Service`. `Service` gak
boleh manggil `AppDbContext` langsung (harus lewat `Repository`). `Controller` gak boleh query
database langsung (harus lewat `Service`).

Kenapa penting: kalau aturan ini dilanggar (misal Controller langsung query `AppDbContext`),
lama-lama business logic ke-scatter di banyak tempat, dan pas ada bug soal "kenapa booking bisa
kebooking padahal kuota penuh", lo harus nyari ke semua Controller satu-satu — bukan cukup buka
1 file `BookingService.cs`.

---

## 2. Penjelasan Tiap Folder

### `Models/`
**Apa isinya:** class yang representasiin 1 tabel di database (`User`, `Mountain`, `Booking`, dst).
**Fungsinya:** EF Core baca class ini buat tau struktur tabel & bikin migration.
**Dipanggil oleh:** `Repositories/` (query & simpan data), `Data/AppDbContext.cs` (`DbSet<T>`).
**Gak boleh:** ada logic di sini (validasi, hitung harga, dll). Model itu cuma "bentuk data",
bukan tempat proses.

### `DTOs/`
**Apa isinya:** class polos buat data masuk/keluar antara `Controller` dan `Service`. Contoh:
`BookingCreateDto`, `MountainListDto`.
**Fungsinya:** biar `Service` gak nerima/return `Model` langsung. Kalau `Service` langsung
pakai `Model` (misal `Booking`), Controller/View jadi punya akses ke field yang harusnya gak
perlu ditampilin (misal `PasswordHash` di `User`), dan susah dikontrol field mana yang boleh
diisi user.
**Dipanggil oleh:** `Controllers/` (nerima input form, ngirim hasil query buat ditampilin),
`Services/` (return hasil, terima parameter).

### `ViewModels/`
**Apa isinya:** class khusus buat 1 halaman Razor View, sering gabungan beberapa DTO + data
tambahan (dropdown, flag UI).
**Fungsinya:** View gak harus tau soal `DTO`/`Model` mentah — dia cukup terima 1 objek yang
persis sesuai kebutuhan tampilannya.
**Dipanggil oleh:** `Controllers/` (bikin ViewModel dari hasil Service, kirim ke View lewat
`return View(viewModel)`), `Views/*.cshtml` (`@model BookingCreateViewModel`).
**Bedanya sama DTO:** lihat penjelasan detail di `IMPLEMENTATION_PLAN.md` bagian "DTO vs
ViewModel".

### `Repositories/`
**Apa isinya:** kode yang manggil `AppDbContext` buat CRUD ke database. Dipecah jadi:
- `Interfaces/` — kontrak (`IBookingRepository`), isinya cuma daftar method tanpa isi.
- `Implementations/` — kode aslinya (`BookingRepository`), yang manggil `DbSet<Booking>`.

**Fungsinya:** Ini satu-satunya lapisan yang boleh "ngomong" ke `AppDbContext`. Kalau nanti mau
ganti cara nyimpen data (misal sebagian pindah ke Dapper/raw SQL buat query berat), yang
diubah cukup isi `Implementations/`, `Service` yang manggilnya gak perlu tau/berubah sama
sekali — karena dia cuma kenal `interface`-nya.

**Dipanggil oleh:** `Services/` doang. **Manggil:** `Data/AppDbContext.cs`, `Models/`.

### `Services/`
**Apa isinya:** business logic / aturan bisnis. Dipecah `Interfaces/` (`IBookingService`) &
`Implementations/` (`BookingService`).
**Fungsinya:** tempat semua ATURAN aplikasi ditulis — contoh: "booking gak boleh ngelebihin
kuota", "review cuma bisa dibuat kalau booking udah `Completed`", "password harus di-hash
sebelum disimpan". Kalau aturan-aturan ini ditulis di `Controller`, dan ada 2 Controller yang
butuh aturan sama, lo harus copy-paste. Ditulis di `Service`, tinggal dipanggil dari mana aja.
**Dipanggil oleh:** `Controllers/`. **Manggil:** `Repositories/` (buat ambil/simpan data),
`Exceptions/` (buat nolak aksi yang gak valid), `Mappings/` (ubah Model→DTO).

### `Mappings/`
**Apa isinya:** extension method buat ubah bentuk data, misal `.ToDto()`, `.ToViewModel()`.
Contoh: `MountainMappings.cs` isinya `public static MountainListDto ToListDto(this Mountain m) => new() { Id = m.Id, Name = m.Name, ... };`
**Fungsinya:** tanpa ini, kode `new MountainListDto { Id = m.Id, Name = m.Name, ... }` bakal
ditulis ulang di banyak tempat (Controller lain, Service lain) tiap kali butuh ubah `Mountain`
jadi `MountainListDto`. Ditulis sekali di sini, tinggal dipanggil `mountain.ToListDto()`.
**Dipanggil oleh:** `Services/` (paling sering), kadang `Controllers/` kalau mapping DTO→ViewModel.

### `Exceptions/`
**Apa isinya:** class exception custom, contoh `NotFoundException`, `BusinessRuleException`.
**Fungsinya:** `Service` bisa `throw new NotFoundException("Gunung tidak ditemukan")` daripada
`return null` yang gampang lupa dicek di Controller (nanti `NullReferenceException` pas render
View). Exception ini nanti ditangkep di 1 tempat oleh `Middlewares/`.
**Dipanggil/dilempar oleh:** `Services/`. **Ditangkep oleh:** `Middlewares/`.

### `Middlewares/`
**Apa isinya:** `ExceptionHandlingMiddleware.cs` — kode yang "membungkus" tiap request masuk.
**Fungsinya:** nangkep exception dari `Exceptions/` di SATU tempat, ubah jadi response yang
rapi (redirect ke halaman error, atau balikin pesan yang enak dibaca user), daripada nulis
try-catch di tiap Controller action satu-satu.
**Didaftarkan di:** `Program.cs` (`app.UseMiddleware<ExceptionHandlingMiddleware>()`).

### `Constants/`
**Apa isinya:** class isinya `const string`/`static readonly`, contoh `Roles.cs` (`public const string Admin = "Admin";`).
**Fungsinya:** hindari nulis string `"Admin"` berkali-kali di banyak file (`[Authorize(Roles = "Admin")]`,
`if (user.Role == "Admin")`, dst) — kalau ada typo (`"admn"`) di salah satu tempat, itu bug yang
susah ketauan. Pakai `Roles.Admin` (constant), typo langsung ketauan pas compile.
**Dipakai oleh:** siapa aja (Controller, Service, dll) yang butuh nilai tetap itu.

### `Helpers/`
**Apa isinya:** logic kecil yang dipakai berulang di **lebih dari 1 tempat**, contoh
`FileStorageHelper.cs` (generate nama file unik + validasi tipe file, dipakai `MountainService`
buat upload gambar gunung DAN `PaymentService` buat upload bukti transfer).
**Aturan penting:** kalau suatu logic cuma dipakai 1 tempat, JANGAN ditaruh di sini — taruh
langsung di tempat yang butuh. Folder ini gampang jadi "tempat sampah" kalau gak dijaga.

### `Extensions/`
**Apa isinya:** `ServiceCollectionExtensions.cs` — extension method buat `IServiceCollection`,
contoh `builder.Services.AddRepositories()` yang di dalemnya baru manggil semua
`AddScoped<IUserRepository, UserRepository>()` dst.
**Fungsinya:** `Program.cs` jadi pendek & gampang dibaca (cuma manggil `AddRepositories()`,
`AddApplicationServices()`), daripada isinya puluhan baris `AddScoped` ditumpuk semua.

### `Common/Enums/`
**Apa isinya:** `enum` yang dipakai lintas layer: `UserRole`, `BookingStatus`, `PaymentStatus`,
`DifficultyLevel`, `MountainStatus`.
**Fungsinya:** representasiin "pilihan tetap" (status booking cuma bisa salah satu dari 5
kemungkinan) sebagai tipe data, bukan `string` bebas yang bisa ketik apa aja.
**Dipakai oleh:** `Models/` (property), `DTOs/`, `Services/` (logic `if (booking.Status == BookingStatus.Completed)`).

### `Data/`
- **`AppDbContext.cs`** — jembatan EF Core ke database. Isinya daftar `DbSet<T>` (satu per
  Model) + konfigurasi model.
- **`Configurations/`** — `IEntityTypeConfiguration<T>` per entity (misal `BookingConfiguration.cs`),
  isinya aturan detail kolom (`MaxLength`, index, relasi) yang kalau ditulis semua di 1 method
  `OnModelCreating` bakal jadi ratusan baris. Dipanggil otomatis lewat
  `modelBuilder.ApplyConfigurationsFromAssembly(...)`.
- **`Seed/`** — `DbSeeder.cs`, isi data awal (role, sample gunung) pas app pertama jalan di
  Development, biar gak perlu input manual tiap kali `Update-Database` dari nol.

**Dipanggil oleh:** `Repositories/` (satu-satunya yang boleh).

### `Migrations/`
Folder ini **di-generate otomatis** oleh EF Core CLI (`dotnet ef migrations add ...`), isinya
riwayat perubahan schema database. Jangan ditulis manual.

### `Controllers/`
**Apa isinya:** class yang nerima HTTP request, manggil `Service`, terus pilih `View` mana
yang di-render (atau redirect).
**Fungsinya:** cuma jadi "penghubung" — terima input dari user (form/URL), lempar ke `Service`,
terima hasilnya, mapping ke `ViewModel`, kirim ke `View`. **Gak boleh ada business logic di
sini** (itu tugas `Service`).
**Manggil:** `Services/`, `Mappings/`, `ViewModels/`.

### `Views/`
File `.cshtml` (Razor) — HTML + sedikit C# buat nampilin data dari `ViewModel`. Gak boleh
manggil `Service`/`Repository` langsung.

---

## 3. Tabel `using` — Folder Mana Butuh `using` Namespace Apa

Ini yang kemarin belum eksplisit: tiap folder = 1 `namespace` (`Tebing.<NamaFolder>`, ngikutin
folder `Models/` → `namespace Tebing.Models;`, persis kayak `Models/User.cs` yang udah ada
sekarang). Supaya 1 file bisa pakai class dari folder lain, dia harus `using` namespace folder
itu. Berikut daftar lengkapnya per folder — kalau kode lo butuh `using` di luar daftar ini
(terutama yang "lompat lapisan"), itu tanda ada yang salah taruh:

| Folder (namespace) | Butuh `using` | Kenapa |
|---|---|---|
| `Common/Enums/` (`Tebing.Common.Enums`) | *(gak butuh apa-apa)* | Cuma `enum` polos, gak depend ke apa pun. |
| `Models/` (`Tebing.Models`) | `Tebing.Common.Enums` | Property kayak `public BookingStatus Status { get; set; }` butuh tipe `BookingStatus`. |
| `Constants/` (`Tebing.Constants`) | *(gak butuh apa-apa)* | Isinya cuma `const string`. |
| `Exceptions/` (`Tebing.Exceptions`) | *(gak butuh apa-apa, cukup `System`)* | Cuma turunan `Exception` bawaan .NET. |
| `DTOs/` (`Tebing.DTOs.*`) | `Tebing.Common.Enums` | Contoh `BookingListDto.Status` juga tipe `BookingStatus`. **Gak** `using Tebing.Models` — DTO harus tetap gak kenal EF Core entity. |
| `Data/` (`Tebing.Data`) | `Tebing.Models`, `Microsoft.EntityFrameworkCore` | `AppDbContext` isinya `DbSet<Booking> Bookings => Set<Booking>();` — `Booking` itu dari `Models/`. `Data/Configurations/BookingConfiguration.cs` juga butuh `Tebing.Models` (buat `IEntityTypeConfiguration<Booking>`). |
| `Repositories/Interfaces/` (`Tebing.Repositories.Interfaces`) | `Tebing.Models` | Signature method kayak `Task<Booking?> GetByIdAsync(int id)` butuh tipe `Booking`. **Gak** butuh `Tebing.Data` — interface gak boleh tau soal EF Core. |
| `Repositories/Implementations/` (`Tebing.Repositories.Implementations`) | `Tebing.Repositories.Interfaces`, `Tebing.Data`, `Tebing.Models`, `Microsoft.EntityFrameworkCore` | Implementasi butuh `AppDbContext` (dari `Data/`) buat query, `Models` buat tipe entity-nya, dan `Microsoft.EntityFrameworkCore` buat method LINQ (`FirstOrDefaultAsync`, dll). |
| `Mappings/` (`Tebing.Mappings`) | `Tebing.Models`, `Tebing.DTOs.*` | Extension method `this Booking booking` (dari Model) yang return `BookingDetailDto` (dari DTOs). |
| `Services/Interfaces/` (`Tebing.Services.Interfaces`) | `Tebing.DTOs.*` | Signature method kayak `Task<BookingDetailDto> CreateAsync(BookingCreateDto dto, int userId)` cuma "ngomong" pakai DTO. **Gak** butuh `Tebing.Models`/`Tebing.Repositories` — interface Service gak boleh bocorin detail internal. |
| `Services/Implementations/` (`Tebing.Services.Implementations`) | `Tebing.Services.Interfaces`, `Tebing.Repositories.Interfaces`, `Tebing.DTOs.*`, `Tebing.Models`, `Tebing.Mappings`, `Tebing.Exceptions`, `Tebing.Common.Enums` | Ini layer paling banyak `using`-nya karena di sinilah business logic ketemu: ambil data lewat Repository (dapat `Model`), validasi pakai `Enums`, lempar `Exceptions` kalau gak valid, terus mapping `Model` → `DTO` pakai `Mappings` buat dikembaliin. **Gak** boleh `using Tebing.Data` atau `Microsoft.EntityFrameworkCore` — itu hak `Repositories/` doang. |
| `Middlewares/` (`Tebing.Middlewares`) | `Tebing.Exceptions`, `Microsoft.AspNetCore.Http` | Nangkep tipe exception dari `Exceptions/`, butuh `HttpContext`/`RequestDelegate` dari ASP.NET Core buat nulis response. |
| `Helpers/` (`Tebing.Helpers`) | *(tergantung isinya)*, umumnya cuma `Microsoft.AspNetCore.Http` (misal `IFormFile` di `FileStorageHelper`) | Helper idealnya berdiri sendiri, gak depend ke `Models`/`DTOs`. |
| `Extensions/` (`Tebing.Extensions`) | `Tebing.Repositories.Interfaces`, `Tebing.Repositories.Implementations`, `Tebing.Services.Interfaces`, `Tebing.Services.Implementations`, `Microsoft.Extensions.DependencyInjection` | `ServiceCollectionExtensions.AddRepositories()` harus kenal interface DAN implementasinya buat bisa `AddScoped<IBookingRepository, BookingRepository>()`. |
| `ViewModels/` (`Tebing.ViewModels.*`) | `Tebing.DTOs.*`, `Microsoft.AspNetCore.Mvc.Rendering` | ViewModel sering nge-wrap DTO (`public BookingCreateDto Booking { get; set; }`) + butuh `SelectListItem` buat dropdown. |
| `Controllers/` (`Tebing.Controllers`) | `Tebing.Services.Interfaces`, `Tebing.ViewModels.*`, `Tebing.Constants` | Controller manggil Service lewat interface-nya, kirim/terima `ViewModel` ke View, dan pakai `Roles.Admin` (dari `Constants/`) di `[Authorize(Roles = ...)]`. **Gak** boleh `using Tebing.Repositories.*` atau `Tebing.Data` — itu lompat 2 lapisan sekaligus. |
| `Views/*.cshtml` | `@model Tebing.ViewModels.<Nama>` | Razor gak pakai `using` C# biasa, tapi `@model` di baris pertama itu fungsinya sama — bilang "file ini butuh tipe X". Views **gak** boleh `@inject` Service/Repository/DbContext langsung. |

**Cara gampang ingetnya:** interface (`Repositories/Interfaces`, `Services/Interfaces`) selalu
"ngomong" pakai tipe data dari lapisan **di bawahnya** langsung (Repository interface pakai
`Models`, Service interface pakai `DTOs`) — sedangkan implementasinya (`Repositories/Implementations`,
`Services/Implementations`) yang nambahin `using` ke lapisan teknis di bawahnya lagi (`Data`,
`EntityFrameworkCore`).

---

## 4. Contoh Alur Nyata (End-to-End)

Kasus: **user submit form booking pendakian**.

```
1. Views/Booking/Create.cshtml
   Form di-submit (POST) ke BookingController.Create

2. Controllers/BookingController.cs
   using Tebing.Services.Interfaces;
   using Tebing.ViewModels.Booking;

   public class BookingController(IBookingService bookingService) : Controller
   {
       [HttpPost]
       public async Task<IActionResult> Create(BookingCreateViewModel vm)
       {
           var dto = vm.ToCreateDto();              // dari Mappings/
           var result = await bookingService.CreateAsync(dto, currentUserId);
           return RedirectToAction("Detail", new { id = result.Id });
       }
   }

3. Services/Implementations/BookingService.cs
   using Tebing.Repositories.Interfaces;
   using Tebing.Exceptions;

   public class BookingService(IBookingRepository bookingRepo, IHikingRouteRepository routeRepo)
       : IBookingService
   {
       public async Task<BookingDetailDto> CreateAsync(BookingCreateDto dto, int userId)
       {
           var route = await routeRepo.GetByIdAsync(dto.HikingRouteId)
               ?? throw new NotFoundException("Jalur tidak ditemukan");   // dari Exceptions/

           // business rule: cek kuota (pakai Common/Enums/BookingStatus)
           var booked = await bookingRepo.GetPendingCountByRouteAndDateAsync(route.Id, dto.ClimbDate);
           if (route.DailyQuota is not null && booked + dto.Participants.Count > route.DailyQuota)
               throw new BusinessRuleException("Kuota sudah penuh");

           var booking = dto.ToEntity(userId);       // dari Mappings/, DTO -> Models/Booking
           await bookingRepo.AddAsync(booking);
           await bookingRepo.SaveChangesAsync();

           return booking.ToDetailDto();             // Models/Booking -> DTOs/BookingDetailDto
       }
   }

4. Repositories/Implementations/BookingRepository.cs
   using Tebing.Data;
   using Tebing.Models;

   public class BookingRepository(AppDbContext context) : Repository<Booking>(context), IBookingRepository
   {
       public async Task<int> GetPendingCountByRouteAndDateAsync(int routeId, DateOnly date) =>
           await DbSet.Where(b => b.HikingRouteId == routeId && b.ClimbDate == date
                                  && b.Status != BookingStatus.Rejected && b.Status != BookingStatus.Cancelled)
                      .SumAsync(b => b.NumberOfPeople);
   }

5. Data/AppDbContext.cs — DbSet<Booking> yang dipakai Repository di atas cuma:
   public DbSet<Booking> Bookings => Set<Booking>();

   EF Core yang nerjemahin LINQ di atas jadi SQL, dikirim ke SQL Server.
```

Titik pentingnya: **`Controller` gak pernah `using Tebing.Data;` atau `using Tebing.Repositories.*`**
— dia cuma kenal `Tebing.Services.Interfaces`. Begitu juga `Service` gak pernah `using
Microsoft.EntityFrameworkCore` buat query langsung — dia cuma kenal `Tebing.Repositories.Interfaces`.
Kalau lo lihat ada `using` yang "lompat lapisan" (misal Controller `using Tebing.Data;`), itu
tanda arsitekturnya udah dilanggar.

> Catatan: contoh `Booking` di atas itu **rencana** (belum ditulis, itu Tahap 4 di
> `IMPLEMENTATION_PLAN.md`). Biar ada yang bisa dicek langsung ke kode asli yang **udah beneran
> jalan sekarang**, ini contoh yang sama tapi buat fitur Login yang udah di-commit.

### Contoh Alur Nyata #2: Login (kode yang beneran sudah ada, bukan rencana)

```
1. Views/Account/Login.cshtml
   Form di-submit (POST) ke AccountController.Login

2. Controllers/AccountController.cs
   using Tebing.DTOs.Auth;
   using Tebing.Services.Interfaces;

   public class AccountController(IAuthService authService) : Controller
   {
       [HttpPost]
       [ValidateAntiForgeryToken]
       public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
       {
           var result = await authService.LoginAsync(dto);
           if (!result.Success)
           {
               ModelState.AddModelError(string.Empty, result.Error ?? "Login gagal.");
               return View(dto);
           }
           return RedirectToAction("Index", "Home");
       }
   }

3. Services/Implementations/AuthService.cs
   using Microsoft.AspNetCore.Identity;
   using Tebing.Repositories.Interfaces;

   public class AuthService(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor)
       : IAuthService
   {
       public async Task<AuthResult> LoginAsync(LoginDto dto)
       {
           var user = await userRepository.GetByEmailAsync(dto.Email);
           if (user is null)
               return new AuthResult(false, "Email atau password salah.");

           var verification = PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
           if (verification == PasswordVerificationResult.Failed)
               return new AuthResult(false, "Email atau password salah.");

           var claims = new List<Claim> { /* NameIdentifier, Email, Name, Role */ };
           var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

           await httpContextAccessor.HttpContext!.SignInAsync(
               CookieAuthenticationDefaults.AuthenticationScheme, principal,
               new AuthenticationProperties { IsPersistent = dto.RememberMe });

           return new AuthResult(true);
       }
   }

4. Repositories/Implementations/UserRepository.cs
   using Tebing.Data;
   using Tebing.Models;

   public class UserRepository(AppDbContext context) : Repository<User>(context), IUserRepository
   {
       public async Task<User?> GetByEmailAsync(string email) =>
           await DbSet.FirstOrDefaultAsync(u => u.Email == email);
   }

5. Data/AppDbContext.cs — DbSet<User> yang dipakai Repository di atas cuma:
   public DbSet<User> Users => Set<User>();

   EF Core nerjemahin `FirstOrDefaultAsync` di atas jadi `SELECT ... WHERE Email = @email`,
   dikirim ke SQL Server lewat connection string di `appsettings.json`.
```

Balik lagi ke arah datanya (dari bawah ke atas): `User` entity (dari `Models/`) balik ke
`AuthService`, di situ diverifikasi & dibungkus jadi cookie auth, lalu return `AuthResult`
(bukan `User` mentah!) ke `AccountController`. Controller **gak pernah lihat** `User.PasswordHash`
atau objek `User` sama sekali — dia cuma tau "sukses" atau "gagal + alasan". Ini contoh nyata
kenapa `Service` gak boleh `return User` ke `Controller`: kalau iya, Controller/View jadi punya
akses ke `PasswordHash` walau gak butuh sama sekali.

---

## 5. "Namanya beneran harus itu?"

**Enggak.** C# compiler **sama sekali gak peduli** nama folder lo apa. Folder cuma organisasi
file system — bukan bagian dari bahasa C#. Lo bisa aja rename `Repositories/` jadi
`DataAccess/` atau bahkan `Foo/`, project tetap kompilasi & jalan normal, selama `namespace`
di dalam file-nya konsisten.

Yang bikin nama-nama ini (`Repositories`, `Services`, `DTOs`, `Controllers`, dst) kerasa
"wajib" adalah **konvensi komunitas .NET**, bukan aturan bahasa:
- Ini pola yang dipakai di hampir semua tutorial, buku, dan project ASP.NET Core beneran
  (sering disebut *layered architecture* / *n-tier architecture*).
- Kalau lo pakai nama standar ini, developer lain (atau lo sendiri 6 bulan lagi) langsung
  paham isinya apa tanpa perlu baca kode dulu — cukup lihat nama foldernya.
- Kalau lo pakai nama sendiri (misal `Otak/` buat business logic), orang lain harus belajar
  ulang "oh di project ini `Otak/` itu isinya kayak Service di project lain" — nambah beban
  mental yang gak perlu.

**Satu hal teknis yang perlu diperhatiin (bukan soal nama, tapi soal konsistensi):** secara
default, Visual Studio/Rider bikin `namespace` mengikuti path folder (`Tebing.Repositories.Interfaces`
buat file di `Repositories/Interfaces/`). Ini juga cuma **fitur bantuan IDE**, bukan keharusan
compiler — tapi tetap disarankan diikuti, karena kalau `namespace` gak nyambung sama lokasi
file, itu bikin bingung pas nyari file lewat "Go to Definition".

**Kesimpulan:** pakai nama-nama di dokumen ini karena itu bahasa yang dipahami semua orang di
ekosistem .NET, bukan karena keharusan teknis. Kalau lo punya alasan kuat buat pakai nama lain,
itu sah-sah aja — yang penting konsisten di seluruh project.

---

## 6. File Konfigurasi & Bootstrap Project

Ini file-file yang **di luar** folder layer di atas (bukan `Models/`, bukan `Controllers/`, dst).
Fungsinya beda: bukan nyimpen kode aplikasi, tapi **ngatur gimana aplikasi di-compile, di-jalanin,
dan di-configure**. Karena bukan bagian dari alur request (View→Controller→Service→dst), gampang
kelewat dipahami padahal ini yang bikin app lo bisa nyala sama sekali.

### `appsettings.json`
**Apa isinya:** config dasar yang **selalu** kebaca, di environment apa pun (Development,
Staging, Production). Isinya sekarang:
```json
{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=db_net_mvc_tebing;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```
**Fungsinya:** file ini dibaca otomatis sama `WebApplication.CreateBuilder(args)` di `Program.cs`
pas app start, dijadiin objek `IConfiguration`. Yang lo pakai selama ini (`DefaultConnection`)
diambil lewat `builder.Configuration.GetConnectionString("DefaultConnection")` — itu jembatan
dari file JSON ini ke `AppDbContext`.

Breakdown connection string yang lo punya sekarang:
- `Server=.\SQLEXPRESS` → connect ke SQL Server Express yang jalan lokal di komputer lo (`.`
  artinya localhost).
- `Database=db_net_mvc_tebing` → nama database yang dipakai.
- `Trusted_Connection=True` → pakai Windows Authentication (akun Windows lo), **gak** ada
  username/password ke-tulis di sini. Ini kenapa aman buat di-commit ke git sekarang — begitu
  nanti ganti ke SQL Authentication (username+password) atau connection string server
  production, **jangan** commit itu di sini, pindah ke User Secrets (dev) atau environment
  variable (production).
- `TrustServerCertificate=True` → skip validasi SSL certificate, umum dipakai buat localhost dev
  yang gak punya certificate resmi.

**Selain `ConnectionStrings`, ini juga tempat buat:** config custom lain kalau nanti nambah fitur
— misal `EmailSettings` (SMTP host/port buat kirim notifikasi booking), `PaymentGatewaySettings`
(API key Midtrans/Xendit), dst. Pola: tulis section baru di JSON, terus baca lewat
`builder.Configuration.GetSection("EmailSettings")` di `Program.cs` atau lewat
`IOptions<T>` pattern.

### `appsettings.Development.json`
**Apa isinya:** overlay config yang **cuma** kebaca kalau environment = `Development`. Isinya
sekarang cuma `Logging` (sama persis kayak yang di `appsettings.json`, jadi saat ini efeknya
kosong — belum ada override unik).

**Mekanismenya:** ASP.NET Core baca `appsettings.json` dulu, terus baca
`appsettings.{ASPNETCORE_ENVIRONMENT}.json` di atasnya, dan yang kedua ini **override key yang
sama** (merge per key, bukan replace seluruh file). Environment-nya sendiri ditentuin dari
environment variable `ASPNETCORE_ENVIRONMENT` — kalau lo run lewat `dotnet run` atau profile
default di Visual Studio/Rider, defaultnya `Development`.

**Fungsi sebenarnya (kenapa file ini dipisah dari `appsettings.json`):**
- Development biasanya pengen `LogLevel` lebih detail (`Debug`/`Trace`) buat ngeliat query EF
  Core yang di-generate, sementara Production cukup `Information`/`Warning` biar log gak
  kebanjiran.
- Kalau nanti connection string Production beda server dari SQL Express lokal lo, `Development`
  bisa punya connection string sendiri di sini (atau lebih aman, di User Secrets) tanpa nyentuh
  `appsettings.json` yang jadi baseline buat semua environment.
- Bisa juga buat matiin/nyalain fitur yang cuma mau aktif pas development (misal `DbSeeder` di
  `Data/Seed/` biasanya dipanggil cuma kalau `app.Environment.IsDevelopment()` true).

**Kapan harus edit ini vs `appsettings.json`:** kalau config itu emang beda antar environment
(logging verbosity, connection string per environment) → taruh di sini. Kalau config itu sama di
semua environment (nama aplikasi, default culture, dst) → cukup di `appsettings.json`.

### `Program.cs`
Ini **entry point** aplikasi — baris pertama yang jalan pas `dotnet run`. Isinya sekarang, dibedah
baris per baris:

```csharp
var builder = WebApplication.CreateBuilder(args);
```
Bikin `builder`. Di titik ini, `appsettings.json` + `appsettings.{env}.json` + environment
variables + command-line args **otomatis** ke-load jadi `builder.Configuration` — makanya baris
`GetConnectionString(...)` di bawah bisa langsung jalan tanpa kode baca-file manual.

```csharp
builder.Services.AddControllersWithViews();
```
Daftarin MVC (Controller + Razor View rendering) ke **DI container**. Tanpa baris ini,
`Controllers/` gak bakal ke-detect dan `.cshtml` gak bisa di-render.

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```
Daftarin `AppDbContext` (dari `Data/`) ke DI container sebagai `Scoped` service (1 instance per
HTTP request — default `AddDbContext`), pakai provider SQL Server, connection string-nya dari
`appsettings.json` yang dibaca di baris pertama tadi. Ini yang bikin `Repositories/` bisa
"minta" `AppDbContext` lewat constructor tanpa `new AppDbContext()` manual.

```csharp
var app = builder.Build();
```
Dari titik ini, DI container udah "dikunci" — gak bisa nambah `AddScoped`/`AddDbContext` lagi
setelah baris ini. Sisa kode di bawah cuma ngatur **urutan middleware** (request pipeline).

```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
```
Contoh nyata pakai environment check: kalau **bukan** Development (berarti Staging/Production),
pasang `UseExceptionHandler` (redirect ke halaman error generik, gak nampilin stack trace mentah
ke user asli) dan `UseHsts` (paksa browser selalu pakai HTTPS). Pas Development, dua ini
**sengaja di-skip** biar lo masih bisa liat stack trace error asli buat debugging.

```csharp
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.Run();
```
- `UseHttpsRedirection()` — request http otomatis di-redirect ke https.
- `UseRouting()` — middleware yang nentuin URL match ke endpoint/Controller mana.
- `UseAuthorization()` — cek attribute `[Authorize]` di Controller/Action. **Catatan penting:**
  belum ada `app.UseAuthentication()` di file ini karena project belum punya sistem login. Begitu
  nanti nambah login (cookie auth / Identity / JWT), `UseAuthentication()` **harus** ditaruh
  **sebelum** `UseAuthorization()` — urutan middleware itu penting banget, kebalik = auth gak
  jalan bener.
- `MapStaticAssets()` / `.WithStaticAssets()` — fitur .NET terbaru buat optimasi serving file
  static (`wwwroot/`, kayak CSS/JS/gambar) dengan fingerprinting & caching otomatis.
- `MapControllerRoute(...)` — pattern URL default: `/Booking/Create/5` → controller `Booking`,
  action `Create`, id `5`.
- `app.Run()` — start web server (Kestrel), baris ini blocking, app "nyala" di sini.

**Analoginya:** tiap baris `app.Use...`/`app.Map...` itu kayak lapisan yang dilewatin request
dari atas ke bawah (dan balik lagi dari bawah ke atas pas response). Kalau nanti nambah
`app.UseMiddleware<ExceptionHandlingMiddleware>()` (yang disebut di section `Middlewares/` di
atas), taruh **sebelum** `UseRouting()` biar exception dari Controller manapun ketangkep.

### `Tebing.sln`
**Apa isinya:** *solution file* — daftar project (`.csproj`) yang tergabung dalam 1 solution,
dibaca sama Visual Studio/Rider (dan `dotnet` CLI kalau perlu build banyak project sekaligus).
Isinya sekarang cuma daftarin 1 project: `Tebing.csproj`.

**Fungsinya:** kalau project ini tetap 1 project kayak sekarang, `.sln` ini kurang lebih cuma
formalitas biar IDE bisa "buka project" — `dotnet run`/`dotnet build` sebenernya bisa langsung
jalan dari `Tebing.csproj` tanpa `.sln`.

**Kapan jadi penting:** begitu project ini dipecah jadi lebih dari 1 project dalam repo yang sama
— misal nanti nambah `Tebing.Tests` (project unit test terpisah) atau `Tebing.Api` (REST API
terpisah dari MVC) — project baru itu didaftarin di `.sln` ini biar bisa di-build bareng dan
IDE bisa nampilin semuanya dalam 1 workspace.

**Jangan edit manual isinya** (format GUID-nya gampang salah) — biarin IDE atau `dotnet sln add
<path-ke-csproj>` yang generate.

### `Tebing.csproj`
**Apa isinya:** *project file* (format MSBuild) — ini yang **beneran** nentuin cara project
di-compile, bukan `.sln`. Isinya sekarang:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
    <PropertyGroup>
        <TargetFramework>net10.0</TargetFramework>
        <Nullable>enable</Nullable>
        <ImplicitUsings>enable</ImplicitUsings>
    </PropertyGroup>
    <ItemGroup>
      <Folder Include="ViewModels\" />
    </ItemGroup>
    <ItemGroup>
      <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.0">
        <PrivateAssets>all</PrivateAssets>
        <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      </PackageReference>
      <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.0" />
    </ItemGroup>
</Project>
```

- `Sdk="Microsoft.NET.Sdk.Web"` → bilang ini web project (ASP.NET Core). SDK ini otomatis
  nge-include reference ke Kestrel, MVC, Razor, dll tanpa perlu declare manual satu-satu.
- `<TargetFramework>net10.0</TargetFramework>` → versi .NET yang dipakai compile & run. Ganti
  ini kalau mau upgrade/downgrade versi .NET.
- `<Nullable>enable</Nullable>` → compiler bakal **warning** kalau ada reference type
  (`string`, `Booking`, dll) yang berpotensi null tapi gak di-handle — beda `string` (gak boleh
  null) vs `string?` (boleh null) jadi eksplisit. Ini alasan teknis kenapa pola
  `?? throw new NotFoundException(...)` di section 4 di atas bukan cuma gaya nulis, tapi beneran
  dicek compiler.
- `<ImplicitUsings>enable</ImplicitUsings>` → otomatis nambahin `using System;`,
  `using System.Linq;`, `using System.Threading.Tasks;`, dst di **semua** file tanpa perlu
  ditulis manual. Yang tetap harus ditulis manual cuma `using` yang spesifik project (kayak
  `using Tebing.Data;` di `Program.cs`).
- `<Folder Include="ViewModels\" />` → cuma nandain "folder `ViewModels/` ada", soalnya folder
  kosong gak otomatis ke-track (baik oleh git maupun MSBuild) — begitu ada file `.cs` pertama di
  situ, baris ini gak diperlukan lagi.
- `<PackageReference>` → daftar NuGet package yang dipakai:
  - `Microsoft.EntityFrameworkCore.Design` — cuma dipakai pas development (command
    `dotnet ef migrations add ...`), makanya ada `PrivateAssets=all` biar gak ikut ke-bundle pas
    publish/deploy.
  - `Microsoft.EntityFrameworkCore.SqlServer` — provider EF Core buat connect ke SQL Server,
    ini yang dipanggil di `Program.cs` lewat `options.UseSqlServer(...)`.

**Kapan edit file ini:** biasanya **otomatis** lewat command (`dotnet add package X` nambah
`<PackageReference>` sendiri), jarang perlu edit manual kecuali ganti `TargetFramework` atau
nge-tweak properti build.

### Ringkasan: Kapan Edit File Mana

| Kebutuhan | Edit file |
|---|---|
| Ganti connection string DB (server/nama DB beda) | `appsettings.json` |
| Development mau logging lebih detail / DB lokal beda dari environment lain | `appsettings.Development.json` |
| Ada secret asli (password DB, API key) yang gak boleh ke-commit | **Jangan** taruh di `appsettings*.json` — pakai `dotnet user-secrets` (dev) atau environment variable (production) |
| Nambah middleware baru (auth, CORS, custom exception handler) | `Program.cs`, perhatiin **urutan** `app.Use...` |
| Nambah service baru ke DI container (`AddScoped`, `AddDbContext` lain, dst) | `Program.cs`, atau `Extensions/ServiceCollectionExtensions.cs` kalau udah banyak biar `Program.cs` tetap pendek |
| Nambah NuGet package | `Tebing.csproj` (biasanya otomatis lewat `dotnet add package`) |
| Nambah project baru dalam repo yang sama (misal test project) | `Tebing.sln` (biasanya otomatis lewat `dotnet sln add`) |
| Ganti versi .NET | `Tebing.csproj` (`<TargetFramework>`) |
| Nambah config custom (email settings, API key eksternal, dll) | `appsettings.json` (section baru) + baca lewat `builder.Configuration` di `Program.cs` |

---

## 7. FAQ: "Interface itu apaan?", "DTO juga blueprint?"

Ini pertanyaan yang paling sering muncul begitu pindah dari "semua ditumpuk di Controller" ke
pola berlapis kayak di project ini. Dijawab satu-satu, pakai kode `AuthService` yang udah ada
di project (`Services/Interfaces/IAuthService.cs` & `Services/Implementations/AuthService.cs`).

### Interface itu "janji", isinya cuma daftar method, bener gak?

**Bener, hampir pas.** Interface itu **kontrak** — daftar method (nama, parameter, return
type) **tanpa badan kode sama sekali**:

```csharp
// Services/Interfaces/IAuthService.cs — ISI-nya cuma "janji", gak ada logic
public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterDto dto);
    Task<AuthResult> LoginAsync(LoginDto dto);
    Task LogoutAsync();
    Task<string?> GenerateResetTokenAsync(string email);
    Task<AuthResult> ResetPasswordAsync(ResetPasswordDto dto);
}
```

Isinya cuma bilang: "siapa pun yang mau ngaku sebagai `IAuthService`, WAJIB punya kelima method
ini, dengan signature persis kayak di atas." Gimana caranya method itu jalan (hash password,
cookie, dst) — interface gak peduli dan gak tau. Itu tugas class yang **implement** dia:

```csharp
// Services/Implementations/AuthService.cs — ini yang beneran isi "cara"-nya
public class AuthService(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor)
    : IAuthService
{
    public async Task<AuthResult> RegisterAsync(RegisterDto dto) { /* ...logic beneran... */ }
    // ...4 method lain wajib ada di sini juga, kalau kelupaan satu = compiler error
}
```

Interface **gak bisa di-`new`**. `new IAuthService()` itu **error compile** — gak ada
"instance interface". Yang bisa di-`new` cuma class yang implement dia (`new AuthService(...)`).
Interface itu cuma "bentuk", bukan "barang".

### "Gak wajib dipanggil" — maksud lo gimana persisnya?

Ini bagian yang perlu diluruskan sedikit, karena ada **dua "wajib" yang beda**:

1. **Wajib buat class yang implement** — kalau `AuthService : IAuthService`, class itu **wajib**
   punya semua 5 method di atas, gak boleh kurang satu pun. Ini dicek **compiler**, bukan
   runtime — kalau kelupaan, project langsung gagal build.
2. **Gak wajib buat yang manggil (caller)** — `AccountController` yang punya akses ke
   `IAuthService` **bebas manggil method mana aja yang dia butuh**, gak harus manggil
   kelimanya. Lihat `AccountController.cs`: action `Register` cuma manggil `RegisterAsync`,
   action `Login` cuma manggil `LoginAsync`. Gak ada aturan "harus pakai semua method di
   interface itu di tempat yang sama".

Analogi paling gampang: interface itu kayak **daftar menu restoran**. Menu itu "janji" — restoran
wajib bisa masakin semua yang ada di daftar itu kalau dipesan. Tapi pengunjung (caller) bebas
pesan 1 menu doang, gak wajib pesan semua isi menu. Jadi: **wajib diimplementasikan (semua),
bebas dipanggil (sebagian aja, sesuai kebutuhan).**

### Kenapa `AccountController` gak langsung pakai `AuthService` aja? Ribet nulis interface segala.

Constructor `AccountController(IAuthService authService)` minta **interface**, bukan
`AuthService` (class asli). Tiga alasan konkret kenapa ini kepake, bukan basa-basi:

1. **Testing tanpa database beneran.** Nanti kalau nulis unit test buat `AccountController`,
   lo bisa bikin `FakeAuthService : IAuthService` yang isinya cuma pura-pura sukses/gagal, tanpa
   nyentuh `AppDbContext`/SQL Server. `AccountController` gak akan tau bedanya, karena dia cuma
   kenal `IAuthService` — gak peduli implementasi aslinya `AuthService` atau `FakeAuthService`.
2. **Ganti implementasi tanpa ubah pemanggil.** Kalau suatu saat cara login berubah total
   (misal ganti dari cookie auth ke JWT, atau nambah login via Google), yang diubah cukup isi
   `AuthService`. `AccountController` **sama sekali gak perlu disentuh**, selama signature
   `IAuthService` tetap sama.
3. **Ini yang bikin Dependency Injection bisa jalan.** DI container (`Program.cs`) butuh
   "peta": kalau ada yang minta `IAuthService`, kasih instance `AuthService`. Itu persis baris
   ini:
   ```csharp
   builder.Services.AddScoped<IAuthService, AuthService>();
   ```
   Tanpa interface, gak ada "peta" yang bisa diganti-ganti — Controller bakal `new AuthService(...)`
   manual di setiap tempat yang butuh, dan alasan 1 & 2 di atas jadi gak mungkin dilakuin.

### DTO juga "blueprint", tapi blueprint apa?

**Iya, DTO juga blueprint — tapi blueprint DATA, bukan blueprint PERILAKU.** Ini beda mendasar
sama interface:

| | Interface | DTO |
|---|---|---|
| Isinya | Daftar **method** (perilaku/kemampuan) | Daftar **property** (data/field) |
| Jawab pertanyaan | "Apa yang **bisa dilakuin**?" | "Data ini **bentuknya kayak apa**?" |
| Contoh | `IAuthService` → "bisa `RegisterAsync`, `LoginAsync`, dst" | `RegisterDto` → "punya `Email`, `FullName`, `Password`, `ConfirmPassword`" |
| Ada logic di dalamnya? | Enggak (cuma signature) | Enggak (cuma property, murni data) |
| Bisa di-`new`? | **Enggak** (interface gak bisa di-instansiasi) | **Bisa** (`new RegisterDto()`) — ini class biasa |

```csharp
// DTOs/Auth/RegisterDto.cs — blueprint DATA, bukan blueprint method
public class RegisterDto
{
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}
```

Bedanya lagi sama `Model` (`Models/User.cs`), yang juga "blueprint data": `Model` itu blueprint
data buat **1 baris di tabel database** (makanya ada `PasswordHash`, `CreatedAt`, dst — kolom
yang gak ada urusannya sama form registrasi). `DTO` itu blueprint data buat **1 kebutuhan
transfer spesifik** antara `Controller` ↔ `Service` — makanya `RegisterDto` cuma punya 4 field
yang beneran diisi user di form, gak ada `PasswordHash` (itu di-generate di `AuthService`, user
gak pernah kirim hash, dia kirim password mentah).

### Ringkasan satu paragraf

**Interface** = janji soal **kemampuan** (method apa aja yang PASTI ada), wajib diimplementasikan
lengkap oleh class-nya, tapi bebas dipanggil sebagian aja oleh siapa pun yang pakai. **DTO**
= blueprint soal **bentuk data** buat 1 kebutuhan transfer, gak punya method/logic, dan beda dari
`Model` karena `Model` itu blueprint buat tabel database sedangkan `DTO` blueprint buat lalu-lintas
data antar layer.
