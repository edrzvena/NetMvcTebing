# Implementation Plan — Tebing

Panduan step-by-step buat bangun fitur di `docs/PRD.md`. Urutannya sengaja dibikin bertahap:
tiap tahap harus bisa jalan (build & migrate sukses) sebelum lanjut ke tahap berikutnya.

## 0. Status Saat Ini

Sudah selesai:
- **Setup Awal** — repository pattern (`IRepository<T>` + repository per entity).
- **Tahap 1** — enum di `Common/Enums/` + `User.Role` (migration `AddUserRole`).
- **Tahap 2** — autentikasi cookie: register, login, logout, forgot/reset password (`AccountController`).
- **Tahap 3** — `Mountain` + `HikingRoute` CRUD, admin di-guard `[Authorize(Roles = "Admin")]`
  (migration `AddMountainAndRoute`).
- **Tahap 4** — `Booking` + `BookingParticipant`, cek kuota, state machine status, halaman pendaki
  & admin (migration `AddBookingAndParticipant`, sekalian nambah `HikingRoute.PricePerPerson`).

Belum: Tahap 5 (Payment), Tahap 6 (Review), seed data (`Data/Seed/`), dan Tahap 7 opsional.

## 1. Struktur Folder yang Disarankan

```
Models/                 <- entity EF Core (representasi tabel), murni data + relasi, tanpa logic
DTOs/                   <- data masuk/keluar antara Controller <-> Service
  Auth/
  Mountain/
  Booking/
  Payment/
  Review/
ViewModels/             <- data khusus kebutuhan 1 Razor View (lihat penjelasan di bawah)
  Mountain/
  Booking/
Repositories/
  Interfaces/           <- IRepository<T>, IUserRepository, IMountainRepository, dst
  Implementations/      <- Repository<T>, UserRepository, MountainRepository, dst
Services/
  Interfaces/           <- IAuthService, IMountainService, IBookingService, dst
  Implementations/      <- AuthService, MountainService, BookingService, dst
Mappings/               <- extension method .ToDto()/.ToViewModel() per entity, mis. MountainMappings.cs
Exceptions/             <- NotFoundException, BusinessRuleException, dst (dilempar dari Service)
Middlewares/            <- ExceptionHandlingMiddleware.cs (nangkep exception di atas, jadi response rapi)
Constants/              <- Roles.cs, FileUploadSettings.cs (hindari magic string/number)
Helpers/                <- FileStorageHelper.cs (dipakai bareng MountainService & PaymentService)
Extensions/             <- ServiceCollectionExtensions.cs (rapihin registrasi DI di Program.cs)
Controllers/
Views/
Common/
  Enums/                <- UserRole, BookingStatus, PaymentStatus, DifficultyLevel, MountainStatus
Data/
  AppDbContext.cs
  Configurations/       <- IEntityTypeConfiguration<T> per entity, biar OnModelCreating gak jadi 1 method raksasa
  Seed/                 <- DbSeeder.cs (role awal + sample data gunung buat dev/testing)
Migrations/
```

**Catatan `Helpers/`:** cuma buat logic yang beneran dipakai berulang di >1 tempat (contoh:
generate nama file unik dipakai `MountainService` & `PaymentService`). Kalau suatu logic cuma
dipakai 1 tempat, taruh di situ aja — jangan jadiin folder ini tempat buang semua hal yang
bingung ditaruh mana.

**Sengaja belum ditambah** karena belum ada kebutuhan konkretnya di scope MVP:
- `Filters/` (custom action filter) — `[Authorize]` bawaan + `Exceptions/`+`Middlewares/` udah cukup.
- Project test terpisah (`Tebing.Tests`) — valid ditambah nanti buat unit test business rule
  kuota di `BookingService`, tapi itu **project sendiri** di solution, bukan folder di dalam `Tebing/`.

**Pakai `Repositories/`** — final: **1 database (SQL Server) aja**, ide multi-provider
(SQL Server + PostgreSQL) di-drop supaya lebih simpel. Repository tetap dipertahankan, tapi
alasannya sekarang murni soal kerapihan & testability (bukan syarat teknis buat ganti provider,
seperti yang sempat diklaim sebelumnya dan sudah dikoreksi):
- `Service` cuma manggil `IXxxRepository`, gak pernah sentuh `AppDbContext`/`DbSet` langsung —
  pemisahan tanggung jawab jadi jelas: Repository = akses data, Service = business logic.
- Gampang di-mock pas nulis unit test buat business rule (misal validasi kuota booking di
  `BookingService`) tanpa nyentuh database beneran.

### DTO vs ViewModel — kapan pakai yang mana

Dua-duanya tetap dibutuhkan, beda peran:

- **DTO** — kontrak data antara `Controller` dan `Service`. Isinya murni data (input create/update,
  atau hasil query), gak peduli soal tampilan. Contoh: `BookingCreateDto` cuma berisi
  `MountainId, HikingRouteId, ClimbDate, List<ParticipantDto>`.
- **ViewModel** — bentuk data yang dibutuhkan **satu halaman Razor View**, sering kali gabungan
  dari satu/lebih DTO **plus** data tambahan yang cuma dipakai UI (dropdown `SelectListItem`,
  flag `bool CanReview`, dsb). Contoh: `BookingCreateViewModel` = `BookingCreateDto` +
  `List<SelectListItem> MountainOptions` + `List<SelectListItem> RouteOptions`.

Alur mapping: `View (form) → ViewModel → Controller mapping ke DTO → Service`. Kalau suatu
halaman cuma nampilin data mentah tanpa tambahan apa pun (misal halaman riwayat booking),
langsung pakai DTO-nya di View — gak usah dipaksain bikin ViewModel yang isinya cuma nge-wrap
DTO tanpa nilai tambah.

---

## Setup Awal — Repository Pattern (SQL Server)

Kerjain ini duluan, sebelum Tahap 1, karena semua tahap fitur setelahnya bakal bikin
`IXxxRepository` juga (bukan cuma Service langsung ke `AppDbContext`).

**1. Generic Repository:**

```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
    Task SaveChangesAsync();
}

public class Repository<T>(AppDbContext context) : IRepository<T> where T : class
{
    protected readonly AppDbContext Context = context;
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public async Task<T?> GetByIdAsync(int id) => await DbSet.FindAsync(id);
    public async Task<List<T>> GetAllAsync() => await DbSet.ToListAsync();
    public async Task AddAsync(T entity) => await DbSet.AddAsync(entity);
    public void Update(T entity) => DbSet.Update(entity);
    public void Remove(T entity) => DbSet.Remove(entity);
    public async Task SaveChangesAsync() => await Context.SaveChangesAsync();
}
```

Tiap entity nambahin interface turunan buat query spesifik, contoh:

```csharp
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}

public class UserRepository(AppDbContext context) : Repository<User>(context), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email) =>
        await DbSet.FirstOrDefaultAsync(u => u.Email == email);
}
```

Pola yang sama dipakai buat `IMountainRepository`, `IHikingRouteRepository`,
`IBookingRepository` (+ `GetPendingByMountainAsync`, dst), `IPaymentRepository`,
`IReviewRepository`. Service depend ke interface ini, contoh
`BookingService(IBookingRepository bookingRepo, IHikingRouteRepository routeRepo)`.

**2. Database tetap 1 — SQL Server saja**

Keputusan final: ide multi-provider (SQL Server + PostgreSQL) di-drop, cukup 1 database biar
gak nambah kompleksitas. `AppDbContext` tetap seperti yang sudah ada sekarang (`Data/AppDbContext.cs`,
non-abstract, langsung dipakai lewat `AddDbContext<AppDbContext>()` seperti di `Program.cs`
saat ini) — gak ada perubahan di bagian ini.

**3. Registrasi DI di `Program.cs`:**

```csharp
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMountainRepository, MountainRepository>();
// ...dst untuk tiap repository & service
```

---

## Tahap 1 — Enum & Role dasar

Ini fondasi, dipakai di banyak model lain.

**Buat `Common/Enums/`:**
- `UserRole` : `Pendaki`, `Admin`
- `BookingStatus` : `Pending`, `Approved`, `Rejected`, `Completed`, `Cancelled`
- `PaymentStatus` : `Pending`, `Verified`, `Rejected`
- `DifficultyLevel` : `Easy`, `Medium`, `Hard`
- `MountainStatus` : `Open`, `Closed`

**Ubah `Models/User.cs`:**
- Tambah `public UserRole Role { get; set; } = UserRole.Pendaki;`

**Migration:** `Add-Migration AddUserRole` → `Update-Database`.

---

## Tahap 2 — Autentikasi

**DTOs (`DTOs/Auth/`):**
- `RegisterDto` — Email, FullName, Password, ConfirmPassword
- `LoginDto` — Email, Password
- `ForgotPasswordDto` — Email
- `ResetPasswordDto` — Token, NewPassword, ConfirmPassword

**Repository:** `IUserRepository` — `GetByEmailAsync`, `AddAsync`, `Update` (dipakai `AuthService`).

**Service:**
- `IAuthService` / `AuthService` — Register (hash password pakai `PasswordHasher<User>`), Login
  (verifikasi hash + buat cookie auth), GenerateResetToken, ResetPassword.

**Setup di `Program.cs`:**
- `AddAuthentication(CookieAuthenticationScheme...).AddCookie(...)`
- `app.UseAuthentication()` sebelum `app.UseAuthorization()`.

**Controller:** `AccountController` — Register, Login, Logout, ForgotPassword, ResetPassword
(action GET+POST masing-masing, sesuai pola MVC).

Setelah tahap ini: user bisa daftar & login, dan `[Authorize]` / `[Authorize(Roles = "Admin")]`
sudah bisa dipakai di controller lain.

---

## Tahap 3 — Master Data Gunung & Jalur

**Models:**

```csharp
public class Mountain
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Province { get; set; }
    public int HeightMeters { get; set; }          // mdpl
    public DifficultyLevel Difficulty { get; set; }
    public MountainStatus Status { get; set; } = MountainStatus.Open;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<HikingRoute> Routes { get; set; } = new List<HikingRoute>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}

public class HikingRoute
{
    public int Id { get; set; }
    public int MountainId { get; set; }
    public Mountain Mountain { get; set; } = null!;

    public string Name { get; set; }               // "Via Cibodas"
    public decimal DistanceKm { get; set; }
    public string? EstimatedDuration { get; set; }  // "2 hari 1 malam"
    public int? DailyQuota { get; set; }             // null = tanpa batas kuota
    public decimal PricePerPerson { get; set; }      // decimal(12,2), ditambah di Tahap 4
    public bool IsActive { get; set; } = true;
}
```

> `PricePerPerson` baru ditambah pas Tahap 4 (lewat migration `AddBookingAndParticipant`),
> karena `Booking.TotalPrice` butuh harga per orang. Ikut masuk ke `RouteDto`, `RouteCreateDto`,
> form tambah/edit jalur, dan tabel jalur di detail gunung.

**DTOs (`DTOs/Mountain/`):**
- `MountainListDto` — Id, Name, Province, HeightMeters, Difficulty, Status, ImageUrl, AverageRating
- `MountainDetailDto` — semua field + `List<RouteDto> Routes` + `List<ReviewDto> Reviews`
- `MountainCreateDto` / `MountainUpdateDto` — field yang boleh admin isi
- `RouteDto`, `RouteCreateDto`

**Repository:** `IMountainRepository`, `IHikingRouteRepository` (CRUD dasar dari `IRepository<T>`
+ query tambahan kalau perlu, misal `GetActiveRoutesByMountainIdAsync`).

**Service:** `IMountainService` — GetAll (dengan filter provinsi/difficulty opsional), GetById,
Create, Update, Delete (soft-delete via `IsActive`/`Status` lebih aman daripada hard delete).

**Controller:** `MountainController` (public: Index/Detail) + admin actions di-guard
`[Authorize(Roles = "Admin")]` (bisa di controller sama dengan area terpisah, atau
`AdminMountainController` kalau mau pisah).

**Migration:** `Add-Migration AddMountainAndRoute`.

---

## Tahap 4 — Booking & Peserta Rombongan

**Models:**

```csharp
public class Booking
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int MountainId { get; set; }
    public Mountain Mountain { get; set; } = null!;
    public int HikingRouteId { get; set; }
    public HikingRoute HikingRoute { get; set; } = null!;

    public DateOnly ClimbDate { get; set; }
    public int NumberOfPeople { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public decimal TotalPrice { get; set; }          // snapshot harga saat booking dibuat
    public string? RejectionReason { get; set; }     // max 500, cuma diisi kalau Rejected
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<BookingParticipant> Participants { get; set; } = new List<BookingParticipant>();
    public Payment? Payment { get; set; }            // ditambah di Tahap 5
}

public class BookingParticipant
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    public string FullName { get; set; }
    public string NIK { get; set; }
    public string PhoneNumber { get; set; }
    public string EmergencyContactName { get; set; }
    public string EmergencyContactPhone { get; set; }
}
```

**DTOs (`DTOs/Booking/`):**
- `BookingCreateDto` — MountainId, HikingRouteId, ClimbDate, `List<ParticipantDto> Participants`
  (jumlah item di list ini = `NumberOfPeople`, jadi field itu nggak perlu diinput manual terpisah).
- `ParticipantDto` — FullName, NIK, PhoneNumber, EmergencyContactName, EmergencyContactPhone
- `BookingListDto` — Id, MountainName, RouteName, ClimbDate, Status, NumberOfPeople (buat "riwayat booking saya")
- `BookingDetailDto` — semua di atas + `List<ParticipantDto>` + info payment
- `BookingStatusUpdateDto` — BookingId, Status, RejectionReason (opsional) — dipakai admin

**ViewModel:** `ViewModels/Booking/BookingCreateViewModel` = `BookingCreateDto Booking` +
`MountainName` + `List<SelectListItem> RouteOptions` (jalur aktif aja). Booking dimulai dari
tombol "Booking Pendakian" di detail gunung, jadi gunungnya udah fix dan cuma jalur yang dipilih.

**EF config (`Data/Configurations/BookingConfiguration.cs`):** FK `Booking` → User/Mountain/HikingRoute
pakai `DeleteBehavior.Restrict`. SQL Server nolak multiple cascade path
(Mountain → HikingRoute → Booking dan Mountain → Booking), dan histori booking memang gak boleh
ikut kehapus. `BookingParticipant` tetap cascade ikut `Booking`. Ada index `(HikingRouteId, ClimbDate)`
buat query cek kuota. `AppDbContext.OnModelCreating` pakai `ApplyConfigurationsFromAssembly`.

**Business rule penting (taruh di `BookingService`, bukan di Controller):**
1. `NumberOfPeople` = `Participants.Count` (gak diinput terpisah), minimal 1, NIK gak boleh
   dobel dalam 1 rombongan.
2. Jalur harus aktif & milik gunung yang dipilih, gunung harus `Open`, `ClimbDate` gak boleh
   sebelum hari ini.
3. Kalau `HikingRoute.DailyQuota` diisi, hitung total `NumberOfPeople` dari booking lain
   (status bukan `Rejected`/`Cancelled`) di tanggal & jalur yang sama, cek nggak melebihi kuota.
4. `TotalPrice = HikingRoute.PricePerPerson × NumberOfPeople`, dihitung server-side, jangan
   percaya angka dari client.
5. Perubahan status lewat state machine (`AllowedTransitions` di `BookingService`):

   | Dari | Boleh ke |
   |---|---|
   | `Pending` | `Approved`, `Rejected`, `Cancelled` |
   | `Approved` | `Completed`, `Cancelled` |
   | `Rejected` / `Completed` / `Cancelled` | — (status akhir) |

   `Completed` baru boleh diset mulai tanggal `ClimbDate`. Pendaki cuma boleh batalin booking
   miliknya sendiri yang masih `Pending`. Yang `Approved` dibatalin lewat admin.

**Repository:** `IBookingRepository` — `GetByIdWithDetailsAsync` (include User, Mountain,
HikingRoute, Participants), `GetByUserIdAsync`, `GetFilteredAsync(mountainId, status, climbDate)`
(buat admin), `GetBookedPeopleCountAsync(hikingRouteId, climbDate)` (buat cek kuota, `DailyQuota`-nya
diambil lewat `IHikingRouteRepository`).

**Service:** `IBookingService` — `CreateAsync(userId, dto)`, `GetByUserAsync`, `GetByIdAsync`
(sekalian isi `AllowedNextStatuses` buat tombol aksi admin), `GetAllForAdminAsync`
(filter gunung/status/tanggal), `UpdateStatusAsync`, `CancelByUserAsync`. Hasil create/update
pakai `record BookingResult(bool Success, string? Error, int? BookingId)`, polanya sama kayak `AuthResult`.

**Controller:** `BookingController` (class-level `[Authorize]`):
- Pendaki: `Create` (GET+POST, tambah/hapus peserta pakai JS), `MyBookings`, `Detail`, `Cancel`.
  `Detail` booking orang lain → `NotFound` (bukan `Forbid`), biar gak bocor info ID booking mana yang ada.
- Admin (`[Authorize(Roles = "Admin")]`): `Manage` (filter + total pendaki per tanggal), `UpdateStatus`.

Navbar dapet link "Booking Saya" (login) & "Kelola Booking" (admin).

**Catatan:** di MVP belum ada relasi "gunung milik admin X", jadi admin lihat semua booking
(bisa difilter per gunung). Admin juga masih approve booking langsung. Mulai Tahap 5, approve
idealnya lewat verifikasi pembayaran.

**Known limitation:** cek kuota belum aman dari race condition. 2 submit barengan di jalur &
tanggal yang sama bisa lolos dua-duanya dan bikin total ngelewatin kuota. Masih oke buat MVP.
Kalau mau rapet, bungkus cek + insert dalam transaction `Serializable`.

**Migration:** `Add-Migration AddBookingAndParticipant` (termasuk kolom `HikingRoutes.PricePerPerson`).

---

## Tahap 5 — Pembayaran

**Model:**

```csharp
public class Payment
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    public decimal Amount { get; set; }
    public string PaymentProofUrl { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime? VerifiedAt { get; set; }
}
```

**DTOs (`DTOs/Payment/`):**
- `PaymentUploadDto` — BookingId, file bukti transfer (`IFormFile`), Amount
- `PaymentVerifyDto` — Status (Verified/Rejected)

**Repository:** `IPaymentRepository` — `AddAsync`, `GetByBookingIdAsync`.

**Service:** `IPaymentService` — Upload (simpan file ke `wwwroot/uploads/payments/` atau storage
lain, buat record Payment), Verify (admin, kalau `Verified` → set `Booking.Status = Approved`
lewat `IBookingRepository`). `Amount` bisa diambil dari `Booking.TotalPrice`. Transisi
`Pending → Approved` tetap harus lolos state machine di `BookingService`, dan pertimbangkan
nyembunyiin tombol "Approve" manual di halaman admin begitu alur payment jalan.

**Catatan keamanan:** validasi tipe & ukuran file upload (jpg/png/pdf, max misal 2MB), jangan
simpan file langsung pakai nama asli dari user (rawan path traversal) — generate nama unik
(GUID) sebelum disimpan.

**Migration:** `Add-Migration AddPayment`.

---

## Tahap 6 — Review & Rating

**Model:**

```csharp
public class Review
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int MountainId { get; set; }
    public Mountain Mountain { get; set; } = null!;
    public int BookingId { get; set; }              // bukti pernah booking & completed
    public Booking Booking { get; set; } = null!;

    public int Rating { get; set; }                  // 1-5
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

**DTOs (`DTOs/Review/`):**
- `ReviewCreateDto` — BookingId, Rating, Comment
- `ReviewDto` — UserFullName, Rating, Comment, CreatedAt (buat ditampilkan di detail gunung)

**Business rule:** cuma bisa create review kalau `Booking.Status == Completed`,
`Booking.UserId == current user`, dan belum pernah review buat booking itu (unique constraint
`BookingId` di tabel Review, 1 review per booking).

**Repository:** `IReviewRepository` — `AddAsync`, `GetByMountainIdAsync`, `ExistsForBookingAsync`
(buat cek 1 review per booking).

**Service:** `IReviewService` — Create, GetByMountain (buat halaman detail gunung + hitung average rating).

**Migration:** `Add-Migration AddReview`.

---

## Tahap 7 (Opsional / v2)

Kalau MVP di atas udah jalan, baru pertimbangkan:
- `MountainPhoto` — galeri foto summit dari user.
- `CheckPoint`/`Pos` per `HikingRoute` (Order, ElevationMeters, DistanceFromStart).
- Payment gateway (Midtrans Snap) gantiin upload manual.
- Background job set `Booking.Status = Completed` otomatis H+1 setelah `ClimbDate`
  (pakai `IHostedService` atau library seperti Hangfire/Quartz).
- Notifikasi email pas status booking berubah (`MailKit`/`SendGrid`).

---

## Ringkasan Checklist Model, Repository & DTO

| Model | Repository | DTO terkait |
|---|---|---|
| `User` (+`Role`) | `IUserRepository` | `RegisterDto`, `LoginDto`, `ForgotPasswordDto`, `ResetPasswordDto` |
| `Mountain` | `IMountainRepository` | `MountainListDto`, `MountainDetailDto`, `MountainCreateDto`, `MountainUpdateDto` |
| `HikingRoute` | `IHikingRouteRepository` | `RouteDto`, `RouteCreateDto` |
| `Booking` | `IBookingRepository` | `BookingCreateDto`, `BookingListDto`, `BookingDetailDto`, `BookingStatusUpdateDto` (+ `BookingCreateViewModel`) |
| `BookingParticipant` | *(lewat `IBookingRepository`)* | `ParticipantDto` |
| `Payment` | `IPaymentRepository` | `PaymentUploadDto`, `PaymentVerifyDto` |
| `Review` | `IReviewRepository` | `ReviewCreateDto`, `ReviewDto` |

## Urutan Pengerjaan Disarankan

1. Setup awal: `IRepository<T>` generic + repository per entity (lihat "Setup Awal" di atas) → **test**: app tetap `dotnet run` jalan normal pakai `AppDbContext`/SQL Server yang sudah ada, gak ada perubahan koneksi.
2. Enum + `User.Role` + `IUserRepository` → migrate.
3. Auth (`AuthService` pakai `IUserRepository`, `AccountController`, cookie auth di `Program.cs`) → **test**: register & login manual dulu sebelum lanjut.
4. `Mountain` + `HikingRoute` (model, repository, DTO, service, controller admin+public) → migrate, seed 1-2 data gunung buat testing.
5. `Booking` + `BookingParticipant` → migrate, **test**: bikin booking dari akun Pendaki (termasuk cek validasi kuota).
6. `Payment` → migrate, **test**: upload bukti, verifikasi dari akun Admin.
7. `Review` → migrate, **test**: alur penuh register → booking → approve → completed (manual set dulu kalau belum ada job otomatis) → review.
8. (Opsional) fitur v2 di Tahap 7.

Tiap nomor di atas idealnya jadi 1 commit/PR sendiri — biar gampang ditelusuri kalau ada bug,
dan gampang di-review satu-satu.

---

## Riwayat Keputusan

- Sempat dipertimbangkan setup 2 database provider (SQL Server + PostgreSQL, switchable lewat
  config). Sudah diriset ke dokumentasi resmi EF Core dan ketemu pendekatan yang benar
  (`AppDbContext` abstract + subclass per provider + `AddDbContext<TContextService, TContextImplementation>`),
  tapi **keputusan akhir: di-drop, cukup 1 database (SQL Server)** biar project gak nambah
  kompleksitas yang belum perlu. Kalau suatu saat mau dihidupkan lagi, riset di atas masih valid
  — sumbernya: [Migrations with Multiple Providers — EF Core, Microsoft Learn](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/providers),
  [AddDbContext Method — Microsoft Learn API Reference](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.entityframeworkservicecollectionextensions.adddbcontext?view=efcore-10.0),
  [dotnet/efcore Issue #23023](https://github.com/dotnet/efcore/issues/23023),
  [Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0 — NuGet Gallery](https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL/).
- Repository pattern tetap dipertahankan meskipun cuma 1 database — alasannya murni kerapihan
  (Service gak nyentuh `AppDbContext` langsung) & testability, bukan kebutuhan teknis provider.
- Tahap 4: harga disimpan per jalur (`HikingRoute.PricePerPerson`), bukan per gunung, karena
  tarif SIMAKSI bisa beda tiap jalur. `Booking.TotalPrice` jadi snapshot, jadi kalau admin ubah
  harga jalur nanti, booking lama gak ikut berubah.
- Tahap 4: alasan reject disimpan di `Booking.RejectionReason` (bukan tabel terpisah), cukup buat MVP.
