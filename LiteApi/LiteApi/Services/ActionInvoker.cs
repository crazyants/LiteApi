﻿using LiteApi.Contracts.Abstractions;
using LiteApi.Contracts.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace LiteApi.Services
{
    /// <summary>
    /// Class that is used for invoking actions
    /// </summary>
    /// <seealso cref="LiteApi.Contracts.Abstractions.IActionInvoker" />
    public class ActionInvoker : IActionInvoker
    {
        /// <summary>
        /// Gets or sets JSON serializer.
        /// </summary>
        /// <value>
        /// JSON serializer.
        /// </value>
        public static Func<IJsonSerializer> GetJsonSerializer { get; set; } = () => LiteApiMiddleware.Options.JsonSerializer;

        private readonly IControllerBuilder _controllerBuilder;
        private readonly IModelBinder _modelBinder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionInvoker"/> class.
        /// </summary>
        /// <param name="controllerBuilder">The controller builder.</param>
        /// <param name="modelBinder">The model binder.</param>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public ActionInvoker(IControllerBuilder controllerBuilder, IModelBinder modelBinder)
        {
            if (controllerBuilder == null) throw new ArgumentNullException(nameof(controllerBuilder));
            if (modelBinder == null) throw new ArgumentNullException(nameof(modelBinder));
            _controllerBuilder = controllerBuilder;
            _modelBinder = modelBinder;
        }

        /// <summary>
        /// Invokes the specified <see cref="ActionContext"/>.
        /// </summary>
        /// <param name="httpCtx">The HTTP context, set by the middleware.</param>
        /// <param name="actionCtx">The action context.</param>
        /// <returns></returns>
        public virtual async Task Invoke(HttpContext httpCtx, ActionContext actionCtx)
        {
            ApiFilterRunResult filterResult = await RunFiltersAndCheckIfShouldContinue(httpCtx, actionCtx);

            if (!filterResult.ShouldContinue)
            {
                if (filterResult.SetResponseCode.HasValue)
                {
                    httpCtx.Response.StatusCode = filterResult.SetResponseCode.Value;
                }
                else
                {
                    bool isAuthenticated = httpCtx?.User?.Identity?.IsAuthenticated ?? false;
                    if (!isAuthenticated)
                    {
                        httpCtx.Response.StatusCode = 401;
                    }
                    else
                    {
                        httpCtx.Response.StatusCode = 403;
                    }
                }
                return;
            }

            LiteController ctrl = _controllerBuilder.Build(actionCtx.ParentController, httpCtx);
            object[] paramValues = _modelBinder.GetParameterValues(httpCtx.Request, actionCtx);

            object result = null;
            bool isVoid = true;
            if (actionCtx.Method.ReturnType == typeof(void))
            {
                actionCtx.Method.Invoke(ctrl, paramValues);
            }
            else if (actionCtx.Method.ReturnType == typeof(Task))
            {
                var task = (actionCtx.Method.Invoke(ctrl, paramValues) as Task);
                await task;
            }
            else if (actionCtx.Method.ReturnType.IsConstructedGenericType && actionCtx.Method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            { 
                isVoid = false;
                var task = (dynamic)(actionCtx.Method.Invoke(ctrl, paramValues));
                result = await task;
            }
            else
            {
                isVoid = false;
                result = actionCtx.Method.Invoke(ctrl, paramValues);
            }

            int statusCode = 405; // method not allowed
            switch (httpCtx.Request.Method.ToUpper())
            {
                case "GET": statusCode = 200; break;
                case "POST": statusCode = 201; break;
                case "PUT": statusCode = 201; break;
                case "DELETE": statusCode = 204; break;

            }
            httpCtx.Response.StatusCode = statusCode;
            if (!isVoid)
            {
                httpCtx.Response.ContentType = "application/json";
                await httpCtx.Response.WriteAsync(GetJsonSerializer().Serialize(result));
            }
        }

        private static async Task<ApiFilterRunResult> RunFiltersAndCheckIfShouldContinue(HttpContext httpCtx, ActionContext action)
        {
            if (action.SkipAuth) return new ApiFilterRunResult { ShouldContinue = true };

            ApiFilterRunResult result = await action.ParentController.ValidateFilters(httpCtx);
            if (!result.ShouldContinue)
            {
                return result;
            }

            foreach (var filter in action.Filters)
            {
                result = await filter.ShouldContinueAsync(httpCtx);
                if (!result.ShouldContinue)
                {
                    return result;
                }
            }

            return new ApiFilterRunResult { ShouldContinue = true };
        }
    }
}
