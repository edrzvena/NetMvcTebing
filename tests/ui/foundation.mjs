import { check, result, trackRequests } from './_assert.mjs';

// Task 4: font, warna dasar, sprite ikon, pola kontur, tanpa request ke domain lain.
let BASE; // diisi dari URL yang dibuka runner (lihat run())

export default async function run(page) {
  BASE = new URL(page.url()).origin;
  const failures = [];
  const requests = trackRequests(page);
  await page.goto(`${BASE}/`, { waitUntil: 'networkidle' });

  const fonts = await page.evaluate(async () => {
    await document.fonts.ready;
    return {
      // document.fonts.check() true juga kalau font-nya gak didefinisiin — jadi cek FontFace-nya langsung.
      loaded: [...document.fonts].some(f => f.family.replace(/"/g, '') === 'Plus Jakarta Sans' && f.status === 'loaded'),
      family: getComputedStyle(document.body).fontFamily,
      bg: getComputedStyle(document.body).backgroundColor,
    };
  });
  check(failures, fonts.loaded, 'font Plus Jakarta Sans belum kemuat');
  check(failures, fonts.family.replace(/"/g, '').startsWith('Plus Jakarta Sans'), `font-family body: ${fonts.family}`);
  check(failures, fonts.bg === 'rgb(246, 243, 238)', `background body: ${fonts.bg}`);

  const icons = await page.request.get(`${BASE}/images/icons.svg`);
  check(failures, icons.status() === 200 && (await icons.text()).includes('id="star-fill"'), 'icons.svg gak ada / tanpa star-fill');
  // Sprite harus XML valid — kalau rusak, <use> gak nge-render apa-apa (ukuran 0) tanpa error di console.
  await page.goto(`${BASE}/Mountain`, { waitUntil: 'networkidle' });
  const iconBox = await page.evaluate(() => {
    const use = document.querySelector('.tb-icon use');
    return use ? use.getBBox().width : -1;
  });
  check(failures, iconBox > 0, `ikon gak ke-render (lebar bbox ${iconBox})`);
  // Pola kontur (spiral) dibuang — dekorasi tanpa makna.
  const contour = await page.request.get(`${BASE}/images/contour.svg`);
  check(failures, contour.status() === 404, `contour.svg masih ada (status ${contour.status()})`);

  const font = requests.find(r => r.url.includes('PlusJakartaSans'));
  check(failures, font, 'request file font gak ada');
  // Ukuran dari network layer bisa 0 (preload/cache), jadi ukur file-nya langsung.
  const fontBytes = font ? (await (await page.request.get(font.url)).body()).length : 0;
  check(failures, fontBytes > 0 && fontBytes <= 100 * 1024, `file font kosong / kegedean: ${fontBytes}`);
  const foreign = requests.filter(r => !new URL(r.url).hostname.match(/^(localhost|127\.0\.0\.1)$/) && !r.url.startsWith('data:'));
  check(failures, foreign.length === 0, `request ke domain lain: ${foreign.map(r => r.url).join(', ')}`);

  return result(failures, { fonts, fontBytes });
}
