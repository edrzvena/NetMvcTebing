import { check, result } from './_assert.mjs';

// Task 5: navbar sticky 64px + menu aktif, footer hijau gelap 3 kolom + ridge di atasnya.
export default async function run(page) {
  const failures = [];
  for (const width of [1280, 375]) {
    await page.setViewportSize({ width, height: 900 });
    await page.goto('http://localhost:5181/Mountain', { waitUntil: 'networkidle' });
    const info = await page.evaluate(() => {
      const nav = document.querySelector('header nav.navbar');
      const footer = document.querySelector('footer.site-footer');
      // innerText ikut text-transform (judul kolom UPPERCASE), jadi bandingin tanpa peduli huruf besar.
      const text = (footer?.innerText ?? '').toLowerCase();
      return {
        navPosition: nav && getComputedStyle(nav.closest('header') ?? nav).position,
        navHeight: nav?.getBoundingClientRect().height,
        footerBg: footer && getComputedStyle(footer).backgroundColor,
        columns: ['tebing', 'jelajah', 'bantuan'].filter(t => text.includes(t)).length,
        privacy: !!footer?.querySelector('a[href*="Privacy"]'),
        credit: text.includes('wikimedia commons'),
        islandLinks: footer ? footer.querySelectorAll('a[href*="island="]').length : 0,
        ridges: document.querySelectorAll('svg.ridge').length,
        ridgeInFooter: !!footer?.querySelector('svg.ridge'),
        activeMenu: [...document.querySelectorAll('.navbar .nav-link.active')].map(a => a.textContent.trim()),
        overflow: document.documentElement.scrollWidth - innerWidth,
      };
    });
    check(failures, info.navPosition === 'sticky', `${width}: navbar bukan sticky (${info.navPosition})`);
    if (width === 1280) check(failures, Math.abs(info.navHeight - 64) <= 2, `navbar tinggi ${info.navHeight}`);
    check(failures, info.footerBg === 'rgb(18, 58, 45)', `${width}: footer bg ${info.footerBg}`);
    check(failures, info.columns === 3, `${width}: kolom footer ${info.columns}/3`);
    check(failures, info.privacy, 'link Kebijakan Privasi gak ada');
    check(failures, info.credit, 'kredit Wikimedia Commons gak ada di footer');
    check(failures, info.islandLinks === 6, `link pulau di footer ${info.islandLinks}/6`);
    check(failures, info.ridges === 1 && info.ridgeInFooter, `ridge di /Mountain: ${info.ridges}, di footer: ${info.ridgeInFooter}`);
    check(failures, info.activeMenu.includes('Gunung'), `menu aktif: ${info.activeMenu}`);
    check(failures, info.overflow <= 0, `${width}: scroll horizontal ${info.overflow}px`);
  }
  return result(failures);
}
