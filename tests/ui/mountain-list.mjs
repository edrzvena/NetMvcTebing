import { check, result } from './_assert.mjs';

// Task 8: Daftar Gunung — header eyebrow, chip pulau (bawa filter lain), grid kartu v2, 1 pembatas.
const JAWA = ['Jawa', 'Banten', 'Jakarta', 'Yogyakarta'];

let BASE; // diisi dari URL yang dibuka runner (lihat run())

export default async function run(page) {
  BASE = new URL(page.url()).origin;
  const failures = [];
  await page.setViewportSize({ width: 1280, height: 900 });
  await page.goto(`${BASE}/Mountain`, { waitUntil: 'networkidle' });

  const info = await page.evaluate(() => {
    const cards = [...document.querySelectorAll('.mountain-grid .mountain-card')];
    const tops = cards.slice(0, 8).map(c => Math.round(c.getBoundingClientRect().top));
    return {
      eyebrow: document.querySelector('.page-header .eyebrow')?.textContent.trim(),
      h1: document.querySelector('h1')?.textContent.trim(),
      meta: document.querySelector('.page-header p')?.textContent.trim(),
      chips: document.querySelectorAll('.chip-nav a').length,
      province: !!document.querySelector('select[name=province]'),
      difficulty: !!document.querySelector('select[name=difficulty]'),
      firstRow: tops.filter(t => t === tops[0]).length,
      loading: cards.map(c => c.querySelector('img')?.loading ?? 'none'),
      ridges: document.querySelectorAll('svg.ridge').length,
    };
  });
  check(failures, info.eyebrow?.toUpperCase() === 'JELAJAH', `eyebrow: ${info.eyebrow}`);
  check(failures, info.h1 === 'Daftar Gunung', `h1: ${info.h1}`);
  // Chip = "Semua" + 1 per pulau → jumlah wilayah = chip - 1.
  check(failures, new RegExp(`^\\d+ gunung di ${info.chips - 1} wilayah$`).test(info.meta ?? ''), `meta: ${info.meta}`);
  check(failures, !info.province, 'dropdown provinsi masih ada');
  check(failures, info.difficulty, 'dropdown kesulitan gak ada');
  check(failures, info.firstRow === 4, `kolom grid di 1280 (xl): ${info.firstRow}, harusnya 4`);
  const photoLoading = info.loading.filter(l => l !== 'none');
  check(failures, info.loading.slice(0, 6).every(l => l === 'eager' || l === 'none'), `6 kartu pertama: ${info.loading.slice(0, 6)}`);
  check(failures, info.loading.slice(6).every(l => l === 'lazy' || l === 'none') && photoLoading.length > 6, 'kartu setelah 6 gak lazy');
  check(failures, info.ridges === 1, `pembatas ${info.ridges}/1`);

  await page.goto(`${BASE}/Mountain?search=gunung&difficulty=Hard`, { waitUntil: 'networkidle' });
  const jawaHref = await page.evaluate(() => [...document.querySelectorAll('.chip-nav a')].find(a => a.textContent.includes('Jawa'))?.getAttribute('href') ?? '');
  check(failures, jawaHref.includes('island=Jawa') && jawaHref.includes('search=gunung') && jawaHref.includes('difficulty=Hard'), `chip Jawa href: ${jawaHref}`);

  await page.goto(`${BASE}/Mountain?island=Jawa`, { waitUntil: 'networkidle' });
  const jawa = await page.evaluate(() => ({
    active: [...document.querySelectorAll('.chip-nav a[aria-current=page]')].map(a => a.textContent.trim()),
    provinces: [...document.querySelectorAll('.mountain-card .mountain-card__meta .text-nowrap:first-child')].map(s => s.textContent.trim()),
  }));
  check(failures, jawa.active.length === 1 && jawa.active[0].startsWith('Jawa'), `chip aktif: ${jawa.active}`);
  check(failures, jawa.provinces.length > 0 && jawa.provinces.every(p => JAWA.some(j => p.includes(j))), `provinsi bukan Jawa: ${jawa.provinces.filter(p => !JAWA.some(j => p.includes(j)))}`);

  await page.setViewportSize({ width: 375, height: 800 });
  await page.goto(`${BASE}/Mountain`, { waitUntil: 'networkidle' });
  const mobile = await page.evaluate(() => {
    const cards = [...document.querySelectorAll('.mountain-grid .mountain-card')].slice(0, 2);
    return { oneColumn: cards.length === 2 && cards[0].getBoundingClientRect().top !== cards[1].getBoundingClientRect().top,
             overflow: document.documentElement.scrollWidth - innerWidth };
  });
  check(failures, mobile.oneColumn, '375: grid bukan 1 kolom');
  check(failures, mobile.overflow <= 0, `375: scroll horizontal ${mobile.overflow}px`);

  return result(failures, { info });
}
