using MyProjectRestApi.Models.Entity_Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;

namespace MyProjectRestApi.Infrastructure
{
    public class GroupModelBinder : IModelBinder
    {
        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {

            Group group = new Group();

            if (bindingContext.ModelType != typeof(Group))
            {
                return false;
            }

            var valueProviderResult =
                 bindingContext.ValueProvider.GetValue(bindingContext.ModelName);


            string ct = actionContext.Request.Content.ReadAsStringAsync().Result;

            ct = ct.Substring(ct.IndexOf("CustomerID"));
            string[] vals = ct.Split('&');


            bindingContext.ModelState.AddModelError(
                bindingContext.ModelName, "Cannot convert value to GeoPoint");
            return false;

        }
    }
}