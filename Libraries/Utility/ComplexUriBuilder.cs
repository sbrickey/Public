namespace SBrickey.Libraries.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    /// <summary>
    /// URI builder with support for advanced URI construction and mutable querystring parameters.
    /// </summary>
    /// <remarks>
    /// Constructor supports building a URI from an unlimited number of relative paths. Far simpler code than default URI combination.
    /// Querystring (Parameters) dictionary is entirely mutable for easy procedural manipulation.
    /// </remarks>
    public class ComplexUriBuilder
    {
        public string Scheme { get; set; }
        public int Port { get; set; }
        public string Host { get; set; }
        public string Path { get; set; }

        /// <summary>
        /// Dictionary of QueryString parameters.
        /// </summary>
        /// <remarks>
        /// NOTE: Values should NOT be URL encoded (UriBuilder will handle that for you).
        /// </remarks>
        public Dictionary<string, string> Parameters { get; private set; }


        public ComplexUriBuilder()
        {
            this.Parameters = new Dictionary<string, string>();
        }
        public ComplexUriBuilder(string uri) : this()
        {
            var inp = new UriBuilder(uri);
            this.Scheme = inp.Scheme;
            this.Port = inp.Port;
            this.Host = inp.Host;
            this.Path = inp.Path;

            AddFromQuery(inp.Query);
        }
        public ComplexUriBuilder(params string[] PathSegments) : this()
        {
            var currentUri = new Uri(PathSegments[0]);
            foreach (var nextSegment in PathSegments.Skip(1))
                currentUri = new Uri(currentUri, nextSegment);

            this.Scheme = currentUri.Scheme;
            this.Port = currentUri.Port;
            this.Host = currentUri.Host;
            this.Path = currentUri.LocalPath;

            AddFromQuery(currentUri.Query);
        }

        public void AddFromQuery(string querystring)
        {
            // input validation
            if (String.IsNullOrWhiteSpace(querystring))
                return;

            foreach (var param in querystring.Split(new char[] { '&' }))
            {
                var kv = param.Split(new char[] { '=' });
        		this.Parameters.Add(kv[0], HttpUtility.UrlDecode(kv[1]));
            }
        }

        public UriBuilder UriBuilder
        {
            get
            {
                return new UriBuilder(
                    scheme: this.Scheme,
                    host: this.Host,
                    port: this.Port,
                    path: this.Path,
                    extraValue: String.Format(
                        format: "{0}{1}",
                        arg0: this.Parameters.Any() ? "?" : String.Empty,
                        arg1: String.Join(
                                separator: "&",
                                values: this.Parameters.Select(kv => String.Concat(kv.Key, "=", HttpUtility.UrlEncode(kv.Value)))
                            )
                    )
                );
            }
        }
        public Uri Uri { get { return this.UriBuilder.Uri; } }

        public override string ToString() { return this.Uri.ToString(); }

    } // class
} // namespace