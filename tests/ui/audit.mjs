import { check, result, trackRequests } from './_assert.mjs';

// Task 12: audit budget (docs/UI_UX.md §9) & checklist review (§11) di halaman utama, 1280 & 375.
let BASE; // diisi dari URL yang dibuka runner (lihat run())
const KB = 1024;

async function detailUrl(page, name) {
  await page.goto(`${BASE}/Mountain?search=${encodeURIComponent(name)}`, { waitUntil: 'networkidle' });
  return page.evaluate(n => [...document.querySelectorAll('.mountain-card a.stretched-link')]
    .find(a => a.textContent.trim() === n)?.getAttribute('href'), name);
}

// Baca 1 piksel dari screenshot (decode PNG pakai canvas di halaman itu sendiri).
async function pixel(page, x, y) {
  const png = await page.screenshot({ clip: { x, y, width: 1, height: 1 } });
  return page.evaluate(async (src) => {
    const img = new Image();
    img.src = `data:image/png;base64,${src}`;
    await img.decode();
    const canvas = document.createElement('canvas');
    canvas.width = canvas.height = 1;
    const ctx = canvas.getContext('2d');
    ctx.drawImage(img, 0, 0);
    return [...ctx.getImageData(0, 0, 1, 1).data.slice(0, 3)];
  }, png.toString('base64'));
}

export default async function run(page) {
  BASE = new URL(page.url()).origin;
  const failures = [];
  const gede = await detailUrl(page, 'Gunung Gede');
  const klabat = await detailUrl(page, 'Gunung Klabat');
  const pages = [
    ['/', 3], ['/Mountain', 1], [gede, 2], [klabat, 2], ['/Account/Login', 0], ['/tidak-ada', 1],
  ];

  for (const width of [1280, 375]) {
    await page.setViewportSize({ width, height: width === 375 ? 800 : 900 });
    for (const [path, expectedRidges] of pages) {
      const label = `${width} ${path}`;
      const requests = trackRequests(page);
      await page.goto(BASE + path, { waitUntil: 'networkidle' });
      await page.waitForTimeout(500);

      const info = await page.evaluate(async () => {
        const cls = await new Promise((resolve) => {
          let total = 0;
          new PerformanceObserver((list) => {
            for (const e of list.getEntries()) if (!e.hadRecentInput) total += e.value;
          }).observe({ type: 'layout-shift', buffered: true });
          setTimeout(() => resolve(total), 300);
        });
        const hexStyle = [...document.querySelectorAll('[style]')].filter(el => /#[0-9a-f]{3,8}\b/i.test(el.getAttribute('style')));
        return {
          cls,
          ridges: document.querySelectorAll('svg.ridge').length,
          ridgeBytes: [...document.querySelectorAll('svg.ridge')].reduce((s, el) => s + el.outerHTML.length, 0),
          hexStyle: hexStyle.length,
          overflow: document.documentElement.scrollWidth - innerWidth,
          pageHeight: document.documentElement.scrollHeight,
        };
      });
      check(failures, info.cls < 0.05, `${label}: CLS ${info.cls.toFixed(3)}`);
      check(failures, info.ridges === expectedRidges, `${label}: pembatas ${info.ridges}/${expectedRidges}`);
      check(failures, info.ridgeBytes <= 8 * KB, `${label}: markup pembatas ${info.ridgeBytes} B > 8 KB`);
      check(failures, info.hexStyle === 0, `${label}: ${info.hexStyle} elemen pakai hex di atribut style`);
      check(failures, info.overflow <= 0, `${label}: scroll horizontal ${info.overflow}px`);

      const foreign = requests.filter(r => !/^(localhost|127\.0\.0\.1)$/.test(new URL(r.url).hostname) && !r.url.startsWith('data:'));
      check(failures, foreign.length === 0, `${label}: request ke domain lain ${foreign.map(r => r.url)}`);
      if (path === '/') {
        const imageBytes = requests.filter(r => r.type === 'image').reduce((s, r) => s + r.bytes, 0);
        check(failures, imageBytes <= 500 * KB, `${label}: foto layar pertama ${Math.round(imageBytes / KB)} KB`);
      }

      // Maks 1 btn-primary kelihatan per layar, dicek tiap posisi scroll setinggi layar.
      for (let y = 0; y < info.pageHeight; y += page.viewportSize().height) {
        await page.evaluate(top => window.scrollTo(0, top), y);
        await page.waitForTimeout(80);
        // Tombol "Daftar" di navbar sengaja primary (§4.5), jadi navbar gak dihitung.
        const visible = await page.evaluate(() => [...document.querySelectorAll('.btn-primary')].filter((b) => !b.closest('header')).filter((b) => {
          const r = b.getBoundingClientRect();
          return b.offsetParent && r.bottom > 0 && r.top < innerHeight;
        }).length);
        check(failures, visible <= 1, `${label} @scroll ${y}: ${visible} btn-primary kelihatan`);
      }
    }
  }

  // Ukuran aset bersama.
  const font = (await (await page.request.get(`${BASE}/lib/fonts/plus-jakarta-sans/PlusJakartaSans-latin.var.woff2`)).body()).length;
  const js = (await (await page.request.get(`${BASE}/js/carousel.js`)).body()).length;
  check(failures, font <= 100 * KB, `font ${font} B > 100 KB`);
  check(failures, js <= 5 * KB, `carousel.js ${js} B > 5 KB`);

  // Focus ring kelihatan di 15 elemen pertama yang bisa di-Tab (Home, 1280).
  await page.setViewportSize({ width: 1280, height: 900 });
  await page.goto(BASE + '/', { waitUntil: 'networkidle' });
  for (let i = 0; i < 15; i++) {
    await page.keyboard.press('Tab');
    const focus = await page.evaluate(() => {
      const el = document.activeElement;
      if (!el || el === document.body) return null;
      const s = getComputedStyle(el);
      return { tag: el.tagName, text: (el.textContent || el.getAttribute('aria-label') || '').trim().slice(0, 30),
               outline: s.outlineStyle !== 'none' && parseFloat(s.outlineWidth) >= 2, shadow: s.boxShadow !== 'none' };
    });
    if (focus) check(failures, focus.outline || focus.shadow, `focus ring gak kelihatan di ${focus.tag} "${focus.text}"`);
  }

  // Gak ada garis tipis bocor di bawah pembatas (zoom browser 90% & 125% ≈ deviceScaleFactor):
  // piksel tepat di bawah ridge harus sama dengan piksel beberapa px di bawahnya (warna section berikutnya).
  for (const scale of [0.9, 1.25]) {
    const zoomed = await page.context().browser().newPage({ deviceScaleFactor: scale, viewport: { width: 1280, height: 900 } });
    await zoomed.goto(BASE + '/', { waitUntil: 'networkidle' });
    const count = await zoomed.evaluate(() => document.querySelectorAll('svg.ridge').length);
    for (let i = 0; i < count; i++) {
      const box = await zoomed.evaluate((index) => {
        const el = document.querySelectorAll('svg.ridge')[index];
        el.scrollIntoView({ block: 'center', behavior: 'instant' });
        const r = el.getBoundingClientRect();
        return { x: Math.round(innerWidth / 2), bottom: Math.min(Math.floor(r.bottom), innerHeight - 6) };
      }, i);
      await zoomed.waitForTimeout(150);
      const just = await pixel(zoomed, box.x, box.bottom);
      const below = await pixel(zoomed, box.x, box.bottom + 4);
      const diff = Math.max(...just.map((v, k) => Math.abs(v - below[k])));
      // Toleransi 40: garis kontur di footer geser warna ±17; garis bocor beneran (krem di atas hijau) > 200.
      check(failures, diff <= 40, `zoom ${scale}: garis bocor di bawah pembatas #${i + 1} (${just} vs ${below})`);
    }
    await zoomed.close();
  }

  return result(failures);
}
