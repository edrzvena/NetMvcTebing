using Microsoft.AspNetCore.Mvc;
using Tebing.Services.Interfaces;

namespace Tebing.ViewComponents;

// Panel foto gunung acak di kiri halaman akun (docs/UI_UX.md §5.8). Dipanggil dari _AuthLayout,
// jadi tiap action di AccountController gak perlu ngisi data foto sendiri.
public class AuthPhotoViewComponent(IMountainService mountainService) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync() => View(await mountainService.GetRandomPhotoAsync());
}
