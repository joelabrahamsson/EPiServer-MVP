using System;
using System.Linq;
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
            return
                constructorParameters.Length >= 2 &&
                constructorParameters[0].ParameterType.IsAssignableFrom(viewType) &&
                constructorParameters[1].ParameterType.IsAssignableFrom(pageDataType);
        }

        protected override IPresenter CreatePresenterInstance(Type presenterType, TypedPageData pageData, Type viewType, IEPiView view)
        {
            var constructors = presenterType.GetConstructors();
            var pageDataType = GetPageDataType(view);
            ConstructorInfo constructorToUse = constructors.FirstOrDefault(c => CanUseConstructor(c, viewType, pageDataType));
            if (constructorToUse == null)
            {
                throw new NullReferenceException("Did not find a suitable constructor on the presenter of type " + presenterType + ". "
                                 + "The presenter constructor requires at least two parameters, the FIRST one accepting a " + viewType + " and a the SECOND one a " + pageDataType + ".");
            }

            ParameterInfo[] parameters = constructorToUse.GetParameters();
            object[] constructorParametersToUse = new object[parameters.Length];
            constructorParametersToUse[0] = view;
            constructorParametersToUse[1] = view.CurrentPage;
            for (int i = 2; i < constructorParametersToUse.Length; i++)
            {
                constructorParametersToUse[i] = ResolveParameter(parameters[i].ParameterType, i, view);
            }
            return (IPresenter)Activator.CreateInstance(presenterType, constructorParametersToUse);
        }

        protected virtual object ResolveParameter(Type parameterType, int parameterIndex, IEPiView view)
        {
            return container.GetInstance(parameterType);
        }
    }
}
