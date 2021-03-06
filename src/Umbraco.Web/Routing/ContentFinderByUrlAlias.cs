﻿using System;
using System.Text;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Xml;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Provides an implementation of <see cref="IContentFinder"/> that handles page aliases.
    /// </summary>
    /// <remarks>
    /// <para>Handles <c>/just/about/anything</c> where <c>/just/about/anything</c> is contained in the <c>umbracoUrlAlias</c> property of a document.</para>
    /// <para>The alias is the full path to the document. There can be more than one alias, separated by commas.</para>
    /// </remarks>
    public class ContentFinderByUrlAlias : IContentFinder
    {
        protected ILogger Logger { get; }

        public ContentFinderByUrlAlias(ILogger logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Tries to find and assign an Umbraco document to a <c>PublishedContentRequest</c>.
        /// </summary>
        /// <param name="frequest">The <c>PublishedContentRequest</c>.</param>
        /// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
        public bool TryFindContent(PublishedRequest frequest)
        {
            IPublishedContent node = null;

            if (frequest.Uri.AbsolutePath != "/") // no alias if "/"
            {
                node = FindContentByAlias(frequest.UmbracoContext.ContentCache,
                    frequest.HasDomain ? frequest.Domain.ContentId : 0,
                    frequest.Uri.GetAbsolutePathDecoded());

                if (node != null)
                {
                    frequest.PublishedContent = node;
                    Logger.Debug<ContentFinderByUrlAlias>(() => $"Path \"{frequest.Uri.AbsolutePath}\" is an alias for id={frequest.PublishedContent.Id}");
                }
            }

            return node != null;
        }

        private static IPublishedContent FindContentByAlias(IPublishedContentCache cache, int rootNodeId, string alias)
        {
            if (alias == null) throw new ArgumentNullException(nameof(alias));

            // the alias may be "foo/bar" or "/foo/bar"
            // there may be spaces as in "/foo/bar,  /foo/nil"
            // these should probably be taken care of earlier on

            // TODO
            // can we normalize the values so that they contain no whitespaces, and no leading slashes?
            // and then the comparisons in IsMatch can be way faster - and allocate way less strings

            const string propertyAlias = "umbracoUrlAlias";

            var test1 = alias.TrimStart('/') + ",";
            var test2 = ",/" + test1; // test2 is ",/alias,"
            test1 = "," + test1; // test1 is ",alias,"

            bool IsMatch(IPublishedContent c, string a1, string a2)
            {
                // this basically implements the original XPath query ;-(
                //
                // "//* [@isDoc and (" +
                // "contains(concat(',',translate(umbracoUrlAlias, ' ', ''),','),',{0},')" +
                // " or contains(concat(',',translate(umbracoUrlAlias, ' ', ''),','),',/{0},')" +
                // ")]"

                if (!c.HasProperty(propertyAlias)) return false;
                var v = c.Value<string>(propertyAlias);
                if (string.IsNullOrWhiteSpace(v)) return false;
                v = "," + v.Replace(" ", "") + ",";
                return v.Contains(a1) || v.Contains(a2);
            }

            if (rootNodeId > 0)
            {
                var rootNode = cache.GetById(rootNodeId);
                return rootNode?.Descendants().FirstOrDefault(x => IsMatch(x, test1, test2));
            }

            foreach (var rootContent in cache.GetAtRoot())
            {
                var c = rootContent.DescendantsOrSelf().FirstOrDefault(x => IsMatch(x, test1, test2));
                if (c != null) return c;
            }

            return null;
        }
    }
}
