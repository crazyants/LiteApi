﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteApi.Contracts.Models.ActionMatchingByParameters
{
    public static class PossibleParameterTypeExtensions
    {
        public static IEnumerable<PossibleParameterType> GetPossibleParameterTypes(this HttpRequest request)
        {
            var httpMethod = request.Method.ToLower();
            if (httpMethod == "post" || httpMethod == "put")
            {
                if (request.Body.Length > 0)
                {
                    yield return new PossibleParameterType
                    {
                        Source = ParameterSources.Body,
                        HasValue = true,
                        OrderId = 0
                    };
                }
            }
            int queryOrder = 0;
            foreach (var param in request.Query)
            {
                var possibleType = new PossibleParameterType
                {
                    Source = ParameterSources.Query,
                    Name = param.Key,
                    OrderId = queryOrder,
                    QueryValues = param.Value
                };

                if (possibleType.QueryValues.Any())
                {
                    // TODO: add support for arrays
                    string first = possibleType.QueryValues.FirstOrDefault();
                    if (!string.IsNullOrEmpty(first))
                    {
                        possibleType.PossibleTypes = GetPossibleTypes(first).Select(x => new TypeWithPriority(x)).ToArray();
                    }
                }

                yield return possibleType;
            }
        }

        private static IEnumerable<Type> GetPossibleTypes(string value)
        {
            bool tempBool;
            Int16 tempInt16;
            Int32 tempInt32;
            Int64 tempInt64;
            UInt16 tempUInt16;
            UInt32 tempUInt32;
            UInt64 tempUInt64;
            Byte tempByte;
            SByte tempSByte;
            decimal tempDecimal;
            float tempFloat;
            double tempDouble;
            Guid tempGuid;
            DateTime tempDateTime;

            yield return typeof(string);
            if (value.Length == 1) yield return typeof(char);

            if (bool.TryParse(value, out tempBool))
            {
                yield return typeof(bool);
            }
            else
            {
                if (decimal.TryParse(value, out tempDecimal))
                {
                    yield return typeof(decimal);
                    yield return typeof(double);
                    yield return typeof(float);
                }
                else if (double.TryParse(value, out tempDouble))
                {
                    yield return typeof(double);
                    yield return typeof(float);
                }
                else if (float.TryParse(value, out tempFloat))
                {
                    yield return typeof(float);
                }

                if (UInt64.TryParse(value, out tempUInt64))
                {
                    yield return typeof(UInt64);
                    yield return typeof(UInt32);
                    yield return typeof(UInt16);
                }
                else if (UInt32.TryParse(value, out tempUInt32))
                {
                    yield return typeof(UInt32);
                    yield return typeof(UInt16);
                }
                else if (UInt16.TryParse(value, out tempUInt16))
                {
                    yield return typeof(UInt16);
                }

                if (Int64.TryParse(value, out tempInt64))
                {
                    yield return typeof(Int64);
                    yield return typeof(Int32);
                    yield return typeof(Int16);
                }
                else if (Int32.TryParse(value, out tempInt32))
                {
                    yield return typeof(Int32);
                    yield return typeof(Int16);
                }
                else if (Int16.TryParse(value, out tempInt16))
                {
                    yield return typeof(Int16);
                }

                if (Byte.TryParse(value, out tempByte))
                {
                    yield return typeof(byte);
                }
                if (SByte.TryParse(value, out tempSByte))
                {
                    yield return typeof(sbyte);
                }
            }

            if (DateTime.TryParse(value, out tempDateTime))
            {
                yield return typeof(DateTime);
            }
            else if (Guid.TryParse(value, out tempGuid))
            {
                yield return typeof(Guid);
            }
        }

        public static bool IsMatchedByName(this ActionParameter actionParam, PossibleParameterType possibleParam)
            => actionParam.Name == possibleParam.Name;

        public static bool IsMatchedByName(this PossibleParameterType possibleParam, ActionParameter actionParam)
            => actionParam.Name == possibleParam.Name;

        public static int GetParameterMatchingWeight(this PossibleParameterType possibleParam, ActionParameter actionParam)
        {
            var matchingType = possibleParam.GetNotNullableType(actionParam.Type);
            return new TypeWithPriority(matchingType).TypePriority;
        }
    }
}