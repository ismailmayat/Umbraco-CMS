﻿using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Declares model mappings for media.
    /// </summary>
    internal class MediaMapperProfile : Profile
    {
        public MediaMapperProfile(IUserService userService, ILocalizedTextService textService, IDataTypeService dataTypeService, IMediaService mediaService, IMediaTypeService mediaTypeService, ILogger logger)
        {
            // create, capture, cache
            var mediaOwnerResolver = new OwnerResolver<IMedia>(userService);
            var tabsAndPropertiesResolver = new TabsAndPropertiesResolver<IMedia, MediaItemDisplay>(textService);
            var childOfListViewResolver = new MediaChildOfListViewResolver(mediaService, mediaTypeService);
            var contentTreeNodeUrlResolver = new ContentTreeNodeUrlResolver<IMedia, MediaTreeController>();
            var mediaTypeBasicResolver = new ContentTypeBasicResolver<IMedia, MediaItemDisplay>();

            //FROM IMedia TO MediaItemDisplay
            CreateMap<IMedia, MediaItemDisplay>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(content => Udi.Create(Constants.UdiEntityType.Media, content.Key)))
                .ForMember(dest => dest.Owner, opt => opt.ResolveUsing(src => mediaOwnerResolver.Resolve(src)))
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(content => content.ContentType.Icon))
                .ForMember(dest => dest.ContentTypeAlias, opt => opt.MapFrom(content => content.ContentType.Alias))
                .ForMember(dest => dest.IsChildOfListView, opt => opt.ResolveUsing(childOfListViewResolver))
                .ForMember(dest => dest.Trashed, opt => opt.MapFrom(content => content.Trashed))
                .ForMember(dest => dest.ContentTypeName, opt => opt.MapFrom(content => content.ContentType.Name))
                .ForMember(dest => dest.Properties, opt => opt.Ignore())
                .ForMember(dest => dest.TreeNodeUrl, opt => opt.ResolveUsing(contentTreeNodeUrlResolver))
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.Errors, opt => opt.Ignore())
                .ForMember(dest => dest.Published, opt => opt.Ignore())
                .ForMember(dest => dest.Edited, opt => opt.Ignore())
                .ForMember(dest => dest.Updater, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.IsContainer, opt => opt.Ignore())
                .ForMember(dest => dest.Tabs, opt => opt.ResolveUsing(tabsAndPropertiesResolver))
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore())
                .ForMember(dest => dest.ContentType, opt => opt.ResolveUsing(mediaTypeBasicResolver))
                .ForMember(dest => dest.MediaLink, opt => opt.ResolveUsing(content => string.Join(",", content.GetUrls(UmbracoConfig.For.UmbracoSettings().Content, logger))))
                .AfterMap((media, display) =>
                {
                    if (media.ContentType.IsContainer)
                        TabsAndPropertiesResolver.AddListView(display, "media", dataTypeService, textService);
                });

            //FROM IMedia TO ContentItemBasic<ContentPropertyBasic, IMedia>
            CreateMap<IMedia, ContentItemBasic<ContentPropertyBasic, IMedia>>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(src => Udi.Create(Constants.UdiEntityType.Media, src.Key)))
                .ForMember(dest => dest.Owner, opt => opt.ResolveUsing(src => mediaOwnerResolver.Resolve(src)))
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => src.ContentType.Icon))
                .ForMember(dest => dest.Trashed, opt => opt.MapFrom(src => src.Trashed))
                .ForMember(dest => dest.ContentTypeAlias, opt => opt.MapFrom(src => src.ContentType.Alias))
                .ForMember(dest => dest.Published, opt => opt.Ignore())
                .ForMember(dest => dest.Edited, opt => opt.Ignore())
                .ForMember(dest => dest.Updater, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore());

            //FROM IMedia TO ContentItemDto<IMedia>
            CreateMap<IMedia, ContentItemDto<IMedia>>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(src => Udi.Create(Constants.UdiEntityType.Media, src.Key)))
                .ForMember(dest => dest.Owner, opt => opt.ResolveUsing(src => mediaOwnerResolver.Resolve(src)))
                .ForMember(dest => dest.Published, opt => opt.Ignore())
                .ForMember(dest => dest.Edited, opt => opt.Ignore())
                .ForMember(dest => dest.Updater, opt => opt.Ignore())
                .ForMember(dest => dest.Icon, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore());
        }
    }
}
