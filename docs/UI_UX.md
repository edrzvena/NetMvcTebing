# UI/UX Guide — Tebing

Dokumen ini beda dari dokumen lain di `docs/`:
- `PRD.md` = mau bikin fitur apa.
- `IMPLEMENTATION_PLAN.md` = urutan ngerjain backend & contoh kode tiap tahap.
- `ARCHITECTURE.md` = kenapa folder dibagi kayak gitu & alur request.
- **`UI_UX.md` (ini) = tampilan & pengalaman pakai: warna, tipografi, komponen, layout tiap
  halaman, dan aturan biar UI-nya konsisten.**

Baca ini sebelum bikin/ubah file `.cshtml` atau `site.css`.

> **Versi 2 (Okt 2026).** Versi 1 sengaja polos ("data dulu, tanpa hero, tanpa foto") karena
> waktu itu belum ada foto gunung. Hasilnya kerasa kaku & jadul. Sekarang 43 dari 46 gunung udah
> punya foto, jadi arahnya diganti: **foto jadi bintang, tetap gampang dibaca & ringan.**
> Yang berubah besar: §1 arah desain, §3 token, §4.1–4.4 & §4.15–4.16 komponen baru (termasuk
> pembatas section bentuk punggungan gunung), §5.1–5.3 & §5.8 layout, §9 performa. Badge status, form, format angka, halaman booking & admin tetap sama (cuma ikut token baru).

---

## 1. Arah Desain

**Adventure hangat.** Tebing itu alat booking, tapi yang dibooking itu *gunung* — orang harus
kepengin naik waktu buka halamannya, bukan berasa ngisi formulir kelurahan. Foto gunung yang gede,
warna hutan & sunrise, font yang ramah, dan konten yang bisa digeser kayak app travel.

Referensi pola: AllTrails & Komoot (kartu jalur foto-forward, info inti di kartu), Airbnb & Klook
(baris kartu yang bisa digeser horizontal, chip kategori, search yang menonjol).

### Prinsip

1. **Foto dulu, tapi info tetap kebaca dalam 1 detik.** Foto narik perhatian; nama gunung,
   kesulitan, harga, & status tetap elemen teks paling jelas di kartu.
2. **Hijau hutan = aksi, amber sunrise = sorotan.** Hijau buat tombol utama & link. Amber cuma
   buat penanda kecil (bintang rating, badge "Pilihan", garis aktif) — bukan buat teks/tombol.
   Merah/kuning/hijau status tetap cuma buat status.
3. **Hirarki lewat ukuran, ketebalan, foto, dan jarak.** Judul besar & tebal, ruang lega.
4. **Konsisten.** Status yang sama selalu badge yang sama, aksi yang sama selalu tombol yang sama.
5. **Satu aksi utama per layar.** Cuma 1 tombol `btn-primary` yang kelihatan per layar/section.
6. **Konten yang bisa digeser selalu kelihatan bisa digeser.** Ujung item berikutnya nongol
   (*peek*), ada panah di desktop. Gak pernah jalan sendiri.

### Yang DIHINDARI (anti "AI slop")

| Jangan | Kenapa / gantinya |
|---|---|
| Slider/banner yang geser otomatis | Riset usability (NN/g dkk.): slide 2 dst. hampir gak dilihat, bikin pusing, berat di HP. Geser manual + peek (§4.1). |
| Gradient di tombol, teks, atau background halaman | Warna solid. Gradient **cuma** boleh sebagai overlay gelap di atas foto biar teks putih kebaca. |
| Glassmorphism, blur, backdrop transparan | Background solid. |
| Shadow tebal di semua elemen | 2 level shadow aja (§3.3): diam & terangkat. |
| Emoji di UI (🏔️✨🚀) | Ikon SVG dari sprite (§3.5). |
| Wave divider (gelombang sinus) ala template landing page | Pembatas bentuk **punggungan gunung** (§4.15) — tajam tapi ujungnya agak membulat, berlapis. |
| Pembatas bentuk di setiap section | Maks 3 per halaman, cuma di halaman "jalan-jalan" (§4.15). |
| Ilustrasi 3D/blob, foto stok generik | Foto gunung asli dari data. Gunung tanpa foto → kartu tetap rapi tanpa foto (§4.4). |
| Copy lebay ("Taklukkan puncak impianmu! 🚀") | Kalimat lugas yang ngasih info. Boleh hangat, gak boleh lebay. |
| Animasi lebay (bounce, parallax, scale besar) | Transisi `transform`/`opacity` ≤ 200ms, geser 2px maksimal (§3.6). |
| Card di dalam card di dalam card | Maksimal 1 lapis card. |
| Warna cuma buat hiasan (badge warna-warni tanpa arti) | Warna = arti. |
| Teks putih langsung di atas foto tanpa overlay | Selalu pakai overlay gelap (§3.1) — foto terang bikin teks gak kebaca. |
| Icon tanpa label buat aksi penting | Tombol selalu ada teks (kecuali panah carousel, wajib `aria-label`). |

---

## 2. Batasan Teknis

- **Bootstrap 5.3.3** yang udah ada di `wwwroot/lib/bootstrap` — gak ganti framework, gak ada build
  step (SASS/npm). Kustomisasi lewat **CSS variable** di `wwwroot/css/site.css`.
- **Font: Plus Jakarta Sans** (lisensi SIL OFL, buatan Tokotype — studio Indonesia), di-*self-host*:
  1 file **variable woff2 subset Latin** di `wwwroot/lib/fonts/plus-jakarta-sans/` + file `OFL.txt`.
  Gak pakai Google Fonts (request ke domain lain + privasi).
- **Ikon:** 1 file SVG sprite `wwwroot/images/icons.svg` berisi cuma ikon yang dipakai (§3.5).
  Gak pakai icon font (Bootstrap Icons font ±130 KB buat 12 ikon = boros).
- **JavaScript:** jQuery + validation + Bootstrap bundle yang udah ada, plus **1 file kecil**
  `wwwroot/js/carousel.js` (±2 KB, vanilla) buat panah & titik carousel. Gak ada library carousel.
  Carousel harus tetap bisa digeser **tanpa JS** (scroll native).
- **Light mode aja** dulu.
- Foto gunung selalu lewat `MountainImageProcessor` (WebP 640/1200, 16:9). Rasio lain (4:3 di kartu)
  dapat dari `object-fit: cover`, bukan file baru.

---

## 3. Design Tokens

Ditaruh di `site.css` sebagai CSS variable, override variable Bootstrap. Semua warna di view
**wajib** lewat class Bootstrap/variable ini — gak ada warna hex langsung di `.cshtml`.

### 3.1 Warna

| Token | Nilai | Dipakai buat |
|---|---|---|
| `--tb-primary` | `#1F5F4A` | Tombol utama, link, elemen aktif |
| `--tb-primary-hover` | `#174A39` | Hover/active tombol utama |
| `--tb-primary-subtle` | `#E3EFE9` | Background tipis (chip aktif, highlight) |
| `--tb-forest` | `#123A2D` | Footer, overlay foto, section gelap |
| `--tb-accent` | `#E08A2E` | Amber sunrise: bintang, badge "Pilihan", garis aktif. **Bukan buat teks** di background terang (kontras kurang) |
| `--tb-accent-subtle` | `#FBEBD9` | Background badge amber |
| `--tb-accent-text` | `#8A4B0F` | Teks di atas `--tb-accent-subtle` (kontras ±6:1) |
| `--tb-body-bg` | `#F6F3EE` | Background halaman (krem hangat, biar kartu putih "ngangkat") |
| `--tb-surface` | `#FFFFFF` | Card, tabel, form |
| `--tb-border` | `#E6E1D8` | Border/garis pemisah |
| `--tb-text` | `#1C1C1A` | Teks utama |
| `--tb-text-muted` | `#5F5F5A` | Teks sekunder. Kontras ±6:1 di atas putih |
| `--tb-overlay` | `linear-gradient(to top, rgba(18,58,45,.85) 0%, rgba(18,58,45,.35) 45%, rgba(18,58,45,0) 75%)` | Satu-satunya gradient: di atas foto, di bawah teks putih |

Warna status tetap pakai **varian subtle bawaan Bootstrap 5.3** (`bg-*-subtle`, `text-*-emphasis`,
`border-*-subtle`).

Kontras: putih di atas `#1F5F4A` ≈ 7,5:1; putih di atas `--tb-forest` ≈ 12:1.

### 3.2 Tipografi

Font: `"Plus Jakarta Sans", system-ui, -apple-system, "Segoe UI", Roboto, sans-serif`.

| Elemen | Ukuran | Ketebalan | Catatan |
|---|---|---|---|
| Judul hero (banner, header foto) | `clamp(2rem, 5vw, 3.25rem)` | 800 | `letter-spacing: -0.02em`, line-height 1.1 |
| Judul halaman (`h1`) | 2rem (32px) | 700 | `letter-spacing: -0.01em`. Cuma 1 per halaman |
| Judul section (`h2`) | 1.5rem (24px) | 700 | |
| Judul card (`h3`/`.card-title`) | 1.125rem (18px) | 700 | |
| Body | 1rem (16px) | 400 | line-height 1.6 |
| Meta/label | 0.875rem (14px) | 500 | Warna `--tb-text-muted` |
| Eyebrow (label kecil di atas judul) | 0.75rem (12px) | 700 | UPPERCASE, `letter-spacing: 0.08em`, warna `--tb-primary` — satu-satunya tempat ALL CAPS |
| Angka (harga, kuota, tanggal) | — | — | `font-variant-numeric: tabular-nums` |

- Font 16px di semua ukuran layar. Jangan pakai `display-*` Bootstrap.

### 3.3 Bentuk, Jarak, Shadow

- Jarak pakai spacer Bootstrap (`1`=4px … `5`=48px). Antar section di Home: `py-5`.
- **Radius:**

  | Token | Nilai | Dipakai buat |
  |---|---|---|
  | `--tb-radius-lg` | 16px | Card, banner slide, header foto, search mengambang |
  | `--tb-radius` | 10px | Tombol, input, select, tabel dalam card |
  | `--tb-radius-pill` | 999px | Chip, badge |

- **Shadow (cuma 2 level):**

  | Token | Nilai | Dipakai buat |
  |---|---|---|
  | `--tb-shadow-sm` | `0 1px 2px rgba(28,28,26,.06), 0 1px 1px rgba(28,28,26,.04)` | Card diam, chip |
  | `--tb-shadow-lg` | `0 12px 32px rgba(18,58,45,.14)` | Card hover, search mengambang, panel booking sticky |

- **Border:** card tetap punya border 1px `--tb-border` (biar rapi di layar yang kontrasnya jelek).
- Lebar konten: `.container` bawaan. Banner hero & header foto detail = full-bleed (lebar layar).

### 3.4 Tombol

| Jenis | Class | Contoh |
|---|---|---|
| Aksi utama (1 per layar) | `btn btn-primary` | "Booking Pendakian", "Cari", "Simpan" |
| Aksi sekunder | `btn btn-outline-secondary` | "Edit Gunung", "Filter" |
| Aksi di atas foto | `btn btn-light` | "Lihat Gunung" di banner |
| Aksi berbahaya | `btn btn-outline-danger` | "Batalkan Booking", "Tolak" |
| Konfirmasi admin positif | `btn btn-success` | "Verifikasi Pembayaran" |
| Navigasi ringan | link / `btn btn-link` | "Lihat semua →" |

- Radius `--tb-radius`, weight 600, padding `0.625rem 1.25rem` (default), `btn-sm` cuma di tabel & navbar.
- Label tombol = kata kerja + objek. Aksi destruktif tetap `confirm()` yang jelasin akibatnya.

### 3.5 Ikon

SVG sprite `wwwroot/images/icons.svg`, dipakai lewat partial `_Icon`:
`<partial name="_Icon" model='"pin"'/>` → `<svg class="tb-icon" aria-hidden="true"><use href="/images/icons.svg#pin"/></svg>`.

Isi sprite (stroke 1.75, 24×24, `currentColor`): `pin` (lokasi), `mountain` (ketinggian),
`route` (jalur), `clock` (durasi), `users` (peserta/kuota), `star` (rating, versi fill),
`ticket` (harga), `search`, `chevron-left`, `chevron-right`, `arrow-right`, `calendar`, `check`.

- Ukuran default 1.125em, `vertical-align: -0.15em`. Selalu dekoratif (`aria-hidden`), teks di sebelahnya
  yang ngasih arti.

### 3.6 Gerak

- Cuma properti `transform` & `opacity` (+ warna), durasi 150–200ms, `ease-out`.
- Card hover: `translateY(-2px)` + `--tb-shadow-lg`. Foto di card hover: `scale(1.03)` di dalam
  `overflow: hidden`.
- Carousel: `scroll-behavior: smooth` waktu digeser lewat panah/titik.
- **`@media (prefers-reduced-motion: reduce)`** → semua transisi & smooth scroll dimatiin.

### 3.7 Snippet `site.css` (titik awal token)

```css
@font-face {
  font-family: "Plus Jakarta Sans";
  src: url("../lib/fonts/plus-jakarta-sans/PlusJakartaSans-latin.var.woff2") format("woff2");
  font-weight: 200 800;
  font-display: swap;
}

:root {
  --tb-primary: #1F5F4A;
  --tb-primary-hover: #174A39;
  --tb-primary-subtle: #E3EFE9;
  --tb-forest: #123A2D;
  --tb-accent: #E08A2E;
  --tb-accent-subtle: #FBEBD9;
  --tb-accent-text: #8A4B0F;
  --tb-body-bg: #F6F3EE;
  --tb-surface: #FFFFFF;
  --tb-border: #E6E1D8;
  --tb-text: #1C1C1A;
  --tb-text-muted: #5F5F5A;
  --tb-radius: 10px;
  --tb-radius-lg: 16px;
  --tb-radius-pill: 999px;
  --tb-shadow-sm: 0 1px 2px rgba(28, 28, 26, .06), 0 1px 1px rgba(28, 28, 26, .04);
  --tb-shadow-lg: 0 12px 32px rgba(18, 58, 45, .14);
  --tb-overlay: linear-gradient(to top, rgba(18, 58, 45, .85) 0%, rgba(18, 58, 45, .35) 45%, rgba(18, 58, 45, 0) 75%);

  --bs-body-font-family: "Plus Jakarta Sans", system-ui, -apple-system, "Segoe UI", Roboto, sans-serif;
  --bs-body-line-height: 1.6;
  --bs-body-bg: var(--tb-body-bg);
  --bs-body-color: var(--tb-text);
  --bs-secondary-color: var(--tb-text-muted);
  --bs-border-color: var(--tb-border);
  --bs-border-radius: var(--tb-radius);
  --bs-border-radius-lg: var(--tb-radius-lg);
  --bs-link-color: var(--tb-primary);
  --bs-link-color-rgb: 31, 95, 74;
  --bs-link-hover-color: var(--tb-primary-hover);
  --bs-link-hover-color-rgb: 23, 74, 57;
  --bs-primary: var(--tb-primary);
  --bs-primary-rgb: 31, 95, 74;
}

.card {
  --bs-card-border-radius: var(--tb-radius-lg);
  --bs-card-border-color: var(--tb-border);
  box-shadow: var(--tb-shadow-sm);
}
```

(Override `.btn-primary`, tabel, input, focus ring dari versi 1 tetap dipakai.)

---

## 4. Komponen

Komponen yang dipakai berulang dibikin **partial view** di `Views/Shared/` biar konsisten.

### 4.1 Hero Banner Carousel — **baru** (`_HeroCarousel`, cuma di Home)

```
┌───────────────────────────────────────────────────────────────┬────┐
│ [FOTO 1200px, full-bleed, radius 16 di desktop]               │ fo │ ← peek slide 2
│                                                               │ to │
│  PILIHAN MINGGU INI                     ← eyebrow amber       │    │
│  Gunung Rinjani                         ← judul hero putih    │    │
│  Nusa Tenggara Barat · 3.726 mdpl                             │    │
│  Danau di dalam kawah dan sabana luas.  ← 1 kalimat           │    │
│  [Lihat Gunung]                          ‹  ● ○ ○ ○ ○  ›      │    │
└───────────────────────────────────────────────────────────────┴────┘
          ╭────────────────────────────────────────────╮
          │ [ikon] Cari nama gunung atau provinsi… [Cari]│ ← search mengambang
          ╰────────────────────────────────────────────╯
```

- **Isi:** 5 gunung `Open` yang punya foto, urut rating tertinggi → jumlah ulasan → ketinggian
  (biar tanpa ulasan pun yang muncul gunung ikonik). Tagline = kalimat pertama `Description`.
- **Ukuran:** tinggi `clamp(380px, 62vh, 560px)`. Slide = 92% lebar di desktop / 88% di HP, jarak 12px
  → slide berikutnya nongol di kanan (peek).
- **Mekanik:** `display: flex; overflow-x: auto; scroll-snap-type: x mandatory;`, tiap slide
  `scroll-snap-align: center`. Scrollbar disembunyiin (`scrollbar-width: none`) — titik penanda gantinya.
  **Gak ada autoplay.** Swipe/trackpad/Shift+scroll jalan tanpa JS.
- **`carousel.js`:** tombol ‹ › (`scrollBy` selebar 1 slide), titik (klik = loncat ke slide),
  titik aktif di-update pakai `IntersectionObserver`. Panah disembunyiin di layar sentuh
  (`@media (hover: none)`) dan di slide pertama/terakhir di-`disabled`.
- **Teks di foto:** di atas `--tb-overlay`, rata kiri bawah, maks 2 baris tagline.
- **Aksesibilitas:** wrapper `<section aria-roledescription="carousel" aria-label="Gunung pilihan">`,
  tiap slide `role="group" aria-roledescription="slide" aria-label="2 dari 5"`, panah `<button aria-label="Slide sebelumnya">`,
  titik `<button aria-label="Ke slide 3" aria-current="true|false">`. Link "Lihat Gunung" di slide yang
  gak kelihatan tetap bisa di-Tab (scroll-snap otomatis geser ke situ).
- **Performa:** foto slide 1 `loading="eager" fetchpriority="high"` + `<link rel="preload">` di `<head>`
  (pakai section `Head` di layout); slide 2–5 `loading="lazy"`. `srcset` 640/1200.

### 4.2 Search Mengambang — **baru** (Home)

- Card putih, radius 16, `--tb-shadow-lg`, `margin-top: -2.5rem` (numpuk bagian bawah banner), lebar
  maks 720px di tengah. Isi: ikon `search` + input + `btn-primary` "Cari" → `Mountain/Index?search=`.
- Di HP: full lebar container, tombol tetap di kanan input.

### 4.3 Chip & Rail — **baru**

**Chip** (`_ChipNav`): baris pill yang bisa digeser horizontal (scroll native, tanpa panah).

```
(Semua 46) (Jawa 20) (Bali & Nusa Tenggara 6) (Sumatra 11) (Sulawesi 4) (Maluku & Papua 2)  →
```

- Pill radius 999, `--tb-shadow-sm`, background putih; aktif = `--tb-primary` + teks putih.
  Selalu `<a>` (link filter), bukan tombol JS. Angka jumlah = muted.
- Pulau diturunin dari `Province` lewat 1 mapping (`Common/Regions/IslandRegion.cs`):
  Jawa (Banten, DKI Jakarta, Jawa Barat/Tengah/Timur, DI Yogyakarta) · Sumatra (Aceh, Sumatera
  Utara/Barat/Selatan, Riau, Kepulauan Riau, Jambi, Bengkulu, Lampung, Bangka Belitung) · Bali & Nusa
  Tenggara (Bali, NTB, NTT) · Kalimantan (Kalimantan *) · Sulawesi (Sulawesi *, Gorontalo) · Maluku & Papua
  (Maluku, Maluku Utara, Papua *). Chip pulau yang gak punya gunung **gak ditampilin**.

**Rail** (`_MountainRail`): judul section + deretan kartu yang bisa digeser.

```
CARI YANG RINGAN                                   ← eyebrow
Cocok buat pemula                    Lihat semua →  ‹ ›   ← panah cuma desktop
╭──────────╮ ╭──────────╮ ╭──────────╮ ╭────
│  kartu   │ │  kartu   │ │  kartu   │ │ ka     ← peek
╰──────────╯ ╰──────────╯ ╰──────────╯ ╰────
```

- Mekanik sama kayak banner (scroll-snap `start`, `carousel.js` buat panah). Lebar kartu:
  `clamp(240px, 72vw, 300px)` di HP, 4 kartu + peek di desktop.
- Maks 10 kartu per rail + "Lihat semua →" ke daftar gunung yang udah difilter.
- Foto di rail selalu `loading="lazy"`.
- Rail kosong → gak dirender sama sekali (bukan empty state).

### 4.4 Kartu Gunung v2 (`_MountainCard`)

```
╭──────────────────────────────╮
│ [Sedang]            [Ditutup]│ ← badge di atas foto (putih, pill)
│          FOTO 4:3            │
│                              │
├──────────────────────────────┤
│ Gunung Gede            ★ 4,6 │ ← judul 700 · rating (bintang amber)
│ [pin] Jawa Barat · [mtn] 2.958│
│ 3 jalur                      │
│ Mulai Rp 35.000 /orang       │ ← harga 700
╰──────────────────────────────╯
```

- Foto `ratio` 4:3 (`object-fit: cover` dari file 640), radius atas 16. Badge kesulitan di kiri atas
  (`bg-white text-body` pill + shadow-sm, **tetap netral**), "Ditutup" di kanan atas (danger).
- Seluruh kartu bisa diklik (`stretched-link`). Hover: naik 2px + shadow-lg + foto zoom 1.03.
- **Gunung tanpa foto:** area foto diganti blok `--tb-primary-subtle` setinggi sama + ikon `mountain`
  besar warna `--tb-primary` (bukan foto stok). Tinggi kartu di 1 baris tetap rata.
- Gunung `Closed`: foto `opacity: .55`, teks muted, tetap bisa diklik (biar tau kenapa gak bisa booking).
- Rating gak ada → "Belum ada ulasan" muted, gak pakai bintang.

### 4.5 Navbar & Footer (`_Layout.cshtml`)

**Navbar:** putih, border bawah 1px, tinggi 64px, sticky di atas (`sticky-top`).
- Logo = SVG gunung kecil (2 segitiga, `--tb-primary` + `--tb-accent`) + teks "Tebing" weight 800.
- Menu aktif: teks `--tb-primary` weight 700 + garis bawah 2px `--tb-accent`.
- Kanan: belum login → "Masuk" (link) + `btn-primary btn-sm` "Daftar"; udah login → nama (muted) +
  `btn-outline-secondary btn-sm` "Keluar". Mobile: collapse bawaan Bootstrap.

**Footer:** background `--tb-forest` + pola garis kontur (§4.16), teks putih/`rgba(255,255,255,.72)`, `py-5`.
Di atas footer selalu ada pembatas punggungan (§4.15) warna `--tb-forest`.

```
Tebing                      Jelajah                 Bantuan
Booking pendakian gunung    Jawa                    Cara booking (→ Home#cara-kerja)
se-Indonesia, tanpa antre   Sumatra                 Kebijakan Privasi
di basecamp.                Bali & Nusa Tenggara
                            …
─────────────────────────────────────────────────────────────
© 2026 Tebing · Foto gunung: kontributor Wikimedia Commons (kredit lengkap di tiap halaman gunung)
```

### 4.6 Page Header

- **Halaman biasa** (daftar, booking, admin): eyebrow opsional + `h1` + meta muted, aksi di kanan.
  Garis bawah dihapus — diganti jarak `mb-4`.
- **Halaman gunung:** header foto (§5.3).

### 4.7 Stat Tile — **baru** (`_StatTile`, detail gunung)

```
╭──────────────╮
│ [ikon]        │  ← lingkaran 40px --tb-primary-subtle, ikon --tb-primary
│ 2.958 mdpl    │  ← 700, tabular-nums
│ Ketinggian    │  ← muted 14px
╰──────────────╯
```

4 tile sejajar (`row-cols-2 row-cols-md-4 g-3`): Ketinggian · Jalur aktif · Harga mulai · Rating.

### 4.8 Flash Message (`_FlashMessages`)

Tetap: 1 partial di layout, `alert-success`/`alert-danger` + tombol tutup, radius `--tb-radius`.

### 4.9 Badge Status (`StatusBadgeExtensions`)

**Aturan:** warna badge = arti status. Teks badge = Bahasa Indonesia. Bentuk: pill.

**BookingStatus**

| Enum | Label | Class warna |
|---|---|---|
| `Pending` | Menunggu | `bg-warning-subtle text-warning-emphasis border-warning-subtle` |
| `Approved` | Disetujui | `bg-success-subtle text-success-emphasis border-success-subtle` |
| `Completed` | Selesai | `bg-primary-subtle text-primary-emphasis border-primary-subtle` |
| `Rejected` | Ditolak | `bg-danger-subtle text-danger-emphasis border-danger-subtle` |
| `Cancelled` | Dibatalkan | `bg-secondary-subtle text-secondary-emphasis border-secondary-subtle` |

**PaymentStatus**: Belum bayar (secondary) · Menunggu verifikasi (warning) · Terverifikasi
(success) · Ditolak (danger) · Gratis (secondary).

**MountainStatus:** `Open` → gak ada badge. `Closed` → "Ditutup" (danger).

**DifficultyLevel:** Mudah / Sedang / Sulit, **badge netral** — biar gak bentrok sama warna status.

**Badge sorotan** (baru): "Pilihan" = `--tb-accent-subtle` + teks `--tb-accent-text`. Cuma di banner/rail.

Mapping enum → label tetap di `Common/Extensions/EnumDisplayExtensions.cs` (`ToLabel()`).

### 4.10 Tabel

- `table align-middle mb-0` di dalam `card` (card jadi bingkainya), header `table-light` 14px weight 700.
- Gak pakai `table-striped`. Angka rata kanan + `tabular-nums`. Kolom aksi paling kanan.
- Selalu dibungkus `table-responsive` (yang `position: relative`, lihat catatan di `site.css`).

### 4.11 Form

- Label **di atas** input, field opsional ditandai "(opsional)" muted, bantuan pakai `form-text`.
- Input & select: radius `--tb-radius`, tinggi 44px, focus ring 2px `--tb-primary`.
- Kelompokkan field panjang jadi section (`h2` kecil + garis). Tombol di bawah kiri:
  `[Simpan]` (primary) lalu `Batal` (link). Lebar form maks ±640px.

### 4.12 Empty State

Card dashed, teks tengah, padding besar + **ikon `mountain` di lingkaran `--tb-primary-subtle`** di atas judul.
Judul 700, 1 kalimat muted, 1 tombol `btn-outline-secondary` (opsional). Tanpa ilustrasi.

### 4.13 Rating

- Ringkas: ikon `star` (fill `--tb-accent`) + `4,6` + `(12 ulasan)` muted.
- Daftar ulasan: `★★★★☆` warna `--tb-accent` + `aria-label="Rating 4 dari 5"`.
- Form rating: tetap `<select>` berlabel teks.

### 4.14 Format Angka & Tanggal

Harga `Rp 35.000`, tanggal `12 Okt 2026`, waktu `12 Okt 2026, 14.30` — lewat `FormatExtensions`
(culture `id-ID` eksplisit, **bukan** global — lihat komentar di file itu).

### 4.15 Pembatas Punggungan Gunung — **baru** (`_RidgeDivider`)

Nama umumnya *shape divider* / *section divider*: tepi section dibikin berbentuk, bukan garis lurus.
Di Tebing bentuknya **siluet punggungan gunung berlapis** — yang paling jauh paling pudar, kayak
lihat deretan gunung dari puncak.

```
         /\                 /\        /\
    /\  /  \      /\   /\  /  \  /\  /  \            ← lapis belakang (opacity .35)
   /  \/    \    /  \_/  \/    \/  \/    \   /\      ← lapis tengah  (opacity .6)
__/          \__/                         \_/  \___  ← lapis depan   (solid)
███████████████████ section berikutnya ███████████████
```

- **Teknis:** SVG inline lewat partial `_RidgeDivider` (model: `RidgeDividerViewModel(Variant, Flip)`),
  `viewBox="0 0 1440 120"`, `preserveAspectRatio="none"`, lebar 100%, tinggi `clamp(48px, 7vw, 112px)`.
  3 `<path>` pakai `fill="currentColor"` dengan `opacity` .35 / .6 / 1. **Warna = background section
  berikutnya** (diset lewat class `text-*`/`style="color: var(--tb-…)"` di wrapper, bukan hex).
  Total < 1,5 KB, gak ada request tambahan. `aria-hidden="true" focusable="false"`.
- **2 varian** (`Ridge.A` landai-panjang, `Ridge.B` lebih runcing) + `Flip` (cermin horizontal) —
  biar 2 pembatas di 1 halaman gak kelihatan copy-paste.
- **Posisi:** nempel di tepi bawah section sebelumnya (`position: absolute; bottom: -1px`, `-1px` biar
  gak ada garis tipis bocor), section-nya dikasih `padding-bottom` setinggi pembatas biar konten gak ketiban.
- **Di atas foto** (banner Home & header detail gunung): pembatas ditaruh di *dalam* wrapper foto,
  di atas overlay, `pointer-events: none`. Panah & titik carousel tetap di atasnya (`z-index`).
  Pembatasnya **diam** — gak ikut kegeser waktu slide digeser.
- **Aturan pakai (maks 3 per halaman, pembatas ke footer ikut dihitung):**

  | Halaman | Posisi | Jumlah |
  |---|---|---|
  | Home | bawah banner (→ krem) · masuk ke section "Cara kerjanya" (→ putih) · masuk ke footer (→ hijau gelap) | 3 (maks) |
  | Daftar Gunung | masuk ke footer | 1 |
  | Detail Gunung | bawah header foto (→ krem) · masuk ke footer | 2 |
  | Booking, admin, form, error | masuk ke footer aja — halaman kerja, tetap fokus | 1 |
  | Akun (Masuk, Daftar, Lupa/Reset Password) | gak ada — layout terbelah full tinggi tanpa footer (§5.8) | 0 |
- Gak dianimasiin. Gak dipakai di dalam card.

### 4.16 Pola Garis Kontur — **baru**

Garis kontur peta topografi sebagai tekstur halus, biar nuansa "peta pendakian" kerasa.

- 1 file `wwwroot/images/contour.svg` (garis kontur lengkung, stroke putih 1px, ±3 KB), dipakai
  sebagai `background-image` dengan `opacity` efektif ±6–8% (warna stroke `rgba(255,255,255,.07)`).
- **Cuma di 2 tempat:** footer (`--tb-forest`) dan panel foto halaman akun kalau gunungnya gak punya
  foto. Gak di atas foto, gak di background halaman, gak di card.
- Dekoratif murni — konten di atasnya tetap harus lolos kontras tanpa ngitung polanya.

---

## 5. Layout Per Halaman

### 5.1 Home (`Home/Index`) — **dirombak**

```
[navbar]
[HERO BANNER CAROUSEL full-bleed  §4.1  ……………………………………………… peek]
[__/\__/\/\____/\___ pembatas punggungan §4.15 (→ krem), diam di atas foto __]
            [ search mengambang §4.2 ]   ← duduk di atas pembatas

(Semua)(Jawa)(Bali & Nusa Tenggara)(Sumatra)(Sulawesi)(Maluku & Papua) →     ← chip §4.3

PILIHAN
Gunung pilihan                                     Lihat semua →  ‹ ›
[kartu][kartu][kartu][kartu][ka…                                              ← rail

CARI YANG RINGAN
Cocok buat pemula                                  Lihat semua →  ‹ ›
[kartu][kartu][kartu][kartu][ka…                                              ← rail (Mudah)

BUAT YANG CARI TANTANGAN
Jalur menantang                                    Lihat semua →  ‹ ›
[kartu][kartu][kartu][kartu][ka…                                              ← rail (Sulit)

__/\____/\/\_______/\___  ← pembatas (→ putih), varian B
CARA KERJANYA  (id="cara-kerja")      ← section background putih
 (ikon route)        (ikon users)          (ikon ticket)         (ikon star)
 1 Pilih gunung      2 Isi data            3 Upload bukti        4 Naik & kasih
   & jalur             rombongan             bayar                 ulasan
__/\__/\_____/\/\_______  ← pembatas (→ hijau gelap), varian A flip
[footer hijau gelap + garis kontur §4.16]
```

- Rail "Gunung pilihan" = urutan sama kayak banner, **lewati 5 yang udah di banner**.
- "Cocok buat pemula" = `Easy` & `Open`; "Jalur menantang" = `Hard` & `Open`. Urut rating → nama.
- "Lihat semua →" ke `Mountain/Index` dengan filter yang sesuai (`?difficulty=Easy`, dst.).

### 5.2 Daftar Gunung (`Mountain/Index`)

```
JELAJAH                                                        [Tambah Gunung]*
Daftar Gunung
46 gunung di 5 wilayah                  ← dihitung dari data, bukan teks tetap

[ikon][Cari nama gunung atau provinsi…        ] [Kesulitan ▾] [Cari]   ← 1 card putih
(Semua 46)(Jawa 20)(Bali & Nusa Tenggara 6)(Sumatra 11)…                ← chip pulau

[kartu v2][kartu v2][kartu v2]      ← grid row-cols-1 / md-2 / lg-3 (xl-4), g-4
[kartu v2][kartu v2][kartu v2]
```

- Filter pulau (`island`) lewat chip, gabung sama `search` & `difficulty` (chip bawa query lain).
  Dropdown provinsi diganti chip pulau (provinsi tetap kecari lewat kotak search).
- Tetap **grid**, bukan rail — di halaman ini orang lagi bandingin.
- 6 kartu pertama foto `eager`, sisanya `lazy`.

### 5.3 Detail Gunung (`Mountain/Detail`) — **dirombak**

```
[HEADER FOTO full-bleed, tinggi clamp(320px, 50vh, 480px), overlay §3.1      ]
[  Gunung / Gunung Gede                     ← breadcrumb putih 72%            ]
[  Gunung Gede                 [Ditutup]    ← judul hero putih                ]
[  [pin] Jawa Barat · [Sedang]              [Edit Gunung]* [Tutup Gunung]*   ]
[                                    Foto: Afrogindahood / CC BY-SA 3.0 ↗    ]  ← kredit kecil, di atas pembatas
[__/\___/\/\____/\__ pembatas punggungan §4.15 (→ krem) ______________________]

╭stat╮╭stat╮╭stat╮╭stat╮     ← §4.7, numpuk -2rem ke header foto
┌──────────────────────────────────┐  ┌──────────────────────┐
│ Tentang                          │  │ Booking pendakian    │  ← sticky top: 80px
│ Jalur Pendakian      [Tambah]*   │  │ Mulai Rp 35.000/org  │    shadow-lg
│ (tabel)                          │  │ 3 jalur aktif        │
│ Ulasan Pendaki (12)              │  │ [Booking Pendakian]  │
└──────────────────────────────────┘  └──────────────────────┘

PULAU YANG SAMA
Gunung lain di Jawa                                Lihat semua →  ‹ ›
[kartu][kartu][kartu][kartu][ka…                                   ← rail, maks 10, tanpa gunung ini
```

- Gunung tanpa foto: header jadi blok `--tb-forest` polos (teks putih tetap) — layout sama.
- Header foto pakai file 1200 (`srcset` 640/1200, `sizes="100vw"`), `fetchpriority="high"`, gak lazy.
- Mobile: panel booking pindah ke bawah stat tile (tetap 1 kali di layar), sticky dimatiin.
- Jalur nonaktif tetap cuma buat admin (muted + badge "Nonaktif").

### 5.4 Buat Booking (`Booking/Create`)

Struktur tetap (section bernomor + ringkasan live di kanan). Tambahan: ringkasan dapat
**thumbnail foto gunung** (16:9, radius 10) di atas nama gunung.

### 5.5 Booking Saya (`Booking/MyBookings`)

Struktur tetap (tabel di desktop, kartu di HP). Kartu HP dapat thumbnail foto 64×64 radius 10 di kiri.

### 5.6 Detail Booking (`Booking/Detail`)

Struktur tetap (stepper, "Langkah berikutnya", panel Aksi Admin). Header dapat thumbnail foto
gunung 96×64 radius 10 di kiri judul. Titik stepper yang selesai pakai `--tb-primary`, langkah yang
lagi berjalan dapat ring `--tb-accent`.

### 5.7 Kelola Booking (`Booking/Manage`, admin)

Struktur tetap. Tab status pakai gaya chip §4.3.

### 5.8 Halaman Akun (Masuk, Daftar, Lupa/Reset Password) — **dirombak**

```
Desktop (≥ 992px):
┌───────────────────────────────┬───────────────────────────────┐
│ [FOTO GUNUNG full tinggi]     │   Tebing (logo)               │
│                               │                               │
│                               │   Masuk                       │
│  "Sampai ketemu di puncak."   │   Email     [            ]    │
│  Gunung Rinjani · kredit foto │   Password  [            ]    │
│                               │   [        Masuk         ]    │
│                               │   Belum punya akun? Daftar    │
└───────────────────────────────┴───────────────────────────────┘
```

- Kiri: foto gunung (file 1200, `object-fit: cover`, overlay §3.1), 1 kalimat + nama gunung + kredit.
  Foto dipilih acak dari gunung `Open` yang punya foto (server-side, per request); gak ada → panel
  kiri jadi blok `--tb-forest` polos.
- Kanan: form maks 400px di tengah. **Mobile: foto disembunyiin** (`d-none d-lg-block`) — HP gak
  download apa-apa (pakai `<picture>` dengan `media="(min-width: 992px)"` biar beneran gak di-fetch).
- Halaman konfirmasi lupa password: tetap ada `alert-warning` "Mode development".

### 5.9 Form Admin (Tambah/Edit Gunung & Jalur)

Tetap (§4.11, form upload foto dari versi 1).

### 5.10 Error, 404, Akses Ditolak

Pola empty state §4.12 + 1 tombol "Kembali ke Beranda".

---

## 6. Copywriting

- **Bahasa Indonesia santai-sopan, pakai "kamu".** Pakai: **Masuk, Daftar, Keluar**.
- Enum gak pernah tampil mentah — selalu lewat label §4.9.
- Boleh hangat di Home/banner/eyebrow ("Cari yang ringan", "Sampai ketemu di puncak."), tetap lugas
  di form & pesan sistem.
- Pesan sukses = apa yang terjadi. Pesan error = apa yang salah + gimana benerinnya.
- Tanpa tanda seru beruntun, tanpa emoji, tanpa "Oops!".

---

## 7. Responsif

| Breakpoint | Perilaku |
|---|---|
| `< 576px` (HP) | Banner slide 88% + peek. Rail kartu `72vw`. Grid 1 kolom. Panah carousel disembunyiin. |
| `≥ 768px` (tablet) | Grid gunung 2 kolom. Stat tile 4 sejajar. |
| `≥ 992px` (desktop) | Grid 3 kolom (4 di `xl`). Detail gunung 2 kolom + panel sticky. Halaman akun terbelah. |

- Target sentuh minimal 44px (chip, titik carousel diberi area klik 44px walau titiknya kecil).
- Gak ada scroll horizontal di level halaman — yang boleh geser cuma carousel, rail, chip, tabel.

---

## 8. Aksesibilitas (minimum)

- Kontras teks ≥ 4.5:1. Teks di atas foto **selalu** di atas `--tb-overlay`.
- Status gak cuma dibedain warna — badge selalu ada teks.
- Semua input punya `<label>`. Focus ring 2px `--tb-primary` kelihatan di semua elemen, termasuk
  slide & kartu yang di-scroll.
- Carousel: ikutin pola §4.1 (role/label, tombol berlabel, tanpa autoplay). Rail = `<section aria-labelledby>`.
- Foto: kartu `alt=""` (nama udah di judul), header/banner `alt` = nama gunung.
- `prefers-reduced-motion` dihormati (§3.6). Bisa dipakai full keyboard.

---

## 9. Performa — **baru**

Budget per halaman (diukur di DevTools, cache kosong, lebar 1280 & 375):

| Item | Budget |
|---|---|
| Foto yang ke-download waktu halaman pertama kebuka (Home) | ≤ 500 KB |
| Font | ≤ 100 KB (1 file variable woff2) |
| JS tambahan selain yang udah ada | ≤ 5 KB (`carousel.js`) |
| Dekorasi (pembatas punggungan inline + `contour.svg`) | ≤ 8 KB total per halaman |
| Layout shift (CLS) | ≈ 0 — semua foto punya `width`/`height` atau `ratio` |

Aturan:
- Foto di luar layar pertama **wajib** `loading="lazy" decoding="async"`.
- Cuma 1 foto per halaman yang `fetchpriority="high"` (banner slide 1 / header foto detail).
- Kartu pakai file 640; banner & header pakai `srcset` 640/1200.
- Font: `<link rel="preload" as="font" type="font/woff2" crossorigin>` di layout.
- Gak ada request ke domain lain (font, ikon, library — semua self-host).

---

## 10. Rencana Implementasi

1. **Fondasi v2** — font (self-host + OFL), token baru di `site.css`, sprite ikon + partial `_Icon`,
   `carousel.js`.
2. **Layout global** — navbar baru (logo SVG, garis aktif amber), footer hijau gelap.
3. **Komponen** — kartu gunung v2, `_ChipNav`, `_MountainRail`, `_HeroCarousel`, `_StatTile`,
   `_RidgeDivider` + `contour.svg`,
   empty state + ikon.
4. **Backend kecil** — `IslandRegion` (mapping provinsi → pulau + filter `island`), data Home
   (banner + 3 rail), "gunung lain di pulau yang sama".
5. **Home** → **Daftar Gunung** → **Detail Gunung**.
6. **Halaman akun** (layout terbelah).
7. **Sentuhan halaman booking & admin** — thumbnail foto, chip status, token baru.
8. **Cek budget §9 & checklist §11** di 1280px & 375px.

---

## 11. Checklist Review Tiap Halaman

- [ ] Cuma 1 tombol `btn-primary` yang kelihatan per layar.
- [ ] Gak ada gradient (selain overlay foto), glassmorphism, emoji, atau warna hex di `.cshtml`.
- [ ] Teks di atas foto pakai overlay & kebaca.
- [ ] Carousel/rail: ada peek, bisa di-swipe, panah jalan, gak autoplay.
- [ ] Pembatas punggungan maks 3 (termasuk yang ke footer), warnanya nyambung mulus ke section berikutnya (gak ada garis
      tipis bocor di zoom 90–125%), gak nutupin konten/panah carousel.
- [ ] Enum tampil pakai label Indonesia, status pakai badge.
- [ ] Harga `Rp 35.000`, tanggal `12 Okt 2026`.
- [ ] Empty state ada (kalau halaman berupa list). Gunung tanpa foto tetap rapi.
- [ ] Rapi di 375px — gak ada scroll horizontal di level halaman.
- [ ] Budget §9 lolos. Foto di bawah layar pertama lazy.
- [ ] Bisa dipakai pakai keyboard, focus ring kelihatan, `prefers-reduced-motion` dihormati.
- [ ] Copy pakai "kamu", tombol kata kerja, konsisten Masuk/Daftar/Keluar.
