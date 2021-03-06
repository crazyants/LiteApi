﻿using LiteApi.Contracts.Models;

namespace LiteApi.Contracts.Abstractions
{
    /// <summary>
    /// Contract for resolving parameters metadata in an action.
    /// </summary>
    public interface IParametersDiscoverer
    {
        /// <summary>
        /// Gets the action parameters metadata for the given action context.
        /// </summary>
        /// <param name="actionCtx">The action context.</param>
        /// <returns>Array of <see cref="ActionParameter"/> retrieved from action context.</returns>
        ActionParameter[] GetParameters(ActionContext actionCtx);
    }
}
