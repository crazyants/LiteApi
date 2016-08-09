﻿namespace LiteApi.Attributes
{
    public static class AttributeConventions
    {
        public static string ErrorResolutionSuggestion =>
            "When action doesn't have one of ([LiteApi.Attributes.HttpGetAttribute], [LiteApi.Attributes.HttpPostAttribute], [LiteApi.Attributes.HttpPutAttribute], [LiteApi.Attributes.HttpDeleteAttribute]) attributes, it's consider to be HTTP GET method. "
            + "When parameter doesn't have [LiteApi.Attributes.FromBodyAttribute] or [LiteApi.Attributes.FromUrlAttribute] "
            + "source of parameter (from body or from URL) is resolved by type of parameter. If type of parameter can be found in LiteApi.Contracts.Models.ActionParameter.SupportedTypesFromUrl (most of the basic types) "
            + "than it's considered that parameters source is from URL, otherwise it's considered to be from body. Please note that body is supported by POST and PUT "
            + "HTTP methods and not supported by GET and DELETE HTTP methods. In order to set parameters source explicitly consider using [LiteApi.Attributes.FromBodyAttribute] or [LiteApi.Attributes.FromUrlAttribute] on parameters.";

    }
}