using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using ThinkLib.Annotation;
using ThinkLib.Composition;
using ThinkLib.Interception;
using ThinkLib.Serialization;

namespace ThinkLib
{
    /// <summary>
    /// 引导程序
    /// </summary>
    public class Bootstrapper
    {
        class Component
        {
            /// <summary>
            /// Parameterized constructor.
            /// </summary>
            public Component(Type type, string name, Lifecycle lifecycle)
            {
                this.ForType = type;
                this.ContractName = name ?? string.Empty;
                this.Lifecycle = lifecycle;
            }
            /// <summary>
            /// Parameterized constructor.
            /// </summary>
            public Component(Type from, Type to, string name, Lifecycle lifecycle)
                : this(to, name, lifecycle)
            {
                this.ContractType = from;
            }

            /// <summary>
            /// 要注册的名称
            /// </summary>
            public string ContractName { get; set; }
            /// <summary>
            /// 要注册的类型
            /// </summary>
            public Type ContractType { get; private set; }
            /// <summary>
            /// 要注册类型的实现类型
            /// </summary>
            public Type ForType { get; private set; }
            /// <summary>
            /// 生命周期
            /// </summary>
            public Lifecycle Lifecycle { get; private set; }

            private Type GetServiceType()
            {
                return this.ContractType ?? this.ForType;
            }

            /// <summary>
            /// 返回一个值，该值指示此实例是否与指定的对象相等。
            /// </summary>
            public override bool Equals(object obj)
            {
                var other = obj as Component;

                if (other == null)
                    return false;

                if (this.GetServiceType() != other.GetServiceType())
                    return false;

                if (!String.Equals(this.ContractName, other.ContractName))
                    return false;

                return true;
            }
            /// <summary>
            /// 返回此实例的哈希代码。
            /// </summary>
            public override int GetHashCode()
            {
                return String.Concat(this.GetServiceType().FullName, "|", this.ContractName).GetHashCode();
            }

            internal int GetUniqueCode()
            {
                if (IsInitializeType(this.ForType))
                    return this.ForType.GetHashCode();

                if (IsInitializeType(this.ContractType))
                    return this.ContractType.GetHashCode();


                return this.GetHashCode();
            }

            internal bool MustbeInitialize()
            {
                return this.Lifecycle == Lifecycle.Singleton &&
                    (IsInitializeType(this.ContractType) || IsInitializeType(this.ForType));
            }

            private static bool IsInitializeType(Type type)
            {
                return type != null && type.IsClass && !type.IsAbstract && typeof(IInitializer).IsAssignableFrom(type);
            }

            internal object GetInstance(IObjectContainer container)
            {
                var serviceType = this.GetServiceType();
                var key = this.ContractName;
                return string.IsNullOrWhiteSpace(key) ?
                    container.Resolve(serviceType) :
                    container.Resolve(serviceType, key);
            }

            internal void Register(IObjectContainer container)
            {
                if (container.IsRegistered(this.GetServiceType(), this.ContractName)) {
                    return;
                }

                if (this.ContractType == null)
                    container.RegisterType(this.ForType, this.ContractName, this.Lifecycle);
                else
                    container.RegisterType(this.ContractType, this.ForType, this.ContractName, this.Lifecycle);
            }
        }

        class ComponentComparer : IEqualityComparer<Component>
        {
            public bool Equals(Component x, Component y)
            {
                return x.ForType == y.ForType;
            }

            public int GetHashCode(Component obj)
            {
                return obj.ForType.FullName.GetHashCode();
            }
        }

        /// <summary>
        /// 服务状态
        /// </summary>
        public enum ServerStatus
        {
            /// <summary>
            /// 启动中
            /// </summary>
            Starting,
            /// <summary>
            /// 运行中
            /// </summary>
            Running,
            /// <summary>
            /// 已停止
            /// </summary>
            Stopped
        }

        public class AssembliesLoadedEventArgs : EventArgs
        {
            internal AssembliesLoadedEventArgs(List<Assembly> assemblies)
            {
                this.Assemblies = assemblies;
                this.NonAbstractTypes= assemblies.SelectMany(assembly => assembly.GetTypes())
                    .Where(type => type.IsClass && !type.IsAbstract).ToArray();
            }

            /// <summary>
            /// 程序集列表
            /// </summary>
            public IEnumerable<Assembly> Assemblies { get; private set; }

            /// <summary>
            /// 所有的非抽象类型
            /// </summary>
            public IEnumerable<Type> NonAbstractTypes { get; private set; }
        }

        /// <summary>
        /// 当前配置
        /// </summary>
        public static readonly Bootstrapper Current = new Bootstrapper();


        private List<Assembly> _assemblies;
        private HashSet<Component> _components;
        private readonly Stopwatch _stopwatch;
        /// <summary>
        /// Default Constructor.
        /// </summary>
        protected Bootstrapper()
        {
            this._assemblies = new List<Assembly>();
            this._components = new HashSet<Component>();
            this._stopwatch = Stopwatch.StartNew();
            this.Status = ServerStatus.Starting;            
        }
        

        /// <summary>
        /// 表示程序集加载完成后的处理方式
        /// </summary>
        public event EventHandler<AssembliesLoadedEventArgs> AssembliesLoaded = (sender, args) => { };

        /// <summary>
        /// 当前服务器状态
        /// </summary>
        public ServerStatus Status { get; private set; }

        /// <summary>
        /// 加载程序集
        /// </summary>
        public Bootstrapper LoadAssemblies(Assembly[] assemblies)
        {
            _assemblies.Clear();
            _assemblies.AddRange(assemblies);

            return this;
        }

        /// <summary>
        /// 加载程序集
        /// </summary>
        public Bootstrapper LoadAssemblies(string[] assemblyNames)
        {
            var assemblies = assemblyNames.Select(Assembly.Load).ToArray();

            return this.LoadAssemblies(assemblies);
        }

        /// <summary>
        /// 扫描bin目录的程序集
        /// </summary>
        public Bootstrapper LoadAssemblies()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string relativeSearchPath = AppDomain.CurrentDomain.RelativeSearchPath;
            string binPath = string.IsNullOrEmpty(relativeSearchPath) ? baseDir : Path.Combine(baseDir, relativeSearchPath);
            //string applicationAssemblyDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Bin");
            //if (!FileUtils.DirectoryExists(applicationAssemblyDirectory)) {
            //    applicationAssemblyDirectory = AppDomain.CurrentDomain.BaseDirectory;
            //}


            var assemblies = Directory.GetFiles(binPath)
                .Where(file => {
                    var ext = Path.GetExtension(file).ToLower();
                    return ext.EndsWith(".dll") || ext.EndsWith(".exe");
                })
                .Select(Assembly.LoadFrom)
                .ToArray();

            return this.LoadAssemblies(assemblies);
        }

        
        ///// <summary>
        ///// 配置完成。
        ///// </summary>
        //public void Done()
        //{
        //    this.Done(new OwnObjectContainer());
        //}

        /// <summary>
        /// 配置完成。
        /// </summary>
        public void Done(IObjectContainer container)
        {
            if (this.Status == ServerStatus.Running) {
                return;
            }
            
            if (_assemblies.Count == 0) {
                this.LoadAssemblies();

                Console.WriteLine("load assemblies completed.");
                Console.WriteLine("[");
                foreach (var assembly in _assemblies) {
                    Console.WriteLine(assembly.FullName);
                }
                Console.WriteLine("]");
            }

            var args = new AssembliesLoadedEventArgs(_assemblies);

            this.RegisterComponents(args.NonAbstractTypes);
            this.RegisterDefaultComponents();

            this.AssembliesLoaded(this, new AssembliesLoadedEventArgs(_assemblies));

            _components.ForEach(item => item.Register(container));

            _components.Where(item => item.MustbeInitialize())
                .Select(item => item.GetInstance(container))
                .Distinct()
                .Cast<IInitializer>()
                .ForEach(item => item.Initialize(container, _assemblies));

            _assemblies.Clear();
            _components.Clear();

            _assemblies = null;
            _components = null;

            this.Status = ServerStatus.Running;

            _stopwatch.Stop();

            Console.WriteLine("system is running, used time:{0}ms.\r\n", _stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// 设置组件
        /// </summary>
        public Bootstrapper SetDefault(Type type, string name, Lifecycle lifecycle)
        {
            if (this.Status != ServerStatus.Starting) {
                throw new ApplicationException("system is running, can not register type, please execute before 'Done' method.");
            }
            type.NotNull("type");

            _components.Add(new Component(type, name, lifecycle));

            return this;
        }

        /// <summary>
        /// 设置组件
        /// </summary>
        public Bootstrapper SetDefault(Type from, Type to, string name, Lifecycle lifecycle)
        {
            if (this.Status != ServerStatus.Starting) {
                throw new ApplicationException("system is running, can not register type, please execute before 'done' method.");
            }
            from.NotNull("from");
            to.NotNull("to");

            _components.Add(new Component(from, to, name, lifecycle));

            return this;
        }

        
        private void RegisterComponents(IEnumerable<Type> types)
        {
            var registionTypes = types.Where(p => p.IsDefined(typeof(RegisterAttribute), false));

            foreach (var type in registionTypes) {
                var lifecycle = LifeCycleAttribute.GetLifecycle(type);

                var attribute = type.GetCustomAttribute<RegisterAttribute>(false);
                if (attribute != null) {
                    var contractType = attribute.ContractType;
                    var contractName = attribute.ContractName;
                    if (attribute.ContractType == null) {
                        this.SetDefault(type, contractName, lifecycle);
                    }
                    else {
                        this.SetDefault(attribute.ContractType, type, contractName, lifecycle);
                    }
                }
            }
        }

        private void RegisterDefaultComponents()
        {
            this.SetDefault<IBinarySerializer, DefaultBinarySerializer>();
            this.SetDefault<ITextSerializer, DefaultTextSerializer>();
            this.SetDefault<IInterceptorProvider, InterceptorProvider>();
        }
    }
}
