﻿using System.Collections.Generic;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Provides an implementation of <see cref="IContentFinder"/> that handles page url rewrites
    /// that are stored when moving, saving, or deleting a node.
    /// </summary>
    /// <remarks>
    /// <para>Assigns a permanent redirect notification to the request.</para>
    /// </remarks>
    public class ContentFinderByRedirectUrl : IContentFinder
    {
        private readonly IRedirectUrlService _redirectUrlService;
        private readonly ILogger _logger;

        public ContentFinderByRedirectUrl(IRedirectUrlService redirectUrlService, ILogger logger)
        {
            _redirectUrlService = redirectUrlService;
            _logger = logger;
        }

        /// <summary>
        /// Tries to find and assign an Umbraco document to a <c>PublishedContentRequest</c>.
        /// </summary>
        /// <param name="frequest">The <c>PublishedContentRequest</c>.</param>
        /// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
        /// <remarks>Optionally, can also assign the template or anything else on the document request, although that is not required.</remarks>
        public bool TryFindContent(PublishedRequest frequest)
        {
            var route = frequest.HasDomain
                ? frequest.Domain.ContentId + DomainHelper.PathRelativeToDomain(frequest.Domain.Uri, frequest.Uri.GetAbsolutePathDecoded())
                : frequest.Uri.GetAbsolutePathDecoded();

            var redirectUrl = _redirectUrlService.GetMostRecentRedirectUrl(route);

            // From: http://stackoverflow.com/a/22468386/5018
            // See http://issues.umbraco.org/issue/U4-8361#comment=67-30532
            // Setting automatic 301 redirects to not be cached because browsers cache these very aggressively which then leads
            // to problems if you rename a page back to it's original name or create a new page with the original name
            frequest.Cacheability = HttpCacheability.NoCache;
            frequest.CacheExtensions = new List<string> { "no-store, must-revalidate" };
            frequest.Headers = new Dictionary<string, string> { { "Pragma", "no-cache" }, { "Expires", "0" } };

            if (redirectUrl == null)
            {
                _logger.Debug<ContentFinderByRedirectUrl>(() => $"No match for route: \"{route}\".");
                return false;
            }

            var content = frequest.UmbracoContext.ContentCache.GetById(redirectUrl.ContentId);
            var url = content == null ? "#" : content.Url;
            if (url.StartsWith("#"))
            {
                _logger.Debug<ContentFinderByRedirectUrl>(() => $"Route \"{route}\" matches content {redirectUrl.ContentId} which has no url.");
                return false;
            }

            _logger.Debug<ContentFinderByRedirectUrl>(() => $"Route \"{route}\" matches content {content.Id} with url \"{url}\", redirecting.");
            frequest.SetRedirectPermanent(url);
            return true;
        }
    }
}
