using System.Collections.Generic;
using System;
using VK_UI3.VKs;
using VkNet.Utils;
using VkNet.Model;
using System.Threading.Tasks;

namespace VK_UI3.Helpers
{
    class GroupsGet
    {
        static List<Group> _groups = new List<Group>();
        static DateTime? LastUpdated { get; set; }
  

        public async static Task<List<Group>> getGroups()
        {
            if (LastUpdated == null || (DateTime.Now - LastUpdated.Value).TotalSeconds >= 30)
            {
                var param = new VkNet.Model.RequestParams.GroupsGetParams();
                param.Filter = VkNet.Enums.Filters.GroupsFilters.Moderator;
  
                param.Fields = VkNet.Enums.Filters.GroupsFields.CanPost | VkNet.Enums.Filters.GroupsFields.AllUndocumented | VkNet.Enums.Filters.GroupsFields.Description;
                param.Count = 1000;
                param.Extended = true;
                VkCollection<Group> groups = await VK.api.Groups.GetAsync(param);
                _groups.Clear();
                foreach (var item in groups)
                {
                    _groups.Add(item);
                }
                LastUpdated = DateTime.Now;
            }
            return _groups;
        }
    }
}
