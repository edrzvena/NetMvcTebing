import { check, result } from './_assert.mjs';

// Task 11 (bagian admin): tab status Kelola Booking pakai chip. Butuh akun uitest@tebing.local yang
// udah dijadiin Admin (Role = 1) di DB dev setelah booking.mjs — dan dihapus lagi setelahnya.
let BASE; // diisi dari URL yang dibuka runner (lihat run())

export default async function run(page) {
  BASE = new URL(page.url()).origin;
  const failures = [];
  await page.setViewportSize({ width: 1280, height: 900 });
  await page.goto(`${BASE}/Account/Login`);
  await page.fill('#Email', 'uitest@tebing.local');
  await page.fill('#Password', 'uitest123');
  await page.click('main button[type=submit]');
  await page.waitForLoadState();

  await page.goto(`${BASE}/Booking/Manage?status=Pending`, { waitUntil: 'networkidle' });
  const info = await page.evaluate(() => {
    const chips = [...document.querySelectorAll('nav.chip-nav[aria-label] a.chip')];
    return {
      chips: chips.map(a => a.textContent.replace(/\s+/g, ' ').trim()),
      current: chips.filter(a => a.getAttribute('aria-current') === 'page').map(a => a.textContent.trim()),
      oldPills: !!document.querySelector('.nav-pills'),
      ridges: document.querySelectorAll('svg.ridge').length,
    };
  });
  check(failures, info.chips.length === 6, `chip status ${info.chips.length}/6: ${info.chips}`);
  check(failures, info.current.length === 1 && info.current[0].startsWith('Menunggu'), `chip aktif: ${info.current}`);
  check(failures, !info.oldPills, 'nav-pills lama masih ada');
  check(failures, info.ridges === 1, `pembatas ${info.ridges}/1`);

  return result(failures, { info });
}
