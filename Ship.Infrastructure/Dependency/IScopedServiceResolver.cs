﻿using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace Ship.Infrastructure.Dependency
{
    /// <summary>
    /// <see cref="ServiceLifetime.Scoped"/>服务解析器
    /// </summary>
    public interface IScopedServiceResolver
    {
        /// <summary>
        /// 获取 是否可解析
        /// </summary>
        bool ResolveEnabled { get; }

        /// <summary>
        /// 获取 <see cref="ServiceLifetime.Scoped"/>生命周期的服务提供者
        /// </summary>
        IServiceProvider ScopedProvider { get; }

        /// <summary>
        /// 获取指定服务类型的实例
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <returns></returns>
        T GetService<T>();

        /// <summary>
        /// 获取指定服务类型的实例
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <returns></returns>
        object GetService(Type serviceType);

        /// <summary>
        /// 获取指定服务类型的所有实例
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <returns></returns>
        IEnumerable<T> GetServices<T>();

        /// <summary>
        /// 获取指定服务类型的所有实例
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <returns></returns>
        IEnumerable<object> GetServices(Type serviceType);
        IPrincipal GetCurrentUser();
    }
}
