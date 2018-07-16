namespace PassiveBOT.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    ///     The search helper.
    /// </summary>
    public class SearchHelper
    {
        /// <summary>
        ///     The request http method.
        /// </summary>
        public enum RequestHttpMethod
        {
            Get,

            Post
        }

        /// <summary>
        ///     The get response string async.
        /// </summary>
        /// <param name="url">
        ///     The url.
        /// </param>
        /// <param name="headers">
        ///     The headers.
        /// </param>
        /// <param name="method">
        ///     The method.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        public static async Task<string> GetResponseStringAsync(string url, IEnumerable<KeyValuePair<string, string>> headers = null, RequestHttpMethod method = RequestHttpMethod.Get)
        {
            using (var streamReader = new StreamReader(await GetResponseStreamAsync(url, headers, method).ConfigureAwait(false)))
            {
                return await streamReader.ReadToEndAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     The get response stream async.
        /// </summary>
        /// <param name="url">
        ///     The url.
        /// </param>
        /// <param name="headers">
        ///     The headers.
        /// </param>
        /// <param name="method">
        ///     The method.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        /// <exception cref="Exception">
        ///     Throws if incorrect method input.
        /// </exception>
        private static async Task<Stream> GetResponseStreamAsync(string url, IEnumerable<KeyValuePair<string, string>> headers = null, RequestHttpMethod method = RequestHttpMethod.Get)
        {
            var cl = new HttpClient();
            cl.DefaultRequestHeaders.Clear();
            switch (method)
            {
                case RequestHttpMethod.Get:
                    if (headers == null)
                    {
                        return await cl.GetStreamAsync(url).ConfigureAwait(false);
                    }

                    foreach (var header in headers)
                    {
                        cl.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                    }

                    return await cl.GetStreamAsync(url).ConfigureAwait(false);
                case RequestHttpMethod.Post:
                    FormUrlEncodedContent formContent = null;
                    if (headers != null)
                    {
                        formContent = new FormUrlEncodedContent(headers);
                    }

                    var message = await cl.PostAsync(url, formContent).ConfigureAwait(false);
                    return await message.Content.ReadAsStreamAsync().ConfigureAwait(false);
                default:
                    throw new Exception("That type of request is unsupported.");
            }
        }
    }
}