import { check, result, trackRequests } from './_assert.mjs';

// Task 7: banner carousel (manual + peek), search mengambang, chip pulau, 3 rail, 3 pembatas punggungan.
let BASE; // diisi dari URL yang dibuka runner (lihat run())

export default async function run(page) {
  BASE = new URL(page.url()).origin;
  const failures = [];
  const requests = trackRequests(page);
  await page.setViewportSize({ width: 1280, height: 900 });
  await page.goto(`${BASE}/`, { waitUntil: 'networkidle' });

  const imageBytes = requests.filter(r => r.type === 'image').reduce((sum, r) => sum + r.bytes, 0);
  check(failures, imageBytes <= 500 * 1024, `foto layar pertama ${Math.round(imageBytes / 1024)} KB > 500 KB`);

  const hero = await page.evaluate(() => {
    const root = document.querySelector('.hero[data-carousel]');
    const items = root ? [...root.querySelectorAll('[data-carousel-item]')] : [];
    const dots = root ? [...root.querySelectorAll('[data-carousel-dot]')] : [];
    const form = document.querySelector('form[role=search]');
    return {
      exists: !!root,
      label: root?.getAttribute('aria-roledescription'),
      items: items.length,
      dots: dots.length,
      peek: items[1] ? items[1].getBoundingClientRect().left < innerWidth : false,
      prevDisabled: root?.querySelector('[data-carousel-prev]')?.disabled,
      dot0: dots[0]?.getAttribute('aria-current'),
      dotSize: dots[0] ? (({ width, height }) => Math.min(width, height))(dots[0].getBoundingClientRect()) : 0,
      firstImg: items[0]?.querySelector('img')?.getAttribute('fetchpriority'),
      secondLoading: items[1]?.querySelector('img')?.loading,
      overlapsSearch: form && root ? form.getBoundingClientRect().top < root.getBoundingClientRect().bottom : false,
      ridges: document.querySelectorAll('svg.ridge').length,
    };
  });
  check(failures, hero.exists && hero.label === 'carousel', 'banner carousel gak ada');
  check(failures, hero.items >= 1 && hero.items <= 5 && hero.items === hero.dots, `slide ${hero.items}, titik ${hero.dots}`);
  check(failures, hero.peek, 'slide 2 gak nongol (peek)');
  check(failures, hero.prevDisabled === true, 'panah ‹ gak disabled di awal');
  check(failures, hero.dot0 === 'true', 'titik 1 gak aktif di awal');
  check(failures, hero.dotSize >= 44, `area klik titik ${hero.dotSize}px < 44px`);
  check(failures, hero.firstImg === 'high' && hero.secondLoading === 'lazy', `prioritas foto: ${hero.firstImg}/${hero.secondLoading}`);
  check(failures, hero.overlapsSearch, 'search gak numpuk bagian bawah banner');
  check(failures, hero.ridges === 3, `pembatas punggungan ${hero.ridges}/3`);

  // Panah › geser ke slide 2, titik ikut pindah.
  const track = page.locator('.hero [data-carousel-track]');
  await page.locator('.hero [data-carousel-next]').click();
  await page.waitForFunction(() => document.querySelector('.hero [data-carousel-track]').scrollLeft > 0);
  await page.waitForTimeout(700);
  const dot1 = await page.locator('.hero [data-carousel-dot]').nth(1).getAttribute('aria-current');
  check(failures, dot1 === 'true', `setelah ›, titik 2 aria-current=${dot1}`);

  // Ujung kanan → › disabled.
  await track.evaluate(el => el.scrollTo({ left: el.scrollWidth, behavior: 'auto' }));
  await page.waitForTimeout(400);
  check(failures, await page.locator('.hero [data-carousel-next]').isDisabled(), 'panah › gak disabled di ujung');

  // Gak ada autoplay: diam 8 detik, posisi gak berubah.
  await track.evaluate(el => el.scrollTo({ left: 0, behavior: 'auto' }));
  await page.waitForTimeout(300);
  const before = await track.evaluate(el => el.scrollLeft);
  await page.waitForTimeout(8000);
  check(failures, before === await track.evaluate(el => el.scrollLeft), 'banner geser sendiri (autoplay)');

  // Fokus keyboard ke link di slide 3 → slide 3 ke-scroll kelihatan.
  if (hero.items >= 3) {
    await page.locator('.hero [data-carousel-item]').nth(2).locator('a').focus();
    await page.waitForTimeout(700);
    const left = await page.locator('.hero [data-carousel-item]').nth(2).evaluate(el => el.getBoundingClientRect().left);
    check(failures, left >= 0 && left < 400, `slide 3 gak kelihatan setelah fokus (left ${left})`);
  }

  const rails = await page.evaluate(() => {
    const heroLinks = [...document.querySelectorAll('.hero [data-carousel-item] a')].map(a => a.getAttribute('href'));
    const sections = [...document.querySelectorAll('section[aria-labelledby]')].filter(s => s.querySelector('[data-carousel-track]'));
    const chips = [...document.querySelectorAll('.chip-nav a')];
    return {
      count: sections.length,
      sizes: sections.map(s => s.querySelectorAll('[data-carousel-item]').length),
      picksOverlap: sections[0] ? [...sections[0].querySelectorAll('a.stretched-link')].some(a => heroLinks.includes(a.getAttribute('href'))) : true,
      firstChip: chips[0]?.textContent.trim(),
      firstChipCurrent: chips[0]?.getAttribute('aria-current'),
      jawaChip: chips.some(a => a.href.includes('island=Jawa')),
      howItWorks: !!document.querySelector('#cara-kerja'),
      h1: document.querySelectorAll('h1').length,
    };
  });
  check(failures, rails.count === 3, `rail ${rails.count}/3`);
  check(failures, rails.sizes.every(n => n >= 1 && n <= 10), `isi rail ${rails.sizes}`);
  check(failures, !rails.picksOverlap, 'gunung di banner muncul lagi di rail "Gunung pilihan"');
  check(failures, rails.firstChip?.startsWith('Semua') && rails.firstChipCurrent === 'page', `chip pertama: ${rails.firstChip} / ${rails.firstChipCurrent}`);
  check(failures, rails.jawaChip, 'chip Jawa gak ada');
  check(failures, rails.howItWorks, 'section #cara-kerja gak ada');
  check(failures, rails.h1 === 1, `jumlah h1 ${rails.h1}`);

  await page.emulateMedia({ reducedMotion: 'reduce' });
  check(failures, await track.evaluate(el => getComputedStyle(el).scrollBehavior) === 'auto', 'reduced-motion: scroll-behavior bukan auto');
  await page.emulateMedia({ reducedMotion: 'no-preference' });

  await page.setViewportSize({ width: 375, height: 800 });
  await page.goto(`${BASE}/`, { waitUntil: 'networkidle' });
  const mobile = await page.evaluate(() => ({
    arrows: getComputedStyle(document.querySelector('.hero [data-carousel-next]')).display,
    overflow: document.documentElement.scrollWidth - innerWidth,
  }));
  check(failures, mobile.arrows === 'none', `375: panah carousel tampil (${mobile.arrows})`);
  check(failures, mobile.overflow <= 0, `375: scroll horizontal ${mobile.overflow}px`);

  return result(failures, { imageKB: Math.round(imageBytes / 1024), hero, rails });
}
