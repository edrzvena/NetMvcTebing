# PRD — Tebing

**Tebing** adalah platform web untuk pendaki gunung (muncak) di Indonesia. Fokus utamanya:
cari info gunung & jalur pendakian, booking/reservasi pendakian (semacam SIMAKSI online),
kelola data rombongan, dan kasih ulasan setelah selesai naik.

Nama "Tebing" dipilih karena masih satu tema sama dunia pendakian/panjat.

---

## 1. Latar Belakang & Masalah

Saat ini banyak pendaki masih daftar SIMAKSI/registrasi pendakian manual (WA, telepon basecamp,
datang langsung). Ini bikin:

- Susah cek sisa kuota pendakian per tanggal.
- Data peserta rombongan (KTP, kontak darurat) sering hilang/nggak lengkap.
- Nggak ada histori pendakian & review dari pendaki lain buat referensi gunung/jalur mana yang cocok.
- Basecamp/pengelola susah rekap siapa aja yang naik di suatu tanggal.

**Tebing** menjawab itu dengan sistem booking pendakian online + data gunung/jalur yang terpusat.

---

## 2. Visi Produk

Jadi platform satu pintu buat pendaki: cari gunung → pilih jalur → booking & daftar rombongan →
bayar → naik → kasih review. Sementara pengelola basecamp bisa approve booking & pantau kuota
dari dashboard admin.

---

## 3. Target Pengguna & Role

| Role | Deskripsi |
|---|---|
| **Pendaki** (`User`) | User umum. Cari info gunung, booking pendakian, isi data rombongan, upload bukti bayar, kasih review setelah selesai. |
| **Admin Basecamp** (`Admin`) | Kelola data gunung & jalur miliknya, approve/reject booking, verifikasi pembayaran, atur kuota harian. |
| **Super Admin** (opsional, v2) | Kelola semua data gunung, kelola akun Admin Basecamp, lihat laporan global. |

MVP: cukup 2 role dulu — `Pendaki` dan `Admin`. Super Admin bisa nyusul kalau memang butuh
multi-basecamp yang diverifikasi terpisah.

---

## 4. Ruang Lingkup

### MVP (harus ada)
1. Autentikasi (register, login, lupa password) — field-nya sudah ada di `User`, tinggal alur & hashing-nya.
2. Data master Gunung (list & detail, dikelola Admin).
3. Data master Jalur Pendakian per gunung (misal Gunung Gede: via Cibodas, via Gunung Putri).
4. Booking pendakian: pilih gunung + jalur + tanggal + jumlah orang.
5. Data peserta rombongan per booking (nama, NIK, kontak darurat).
6. Pembayaran sederhana: upload bukti transfer, admin verifikasi manual.
7. Riwayat booking per user (status: pending/approved/rejected/completed/cancelled).
8. Review & rating gunung, hanya bisa oleh user yang bookingnya sudah `completed`.
9. Dashboard admin: approve/reject booking, verifikasi pembayaran, lihat kuota tanggal tertentu.

### Out of scope untuk MVP (v2/future)
- Payment gateway (Midtrans/Xendit) — MVP pakai transfer manual + upload bukti dulu.
- Peta jalur interaktif & data pos/checkpoint detail per jalur.
- Info cuaca real-time (integrasi API eksternal).
- Sewa alat/perlengkapan pendakian.
- Notifikasi email/WhatsApp otomatis.
- Blog/artikel tips pendakian.
- Multi-basecamp dengan Super Admin terpisah.

---

## 5. Fitur & User Stories

### 5.1 Autentikasi
- Sebagai pendaki, saya bisa daftar akun dengan email & password.
- Sebagai pendaki, saya bisa login dan tetap login (cookie/session).
- Sebagai pendaki, saya bisa reset password lewat token (field sudah ada di `User`).

### 5.2 Data Gunung & Jalur
- Sebagai pendaki, saya bisa lihat daftar gunung (nama, lokasi, ketinggian, tingkat kesulitan, status buka/tutup).
- Sebagai pendaki, saya bisa lihat detail gunung: deskripsi, daftar jalur pendakian, rating rata-rata.
- Sebagai admin, saya bisa CRUD data gunung & jalur pendakian.

### 5.3 Booking Pendakian
- Sebagai pendaki, saya bisa pilih gunung, jalur, tanggal naik, dan jumlah rombongan.
- Sebagai pendaki, saya harus isi data tiap anggota rombongan (nama, NIK, no. HP, kontak darurat) sebelum booking bisa disubmit.
- Sebagai pendaki, saya bisa lihat status booking saya (pending/approved/rejected/completed/cancelled).
- Sebagai admin, saya bisa lihat semua booking masuk untuk gunung yang saya kelola, dan approve/reject.
- Sistem harus cegah booking melebihi kuota harian jalur tsb (kalau kuota diaktifkan).

### 5.4 Pembayaran
- Sebagai pendaki, setelah booking dibuat saya upload bukti transfer.
- Sebagai admin, saya verifikasi pembayaran (valid/tidak) sebelum booking difinalkan jadi `approved`.

### 5.5 Review & Rating
- Sebagai pendaki, setelah booking saya berstatus `completed`, saya bisa kasih rating (1–5) & komentar untuk gunung tsb.
- Sebagai pendaki lain, saya bisa baca review sebelum memutuskan naik ke gunung itu.

---

## 6. Alur Utama (Happy Path)

```
Register/Login
   │
   ▼
Lihat daftar Gunung → pilih Gunung → lihat detail & Jalur
   │
   ▼
Buat Booking (pilih Jalur, tanggal, jumlah orang)
   │
   ▼
Isi data Peserta Rombongan (sejumlah "jumlah orang")
   │
   ▼
Submit booking → status "Pending"
   │
   ▼
Upload bukti pembayaran
   │
   ▼
Admin verifikasi pembayaran & approve booking → status "Approved"
   │
   ▼
Setelah tanggal pendakian lewat → Admin/sistem set status "Completed"
   │
   ▼
User kasih Review & Rating gunung tsb
```

---

## 7. Entitas Data Utama (ringkas)

Detail model, field, dan DTO ada di `docs/IMPLEMENTATION_PLAN.md`. Ringkasnya:

- **User** (sudah ada) — perlu tambah `Role`.
- **Mountain** (Gunung)
- **HikingRoute** (Jalur Pendakian, milik satu Mountain)
- **Booking** (Reservasi, milik satu User + Mountain + HikingRoute)
- **BookingParticipant** (Peserta rombongan, milik satu Booking)
- **Payment** (Pembayaran, milik satu Booking)
- **Review** (Ulasan, milik satu User + Mountain, terkait satu Booking)

---

## 8. Non-Functional Requirements

- **Validasi**: semua input dari form/API divalidasi di server (Data Annotations minimal), jangan percaya validasi client-side aja.
- **Keamanan**: password di-hash (jangan pernah simpan plain text — pakai `PasswordHasher<T>` bawaan ASP.NET Core atau BCrypt). Endpoint admin harus di-guard authorization, bukan cuma disembunyikan di UI.
- **Konsistensi status**: perubahan status booking/payment harus melalui alur yang jelas (state machine sederhana), nggak asal update field.
- **Skalabilitas MVP**: nggak perlu microservices dulu, cukup monolith MVC + EF Core rapi per layer (Model/DTO/Service/Controller).

---

## 9. Tech Stack (sudah berjalan)

- ASP.NET Core MVC — .NET 10
- Entity Framework Core 10 + SQL Server
- Razor Views + Bootstrap (bawaan template)

---

## 10. Metrik Sukses (buat portofolio/belajar, bukan produksi nyata)

- Bisa demo end-to-end: register → booking → approve → review, tanpa error.
- Struktur kode rapi: pemisahan Model / DTO / Service / Controller jelas, gampang dijelasin kalau ditanya (misalnya pas interview atau review project).

---

## 11. Asumsi & Hal yang Perlu Dikonfirmasi

Beberapa asumsi yang gue pakai supaya PRD ini bisa langsung jalan — koreksi kalau salah:

1. Satu gunung bisa punya banyak jalur pendakian, tiap jalur bisa dibooking terpisah.
2. Kuota harian itu opsional buat MVP awal (bisa ditambah belakangan tanpa ubah struktur inti).
3. Pembayaran manual (upload bukti transfer) dulu, belum integrasi payment gateway.
4. Role cukup `Pendaki` & `Admin` dulu, belum multi-tenant basecamp.
5. Belum butuh notifikasi email/WA otomatis di MVP.
