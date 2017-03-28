﻿using System;
using System.Collections.Generic;
using Azuria.Api.Connection;

namespace Azuria.Api.Services
{
    internal static class HttpClientService
    {
        private static Func<IProxerUser, IHttpClient> _clientFactory = user => new HttpClient(user);

        private static readonly Dictionary<IProxerUser, IHttpClient> ClientCache =
            new Dictionary<IProxerUser, IHttpClient>();

        #region Methods

        internal static IHttpClient GetForUser(IProxerUser user)
        {
            if (!ClientCache.ContainsKey(user))
                ClientCache.Add(user, _clientFactory.Invoke(user));
            return ClientCache[user];
        }

        internal static void Init(Func<IProxerUser, IHttpClient> clientFactory)
        {
            _clientFactory = clientFactory;
        }

        #endregion
    }
}