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

## 3. Contoh Alur Nyata (End-to-End)

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

---

## 4. "Namanya beneran harus itu?"

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
