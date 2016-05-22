﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Azuria.Exceptions;
using Azuria.Utilities.ErrorHandling;
using JetBrains.Annotations;
using RestSharp.Portable;
using RestSharp.Portable.HttpClient;

namespace Azuria.Utilities.Net
{
    /// <summary>
    ///     Eine Klasse, die alle Methoden darstellt, um per HTTP- und HTTPS-
    ///     Protokol mit dem Server zu kommunizieren.
    /// </summary>
    public class HttpUtility
    {
        /// <summary>
        ///     Gibt die Zeit in Millisekunden zurück, die der Client auf eine Antwort wartet bis er abbricht, oder legt diese
        ///     fest.
        ///     Standartwert = 5000
        /// </summary>
        public static int Timeout = 5000;

        /// <summary>
        /// </summary>
        public static bool SolveCloudflare = true;

        [NotNull] private static readonly string UserAgent = "Azuria.Portable/" +
                                                             new AssemblyName(
                                                                 typeof(HttpUtility).GetTypeInfo().Assembly.FullName)
                                                                 .Version +
                                                             "RestSharp.Portable/3.1.0.0";

        #region

        [ItemNotNull]
        internal static async Task<ProxerResult<string>> GetResponseErrorHandling(Uri url,
            [NotNull] ErrorHandler errorHandler, [NotNull] Senpai senpai)
        {
            return await GetResponseErrorHandling(url, null, errorHandler, senpai);
        }

        [ItemNotNull]
        internal static async Task<ProxerResult<string>> GetResponseErrorHandling([NotNull] Uri url,
            [CanBeNull] CookieContainer loginCookies, [NotNull] ErrorHandler errorHandler, [NotNull] Senpai senpai)
        {
            return
                await
                    GetResponseErrorHandling(url, loginCookies, errorHandler, senpai, new Func<string, ProxerResult>[0]);
        }

        [ItemNotNull]
        internal static async Task<ProxerResult<string>> GetResponseErrorHandling([NotNull] Uri url,
            [CanBeNull] CookieContainer loginCookies, [NotNull] ErrorHandler errorHandler, [NotNull] Senpai senpai,
            [CanBeNull] Func<string, ProxerResult>[] checkFuncs)
        {
            ProxerResult<Tuple<string, CookieContainer>> lResult =
                await
                    GetResponseErrorHandling(url, loginCookies, errorHandler, senpai, checkFuncs, loginCookies == null);

            return lResult.Success && lResult.Result != null
                ? new ProxerResult<string>(lResult.Result.Item1)
                : new ProxerResult<string>(lResult.Exceptions);
        }

        [ItemNotNull]
        internal static async Task<ProxerResult<Tuple<string, CookieContainer>>> GetResponseErrorHandling(
            [NotNull] Uri url, [CanBeNull] CookieContainer loginCookies, [NotNull] ErrorHandler errorHandler,
            [NotNull] Senpai senpai, [CanBeNull] Func<string, ProxerResult>[] checkFuncs, bool checkLogin,
            int recursion = 0)
        {
            if (checkLogin && loginCookies != null && !senpai.IsLoggedIn)
                return
                    new ProxerResult<Tuple<string, CookieContainer>>(new Exception[] {new NotLoggedInException()});

            string lResponse;

            IRestResponse lResponseObject;
            try
            {
                lResponseObject =
                    await GetWebRequestResponse(url, loginCookies, null);
            }
            catch (Exception ex)
            {
                return new ProxerResult<Tuple<string, CookieContainer>>(new[] {ex});
            }
            string lResponseString = Encoding.UTF8.GetString(lResponseObject.RawBytes, 0,
                lResponseObject.RawBytes.Length);

            if (lResponseObject.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(lResponseString))
                lResponse = WebUtility.HtmlDecode(lResponseString).Replace("\n", "");
            else if (lResponseObject.StatusCode == HttpStatusCode.ServiceUnavailable &&
                     !string.IsNullOrEmpty(lResponseString))
            {
                if (!SolveCloudflare)
                    return new ProxerResult<Tuple<string, CookieContainer>>(new[] {new CloudflareException()});
                ProxerResult<string> lSolveResult =
                    CloudflareSolver.Solve(WebUtility.HtmlDecode(lResponseString).Replace("\n", ""), url);

                if (!lSolveResult.Success)
                    return new ProxerResult<Tuple<string, CookieContainer>>(new[] {new CloudflareException()});

                await Task.Delay(4000);

                IRestResponse lGetResult;
                try
                {
                    lGetResult =
                        await
                            PostWebRequestResponse(
                                new Uri($"{url.Scheme}://{url.Host}/cdn-cgi/l/chk_jschl?{lSolveResult.Result}"),
                                loginCookies, new Dictionary<string, string>(), null);
                }
                catch (TaskCanceledException)
                {
                    return new ProxerResult<Tuple<string, CookieContainer>>(new[] {new TimeoutException()});
                }

                if (lGetResult.StatusCode != HttpStatusCode.OK)
                    return new ProxerResult<Tuple<string, CookieContainer>>(new[] {new CloudflareException()});

                return
                    await
                        GetResponseErrorHandling(url, loginCookies, errorHandler, senpai, checkFuncs, checkLogin,
                            recursion + 1);
            }
            else
                return
                    new ProxerResult<Tuple<string, CookieContainer>>(new[]
                    {new WrongResponseException()});

            if (checkFuncs != null)
                foreach (Func<string, ProxerResult> checkFunc in checkFuncs)
                {
                    try
                    {
                        ProxerResult lResult = checkFunc?.Invoke(lResponse) ?? new ProxerResult {Success = false};
                        if (!lResult.Success)
                            return new ProxerResult<Tuple<string, CookieContainer>>(lResult.Exceptions);
                    }
                    catch
                    {
                        return new ProxerResult<Tuple<string, CookieContainer>>(new Exception[0]) {Success = false};
                    }
                }

            if (string.IsNullOrEmpty(lResponse) || !Utility.CheckForCorrectResponse(lResponse, errorHandler))
                return
                    new ProxerResult<Tuple<string, CookieContainer>>(new Exception[]
                    {new WrongResponseException {Response = lResponse}});

            return
                new ProxerResult<Tuple<string, CookieContainer>>(
                    new Tuple<string, CookieContainer>(lResponse, loginCookies));
        }

        [ItemNotNull]
        internal static async Task<IRestResponse> GetWebRequestResponse([NotNull] Uri url,
            [CanBeNull] CookieContainer cookies, Dictionary<string, string> headers)
        {
            RestClient lClient = new RestClient(url)
            {
                CookieContainer = cookies,
                Timeout = TimeSpan.FromMilliseconds(Timeout == 0 ? 5000 : Timeout),
                UserAgent = UserAgent
            };
            RestRequest lRequest = new RestRequest(Method.GET);
            if (headers == null) return await lClient.Execute(lRequest);

            foreach (KeyValuePair<string, string> header in headers)
            {
                lRequest.AddHeader(header.Key, header.Value);
            }
            return await lClient.Execute(lRequest);
        }

        [ItemNotNull]
        internal static async Task<ProxerResult<string>> PostResponseErrorHandling([NotNull] Uri url,
            [NotNull] Dictionary<string, string> postArgs, [NotNull] ErrorHandler errorHandler, [NotNull] Senpai senpai)
        {
            return await PostResponseErrorHandling(url, postArgs, null, errorHandler, senpai);
        }

        [ItemNotNull]
        internal static async Task<ProxerResult<string>> PostResponseErrorHandling([NotNull] Uri url,
            [NotNull] Dictionary<string, string> postArgs, [CanBeNull] CookieContainer loginCookies,
            [NotNull] ErrorHandler errorHandler, [NotNull] Senpai senpai)
        {
            return
                await
                    PostResponseErrorHandling(url, postArgs, loginCookies, errorHandler, senpai,
                        new Func<string, ProxerResult>[0]);
        }

        [ItemNotNull]
        internal static async Task<ProxerResult<string>> PostResponseErrorHandling([NotNull] Uri url,
            [NotNull] Dictionary<string, string> postArgs, [CanBeNull] CookieContainer loginCookies,
            [NotNull] ErrorHandler errorHandler, [NotNull] Senpai senpai,
            [CanBeNull] Func<string, ProxerResult>[] checkFuncs)
        {
            ProxerResult<Tuple<string, CookieContainer>> lResult =
                await
                    PostResponseErrorHandling(url, postArgs, loginCookies, errorHandler, senpai, checkFuncs,
                        loginCookies == null);

            return lResult.Success && lResult.Result != null
                ? new ProxerResult<string>(lResult.Result.Item1)
                : new ProxerResult<string>(lResult.Exceptions);
        }

        [ItemNotNull]
        internal static async Task<ProxerResult<Tuple<string, CookieContainer>>> PostResponseErrorHandling(
            [NotNull] Uri url, [NotNull] Dictionary<string, string> postArgs,
            [CanBeNull] CookieContainer loginCookies, [NotNull] ErrorHandler errorHandler, [NotNull] Senpai senpai,
            [CanBeNull] Func<string, ProxerResult>[] checkFuncs, bool checkLogin, int recursion = 0)
        {
            if (checkLogin && loginCookies != null && !senpai.IsLoggedIn)
                return
                    new ProxerResult<Tuple<string, CookieContainer>>(new Exception[] {new NotLoggedInException()});

            string lResponse;

            IRestResponse lResponseObject;
            try
            {
                lResponseObject =
                    await PostWebRequestResponse(url, loginCookies, postArgs, null);
            }
            catch (Exception ex)
            {
                return new ProxerResult<Tuple<string, CookieContainer>>(new[] {ex});
            }
            string lResponseString = Encoding.UTF8.GetString(lResponseObject.RawBytes, 0,
                lResponseObject.RawBytes.Length);

            if (lResponseObject.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(lResponseString))
                lResponse = WebUtility.HtmlDecode(lResponseString).Replace("\n", "");
            else if (lResponseObject.StatusCode == HttpStatusCode.ServiceUnavailable &&
                     !string.IsNullOrEmpty(lResponseString))
            {
                if (!SolveCloudflare)
                    return new ProxerResult<Tuple<string, CookieContainer>>(new[] {new CloudflareException()});
                ProxerResult<string> lSolveResult =
                    CloudflareSolver.Solve(WebUtility.HtmlDecode(lResponseString).Replace("\n", ""), url);

                if (!lSolveResult.Success)
                    return new ProxerResult<Tuple<string, CookieContainer>>(new[] {new CloudflareException()});

                await Task.Delay(4000);

                IRestResponse lGetResult;
                try
                {
                    lGetResult =
                        await
                            PostWebRequestResponse(
                                new Uri($"{url.Scheme}://{url.Host}/cdn-cgi/l/chk_jschl?{lSolveResult.Result}"),
                                loginCookies, new Dictionary<string, string>(), null);
                }
                catch (TaskCanceledException)
                {
                    return new ProxerResult<Tuple<string, CookieContainer>>(new[] {new TimeoutException()});
                }

                if (lGetResult.StatusCode != HttpStatusCode.OK)
                    return new ProxerResult<Tuple<string, CookieContainer>>(new[] {new CloudflareException()});

                return
                    await
                        PostResponseErrorHandling(url, postArgs, loginCookies, errorHandler, senpai, checkFuncs,
                            checkLogin, recursion + 1);
            }
            else
                return
                    new ProxerResult<Tuple<string, CookieContainer>>(new[]
                    {new WrongResponseException()});

            if (checkFuncs != null)
                foreach (Func<string, ProxerResult> checkFunc in checkFuncs)
                {
                    try
                    {
                        ProxerResult lResult = checkFunc?.Invoke(lResponse) ?? new ProxerResult {Success = false};
                        if (!lResult.Success)
                            return new ProxerResult<Tuple<string, CookieContainer>>(lResult.Exceptions);
                    }
                    catch
                    {
                        return new ProxerResult<Tuple<string, CookieContainer>>(new Exception[0])
                        {
                            Success = false
                        };
                    }
                }

            if (string.IsNullOrEmpty(lResponse) || !Utility.CheckForCorrectResponse(lResponse, errorHandler))
                return
                    new ProxerResult<Tuple<string, CookieContainer>>(new Exception[]
                    {new WrongResponseException {Response = lResponse}});

            return
                new ProxerResult<Tuple<string, CookieContainer>>(
                    new Tuple<string, CookieContainer>(lResponse, loginCookies));
        }

        [ItemNotNull]
        internal static async Task<IRestResponse> PostWebRequestResponse([NotNull] Uri url,
            [CanBeNull] CookieContainer cookies, [NotNull] Dictionary<string, string> postArgs,
            Dictionary<string, string> headers)
        {
            RestClient lClient = new RestClient(url)
            {
                CookieContainer = cookies,
                Timeout = TimeSpan.FromMilliseconds(Timeout == 0 ? 5000 : Timeout),
                UserAgent = UserAgent
            };
            RestRequest lRequest = new RestRequest(Method.POST);
            if (headers != null)
                foreach (KeyValuePair<string, string> header in headers)
                {
                    lRequest.AddHeader(header.Key, header.Value);
                }
            foreach (KeyValuePair<string, string> pair in postArgs)
                lRequest.AddParameter(pair.Key, pair.Value);

            return await lClient.Execute(lRequest);
        }

        #endregion
    }
}