using System;
using System.Reflection;
using PageTypeBuilder;
using StructureMap;
using WebFormsMvp;

namespace EPiMVP.StructureMap
{
    public class StructureMapPresenterFactory : EPiPresenterFactory
    {
        private IContainer container;

        public StructureMapPresenterFactory(IContainer container)
        {
            this.container = container;
        }

        protected override bool CanUseConstructor(ConstructorInfo constructor, Type viewType, Type pageDataType)
        {
            var constructorParameters = constructor.GetParameters();
            return constructorParameters[0].ParameterType.IsAssignableFrom(viewType) &&
                   constructorParameters[1].ParameterType.IsAssignableFrom(pageDataType);
        }

        protected override IPresenter CreatePresenterInstance(Type presenterType, TypedPageData pageData, Type viewType, IEPiView view)
        {
            ConstructorInfo constructorToUse = null;
            var constructors = presenterType.GetConstructors();
            foreach (var constructor in constructors)
            {
                if (CanUseConstructor(constructor, viewType, GetPageDataType(view)))
                {
                    constructorToUse = constructor;
                    break;
                }
            }

            ParameterInfo[] parameters = constructorToUse.GetParameters();
            object[] constructorParametersToUse = new object[parameters.Length];
            constructorParametersToUse[0] = view;
            constructorParametersToUse[1] = view.CurrentPage;
            for (int i = 2; i < constructorParametersToUse.Length; i++)
            {
                constructorParametersToUse[i] = ResolveParameter(parameters[i].ParameterType, i);
            }
            return (IPresenter)Activator.CreateInstance(presenterType, constructorParametersToUse);
        }

        protected virtual object ResolveParameter(Type parameterType, int parameterIndex)
        {
            return container.GetInstance(parameterType);
        }
    }
}
