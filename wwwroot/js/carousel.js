// Panah & titik buat carousel scroll-snap (docs/UI_UX.md §4.1, §4.3). Gak ada autoplay.
// Geser tetap jalan tanpa file ini (scroll native + scroll-snap) — JS cuma nambah tombol.
//
// Markup: [data-carousel] > [data-carousel-track] > [data-carousel-item]…
//         tombol [data-carousel-prev] / [data-carousel-next], opsional button[data-carousel-dot="<index>"]
(() => {
  const behavior = () => (matchMedia('(prefers-reduced-motion: reduce)').matches ? 'auto' : 'smooth');

  document.querySelectorAll('[data-carousel]').forEach((root) => {
    const track = root.querySelector('[data-carousel-track]');
    if (!track) return;
    const items = [...track.querySelectorAll('[data-carousel-item]')];
    const prev = root.querySelector('[data-carousel-prev]');
    const next = root.querySelector('[data-carousel-next]');
    const dots = [...root.querySelectorAll('[data-carousel-dot]')];

    // Jarak antar item (lebar + gap), diukur dari posisi item biar gak perlu tau CSS-nya.
    const step = () => (items.length > 1 ? items[1].offsetLeft - items[0].offsetLeft : track.clientWidth);

    const updateArrows = () => {
      const max = track.scrollWidth - track.clientWidth;
      if (prev) prev.disabled = track.scrollLeft <= 2;
      if (next) next.disabled = track.scrollLeft >= max - 2;
    };

    prev?.addEventListener('click', () => track.scrollBy({ left: -step(), behavior: behavior() }));
    next?.addEventListener('click', () => track.scrollBy({ left: step(), behavior: behavior() }));

    dots.forEach((dot) => {
      dot.addEventListener('click', () => {
        const item = items[Number(dot.dataset.carouselDot)];
        if (item) track.scrollTo({ left: item.offsetLeft - items[0].offsetLeft, behavior: behavior() });
      });
    });

    if (dots.length && 'IntersectionObserver' in window) {
      const observer = new IntersectionObserver((entries) => {
        entries.forEach((entry) => {
          if (!entry.isIntersecting) return;
          const index = items.indexOf(entry.target);
          dots.forEach((dot, i) => dot.setAttribute('aria-current', i === index ? 'true' : 'false'));
        });
      }, { root: track, threshold: 0.6 });
      items.forEach((item) => observer.observe(item));
    }

    track.addEventListener('scroll', updateArrows, { passive: true });
    window.addEventListener('resize', updateArrows);
    updateArrows();
  });
})();
