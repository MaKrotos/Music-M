using System;
using JetBrains.Annotations;
using VkNet.Model;

namespace VkNet.AudioBypassService.Exceptions
{
	[Serializable]
	public class VkAuthException : System.Exception
	{
		[NotNull]
		public VkAuthError AuthError { get; }

		public VkAuthException([NotNull] VkAuthError vkAuthError) : base(vkAuthError.ErrorDescription ?? vkAuthError.Error)
		{
			AuthError = vkAuthError;
		}
	}
}