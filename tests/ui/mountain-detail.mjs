import { check, result } from './_assert.mjs';

// Task 9: Detail Gunung — header foto full-bleed, 4 stat tile, panel booking sticky, rail pulau yang sama, 2 pembatas.
async function detailUrl(page, name) {
  await page.goto(`${BASE}/Mountain?search=${encodeURIComponent(name)}`, { waitUntil: 'networkidle' });
  return page.evaluate(n => [...document.querySelectorAll('.mountain-card a.stretched-link')]
    .find(a => a.textContent.trim() === n)?.href, name);
}

let BASE; // diisi dari URL yang dibuka runner (lihat run())

export default async function run(page) {
  BASE = new URL(page.url()).origin;
  const failures = [];
  await page.setViewportSize({ width: 1280, height: 900 });
  const gede = await detailUrl(page, 'Gunung Gede');
  const klabat = await detailUrl(page, 'Gunung Klabat');
  check(failures, gede && klabat, 'URL detail Gede/Klabat gak ketemu');
  if (!gede || !klabat) return result(failures);

  await page.goto(gede, { waitUntil: 'networkidle' });
  const info = await page.evaluate(() => {
    const hero = document.querySelector('.mountain-hero');
    const img = hero?.querySelector('img');
    const h1 = hero?.querySelector('h1');
    const panel = document.querySelector('.booking-panel');
    const rail = [...document.querySelectorAll('section.rail')].find(s => s.textContent.includes('Gunung lain di'));
    return {
      heroWidth: hero?.getBoundingClientRect().width,
      fetchpriority: img?.getAttribute('fetchpriority'),
      loading: img?.loading,
      h1Color: h1 && getComputedStyle(h1).color,
      credit: !!hero?.querySelector('a[href*="commons.wikimedia.org"]'),
      stats: [...document.querySelectorAll('.stat-tile__label')].map(e => e.textContent.trim()),
      panelSticky: panel && getComputedStyle(panel).position,
      railTitle: rail?.querySelector('h2')?.textContent.trim(),
      railSelf: rail ? [...rail.querySelectorAll('.card-title')].some(t => t.textContent.trim() === 'Gunung Gede') : true,
      railCount: rail ? rail.querySelectorAll('[data-carousel-item]').length : 0,
      ridges: document.querySelectorAll('svg.ridge').length,
    };
  });
  check(failures, Math.abs(info.heroWidth - 1280) <= 1, `header foto lebar ${info.heroWidth} (harus full-bleed)`);
  check(failures, info.fetchpriority === 'high' && info.loading !== 'lazy', `foto header ${info.fetchpriority}/${info.loading}`);
  check(failures, info.h1Color === 'rgb(255, 255, 255)', `h1 di header warnanya ${info.h1Color}`);
  check(failures, info.credit, 'kredit foto Commons gak ada di header');
  check(failures, ['Ketinggian', 'Jalur aktif', 'Harga mulai', 'Rating'].every(l => info.stats.includes(l)), `stat tile: ${info.stats}`);
  check(failures, info.panelSticky === 'sticky', `panel booking ${info.panelSticky}`);
  check(failures, info.railTitle === 'Gunung lain di Jawa', `judul rail: ${info.railTitle}`);
  check(failures, !info.railSelf && info.railCount >= 1 && info.railCount <= 10, `rail: self=${info.railSelf}, isi=${info.railCount}`);
  check(failures, info.ridges === 2, `pembatas ${info.ridges}/2`);

  await page.setViewportSize({ width: 375, height: 800 });
  await page.goto(gede, { waitUntil: 'networkidle' });
  const mobile = await page.evaluate(() => {
    const panel = document.querySelector('.booking-panel');
    const stats = document.querySelector('.stat-tiles');
    const visiblePrimary = [...document.querySelectorAll('.btn-primary')].filter(b => b.offsetParent).length;
    return {
      position: panel && getComputedStyle(panel).position,
      afterStats: panel && stats ? panel.getBoundingClientRect().top > stats.getBoundingClientRect().top : false,
      visiblePrimary,
      overflow: document.documentElement.scrollWidth - innerWidth,
    };
  });
  check(failures, mobile.position !== 'sticky', '375: panel booking masih sticky');
  check(failures, mobile.afterStats, '375: panel booking gak di bawah stat tile');
  check(failures, mobile.visiblePrimary === 1, `375: btn-primary kelihatan ${mobile.visiblePrimary}`);
  check(failures, mobile.overflow <= 0, `375: scroll horizontal ${mobile.overflow}px`);

  await page.setViewportSize({ width: 1280, height: 900 });
  await page.goto(klabat, { waitUntil: 'networkidle' });
  const noPhoto = await page.evaluate(() => {
    const hero = document.querySelector('.mountain-hero');
    return { img: !!hero?.querySelector('img'), bg: hero && getComputedStyle(hero).backgroundColor, h1: !!hero?.querySelector('h1') };
  });
  check(failures, !noPhoto.img && noPhoto.bg === 'rgb(18, 58, 45)' && noPhoto.h1, `Klabat tanpa foto: ${JSON.stringify(noPhoto)}`);

  return result(failures, { info });
}
