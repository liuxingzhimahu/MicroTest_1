using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User.API.Data
{
    public static class UserContextSeed
    {

        public async static Task SeedAsync(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var userContext = scope.ServiceProvider.GetRequiredService<UserContext>();
                if (!userContext.Users.Any())
                {
                    userContext.Users.Add(new Models.APPUser
                    {
                        Name = "jesse"
                    });
                    await userContext.SaveChangesAsync();
                }

            }
        }

    }
}
