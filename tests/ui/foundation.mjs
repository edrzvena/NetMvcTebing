import { check, result, trackRequests } from './_assert.mjs';

// Task 4: font, warna dasar, sprite ikon, pola kontur, tanpa request ke domain lain.
export default async function run(page) {
  const failures = [];
  const requests = trackRequests(page);
  await page.goto('http://localhost:5181/', { waitUntil: 'networkidle' });

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

  const icons = await page.request.get('http://localhost:5181/images/icons.svg');
  check(failures, icons.status() === 200 && (await icons.text()).includes('id="star-fill"'), 'icons.svg gak ada / tanpa star-fill');
  // Sprite harus XML valid — kalau rusak, <use> gak nge-render apa-apa (ukuran 0) tanpa error di console.
  await page.goto('http://localhost:5181/Mountain', { waitUntil: 'networkidle' });
  const iconBox = await page.evaluate(() => {
    const use = document.querySelector('.tb-icon use');
    return use ? use.getBBox().width : -1;
  });
  check(failures, iconBox > 0, `ikon gak ke-render (lebar bbox ${iconBox})`);
  const contour = await page.request.get('http://localhost:5181/images/contour.svg');
  check(failures, contour.status() === 200 && (await contour.body()).length <= 3072, 'contour.svg gak ada / > 3 KB');

  const font = requests.find(r => r.url.includes('PlusJakartaSans'));
  check(failures, font, 'request file font gak ada');
  // Ukuran dari network layer bisa 0 (preload/cache), jadi ukur file-nya langsung.
  const fontBytes = font ? (await (await page.request.get(font.url)).body()).length : 0;
  check(failures, fontBytes > 0 && fontBytes <= 100 * 1024, `file font kosong / kegedean: ${fontBytes}`);
  const foreign = requests.filter(r => !new URL(r.url).hostname.match(/^(localhost|127\.0\.0\.1)$/) && !r.url.startsWith('data:'));
  check(failures, foreign.length === 0, `request ke domain lain: ${foreign.map(r => r.url).join(', ')}`);

  return result(failures, { fonts, fontBytes });
}
