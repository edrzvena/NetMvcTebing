# UI/UX Guide — Tebing

Dokumen ini beda dari dokumen lain di `docs/`:
- `PRD.md` = mau bikin fitur apa.
- `IMPLEMENTATION_PLAN.md` = urutan ngerjain backend & contoh kode tiap tahap.
- `ARCHITECTURE.md` = kenapa folder dibagi kayak gitu & alur request.
- **`UI_UX.md` (ini) = tampilan & pengalaman pakai: warna, tipografi, komponen, layout tiap
  halaman, dan aturan biar UI-nya konsisten.**

Baca ini sebelum bikin/ubah file `.cshtml` atau `site.css`.

---

## 1. Arah Desain

**Clean, tenang, fokus ke data.** Tebing itu alat buat booking, bukan landing page startup.
Orang buka Tebing buat: cari gunung → cek jalur, kuota, harga → booking → pantau status. Tampilan
harus bikin informasi itu cepet kebaca, bukan pamer efek.

Referensinya: tampilan web pemerintahan/booking yang rapi (kayak web tiket kereta yang bagus),
atau dashboard admin yang gak norak. Polos tapi tertata.

### Prinsip

1. **Data dulu, dekorasi belakangan.** Nama gunung, tanggal, status, harga = elemen paling
   menonjol. Hiasan cuma boleh kalau bantu baca.
2. **Satu warna aksen.** Hijau tua dipakai buat aksi utama & link. Warna lain cuma buat status
   (sukses/peringatan/bahaya), bukan buat dekorasi.
3. **Hirarki lewat ukuran, ketebalan, dan jarak** — bukan lewat warna-warni atau bayangan.
4. **Konsisten.** Status yang sama selalu badge yang sama, aksi yang sama selalu tombol yang sama,
   di halaman mana pun.
5. **Satu aksi utama per layar.** Cuma 1 tombol `btn-primary` per halaman/section. Sisanya
   `btn-outline-secondary` atau link.

### Yang DIHINDARI (anti "AI slop")

| Jangan | Kenapa / gantinya |
|---|---|
| Gradient (background, tombol, teks) | Warna solid aja. |
| Glassmorphism, blur, backdrop transparan | Background solid putih/abu muda. |
| Bayangan tebal di mana-mana (`shadow-lg`) | Pakai border tipis 1px. Bayangan cuma buat dropdown & modal (bawaan Bootstrap). |
| Emoji di UI (🏔️✨🚀) | Pakai teks. Kalau butuh ikon, Bootstrap Icons secukupnya. |
| Hero section raksasa + ilustrasi 3D/blob | Header teks biasa, langsung ke konten. |
| Copy lebay ("Taklukkan puncak impianmu! 🚀") | Kalimat lugas yang ngasih info. |
| Rounded berlebihan (pill di semua tempat, radius 24px) | Radius kecil & konsisten (lihat token). |
| Animasi masuk/hover yang lebay (bounce, scale, parallax) | Transisi warna standar Bootstrap aja. |
| Card di dalam card di dalam card | Maksimal 1 lapis card. Di dalamnya pakai garis pemisah/jarak. |
| Warna dipakai cuma buat hiasan (badge warna-warni tanpa arti) | Warna = arti (status). |
| Teks abu-abu muda yang susah kebaca | Teks sekunder tetap kontras cukup (lihat token). |
| Icon tanpa label buat aksi penting | Tombol selalu ada teksnya. |

---

## 2. Batasan Teknis

- **Bootstrap 5.3.3** yang udah ada di `wwwroot/lib/bootstrap` — gak ganti framework, gak pakai
  Tailwind, gak ada build step (SASS/npm). Kustomisasi lewat **CSS variable** di `wwwroot/css/site.css`.
- **Font:** system font stack bawaan Bootstrap (Segoe UI di Windows, SF di Mac, Roboto di Android).
  Gak perlu Google Fonts — lebih cepet & tetap bersih.
- **Ikon (opsional):** Bootstrap Icons, di-*self-host* di `wwwroot/lib/bootstrap-icons` (bukan CDN).
  Dipakai hemat: cuma di samping teks (contoh: ikon kalender di samping tanggal), gak berdiri sendiri.
- **JavaScript:** cuma yang udah ada (jQuery + validation, Bootstrap bundle). Interaksi kecil pakai
  vanilla JS di view terkait (kayak tambah/hapus peserta di `Booking/Create`).
- **Light mode aja** dulu. Dark mode bukan prioritas.
- `Views/Shared/_Layout.cshtml.css` (CSS bawaan template, warna biru `#1b6ec2`) **dihapus/dikosongin**,
  semua style global pindah ke `site.css` biar gak ada dua sumber warna.

---

## 3. Design Tokens

Ditaruh di `site.css` sebagai CSS variable, override variable Bootstrap. Semua warna di view
**wajib** lewat class Bootstrap/variable ini — gak ada warna hex langsung di `.cshtml`.

### 3.1 Warna

Aksen: **hijau hutan tua** (nyambung sama tema gunung, gak norak, kontras tinggi).

| Token | Nilai | Dipakai buat |
|---|---|---|
| `--tb-primary` | `#1F5F4A` | Tombol utama, link, elemen aktif |
| `--tb-primary-hover` | `#174A39` | Hover/active tombol utama |
| `--tb-primary-subtle` | `#E6F0EC` | Background tipis (nav aktif, highlight) |
| `--tb-body-bg` | `#F7F7F5` | Background halaman (off-white, biar card putih kebaca) |
| `--tb-surface` | `#FFFFFF` | Card, tabel, form |
| `--tb-border` | `#E3E3DF` | Semua border/garis pemisah |
| `--tb-text` | `#1C1C1A` | Teks utama |
| `--tb-text-muted` | `#5F5F5A` | Teks sekunder (label, meta). Kontras ±6:1 di atas putih, tetap kebaca |

Warna status pakai **varian subtle bawaan Bootstrap 5.3** (`bg-*-subtle`, `text-*-emphasis`,
`border-*-subtle`) — udah lembut & kontrasnya aman, gak perlu bikin sendiri.

Kontras: putih di atas `#1F5F4A` ≈ 7,5:1 (lolos WCAG AAA buat teks normal).

### 3.2 Tipografi

| Elemen | Ukuran | Ketebalan | Catatan |
|---|---|---|---|
| Judul halaman (`h1`) | 1.75rem (28px) | 600 | Cuma 1 per halaman |
| Judul section (`h2`) | 1.25rem (20px) | 600 | |
| Judul card (`h3`/`.card-title`) | 1.0625rem (17px) | 600 | |
| Body | 1rem (16px) | 400 | Line-height 1.5 |
| Meta/label | 0.875rem (14px) | 400 | Warna `--tb-text-muted` |
| Angka di tabel (harga, kuota, tanggal) | — | — | `font-variant-numeric: tabular-nums` biar lurus |

- Hapus aturan `html { font-size: 14px }` di mobile dari template — tetap 16px biar kebaca.
- Jangan pakai `display-*` Bootstrap (kegedean). Jangan ALL CAPS kecuali label kecil.

### 3.3 Jarak, Radius, Border

- Jarak pakai spacer Bootstrap (`1`=4px, `2`=8px, `3`=16px, `4`=24px, `5`=48px).
  - Antar section dalam halaman: `mb-4` / `mb-5`.
  - Dalam card: `p-3` (mobile) / `p-4` (desktop).
- **Radius:** `--bs-border-radius: 0.375rem` (6px) buat semua. Badge 4px. Gak ada pill kecuali
  tidak ada pilihan lain.
- **Border:** 1px `--tb-border`. Card = border, **tanpa shadow**.
- Lebar konten: `.container` bawaan (maks 1320px). Form & halaman auth lebih sempit (lihat §5).

### 3.4 Snippet `site.css` (titik awal)

```css
:root {
  --tb-primary: #1F5F4A;
  --tb-primary-hover: #174A39;
  --tb-primary-subtle: #E6F0EC;
  --tb-body-bg: #F7F7F5;
  --tb-border: #E3E3DF;
  --tb-text: #1C1C1A;
  --tb-text-muted: #5F5F5A;

  --bs-body-bg: var(--tb-body-bg);
  --bs-body-color: var(--tb-text);
  --bs-secondary-color: var(--tb-text-muted);
  --bs-border-color: var(--tb-border);
  --bs-border-radius: 0.375rem;
  --bs-link-color: var(--tb-primary);
  --bs-link-color-rgb: 31, 95, 74;
  --bs-link-hover-color: var(--tb-primary-hover);
  --bs-link-hover-color-rgb: 23, 74, 57;
  --bs-primary: var(--tb-primary);
  --bs-primary-rgb: 31, 95, 74;
}

/* .btn-primary di Bootstrap 5.3 punya variable sendiri, gak ikut --bs-primary — harus di-override. */
.btn-primary {
  --bs-btn-bg: var(--tb-primary);
  --bs-btn-border-color: var(--tb-primary);
  --bs-btn-hover-bg: var(--tb-primary-hover);
  --bs-btn-hover-border-color: var(--tb-primary-hover);
  --bs-btn-active-bg: var(--tb-primary-hover);
  --bs-btn-active-border-color: var(--tb-primary-hover);
  --bs-btn-disabled-bg: var(--tb-primary);
  --bs-btn-disabled-border-color: var(--tb-primary);
}

.card { --bs-card-border-color: var(--tb-border); box-shadow: none; }
.tabular-nums { font-variant-numeric: tabular-nums; }
```

---

## 4. Komponen

Komponen yang dipakai berulang dibikin **partial view** di `Views/Shared/` biar konsisten dan gak
copy-paste.

### 4.1 Navbar (`_Layout.cshtml`)

```
┌──────────────────────────────────────────────────────────────────────────┐
│ Tebing     Gunung   Booking Saya   Kelola Booking*        Hi, Pedro  [Keluar] │
└──────────────────────────────────────────────────────────────────────────┘
                                                     * cuma muncul buat Admin
```

- Background putih, border bawah 1px, tanpa shadow. Tinggi ±56px.
- Logo = teks "Tebing" weight 700, warna `--tb-primary`. Gak perlu logo gambar.
- Menu aktif: teks `--tb-primary` + weight 600 (bukan background warna-warni).
- **Buang** link "Home" (logo udah jadi link home) dan "Privacy" (pindah ke footer aja).
- Kanan: kalau belum login → link "Masuk" + tombol `btn-primary btn-sm` "Daftar".
  Kalau udah login → nama user (muted) + tombol `btn-outline-secondary btn-sm` "Keluar".
- Mobile: collapse bawaan Bootstrap.

### 4.2 Page Header (`_PageHeader` partial atau pola manual)

Dipakai di semua halaman, bentuknya sama:

```
Gunung / Gunung Gede                                  ← breadcrumb (opsional, kecil, muted)
Gunung Gede                               [Edit] [Booking Pendakian]
Jawa Barat · 2.958 mdpl · ★ 4,6 (12 ulasan)          ← meta, muted
────────────────────────────────────────────────────────────────────
```

- Judul `h1` kiri, aksi kanan (`d-flex justify-content-between align-items-start flex-wrap gap-2`).
- Di mobile aksi turun ke bawah judul.

### 4.3 Flash Message (`_FlashMessages` partial)

Sekarang tiap view nulis ulang blok `TempData["SuccessMessage"]` / `TempData["ErrorMessage"]`.
Ganti jadi 1 partial, dipanggil **sekali di `_Layout.cshtml`** di atas `@RenderBody()`.

- Sukses: `alert alert-success` · Error: `alert alert-danger`. Ada tombol tutup (`alert-dismissible`).
- Teks pendek, 1 kalimat. Gak pakai judul "Sukses!" — langsung isi pesannya.

### 4.4 Tombol

| Jenis | Class | Contoh |
|---|---|---|
| Aksi utama (1 per halaman/section) | `btn btn-primary` | "Booking Pendakian", "Submit Booking", "Simpan" |
| Aksi sekunder | `btn btn-outline-secondary` | "Edit Gunung", "Batal", "Filter" |
| Aksi berbahaya | `btn btn-outline-danger` | "Batalkan Booking", "Tutup Gunung", "Tolak" |
| Konfirmasi admin positif | `btn btn-success` | "Verifikasi Pembayaran" |
| Navigasi ringan | link biasa / `btn btn-link` | "← Kembali ke daftar" |

- Label tombol = **kata kerja + objek**: "Simpan Jalur", bukan "OK"/"Submit".
- Aksi destruktif tetap pakai `confirm()` (udah ada) — teksnya jelasin akibatnya.
- Ukuran default; `btn-sm` cuma di dalam tabel & navbar.

### 4.5 Badge Status (`_StatusBadge` partial / helper)

**Aturan:** warna badge = arti status. Teks badge = Bahasa Indonesia (bukan nama enum mentah kayak
`Pending`). Class dasar: `badge border fw-medium`.

**BookingStatus**

| Enum | Label | Class warna |
|---|---|---|
| `Pending` | Menunggu | `bg-warning-subtle text-warning-emphasis border-warning-subtle` |
| `Approved` | Disetujui | `bg-success-subtle text-success-emphasis border-success-subtle` |
| `Completed` | Selesai | `bg-primary-subtle text-primary-emphasis border-primary-subtle` |
| `Rejected` | Ditolak | `bg-danger-subtle text-danger-emphasis border-danger-subtle` |
| `Cancelled` | Dibatalkan | `bg-secondary-subtle text-secondary-emphasis border-secondary-subtle` |

**PaymentStatus** (+ 2 kondisi tambahan dari data)

| Kondisi | Label | Warna |
|---|---|---|
| Belum ada payment | Belum bayar | secondary |
| `Pending` | Menunggu verifikasi | warning |
| `Verified` | Terverifikasi | success |
| `Rejected` | Ditolak | danger |
| `TotalPrice == 0` | Gratis | secondary |

**MountainStatus:** `Open` → gak usah badge (default normal). `Closed` → badge "Ditutup" (danger).

**DifficultyLevel:** label Mudah / Sedang / Sulit, **badge netral** (`bg-light text-body border`) —
sengaja gak pakai hijau/kuning/merah biar gak bentrok sama arti warna status.

> Mapping enum → label Indonesia ditaruh di 1 tempat (misal `Common/Extensions/EnumDisplayExtensions.cs`
> dengan method `ToLabel()`), jangan di-hardcode di tiap view.

### 4.6 Card

- `card` putih, border 1px, radius 6px, **tanpa shadow**, padding `p-3`/`p-4`.
- Dipakai buat: kartu gunung di daftar, panel ringkasan (booking summary), form, section di detail.
- Hover di card yang bisa diklik: border jadi `--tb-primary` (transisi warna aja, gak scale/naik).

### 4.7 Tabel

- `table align-middle mb-0` di dalam `card` (card jadi bingkainya), header `table-light`
  teks 14px weight 600.
- **Gak pakai `table-striped`** — cukup garis antar baris. Lebih bersih.
- Angka (harga, kuota, jumlah orang) rata kanan + `tabular-nums`. Tanggal rata kiri.
- Kolom aksi paling kanan, isinya link/`btn-sm`.
- Selalu dibungkus `table-responsive` biar gak jebol di mobile.

### 4.8 Form

- Label **di atas** input (`form-label`), bukan floating label, bukan placeholder-as-label.
- Field wajib: label biasa. Field opsional: tambah "(opsional)" di label, warna muted.
- Bantuan singkat di bawah input: `form-text`.
- Error validasi: bawaan (`text-danger` kecil di bawah field) + `is-invalid` di input.
- Kelompokkan field panjang jadi section dengan `h2` kecil + garis (contoh form gunung: "Info Dasar",
  "Detail").
- Tombol di bawah, kiri: `[Simpan]` (primary) lalu `Batal` (link). Rata kiri, bukan kanan.
- Lebar form: `col-lg-7` / maks ±640px — form selebar layar susah dibaca.

### 4.9 Empty State

Kalau list kosong, jangan cuma 1 baris teks polos. Pola:

```
┌──────────────────────────────────────────┐
│           Belum ada booking               │  ← teks 600
│  Cari gunung dulu, terus booking dari     │  ← muted
│  halaman detail gunungnya.                │
│            [Lihat Daftar Gunung]          │  ← btn-outline-secondary
└──────────────────────────────────────────┘
```

Card dengan border dashed (`border-style: dashed`), teks tengah, padding besar. Tanpa ilustrasi.

### 4.10 Rating

- Tampilan ringkas: `★ 4,6` + `(12 ulasan)` muted. Bintang warna `#B7791F` (amber gelap, bukan kuning
  terang) — satu-satunya pengecualian warna di luar status, karena bintang memang konvensi.
- Daftar ulasan: bintang penuh/kosong (`★★★★☆`) + `aria-label="Rating 4 dari 5"`.
- Form rating: tetap `<select>` atau radio 1–5 berlabel teks — jangan bintang klik-klik custom yang
  susah diakses keyboard.

### 4.11 Format Angka & Tanggal

- Harga: `Rp 35.000` (titik ribuan). Tanggal: `12 Okt 2026`. Waktu: `12 Okt 2026, 14.30`.
- **Butuh culture `id-ID`.** Sekarang `ToString("N0")`/`"dd MMM yyyy"` ngikut culture server —
  di mesin en-US hasilnya `35,000` & `Oct`. Set `RequestLocalization` default `id-ID` di
  `Program.cs` (atau pakai `CultureInfo("id-ID")` eksplisit di helper format).

---

## 5. Layout Per Halaman

### 5.1 Home (`Home/Index`)

Ganti halaman "Welcome" template. Gak pakai hero gambar.

```
Booking pendakian gunung, tanpa antre di basecamp.          ← h1, maks ±2 baris
Cek kuota, isi data rombongan, bayar, dan pantau status booking dari satu tempat.  ← muted

[ Cari nama gunung atau provinsi…            ] [Cari]        ← ke Mountain/Index

Cara kerjanya
 1 Pilih gunung & jalur   2 Isi data rombongan   3 Upload bukti bayar   4 Naik & kasih ulasan
                                                  ← 4 kolom teks bernomor, tanpa ikon besar

Gunung dengan rating tertinggi                                    Lihat semua →
┌─────────┐ ┌─────────┐ ┌─────────┐
│ card    │ │ card    │ │ card    │     ← pakai kartu gunung yang sama dengan §5.2
└─────────┘ └─────────┘ └─────────┘
```

Catatan data: pencarian nama butuh filter `search` di `MountainController.Index` (sekarang cuma
provinsi & kesulitan); "rating tertinggi" butuh query top-N.

### 5.2 Daftar Gunung (`Mountain/Index`)

46 gunung → **grid kartu**, bukan tabel.

```
Daftar Gunung                                              [+ Tambah Gunung]*
46 gunung

[Cari nama…        ] [Provinsi ▾] [Kesulitan ▾] [Filter] Reset    ← 1 baris, wrap di mobile

┌──────────────────────────┐ ┌──────────────────────────┐ ┌───────────
│ Gunung Gede              │ │ Gunung Merapi  [Ditutup] │ │ ...
│ Jawa Barat · 2.958 mdpl  │ │ Jawa Tengah · 2.930 mdpl │ │
│ [Sedang]  3 jalur        │ │ [Sulit]  1 jalur         │ │
│ ★ 4,6 (12)               │ │ Belum ada ulasan         │ │
│ Mulai Rp 35.000/orang    │ │ Mulai Rp 25.000/orang    │ │
└──────────────────────────┘ └──────────────────────────┘ └───────────
```

- Grid: `row row-cols-1 row-cols-md-2 row-cols-lg-3 g-3`. Seluruh card bisa diklik (stretched-link).
- Gunung `Closed` tetap tampil, badge "Ditutup", teks agak muted — biar user tau kenapa gak bisa booking.
- Gak ada gambar dulu (`ImageUrl` mostly kosong). Kalau nanti ada, tampil di atas card rasio 16:9;
  kalau gak ada, **gak usah placeholder** — card teks aja.
- Filter provinsi jadi dropdown dari daftar provinsi yang ada (bukan input bebas).
- Catatan data: `MountainListDto` perlu `RouteCount` & `MinPricePerPerson`. Kalau nanti datanya
  makin banyak, tambah pagination (`pagination` Bootstrap, 12 per halaman).

### 5.3 Detail Gunung (`Mountain/Detail`)

Dua kolom di desktop: konten kiri, panel booking kanan (sticky).

```
Gunung / Gunung Gede
Gunung Gede                                         [Edit Gunung]* [Tutup Gunung]*
Jawa Barat · 2.958 mdpl · [Sedang] · ★ 4,6 (12 ulasan)

┌───────────────────────────────────────────────┐  ┌────────────────────────┐
│ Tentang                                       │  │ Booking pendakian      │
│ Bagian dari TN Gede Pangrango…                │  │ Mulai Rp 35.000/orang  │
├───────────────────────────────────────────────┤  │ 3 jalur aktif          │
│ Jalur Pendakian                [+ Jalur]*     │  │                        │
│ Jalur        Jarak  Durasi   Kuota  Harga     │  │ [Booking Pendakian]    │  ← primary, full width
│ Via Cibodas  10,5km 2H1M     300   Rp 35.000  │  └────────────────────────┘
│ Via G. Putri  8,5km 2H1M     200   Rp 35.000  │   (kalau Closed: teks
├───────────────────────────────────────────────┤    "Gunung sedang ditutup",
│ Ulasan Pendaki (12)                           │    tanpa tombol)
│ Pedro  ★★★★★  12 Okt 2026                     │
│ Jalurnya enak, basecamp rapi.                 │
│ ───────────────────────────────────────────── │
│ …                                             │
└───────────────────────────────────────────────┘
```

- Kolom: `col-lg-8` + `col-lg-4` (panel `position-sticky` `top: 1rem`). Mobile: panel booking pindah
  ke atas daftar jalur.
- Jalur nonaktif: cuma kelihatan admin, baris muted + badge "Nonaktif".
- Aksi admin per jalur (Edit / Nonaktifkan) di kolom terakhir, `btn-sm` link.

### 5.4 Buat Booking (`Booking/Create`)

Form panjang → dibagi section bernomor + ringkasan di kanan.

```
Booking Pendakian — Gunung Gede

┌ 1. Jalur & tanggal ───────────────────────┐   ┌ Ringkasan ─────────────┐
│ Jalur pendakian  [Via Cibodas ▾]          │   │ Gunung Gede            │
│ Tanggal naik     [12/10/2026]             │   │ Via Cibodas            │
│ Sisa kuota tampil di sini (kalau ada)     │   │ 12 Okt 2026            │
└───────────────────────────────────────────┘   │ 3 orang × Rp 35.000    │
┌ 2. Peserta rombongan (3) ─────────────────┐   │ ─────────────────────  │
│ Peserta 1                         Hapus   │   │ Total   Rp 105.000     │
│ Nama lengkap [        ] NIK [           ] │   │                        │
│ No. HP [    ] Kontak darurat [    ][    ] │   │ [Submit Booking]       │
│ ───────────────────────────────────────── │   └────────────────────────┘
│ Peserta 2 …                               │
│ [+ Tambah Peserta]                        │
└───────────────────────────────────────────┘
```

- Ringkasan & total di-update live via JS waktu jalur/jumlah peserta berubah (harga per jalur bisa
  ditaruh di `data-price` tiap `<option>`). Total final tetap dihitung server.
- Peserta dipisah garis tipis, bukan card per peserta (hindari card dalam card).
- Tombol submit di panel ringkasan (desktop) dan di bawah form (mobile).

### 5.5 Booking Saya (`Booking/MyBookings`)

```
Booking Saya

┌──────────────────────────────────────────────────────────────────────────┐
│ Gunung        Jalur        Tanggal naik  Orang   Total       Status    Pembayaran     │
│ Gunung Gede   Via Cibodas  12 Okt 2026      3  Rp 105.000  [Menunggu] [Belum bayar] → │
└──────────────────────────────────────────────────────────────────────────┘
```

- Desktop: tabel dalam card, seluruh baris bisa diklik ke detail.
- Mobile (`< md`): tabel diganti list card kecil (nama gunung, tanggal, 2 badge, total).
- Urutan: yang butuh aksi user di atas (Menunggu + Belum bayar / pembayaran Ditolak).
- Kosong → empty state §4.9.

### 5.6 Detail Booking (`Booking/Detail`)

Halaman paling penting buat pendaki. Status harus kebaca dalam 1 detik.

```
Booking Saya / #24
Booking #24                                                   [Menunggu]
Gunung Gede · Via Cibodas · 12 Okt 2026

 ● Dibuat ──── ○ Pembayaran diverifikasi ──── ○ Disetujui ──── ○ Selesai     ← stepper

┌ Langkah berikutnya ───────────────────────────────────────┐   ← card highlight (border primary)
│ Upload bukti transfer Rp 105.000 biar admin bisa verifikasi.│
│ [Pilih file…]  [Upload Bukti]                               │
└───────────────────────────────────────────────────────────┘

Ringkasan          Peserta (3)           Pembayaran          Ulasan
(dl 2 kolom)       (tabel)               (status + link file) (kalau Completed)

[Batalkan Booking]   ← outline-danger, paling bawah, jauh dari aksi utama
```

- **Stepper** sederhana 4 titik (lingkaran kecil + garis), titik selesai warna primary. Kalau
  `Rejected`/`Cancelled`: stepper diganti alert danger/secondary yang jelasin statusnya + alasan.
- **"Langkah berikutnya"** = 1 card yang isinya berubah sesuai kondisi: upload bukti / tunggu
  verifikasi / siap naik / kasih ulasan. Ini yang bikin user gak bingung harus ngapain.
- Section lain cukup pakai `h2` + garis pemisah, gak semuanya harus card.
- **Versi admin:** aksi admin (Verifikasi/Tolak Pembayaran, Tandai Selesai, Reject, Batalkan)
  dikumpulin di 1 panel "Aksi Admin" di kanan (desktop) / atas (mobile), plus info pemesan.

### 5.7 Kelola Booking (`Booking/Manage`, admin)

```
Kelola Booking

[Semua 120] [Menunggu 8] [Disetujui 30] [Selesai 70] [Ditolak 5] [Dibatalkan 7]   ← nav-pills/tabs status
[Gunung ▾] [Tanggal naik 📅] [Filter] Reset

Total pendaki 12 Okt 2026 (selain Ditolak/Dibatalkan): 45 orang      ← kalau filter tanggal aktif

┌ tabel ringkas: # | Pemesan | Gunung · Jalur | Tanggal | Orang | Total | Status | Pembayaran | → ┐
```

- Tab status dengan jumlah → admin langsung tau ada berapa yang nunggu.
- Baris dengan pembayaran "Menunggu verifikasi" dikasih penanda kecil (titik warning di kolom
  pertama) — itu antrian kerja admin.
- Tabel lebih rapat: `table-sm`.

### 5.8 Halaman Akun (Login, Daftar, Lupa/Reset Password)

```
                    Tebing
          ┌──────────────────────────┐
          │ Masuk                    │
          │ Email     [            ] │
          │ Password  [            ] │
          │ [x] Ingat saya           │
          │ [        Masuk         ] │   ← primary full width
          │ Lupa password?           │
          └──────────────────────────┘
          Belum punya akun? Daftar
```

- Satu card di tengah, lebar maks 420px, margin atas ±10vh. Logo teks di atas card.
- Gak ada gambar samping/split screen.
- Halaman konfirmasi lupa password (sementara nampilin link reset, belum ada email): kasih kotak
  `alert-warning` kecil "Mode development: link reset ditampilkan di sini" biar jelas ini bukan
  perilaku final.

### 5.9 Form Admin (Tambah/Edit Gunung & Jalur)

- Layout form §4.8, lebar `col-lg-7`, dalam 1 card.
- Header: judul + breadcrumb balik ke detail gunung.
- Edit Gunung: field status (Buka/Ditutup) dikasih `form-text` yang jelasin efeknya
  ("Gunung yang ditutup gak bisa dibooking").
- Input harga & kuota pakai `input-group` dengan prefix "Rp" / suffix "orang/hari".

### 5.10 Error & Akses Ditolak

Pola sama kayak empty state: judul, 1 kalimat penjelasan, 1 tombol balik ("Kembali ke Beranda").
Halaman 404 (`NotFound()`) juga dikasih tampilan yang sama, bukan halaman kosong browser.

---

## 6. Copywriting

- **Bahasa Indonesia santai-sopan, pakai "kamu".** Konsisten — sekarang campur "Login"/"Masuk",
  "Logout"/"Keluar". Pakai: **Masuk, Daftar, Keluar**.
- Enum gak pernah tampil mentah (`Pending`, `Hard`) — selalu lewat label §4.5.
- Pesan sukses = apa yang terjadi: "Booking dibuat. Upload bukti transfer biar bisa diverifikasi."
- Pesan error = apa yang salah + gimana benerinnya: "Kuota Via Cibodas tanggal 12 Okt tinggal 2
  orang. Kurangi peserta atau pilih tanggal lain."
- Tanpa tanda seru beruntun, tanpa emoji, tanpa "Oops!".

---

## 7. Responsif

| Breakpoint | Perilaku |
|---|---|
| `< 576px` (HP) | 1 kolom. Tabel daftar → list card. Tombol aksi utama full width. |
| `≥ 768px` (tablet) | Grid gunung 2 kolom. Form 1 kolom. |
| `≥ 992px` (desktop) | Grid gunung 3 kolom. Detail gunung & booking jadi 2 kolom (konten + panel). |

- Target sentuh minimal 40px tinggi (tombol default Bootstrap udah cukup; hati-hati sama `btn-sm`
  dan link kecil di tabel).
- Gak ada scroll horizontal di level halaman — tabel lebar selalu di dalam `table-responsive`.

---

## 8. Aksesibilitas (minimum)

- Kontras teks ≥ 4.5:1 (token §3.1 udah lolos).
- Status gak cuma dibedain warna — badge selalu ada teks.
- Semua input punya `<label>` yang nyambung (`asp-for` udah otomatis).
- Focus ring kelihatan: outline 2px `--tb-primary` (ganti focus biru bawaan template di `site.css`).
- Ikon dekoratif `aria-hidden="true"`; rating bintang pakai `aria-label`.
- Bisa dipakai full keyboard (gak ada elemen klik custom tanpa `button`/`a`).

---

## 9. Rencana Implementasi

Urutan biar tiap langkah langsung kelihatan hasilnya:

1. **Fondasi** — token & override Bootstrap di `site.css`, kosongin `_Layout.cshtml.css`, set culture
   `id-ID`.
2. **Layout global** — navbar & footer baru, `_FlashMessages` dipanggil di layout (hapus blok
   TempData dari semua view).
3. **Komponen bersama** — `EnumDisplayExtensions.ToLabel()`, partial `_StatusBadge`,
   `_EmptyState`, pola page header.
4. **Halaman pendaki** — Daftar Gunung → Detail Gunung → Buat Booking → Booking Saya → Detail Booking.
5. **Halaman akun** — Login, Daftar, Lupa/Reset Password, Akses Ditolak, Error.
6. **Halaman admin** — Kelola Booking, form Gunung & Jalur.
7. **Home** — terakhir, karena makai kartu gunung dari langkah 4.

Perubahan backend kecil yang dibutuhkan (dikerjain bareng langkah terkait):
- `MountainListDto` + `RouteCount`, `MinPricePerPerson`; filter `search` (nama) di daftar gunung;
  daftar provinsi buat dropdown filter.
- Hitungan booking per status buat tab di Kelola Booking.
- `data-price` per opsi jalur di `BookingCreateViewModel` buat total live.

---

## 10. Checklist Review Tiap Halaman

Sebelum anggap 1 halaman beres, cek:

- [ ] Cuma ada 1 tombol `btn-primary`.
- [ ] Gak ada gradient, shadow tebal, emoji, atau warna hex langsung di `.cshtml`.
- [ ] Enum tampil pakai label Indonesia, status pakai `_StatusBadge`.
- [ ] Harga `Rp 35.000`, tanggal `12 Okt 2026`.
- [ ] Empty state ada (kalau halaman berupa list).
- [ ] Rapi di lebar 375px (HP) — gak ada scroll horizontal.
- [ ] Bisa dipakai pakai keyboard, focus ring kelihatan.
- [ ] Copy pakai "kamu", tombol kata kerja, konsisten Masuk/Daftar/Keluar.
