using VkNet.Abstractions;
using VkNet.Enums.Filters;
using VkNet.Model;
/* Необъединенное слияние из проекта "VkNet (netstandard2.0)"
До:
using VkNet.Enums.Filters;
После:
using VkNet.Utils;
*/

/* Необъединенное слияние из проекта "VkNet (net6.0)"
До:
using VkNet.Enums.Filters;
После:
using VkNet.Utils;
*/


namespace VkNet.Categories
{
    /// <inheritdoc />
    public partial class DonutCategory : IDonutCategory
    {
        /// <summary>
        /// API.
        /// </summary>
        private readonly IVkApiInvoke _vk;

        /// <summary>
        /// api vk.com
        /// </summary>
        /// <param name="vk"> API. </param>
        public DonutCategory(IVkApiInvoke vk)
        {
            _vk = vk;
        }

        /// <inheritdoc/>
        public bool IsDon(long ownerId)
        {
            var parameters = new VkParameters
            {
                { "owner_id", ownerId }
            };
            return _vk.Call(methodName: "donut.isDon", parameters);
        }

        /// <inheritdoc/>
        public VkCollection<User> GetFriends(long ownerId, ulong offset, byte count, UsersFields fields)
        {
            var parameters = new VkParameters
            {
                { "owner_id", ownerId },
                { "offset", offset },
                { "count", count },
                { "fields", fields }
            };
            return _vk.Call(methodName: "donut.getFriends", parameters).ToVkCollectionOf<User>(x => x);
        }

        /// <inheritdoc/>
        public Subscription GetSubscription(long ownerId)
        {
            var parameters = new VkParameters
            {
                { "owner_id", ownerId },
            };
            return _vk.Call(methodName: "donut.getSubscription", parameters);
        }

        /// <inheritdoc/>
        public SubscriptionsInfo GetSubscriptions(UsersFields fields, ulong offset, byte count)
        {
            var parameters = new VkParameters
            {
                { "fields", fields },
                { "offset", offset },
                { "count", count }
            };
            return _vk.Call(methodName: "donut.getSubscriptions", parameters);
        }
    }
}
