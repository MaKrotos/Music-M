using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VkNet.Abstractions;
using VkNet.Abstractions.Authorization;
using VkNet.Abstractions.Utils;
using VkNet.AudioBypassService.Abstractions;
using VkNet.AudioBypassService.Abstractions.Categories;
using VkNet.AudioBypassService.Categories;
using VkNet.AudioBypassService.Flows;
using VkNet.AudioBypassService.Models.Auth;
using VkNet.AudioBypassService.Utils;
using VkNet.Extensions.DependencyInjection;
using IAuthCategory = VkNet.AudioBypassService.Abstractions.Categories.IAuthCategory;
using VkApiAuth = VkNet.AudioBypassService.Utils.VkApiAuth;
using VkApiInvoke = VkNet.AudioBypassService.Utils.VkApiInvoke;

namespace VkNet.AudioBypassService.Extensions
{
	public static class AudioBypassServiceCollection
	{
		public static IServiceCollection AddAudioBypass([NotNull] this IServiceCollection services)
		{
			if (services == null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			services.TryAddSingleton<FakeSafetyNetClient>();
			services.TryAddSingleton<LibVerifyClient>();
			services.TryAddSingleton<IRestClient, RestClientWithUserAgent>();
			services.TryAddSingleton<IDeviceIdStore, DefaultDeviceIdStore>();
			services.TryAddSingleton<ITokenRefreshHandler, TokenRefreshHandler>();
			services.TryAddSingleton<IVkApiInvoke, VkApiInvoke>();
			services.TryAddSingleton<IVkApiAuthAsync, VkApiAuth>();
			
			services.TryAddKeyedSingleton<IAuthorizationFlow, PasswordAuthorizationFlow>(AndroidGrantType.Password);
			services.TryAddKeyedSingleton(AndroidGrantType.PhoneConfirmationSid,
				(s, _) => s.GetRequiredKeyedService<IAuthorizationFlow>(AndroidGrantType.Password));
			services.TryAddKeyedSingleton<IAuthorizationFlow, WithoutPasswordAuthorizationFlow>(AndroidGrantType.WithoutPassword);
			services.TryAddKeyedSingleton<IAuthorizationFlow, PasskeyAuthorizationFlow>(AndroidGrantType.Passkey);
			
			services.TryAddSingleton(s => s.GetRequiredKeyedService<IAuthorizationFlow>(AndroidGrantType.Password));
			
			services.TryAddSingleton<IAuthCategory, AuthCategory>();
			services.TryAddSingleton<ILoginCategory, LoginCategory>();
			services.TryAddSingleton<IEcosystemCategory, EcosystemCategory>();

			return services;
		}
	}
}
