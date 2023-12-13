using Microsoft.AspNetCore.Identity;
using ShoppingCartUI.Constants;

namespace ShoppingCartUI.Data
{
    public class Seeder
    {
        public static async Task SeedDefaultData(IServiceProvider service)
        {
            var userMgr = service.GetService<UserManager<IdentityUser>>();
            var roleMgr = service.GetService<RoleManager<IdentityRole>>();
            //adding some roles to db
            await roleMgr.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
            await roleMgr.CreateAsync(new IdentityRole(Roles.User.ToString()));

            // create admin user
            var admin = new IdentityUser
            {
                UserName = "abc@gmail.com",
                Email = "abc@gmail.com",
                EmailConfirmed = true

            };

            var UserInDb = await userMgr.FindByEmailAsync(admin.Email);
            if (UserInDb is null)
            {
                await userMgr.CreateAsync(admin, "Admin@123");
                await userMgr.AddToRoleAsync(admin, Roles.Admin.ToString());
            }

        }
    }
}
