﻿using System.Reflection;
using LiteApi.Attributes;
using LiteApi.Contracts.Abstractions;
using LiteApi.Contracts.Models;

namespace LiteApi.Services
{
    public class ParametersDiscoverer : IParametersDiscoverer
    {
        public ActionParameter[] GetParameters(ActionContext actionCtx)
        {
            var methodParams = actionCtx.Method.GetParameters();
            ActionParameter[] parameters = new ActionParameter[methodParams.Length];
            for (int i = 0; i < methodParams.Length; i++)
            {
                var param = actionCtx.Method.GetParameters()[i];
                bool isFromQuery = param.GetCustomAttribute<FromUrlAttribute>() != null;
                bool isFromBody = param.GetCustomAttribute<FromBodyAttribute>() != null;

                ParameterSources source = ParameterSources.Unknown;
                if (isFromQuery && !isFromBody) source = ParameterSources.Query;
                else if (!isFromQuery && isFromBody) source = ParameterSources.Body;

                parameters[i] = new ActionParameter(actionCtx, LiteApiMiddleware.Options.JsonSerializer)
                {
                    Name = param.Name.ToLower(),
                    DefaultValue = param.DefaultValue,
                    HasDefaultValue = param.HasDefaultValue,
                    Type = param.ParameterType,
                    ParameterSource = source
                };
            }

            return parameters;
        }
    }
}