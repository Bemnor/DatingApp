using System.Security.Cryptography.X509Certificates;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers;

public class LogUserActivity : IAsyncActionFilter
{
    //updates user's last-active property after every request recieved from that user.
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next
    )
    {
        //response
        var resultContext = await next();

        if (!resultContext.HttpContext.User.Identity.IsAuthenticated)
            return;

        var userId = resultContext.HttpContext.User.GetUserId();

        //repository for User(i.e object that translates the c# obj to an sql query..)
        var repo = resultContext.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
        var user = await repo.GetUserByIdAsync(int.Parse(userId));
        user.LastActive = DateTime.UtcNow;
        await repo.SaveAllAsync();
    }
}
