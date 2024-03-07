using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Text;

namespace PAS.Common
{
    // From: https://github.com/aspnet/Extensions/blob/9bc79b2f25a3724376d7af19617c33749a30ea3a/src/Caching/Abstractions/src/DistributedCacheEntryOptions.cs
    internal class DistributedTokenCache : TokenCache
    {
        private readonly IDistributedCache cache;
        private readonly string userId;

        public DistributedTokenCache(IDistributedCache cache, string userId)
        {
            this.cache = cache;
            this.userId = userId;

            BeforeAccess = OnBeforeAccess;
            AfterAccess = OnAfterAccess;
        }

        private void OnBeforeAccess(TokenCacheNotificationArgs args)
        {
            var userTokenCachePayload = cache.Get(CacheKey);

            if (userTokenCachePayload != null)
            {
                Deserialize(userTokenCachePayload);
            }
        }

        private void OnAfterAccess(TokenCacheNotificationArgs args)
        {
            if (HasStateChanged)
            {
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(14)
                };

                cache.Set(CacheKey, Serialize(), cacheOptions);

                HasStateChanged = false;
            }
        }

        private string CacheKey => $"TokenCache_{userId}";
    }
}
