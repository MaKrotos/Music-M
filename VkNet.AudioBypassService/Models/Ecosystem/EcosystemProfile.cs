using JetBrains.Annotations;
using Newtonsoft.Json;

namespace VkNet.AudioBypassService.Models.Ecosystem;

public record EcosystemProfile(string FirstName, string LastName, string Phone, bool Has2Fa, bool CanUnbindPhone, [CanBeNull] [property: JsonProperty("photo_200")] string Photo200);