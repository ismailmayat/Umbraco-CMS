﻿using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Filters;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// This sets the IsCurrentUser property on any outgoing <see cref="UserDisplay"/> model or any collection of <see cref="UserDisplay"/> models
    /// </summary>
    internal class IsCurrentUserModelFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Response == null) return;

            var user = UmbracoContext.Current.Security.CurrentUser;
            if (user == null) return;

            var objectContent = actionExecutedContext.Response.Content as ObjectContent;
            if (objectContent != null)
            {
                var model = objectContent.Value as UserBasic;
                if (model != null)
                {
                    model.IsCurrentUser = (int) model.Id == user.Id;
                }
                else
                {
                    var collection = objectContent.Value as IEnumerable<UserBasic>;
                    if (collection != null)
                    {
                        foreach (var userBasic in collection)
                        {
                            userBasic.IsCurrentUser = (int) userBasic.Id == user.Id;
                        }
                    }
                    else
                    {
                        var paged = objectContent.Value as UsersController.PagedUserResult;
                        if (paged != null && paged.Items != null)
                        {
                            foreach (var userBasic in paged.Items)
                            {
                                userBasic.IsCurrentUser = (int)userBasic.Id == user.Id;
                            }
                        }
                    }
                }
            }

            base.OnActionExecuted(actionExecutedContext);
        }
    }
}
