﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net;
using System.Text;
using Azure.Core;
using Azure.Storage.Sas;

namespace Azure.Storage.Files.Shares
{
    /// <summary>
    /// The <see cref="ShareUriBuilder"/> class provides a convenient way to
    /// modify the contents of a <see cref="System.Uri"/> instance to point to
    /// different Azure Storage resources like an account, share, or file.
    ///
    /// For more information, see <see href="https://docs.microsoft.com/en-us/rest/api/storageservices/naming-and-referencing-shares--directories--files--and-metadata" />.
    /// </summary>
    public class ShareUriBuilder
    {
        /// <summary>
        /// The Uri instance constructed by this builder.  It will be reset to
        /// null when changes are made and reconstructed when <see cref="System.Uri"/>
        /// is accessed.
        /// </summary>
        private Uri _uri;

        /// <summary>
        /// Gets or sets the scheme name of the URI.
        /// Example: "https"
        /// </summary>
        public string Scheme
        {
            get => _scheme;
            set { ResetUri(); _scheme = value; }
        }
        private string _scheme;

        /// <summary>
        /// Gets or sets the Domain Name System (DNS) host name or IP address
        /// of a server.
        ///
        /// Example: "account.file.core.windows.net"
        /// </summary>
        public string Host
        {
            get => _host;
            set { ResetUri(); _host = value; }
        }
        private string _host;

        /// <summary>
        /// Gets or sets the port number of the URI.
        /// </summary>
        public int Port
        {
            get => _port;
            set { ResetUri(); _port = value; }
        }
        private int _port;

        /// <summary>
        /// Gets or sets the Azure Storage account name.  This is only
        /// populated for IP-style <see cref="System.Uri"/>s.
        /// </summary>
        public string AccountName
        {
            get => _accountName;
            set { ResetUri(); _accountName = value; }
        }
        private string _accountName;

        /// <summary>
        /// Gets or sets the name of a file storage share.  The value defaults
        /// to <see cref="string.Empty"/> if not present in the
        /// <see cref="System.Uri"/>.
        ///
        /// </summary>
        public string ShareName
        {
            get => _shareName;
            set { ResetUri(); _shareName = value; }
        }
        private string _shareName;

        /// <summary>
        /// Gets or sets the path of the directory or file.  The value defaults
        /// to <see cref="string.Empty"/> if not present in the
        /// <see cref="System.Uri"/>.
        /// Example: "mydirectory/myfile"
        /// </summary>
        public string DirectoryOrFilePath
        {
            get => _directoryOrFilePath;
            set { ResetUri(); _directoryOrFilePath = value.TrimEnd('/'); }
        }
        private string _directoryOrFilePath;

        /// <summary>
        /// Gets or sets the name of a file snapshot.  The value defaults to
        /// <see cref="string.Empty"/> if not present in the <see cref="System.Uri"/>.
        /// </summary>
        public string Snapshot
        {
            get => _snapshot;
            set { ResetUri(); _snapshot = value; }
        }
        private string _snapshot;

        /// <summary>
        /// Gets or sets the Shared Access Signature query parameters, or null
        /// if not present in the <see cref="System.Uri"/>.
        /// </summary>
        public SasQueryParameters Sas
        {
            get => _sas;
            set { ResetUri(); _sas = value; }
        }
        private SasQueryParameters _sas;

        /// <summary>
        /// Get the last directory or file name from the <see cref="DirectoryOrFilePath"/>, or null if
        /// not present in the <see cref="Uri"/>.
        /// </summary>
        internal string LastDirectoryOrFileName =>
            DirectoryOrFilePath.TrimEnd('/').Split('/').LastOrDefault();

        /// <summary>
        /// Gets or sets any query information included in the URI that's not
        /// relevant to addressing Azure storage resources.
        /// </summary>
        public string Query
        {
            get => _query;
            set { ResetUri(); _query = value; }
        }
        private string _query;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShareUriBuilder"/>
        /// class with the specified <see cref="System.Uri"/>.
        /// </summary>
        /// <param name="uri">
        /// The <see cref="System.Uri"/> to a storage resource.
        /// </param>
        public ShareUriBuilder(Uri uri)
        {
            Scheme = uri.Scheme;
            Host = uri.Host;
            Port = uri.Port;
            AccountName = "";

            ShareName = "";
            DirectoryOrFilePath = "";

            Snapshot = "";
            Sas = null;

            // Find the share & directory/file path (if any)
            if (!string.IsNullOrEmpty(uri.AbsolutePath))
            {
                // If path starts with a slash, remove it

                var path =
                    (uri.AbsolutePath[0] == '/')
                    ? uri.AbsolutePath.Substring(1)
                    : uri.AbsolutePath;

                var startIndex = 0;

                if (IsHostIPEndPointStyle(uri.Host))
                {
                    var accountEndIndex = path.IndexOf("/", StringComparison.InvariantCulture);

                    // Slash not found; path has account name & no share name
                    if (accountEndIndex == -1)
                    {
                        AccountName = path;
                        startIndex = path.Length;
                    }
                    else
                    {
                        AccountName = path.Substring(0, accountEndIndex);
                        startIndex = accountEndIndex + 1;
                    }
                }

                // Find the next slash (if it exists)

                var shareEndIndex = path.IndexOf("/", startIndex, StringComparison.InvariantCulture);
                if (shareEndIndex == -1)
                {
                    ShareName = path.Substring(startIndex); // Slash not found; path has share name & no directory/file path
                }
                else
                {
                    ShareName = path.Substring(startIndex, shareEndIndex - startIndex); // The share name is the part between the slashes
                    DirectoryOrFilePath = path.Substring(shareEndIndex + 1);   // The directory/file path name is after the share slash
                }
            }

            // Convert the query parameters to a case-sensitive map & trim whitespace

            var paramsMap = new UriQueryParamsCollection(uri.Query);

            if (paramsMap.TryGetValue(Constants.SnapshotParameterName, out var snapshotTime))
            {
                Snapshot = snapshotTime;

                // If we recognized the query parameter, remove it from the map
                paramsMap.Remove(Constants.SnapshotParameterName);
            }

            if (paramsMap.ContainsKey(Constants.Sas.Parameters.Version))
            {
                Sas = new SasQueryParameters(paramsMap);
            }

            Query = paramsMap.ToString();
        }

        /// <summary>
        /// Returns the <see cref="System.Uri"/> constructed from the
        /// <see cref="ShareUriBuilder"/>'s fields. The <see cref="Uri.Query"/>
        /// property contains the SAS and additional query parameters.
        /// </summary>
        public Uri ToUri()
        {
            if (_uri == null)
            {
                _uri = BuildUri().ToUri();
            }
            return _uri;
        }

        /// <summary>
        /// Returns the display string for the specified
        /// <see cref="ShareUriBuilder"/> instance.
        /// </summary>
        /// <returns>
        /// The display string for the specified <see cref="ShareUriBuilder"/>
        /// instance.
        /// </returns>
        public override string ToString() =>
            BuildUri().ToString();

        /// <summary>
        /// Reset our cached URI.
        /// </summary>
        private void ResetUri() =>
            _uri = null;

        /// <summary>
        /// Construct a <see cref="RequestUriBuilder"/> representing the
        /// <see cref="ShareUriBuilder"/>'s fields. The <see cref="Uri.Query"/>
        /// property contains the SAS, snapshot, and additional query parameters.
        /// </summary>
        /// <returns>The constructed <see cref="RequestUriBuilder"/>.</returns>
        private RequestUriBuilder BuildUri()
        {
            // Concatenate account, share & directory/file path (if they exist)
            var path = new StringBuilder("");
            if (!string.IsNullOrWhiteSpace(AccountName))
            {
                path.Append("/").Append(AccountName);
            }
            if (!string.IsNullOrWhiteSpace(ShareName))
            {
                path.Append("/").Append(ShareName);
                if (!string.IsNullOrWhiteSpace(DirectoryOrFilePath))
                {
                    path.Append("/").Append(DirectoryOrFilePath);
                }
            }

            // Concatenate query parameters
            var query = new StringBuilder(Query);
            if (!string.IsNullOrWhiteSpace(Snapshot))
            {
                if (query.Length > 0) { query.Append("&"); }
                query.Append(Constants.SnapshotParameterName).Append("=").Append(Snapshot);
            }
            var sas = Sas?.ToString();
            if (!string.IsNullOrWhiteSpace(sas))
            {
                if (query.Length > 0) { query.Append("&"); }
                query.Append(sas);
            }

            // Use RequestUriBuilder, which has slightly nicer formatting
            return new RequestUriBuilder
            {
                Scheme = Scheme,
                Host = Host,
                Port = Port,
                Path = path.ToString(),
                Query = query.Length > 0 ? "?" + query.ToString() : null
            };
        }

        // TODO See remarks at https://docs.microsoft.com/en-us/dotnet/api/system.net.ipaddress.tryparse?view=netframework-4.7.2
        // TODO refactor to shared method
        private static bool IsHostIPEndPointStyle(string host)
            => string.IsNullOrEmpty(host) ? false : IPAddress.TryParse(host, out _);
    }
}
