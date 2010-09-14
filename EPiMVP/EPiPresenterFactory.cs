using System;
using System.Reflection;
using PageTypeBuilder;
using WebFormsMvp;
using WebFormsMvp.Binder;

namespace EPiMVP
{
    /// <summary>
    /// A IPresenterFactory that can act as a Factory for the PresenterBinder in the Web Forms MVP framework,
    /// customized to work specifically with the EPiServer extensions to the Web Forms MVP framework.
    /// </summary>
    public class EPiPresenterFactory : IPresenterFactory
    {
        /// <summary>
        /// Creates the specified Presenter type. Called by the Web Forms MVP Framework.
        /// </summary>
        /// <param name="presenterType">Type of the presenter. Must be of type EPiPresenter<TView, TPageDataType> and the type arguments must be of the same type as the View and View Page Data Type, respectively.</param>
        /// <param name="viewType">Type of the view. Must be a subclass of EPiView.</param>
        /// <param name="viewInstance">The view instance. Must be a child of EPiView.</param>
        /// <returns></returns>
        public IPresenter Create(Type presenterType, Type viewType, IView viewInstance)
        {
            // Validate the View 
            if (!typeof(IEPiView).IsAssignableFrom(viewType))
                throw new InvalidCastException("This kernel can (and should) only create a presenter if the View implements IEPiView. Got " + viewType);

            var epiView = viewInstance as IEPiView; // Unchecked cast is ok sicne we check it above.
            if (epiView.CurrentPage == null)
                throw new NullReferenceException("CurrentPage property of the viewInstance was null. The presenter needs a proper page data to render. ");

            Type pageDataType = GetPageDataType(epiView);

            Type genericPresenterViewType = GetGenericPresenterViewType(viewType);

            // Validate and check the Presenter type.
            //var correctPresenterType = typeof(EPiPresenter<,>).MakeGenericType(new Type[] { genericPresenterViewType, pageDataType });
            //if (!presenterType.IsSubclassOf(correctPresenterType))
            //    throw new InvalidCastException("Tried to create presenter of type " +presenterType + 
            //        ". This kernel can (and should) only create presenters that are a subclass of " + correctPresenterType + "."+
            //        " This bugger, however, is a subclass of " + presenterType.BaseType);
            
            // Check if the Presenter has a usable constructor.
            if (!CanCreateInstance(viewType, pageDataType, presenterType))
                throw new NullReferenceException("Did not find a suitable constructor on the presenter of type " + presenterType + ". "
                                                 + "The presenter constructor requires two parameters, the FIRST one accepting a " + viewType + " and a the SECOND one a " + pageDataType + ".");
            return (IPresenter)CreatePresenterInstance(presenterType, (TypedPageData)epiView.CurrentPage, viewType, epiView);
        }

        protected Type GetPageDataType(IEPiView epiView)
        {
            var pageDataType = epiView.CurrentPage.GetType();

            // when PageTypeBuilder is at work, the PageData will be a proxy object. In this case, get the base class.
            var isProxyObject = pageDataType.ToString().ToLower().Contains("proxy");
            if (isProxyObject)
                pageDataType = pageDataType.BaseType;
            return pageDataType;
        }

        private Type GetGenericPresenterViewType(Type viewType)
        {
            // Find out if the View is abstracted into an interface with the same name
            // I.e. WidgetView -> IWidgetView
            Type abstractionInterface = null;
            var codeBehind = viewType.BaseType; // We want the codebehind name, not "ASP.views_widgetview_aspx"
            if (codeBehind != null) // The will be no base type if we are mocking the interface directly
            {
                foreach (var iface in codeBehind.GetInterfaces())
                {
                    if (iface.Name == "I" + codeBehind.Name)
                        abstractionInterface = iface;
                }
            }

            // If it is abstracted, expect the presenter to use the abstraction. 
            // Otherwise, use the vanilla viewtype. 
            return abstractionInterface ?? viewType;
        }

        protected virtual bool CanCreateInstance(Type viewType, Type pageDataType, Type presenterType)
        {
            var constructors = presenterType.GetConstructors();
            foreach (var constructor in constructors)
            {
                if (CanUseConstructor(constructor, viewType, pageDataType))
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual bool CanUseConstructor(ConstructorInfo constructor, Type viewType, Type pageDataType)
        {
            var constructorParameters = constructor.GetParameters();
            return constructorParameters[0].ParameterType.IsAssignableFrom(viewType) &&
                   constructorParameters[1].ParameterType.IsAssignableFrom(pageDataType) &&
                   constructorParameters.Length == 2;
        }

        /// <summary>
        /// This is the actual method that creates the instance. 
        /// Called by Create. It can be 
        /// overridden in case you need to do more stuff to the Presenter
        /// when creating it (injecting services using an IOC container, for instance).
        /// </summary>
        protected virtual IPresenter CreatePresenterInstance(Type presenterType, TypedPageData pageData, Type viewType, IEPiView view)
        {
            return (IPresenter)Activator.CreateInstance(presenterType, new object[] { view, pageData });
        }
        
        

        /// <summary>
        /// Releases the specified presenter.
        /// </summary>
        /// <param name="presenter">The presenter.</param>
        public void Release(IPresenter presenter)
        {
            
        }
    }
}