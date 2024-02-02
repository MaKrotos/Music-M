using System;
using System.Linq;
using System.Threading.Tasks;
using VkNet.Abstractions;
using VkNet.Exception;


public class TokenChecker
{
    private IUsersCategory _usersCategory;

    public TokenChecker(IUsersCategory usersCategory)
    {
        _usersCategory = usersCategory;
    }

    public async Task<bool> IsTokenValid()
    {
        try
        {
            await _usersCategory.GetAsync(Enumerable.Empty<long>());
        }
        catch (Exception e) when (e is VkApiException or InvalidOperationException)
        {
            return false;
        }

        return true;
    }
}

