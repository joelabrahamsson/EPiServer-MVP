using System;
using System.Reflection;
using Ninject;
using Ninject.Parameters;
using PageTypeBuilder;
using WebFormsMvp;

namespace EPiMVP.Ninject
{
    /// <summary>
    /// A Ninject StandardKernel extended with capabilities with allows it to act as a Factory for the PresenterBinder in the Web Forms MVP framework.
    /// It's also customized to work specifically with the EPiServer extensions to the Web Forms MVP framework.
    /// </summary>
    public class NinjectPresenterFactory : EPiPresenterFactory
    {
        public IKernel Kernel { get; private set; }
        public NinjectPresenterFactory(IKernel kernel)
        {
            if (kernel == null)
                throw new ArgumentException("kernel cannot be null");
            Kernel = kernel;
        }

        protected override bool CanUseConstructor(ConstructorInfo constructor, Type viewType, Type pageDataType)
        {
            var constructorParameters = constructor.GetParameters();
            return
                constructorParameters.Length >= 2 &&
                constructorParameters[0].ParameterType.IsAssignableFrom(viewType) &&
                constructorParameters[1].ParameterType.IsAssignableFrom(pageDataType);
        }

        protected override IPresenter CreatePresenterInstance(Type presenterType, TypedPageData pageData, Type viewType, IEPiView view)
        {
            // Unfortunately, Ninject needs the bloody names of the parameters, 
            // so we need to figure them out by reflecting.
            string pageDataParameterName = null;
            string viewParameterName = null;
            foreach (var constructor in presenterType.GetConstructors())
            {
                var constructorParameters = constructor.GetParameters();
                foreach (var parameter in constructorParameters)
                {
                    if (parameter.ParameterType.IsAssignableFrom(pageData.GetType()))
                        pageDataParameterName = parameter.Name;

                    if (parameter.ParameterType.IsAssignableFrom(view.GetType()))
                        viewParameterName = parameter.Name;
                }
            }
            var parameters = new IParameter[]
                                 {
                                     new ConstructorArgument(viewParameterName, view),
                                     new ConstructorArgument(pageDataParameterName, pageData)
                                 };
            return (IPresenter)Kernel.Get(presenterType, parameters);
        }

    }
}
