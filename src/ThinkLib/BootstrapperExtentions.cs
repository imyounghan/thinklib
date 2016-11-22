using System;
using System.Collections.Generic;
using ThinkLib.Composition;

namespace ThinkLib
{
    /// <summary>
    /// <see cref="Bootstrapper"/> 的扩展类
    /// </summary>
    public static class BootstrapperExtentions
    {
        /// <summary>
        /// 注册类型
        /// </summary>
        public static Bootstrapper SetDefault(this Bootstrapper that, Type type, Lifecycle lifecycle)
        {
            return that.SetDefault(type, (string)null, lifecycle);
        }
        /// <summary>
        /// 注册类型
        /// </summary>
        public static Bootstrapper SetDefault(this Bootstrapper that, Type type, string name)
        {
            return that.SetDefault(type, name, Lifecycle.Singleton);
        }
        /// <summary>
        /// 注册类型
        /// </summary>
        public static Bootstrapper SetDefault(this Bootstrapper that, Type type)
        {
            return that.SetDefault(type, (string)null, Lifecycle.Singleton);
        }

        /// <summary>
        /// 注册类型
        /// </summary>
        public static Bootstrapper SetDefault(this Bootstrapper that, Type from, Type to, Lifecycle lifecycle)
        {
            return that.SetDefault(from, to, (string)null, lifecycle);
        }
        /// <summary>
        /// 注册类型
        /// </summary>
        public static Bootstrapper SetDefault(this Bootstrapper that, Type from, Type to, string name)
        {
            return that.SetDefault(from, to, name, Lifecycle.Singleton);
        }
        /// <summary>
        /// 注册类型
        /// </summary>
        public static Bootstrapper SetDefault(this Bootstrapper that, Type from, Type to)
        {
            return that.SetDefault(from, to, (string)null, Lifecycle.Singleton);
        }

        ///// <summary>
        ///// 注册类型
        ///// </summary>
        //public static Bootstrapper RegisterMultiple(this Bootstrapper that, Type registrationType, IEnumerable<Type> implementationTypes)
        //{
        //    return that.RegisterMultiple(registrationType, implementationTypes, Lifecycle.Singleton);
        //}
        ///// <summary>
        ///// 注册类型
        ///// </summary>
        //public static Bootstrapper RegisterMultiple(this Bootstrapper that, Type registrationType, IEnumerable<Type> implementationTypes, Lifecycle lifecycle)
        //{
        //    foreach(var implementationType in implementationTypes) {
        //        that.Register(registrationType, implementationType, implementationType.FullName, lifecycle);
        //    }

        //    return that;
        //}
        ///// <summary>
        ///// 注册类型
        ///// </summary>
        //public static Bootstrapper RegisterMultiple(this Bootstrapper that, IEnumerable<Type> registrationTypes, Type implementationType)
        //{
        //    return that.RegisterMultiple(registrationTypes, implementationType, Lifecycle.Singleton);
        //}
        ///// <summary>
        ///// 注册类型
        ///// </summary>
        //public static Bootstrapper RegisterMultiple(this Bootstrapper that, IEnumerable<Type> registrationTypes, Type implementationType, Lifecycle lifecycle)
        //{
        //    foreach(var registrationType in registrationTypes) {
        //        that.Register(registrationType, implementationType, lifecycle);
        //    }

        //    return that;
        //}

        /// <summary>
        /// 注册类型
        /// </summary>
        public static Bootstrapper SetDefault<T>(this Bootstrapper that)
        {
            return that.SetDefault<T>((string)null, Lifecycle.Singleton);
        }
        /// <summary>
        /// 注册类型
        /// </summary>
        public static Bootstrapper SetDefault<T>(this Bootstrapper that, Lifecycle lifecycle)
        {
            return that.SetDefault<T>((string)null, lifecycle);
        }
        /// <summary>
        /// 注册类型
        /// </summary>
        public static Bootstrapper SetDefault<T>(this Bootstrapper that, string name)
        {
            return that.SetDefault<T>(name, Lifecycle.Singleton);
        }
        /// <summary>
        /// 注册类型
        /// </summary>
        public static Bootstrapper SetDefault<T>(this Bootstrapper that, string name, Lifecycle lifecycle)
        {
            return that.SetDefault(typeof(T), name, lifecycle);
        }
        /// <summary>
        /// 注册类型
        /// </summary>
        public static Bootstrapper SetDefault<TFrom, TTo>(this Bootstrapper that)
             where TTo : TFrom
        {
            return that.SetDefault<TFrom, TTo>((string)null, Lifecycle.Singleton);
        }
        /// <summary>
        /// 注册类型
        /// </summary>
        public static Bootstrapper SetDefault<TFrom, TTo>(this Bootstrapper that, Lifecycle lifecycle)
            where TTo : TFrom
        {
            return that.SetDefault<TFrom, TTo>((string)null, lifecycle);
        }
        /// <summary>
        /// 注册类型
        /// </summary>
        public static Bootstrapper SetDefault<TFrom, TTo>(this Bootstrapper that, string name)
            where TTo : TFrom
        {
            return that.SetDefault<TFrom, TTo>(name, Lifecycle.Singleton);
        }
        /// <summary>
        /// 注册类型
        /// </summary>
        public static Bootstrapper SetDefault<TFrom, TTo>(this Bootstrapper that, string name, Lifecycle lifecycle)
            where TTo : TFrom
        {
            return that.SetDefault(typeof(TFrom), typeof(TTo), name, lifecycle);
        }
    }
}
