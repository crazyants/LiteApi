﻿using LiteApi.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LiteApi.Services
{
    /// <summary>
    /// Generic object instance builder that is using registered services withing the ASP.NET app
    /// </summary>
    public class ObjectBuilder
    {
        private static readonly IDictionary<string, ConstructorInfo> Constructors = new ConcurrentDictionary<string, ConstructorInfo>();
        private static readonly IDictionary<string, ParameterInfo[]> ConstructorParameterTypes = new ConcurrentDictionary<string, ParameterInfo[]>();

        /// <summary>
        /// Builds the object.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>Object instance</returns>
        public object BuildObject(Type objectType)
        {
            ConstructorInfo constructor = GetConstructor(objectType);
            ParameterInfo[] parameters = GetConstructorParameters(constructor);
            object[] parameterValues = GetConstructorParameterValues(parameters);
            object objectInstance = constructor.Invoke(parameterValues);
            return objectInstance;
        }

        /// <summary>
        /// Builds the object.
        /// </summary>
        /// <typeparam name="T">Type to build</typeparam>
        /// <returns>Instance of T</returns>
        public T BuildObject<T>()
            where T: class
        {
            return BuildObject(typeof(T)) as T;
        }

        private ConstructorInfo GetConstructor(Type objectType)
        {
            if (objectType == null) throw new ArgumentNullException(nameof(objectType), "Type is not provided");

            if (Constructors.ContainsKey(objectType.FullName))
            {
                return Constructors[objectType.FullName];
            }

            var constructors = objectType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (constructors.Length > 1)
            {
                constructors = constructors.Where(x => x.GetCustomAttribute<ApiConstructorAttribute>() != null).ToArray();
            }

            if (constructors.Length != 1)
            {
                throw new Exception($"Cannot find constructor for {objectType.FullName}. Class has more than one constructor, or "
                    + "more than one constructor is using ApiConstructorAttribute. If class has more than one constructor, only "
                    + "one should be annotated with ApiConstructorAttribute.");
            }

            Constructors[objectType.FullName] = constructors[0];
            return constructors[0];
        }

        private ParameterInfo[] GetConstructorParameters(ConstructorInfo constructor)
        {
            if (ConstructorParameterTypes.ContainsKey(constructor.DeclaringType.FullName))
            {
                return ConstructorParameterTypes[constructor.DeclaringType.FullName];
            }

            ParameterInfo[] parameters = constructor.GetParameters();
            ConstructorParameterTypes[constructor.DeclaringType.FullName] = parameters;
            return parameters;
        }

        private object[] GetConstructorParameterValues(ParameterInfo[] parameters)
        {
            object[] values = new object[parameters.Length];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = LiteApiMiddleware.Services.GetService(parameters[i].ParameterType);
            }
            return values;
        }
    }
}