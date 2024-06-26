using System.Threading.Tasks;
using VK_UI3.DB;
using VkNet.AudioBypassService.Abstractions;
using static VK_UI3.DB.AccountsDB;



public class RegistryExchangeTokenStore : IExchangeTokenStore
{
    private Accounts? _activeAccount;

    public ValueTask<string?> GetExchangeTokenAsync()
    {
        var a = AccountsDB.GetActiveAccounts();
        if (a.Count == 0)
        {
            return new ValueTask<string?>("");
        }
        _activeAccount = a[0];
        return new ValueTask<string?>(_activeAccount.ExchangeToken);
    }

    public ValueTask SetExchangeTokenAsync(string token)
    {
        if (_activeAccount != null)
        {
            if (token is null)
            {
                _activeAccount.Token = null;
            }
            else
            {
                _activeAccount.Token = token;
            }


            _activeAccount.Update();
        }
        else
        {
            _activeAccount = new Accounts();
            _activeAccount.ExchangeToken = token;
            _activeAccount.Update();

        }

        return ValueTask.CompletedTask;
    }


}
