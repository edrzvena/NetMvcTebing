# Implementation Plan — Tebing

Panduan step-by-step buat bangun fitur di `docs/PRD.md`. Urutannya sengaja dibikin bertahap:
tiap tahap harus bisa jalan (build & migrate sukses) sebelum lanjut ke tahap berikutnya.

## 0. Status Saat Ini

Sudah ada:
- `Models/User.cs` — Id, Email, FullName, PasswordHash, ResetPasswordToken, PasswordResetTokenExpireAt, CreatedAt.
- `Data/AppDbContext.cs` — cuma `DbSet<User> Users`.
- Koneksi ke SQL Server (`DefaultConnection`) sudah di-wire di `Program.cs`.
- Migration `Initial` sudah ada.
- Folder `ViewModels/` sudah dibikin (masih kosong).

Belum ada: authentication flow, authorization/role, dan semua domain model inti (Mountain, Route, Booking, dst).

## 1. Struktur Folder yang Disarankan

```
Models/            <- entity EF Core (representasi tabel)
DTOs/              <- request/response object (input form, output API/View)
  Auth/
  Mountain/
  Booking/
  Payment/
  Review/
Services/          <- business logic (IBookingService, BookingService, dst)
Repositories/       <- (opsional) kalau mau pisahin akses data dari service
Controllers/
Views/
Common/
  Enums/           <- UserRole, BookingStatus, PaymentStatus, DifficultyLevel
```

Catatan: nama folder `ViewModels/` yang udah ada bisa dipakai buat model yang murni buat
kebutuhan tampilan (misal gabungan data buat 1 halaman), sedangkan `DTOs/` buat data
masuk/keluar dari Service/Controller. Kalau mau lebih simpel, boleh juga gabung jadi satu
folder aja (`DTOs/`) — nggak wajib pisah, yang penting konsisten.

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
    public bool IsActive { get; set; } = true;
}
```

**DTOs (`DTOs/Mountain/`):**
- `MountainListDto` — Id, Name, Province, HeightMeters, Difficulty, Status, ImageUrl, AverageRating
- `MountainDetailDto` — semua field + `List<RouteDto> Routes` + `List<ReviewDto> Reviews`
- `MountainCreateDto` / `MountainUpdateDto` — field yang boleh admin isi
- `RouteDto`, `RouteCreateDto`

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
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<BookingParticipant> Participants { get; set; } = new List<BookingParticipant>();
    public Payment? Payment { get; set; }
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
- `BookingStatusUpdateDto` — Status, (opsional) alasan reject — dipakai admin

**Business rule penting (taruh di `BookingService`, bukan di Controller):**
1. Validasi `Participants.Count == NumberOfPeople`.
2. Kalau `HikingRoute.DailyQuota` diisi, hitung total `NumberOfPeople` dari booking lain
   (status bukan `Rejected`/`Cancelled`) di tanggal & jalur yang sama, cek nggak melebihi kuota.
3. `TotalPrice` dihitung server-side (misal harga per orang × jumlah orang), jangan percaya
   angka dari client.

**Service:** `IBookingService` — Create, GetByUser, GetById, GetAllForAdmin (filter by mountain
milik admin itu), UpdateStatus.

**Controller:** `BookingController` (user: Create/MyBookings/Detail), admin actions guard role.

**Migration:** `Add-Migration AddBookingAndParticipant`.

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

**Service:** `IPaymentService` — Upload (simpan file ke `wwwroot/uploads/payments/` atau storage
lain, buat record Payment), Verify (admin, kalau `Verified` → set `Booking.Status = Approved`).

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

## Ringkasan Checklist Model & DTO

| Model | DTO terkait |
|---|---|
| `User` (+`Role`) | `RegisterDto`, `LoginDto`, `ForgotPasswordDto`, `ResetPasswordDto` |
| `Mountain` | `MountainListDto`, `MountainDetailDto`, `MountainCreateDto`, `MountainUpdateDto` |
| `HikingRoute` | `RouteDto`, `RouteCreateDto` |
| `Booking` | `BookingCreateDto`, `BookingListDto`, `BookingDetailDto`, `BookingStatusUpdateDto` |
| `BookingParticipant` | `ParticipantDto` |
| `Payment` | `PaymentUploadDto`, `PaymentVerifyDto` |
| `Review` | `ReviewCreateDto`, `ReviewDto` |

## Urutan Pengerjaan Disarankan

1. Enum + `User.Role` → migrate.
2. Auth (`AuthService`, `AccountController`, cookie auth di `Program.cs`) → **test**: register & login manual dulu sebelum lanjut.
3. `Mountain` + `HikingRoute` (model, DTO, service, controller admin+public) → migrate, seed 1-2 data gunung buat testing.
4. `Booking` + `BookingParticipant` → migrate, **test**: bikin booking dari akun Pendaki.
5. `Payment` → migrate, **test**: upload bukti, verifikasi dari akun Admin.
6. `Review` → migrate, **test**: alur penuh register → booking → approve → completed (manual set dulu kalau belum ada job otomatis) → review.
7. (Opsional) fitur v2 di Tahap 7.

Tiap nomor di atas idealnya jadi 1 commit/PR sendiri — biar gampang ditelusuri kalau ada bug,
dan gampang di-review satu-satu.
