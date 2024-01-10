using Microsoft.AspNetCore.Identity;

namespace ShoppingCartUI.Repositories
{
    public interface IUserRepository
    {
        protected internal string GetUserId(IHttpContextAccessor httpContextAccessor, UserManager<IdentityUser> userManager)
        {
            var principal = httpContextAccessor.HttpContext?.User;
            return userManager.GetUserId(principal!) ?? string.Empty;
        }
    }
}