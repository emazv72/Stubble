﻿// <copyright file="DefaultSettings.cs" company="Stubble Authors">
// Copyright (c) Stubble Authors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CSharp.RuntimeBinder;
using Stubble.Compilation.Contexts;
using Stubble.Compilation.Renderers.TokenRenderers;
using Stubble.Core.Exceptions;
using Stubble.Core.Helpers;
using Stubble.Core.Renderers.Interfaces;

namespace Stubble.Compilation.Settings
{
    /// <summary>
    /// Contains defaults for the <see cref="CompilerSettings"/>
    /// </summary>
    public static class DefaultSettings
    {
        private static readonly CSharpArgumentInfo[] EmptyCSharpArgumentInfo =
        {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
        };

        /// <summary>
        /// Delegate type for value getters
        /// </summary>
        /// <param name="type">The type of member to lookup the value on</param>
        /// <param name="instance">An expression tothe member to lookup the value on</param>
        /// <param name="key">The key to lookup</param>
        /// <param name="ignoreCase">If case should be ignored when looking up value</param>
        /// <returns>The expression to find the value or null if not found</returns>
        public delegate Expression ValueGetterDelegate(Type type, Expression instance, string key, bool ignoreCase);

        /// <summary>
        /// Returns the default value getters
        /// </summary>
        /// <returns>The default value getters</returns>
        public static Dictionary<Type, ValueGetterDelegate> DefaultValueGetters()
        {
            return new Dictionary<Type, ValueGetterDelegate>()
            {
                {
                    typeof(IList),
                    (type, instance, key, ignoreCase) =>
                    {
                        if (int.TryParse(key, out var intVal))
                        {
                            var index = Expression.Constant(intVal);

                            return Expression.Condition(
                                Expression.LessThan(index, Expression.Property(instance, typeof(ICollection), "Count")),
                                Expression.MakeIndex(instance, typeof(IList).GetProperty("Item"), new[] { index }),
                                Expression.Constant(null));
                        }

                        return null;
                    }
                },
                {
                    typeof(IDictionary<string, object>),
                    (type, instance, key, ignoreCase) =>
                    {
                        // Skip dynamic objects since they also implement IDictionary<string, object>
                        if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type))
                        {
                            return null;
                        }

                        var outVar = Expression.Variable(typeof(object));
                        var block = new Expression[]
                        {
                            outVar,
                            Expression.Condition(
                                Expression.Call(instance, typeof(IDictionary<string, object>).GetMethod("TryGetValue"), new Expression[] { Expression.Constant(key), outVar }),
                                outVar,
                                Expression.Constant(null))
                        };

                        return Expression.Block(new[] { outVar }, block);
                    }
                },
                {
                    typeof(IDictionary),
                    (type, instance, key, ignoreCase) =>
                    {
                        return Expression.MakeIndex(instance, typeof(IDictionary).GetProperty("Item"), new[] { Expression.Constant(key) });
                    }
                },
                {
                    typeof(IDynamicMetaObjectProvider),
                    (type, instance, key, ignoreCase) =>
                    {
                        if (ignoreCase)
                        {
                            throw new StubbleException("Dynamic value lookup cannot ignore case");
                        }

                        // First try to access the value through IDictionary. This is fast and will take care of ExpandoObject.
                        if (typeof(IDictionary<string, object>).IsAssignableFrom(type))
                        {
                            var outVar = Expression.Variable(typeof(object));
                            var block = new Expression[]
                            {
                                outVar,
                                Expression.Condition(
                                    Expression.Call(instance, typeof(IDictionary<string, object>).GetMethod("TryGetValue"), new Expression[] { Expression.Constant(key), outVar }),
                                    outVar,
                                    Expression.Constant(null))
                            };

                            return Expression.Block(new[] { outVar }, block);
                        }

                        var binder = Microsoft.CSharp.RuntimeBinder.Binder.GetMember(
                            CSharpBinderFlags.None,
                            key,
                            typeof(DefaultSettings),
                            EmptyCSharpArgumentInfo);

                        return Expression.Dynamic(binder, typeof(object), instance);
                    }
                },
                {
                    typeof(object), (type, instance, key, ignoreCase) =>
                    {
                        var member = type.GetMember(
                            key,
                            BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);

                        if (member.Length > 0)
                        {
                            if (member.Length > 1)
                            {
                                throw new StubbleAmbigousMatchException($"Ambiguous match found when looking up key: '{key}'");
                            }

                            var info = member[0];
                            if (ignoreCase || info.Name == key)
                            {
                                var expression = ReflectionHelper.GetExpressionFromMemberInfo(info, instance);
                                return expression;
                            }
                        }

                        return null;
                    }
                }
            };
        }

        /// <summary>
        /// Returns the default token renderers
        /// </summary>
        /// <returns>Default token renderers</returns>
        internal static IEnumerable<ITokenRenderer<CompilerContext>> DefaultTokenRenderers()
        {
            return new List<ITokenRenderer<CompilerContext>>
             {
                new SectionTokenRenderer(),
                new LiteralTokenRenderer(),
                new InterpolationTokenRenderer(),
                new PartialTokenRenderer(),
                new InvertedSectionTokenRenderer(),
             };
        }

        /// <summary>
        /// Returns the default blacklisted types for sections
        /// </summary>
        /// <returns>A hashset of default blacklisted types for sections</returns>
        internal static HashSet<Type> DefaultSectionBlacklistTypes() => new HashSet<Type>
        {
            typeof(IDictionary),
            typeof(string)
        };
    }
}
