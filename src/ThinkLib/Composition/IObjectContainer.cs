﻿using System;
using System.Collections.Generic;

namespace ThinkLib.Composition
{
    /// <summary>
    /// 对象容器接口
    /// </summary>
    public interface IObjectContainer
    {
        /// <summary>
        /// 判断此类型是否已注册
        /// </summary>
        bool IsRegistered(Type type, string name = null);

        /// <summary>
        /// 注册一个类型
        /// </summary>
        void RegisterInstance(Type type, object instance, string name = null);

        /// <summary>
        /// 注册一个类型
        /// </summary>
        void RegisterType(Type type, string name, Lifecycle lifetime = Lifecycle.Singleton);

        /// <summary>
        /// 注册一个类型
        /// </summary>
        void RegisterType(Type from, Type to, string name, Lifecycle lifetime = Lifecycle.Singleton);




        /// <summary>
        /// 获取类型对应的实例
        /// </summary>
        object Resolve(Type type, string name = null);
        /// <summary>
        /// 获取类型所有的实例
        /// </summary>
        IEnumerable<object> ResolveAll(Type type);
    }
}
