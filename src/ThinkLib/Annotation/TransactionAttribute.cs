using System;
using ThinkLib.Composition;
using ThinkLib.Database;
using ThinkLib.Database.Context;
using ThinkLib.Interception;

namespace ThinkLib.Annotation
{
    /// <summary>
    /// 事务
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TransactionAttribute : InterceptorAttribute
    {
        class TransactionInterceptor : IInterceptor
        {
            public readonly IDataContextFactory dataContextFactory;

            public TransactionInterceptor(IDataContextFactory dataContextFactory)
            {
                this.dataContextFactory = dataContextFactory;
            }

            #region IInterceptor 成员

            public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptorDelegate getNext)
            {
                var context = dataContextFactory.Create() as IContext;

                CurrentContext.Bind(context);

                var methodReturn = getNext().Invoke(input, getNext);

                using (CurrentContext.Unbind(context.ContextManager) as IDisposable) 
                { }

                return methodReturn;
            }

            #endregion
        }

        /// <summary>
        /// 创建事务的拦截器
        /// </summary>
        public override IInterceptor CreateInterceptor(IObjectContainer container)
        {
            var dataContextFactory = container.Resolve<IDataContextFactory>();
            if(dataContextFactory== null) {

            } 
            return new TransactionInterceptor(dataContextFactory);
        }
    }
}
