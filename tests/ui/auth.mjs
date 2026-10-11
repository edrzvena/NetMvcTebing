import { check, result, trackRequests } from './_assert.mjs';

// Task 10: halaman akun terbelah — panel foto di desktop, HP gak download foto sama sekali, tanpa footer/pembatas.
let BASE; // diisi dari URL yang dibuka runner (lihat run())

export default async function run(page) {
  BASE = new URL(page.url()).origin;
  const failures = [];

  await page.setViewportSize({ width: 1280, height: 900 });
  for (const path of ['/Account/Login', '/Account/Register', '/Account/ForgotPassword']) {
    await page.goto(`${BASE}${path}`, { waitUntil: 'networkidle' });
    const info = await page.evaluate(() => {
      const photo = document.querySelector('.auth-photo');
      const form = document.querySelector('.auth-wrapper');
      return {
        visible: !!photo && getComputedStyle(photo).display !== 'none',
        widthRatio: photo ? photo.getBoundingClientRect().width / innerWidth : 0,
        quote: document.body.innerText.includes('Sampai ketemu di puncak.'),
        formWidth: form?.getBoundingClientRect().width ?? 9999,
        footer: !!document.querySelector('footer'),
        ridges: document.querySelectorAll('svg.ridge').length,
      };
    });
    check(failures, info.visible, `${path}: panel foto gak tampil di 1280`);
    check(failures, Math.abs(info.widthRatio - 0.5) <= 0.05, `${path}: lebar panel ${(info.widthRatio * 100).toFixed(0)}%`);
    check(failures, info.quote, `${path}: kalimat "Sampai ketemu di puncak." gak ada`);
    check(failures, info.formWidth <= 400, `${path}: form lebar ${info.formWidth}px > 400`);
    check(failures, !info.footer && info.ridges === 0, `${path}: footer=${info.footer}, pembatas=${info.ridges}`);
  }

  // HP: panel disembunyiin DAN fotonya gak ke-download.
  const requests = trackRequests(page);
  await page.setViewportSize({ width: 375, height: 800 });
  await page.goto(`${BASE}/Account/Login`, { waitUntil: 'networkidle' });
  const mobile = await page.evaluate(() => {
    const photo = document.querySelector('.auth-photo');
    return { display: photo ? getComputedStyle(photo).display : 'missing', overflow: document.documentElement.scrollWidth - innerWidth };
  });
  const photoRequests = requests.filter(r => /\/(images|uploads)\/mountains\//.test(r.url));
  check(failures, mobile.display === 'none', `375: panel foto display=${mobile.display}`);
  check(failures, photoRequests.length === 0, `375: foto tetap ke-download: ${photoRequests.map(r => r.url).join(', ')}`);
  check(failures, mobile.overflow <= 0, `375: scroll horizontal ${mobile.overflow}px`);

  return result(failures);
}
