import { check, result } from './_assert.mjs';

// Task 6: kartu gunung v2 (foto 4:3, badge di atas foto, placeholder tanpa foto, hover) + empty state ikon.
let BASE; // diisi dari URL yang dibuka runner (lihat run())

export default async function run(page) {
  BASE = new URL(page.url()).origin;
  const failures = [];
  await page.setViewportSize({ width: 1280, height: 900 });
  await page.goto(`${BASE}/Mountain`, { waitUntil: 'networkidle' });

  const info = await page.evaluate(() => {
    const cards = [...document.querySelectorAll('.mountain-card')];
    const first = cards[0];
    const media = first?.querySelector('.mountain-card__media');
    const rect = media?.getBoundingClientRect();
    const klabat = cards.find(c => c.textContent.includes('Gunung Klabat'));
    const klabatTop = klabat?.getBoundingClientRect().top;
    const neighbour = cards.find(c => c !== klabat && Math.abs(c.getBoundingClientRect().top - klabatTop) < 1);
    return {
      count: cards.length,
      ratio: rect ? rect.width / rect.height : 0,
      firstLoading: first?.querySelector('.mountain-card__media img')?.loading,
      tenthLoading: cards[9]?.querySelector('.mountain-card__media img')?.loading,
      badgeInMedia: !!media?.querySelector('.mountain-card__badge--start'),
      klabatPlaceholder: !!klabat?.querySelector('.mountain-card__placeholder use[href$="#mountain"]'),
      klabatImg: !!klabat?.querySelector('img'),
      heightDiff: klabat && neighbour ? Math.abs(klabat.getBoundingClientRect().height - neighbour.getBoundingClientRect().height) : 99,
      noReviewText: cards.some(c => c.textContent.includes('Belum ada ulasan')),
    };
  });
  check(failures, info.count >= 40, `jumlah kartu ${info.count}`);
  check(failures, Math.abs(info.ratio - 4 / 3) < 0.03, `rasio foto ${info.ratio.toFixed(3)} (harus 4:3)`);
  check(failures, info.firstLoading === 'eager', `kartu 1 loading=${info.firstLoading}`);
  check(failures, info.tenthLoading === 'lazy', `kartu 10 loading=${info.tenthLoading}`);
  check(failures, info.badgeInMedia, 'badge kesulitan gak di atas foto');
  check(failures, info.klabatPlaceholder && !info.klabatImg, 'Gunung Klabat (tanpa foto) gak pakai placeholder ikon');
  check(failures, info.heightDiff <= 1, `tinggi kartu tanpa foto beda ${info.heightDiff}px dari tetangganya`);
  check(failures, info.noReviewText, 'teks "Belum ada ulasan" gak ada');

  const card = page.locator('.mountain-card').first();
  await card.hover();
  await page.waitForTimeout(300);
  const hovered = await card.evaluate(el => getComputedStyle(el).transform);
  check(failures, hovered !== 'none', `hover gak ngangkat kartu (transform ${hovered})`);

  await page.emulateMedia({ reducedMotion: 'reduce' });
  const duration = await card.evaluate(el => getComputedStyle(el).transitionDuration);
  check(failures, duration.split(',').every(d => parseFloat(d) === 0), `reduced-motion: transition ${duration}`);
  await page.emulateMedia({ reducedMotion: 'no-preference' });

  await page.goto(`${BASE}/Mountain?search=zzzz`, { waitUntil: 'networkidle' });
  const empty = await page.evaluate(() => !!document.querySelector('.empty-state use[href$="#mountain"]'));
  check(failures, empty, 'empty state gak punya ikon gunung');

  return result(failures, { info });
}
