using System;
using System.Linq;
using System.Threading.Tasks;
using VkNet.Abstractions;
using VkNet.Exception;


public class TokenChecker(IUsersCategory usersCategory)
{
    public async Task<bool> IsTokenValid()
    {
        try
        {
            await usersCategory.GetAsync(Enumerable.Empty<long>());
        }
        catch (Exception e) when (e is VkApiException or InvalidOperationException)
        {
            return false;
        }

        return true;
    }
}