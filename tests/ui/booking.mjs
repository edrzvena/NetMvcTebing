import { check, result } from './_assert.mjs';

// Task 11 (bagian pendaki): thumbnail foto gunung di Buat Booking, Detail Booking, Booking Saya.
// Bikin akun tes uitest@tebing.local + 1 booking di DB dev — WAJIB dihapus lagi setelah dites
// (lihat booking-admin.mjs & langkah bersih-bersih di plan Task 11).
const BASE = 'http://localhost:5181';
const EMAIL = 'uitest@tebing.local';
const PASSWORD = 'uitest123';

async function mountainId(page, name) {
  await page.goto(`${BASE}/Mountain?search=${encodeURIComponent(name)}`, { waitUntil: 'networkidle' });
  const href = await page.evaluate(n => [...document.querySelectorAll('.mountain-card a.stretched-link')]
    .find(a => a.textContent.trim() === n)?.getAttribute('href'), name);
  return href?.split('/').pop();
}

async function pageChecks(page, failures, label) {
  const info = await page.evaluate(() => ({
    ridges: document.querySelectorAll('svg.ridge').length,
    primary: [...document.querySelectorAll('.btn-primary')].filter(b => {
      const r = b.getBoundingClientRect();
      return b.offsetParent && r.top < innerHeight && r.bottom > 0;
    }).length,
    overflow: document.documentElement.scrollWidth - innerWidth,
  }));
  check(failures, info.ridges === 1, `${label}: pembatas ${info.ridges}/1`);
  check(failures, info.primary <= 1, `${label}: btn-primary kelihatan ${info.primary}`);
  check(failures, info.overflow <= 0, `${label}: scroll horizontal ${info.overflow}px`);
}

export default async function run(page) {
  const failures = [];
  await page.setViewportSize({ width: 1280, height: 900 });

  await page.goto(`${BASE}/Account/Register`);
  await page.fill('#FullName', 'UI Test');
  await page.fill('#Email', EMAIL);
  await page.fill('#Password', PASSWORD);
  await page.fill('#ConfirmPassword', PASSWORD);
  await page.click('main button[type=submit]');
  await page.waitForLoadState();
  await page.goto(`${BASE}/Account/Login`);
  await page.fill('#Email', EMAIL);
  await page.fill('#Password', PASSWORD);
  await page.click('main button[type=submit]');
  await page.waitForLoadState();

  const gede = await mountainId(page, 'Gunung Gede');
  const klabat = await mountainId(page, 'Gunung Klabat');

  // Gunung tanpa foto → placeholder, bukan <img> rusak.
  await page.goto(`${BASE}/Booking/Create?mountainId=${klabat}`, { waitUntil: 'networkidle' });
  const noPhoto = await page.evaluate(() => ({
    placeholder: !!document.querySelector('.thumb.thumb--placeholder use[href$="#mountain"]'),
    img: !!document.querySelector('img.thumb'),
  }));
  check(failures, noPhoto.placeholder && !noPhoto.img, `Create Klabat: ${JSON.stringify(noPhoto)}`);

  await page.goto(`${BASE}/Booking/Create?mountainId=${gede}`, { waitUntil: 'networkidle' });
  const create = await page.evaluate(() => {
    const img = document.querySelector('aside img.thumb');
    return { src: img?.getAttribute('src'), loading: img?.loading };
  });
  check(failures, create.src?.endsWith('-640.webp') && create.loading === 'lazy', `Create: thumbnail ringkasan ${JSON.stringify(create)}`);
  await pageChecks(page, failures, 'Create');

  await page.selectOption('#Booking_HikingRouteId', { index: 1 });
  const date = new Date(); date.setDate(date.getDate() + 21);
  await page.fill('#Booking_ClimbDate', date.toISOString().slice(0, 10));
  await page.fill('#Booking_Participants_0__FullName', 'Peserta Satu');
  await page.fill('#Booking_Participants_0__NIK', '3201000000000001');
  await page.fill('#Booking_Participants_0__PhoneNumber', '081234567890');
  await page.fill('#Booking_Participants_0__EmergencyContactName', 'Kontak Satu');
  await page.fill('#Booking_Participants_0__EmergencyContactPhone', '081987654321');
  await page.click('aside button[type=submit].btn-primary');
  await page.waitForLoadState('networkidle');
  check(failures, /\/Booking\/Detail\/\d+$/.test(page.url()), `submit booking gagal: ${page.url()}`);

  const detail = await page.evaluate(() => {
    const img = document.querySelector('.page-header img.thumb');
    const r = img?.getBoundingClientRect();
    return { w: r?.width, h: r?.height, current: !!document.querySelector('.stepper-item.current') };
  });
  check(failures, detail.w === 96 && detail.h === 64, `Detail: thumbnail ${detail.w}x${detail.h}`);
  check(failures, detail.current, 'Detail: langkah stepper yang lagi jalan gak ditandai');
  await pageChecks(page, failures, 'Detail');

  await page.setViewportSize({ width: 375, height: 800 });
  await page.goto(`${BASE}/Booking/MyBookings`, { waitUntil: 'networkidle' });
  const mine = await page.evaluate(() => [...document.querySelectorAll('.booking-card')].map(c => {
    const t = c.querySelector('.thumb')?.getBoundingClientRect();
    return t ? `${t.width}x${t.height}` : 'none';
  }));
  check(failures, mine.length >= 1 && mine.every(s => s === '64x64'), `MyBookings HP: thumbnail ${mine}`);
  await pageChecks(page, failures, 'MyBookings 375');

  return result(failures);
}
