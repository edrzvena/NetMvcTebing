// Helper kecil buat UI check di tests/ui/. Jalanin lewat skill browser-automation:
//   node <dir skill>/browser.mjs http://localhost:5181/ --script tests/ui/<file>.mjs
// Tiap script return { pass, failures, ... } — "pass": true artinya lolos.
export function check(failures, cond, msg) {
  if (!cond) failures.push(msg);
}

export function result(failures, extra = {}) {
  return { pass: failures.length === 0, failures, ...extra };
}

// Catat semua request yang selesai (host + ukuran body), buat cek budget & "gak ada domain lain".
export function trackRequests(page) {
  const requests = [];
  page.on('requestfinished', async (req) => {
    // sizes() baca ukuran dari network layer — res.body() gagal buat font hasil preload.
    let bytes = 0;
    try { bytes = (await req.sizes()).responseBodySize; } catch { /* redirect dsb. */ }
    requests.push({ url: req.url(), type: req.resourceType(), bytes });
  });
  return requests;
}
