using System.Threading.Tasks;
using VK_UI3.DB;
using VkNet.AudioBypassService.Abstractions;
using static VK_UI3.DB.AccountsDB;



public class RegistryDeviceIdStore : IDeviceIdStore
{
    private Accounts? _activeAccount;

    public ValueTask<string?> GetDeviceIdAsync()
    {
        var a = AccountsDB.GetActiveAccounts();
        if (a.Count == 0)
        {
            return new ValueTask<string?>("");
        }
        _activeAccount = a[0];

        return new ValueTask<string?>(_activeAccount.DeviceId);
    }

    public ValueTask SetDeviceIdAsync(string deviceId)
    {
        if (_activeAccount != null)
        {
            if (deviceId is null)
            {
                _activeAccount.DeviceId = null;
            }
            else
            {
                _activeAccount.DeviceId = deviceId;
            }


            _activeAccount.Update();
        }
        else
        {
            _activeAccount = new Accounts();
            _activeAccount.DeviceId = deviceId;
            _activeAccount.Update();

        }

        return ValueTask.CompletedTask;
    }



}
