using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using PageTypeBuilder;
using WebFormsMvp;
using WebFormsMvp.Binder;
using EPiServer.Core;

namespace WebFormsMvp.EPiServer
{
    /// <summary>
    /// A IPresenterFactory that can act as a Factory for the PresenterBinder in the Web Forms MVP framework,
    /// customized to work specifically with the EPiServer extensions to the Web Forms MVP framework.
    /// </summary>
    public class EPiPresenterFactory : IPresenterFactory
    {

        #region IPresenterFactory Members

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
            ;
            if (!typeof(IEPiView).IsAssignableFrom(viewType))
                throw new InvalidCastException("This kernel can (and should) only create a presenter if the View implements IEPiView. Got " + viewType);

            var epiView = viewInstance as IEPiView; // Unchecked cast is ok sicne we check it above.
            if (epiView.CurrentPage == null)
                throw new NullReferenceException("CurrentPage property of the viewInstance was null. The presenter needs a proper page data to render. ");

            var pageDataType = epiView.CurrentPage.GetType();

            // when PageTypeBuilder is at work, the PageData will be a proxy object. In this case, get the base class.
            var isProxyObject = pageDataType.ToString().ToLower().Contains("proxy");
            if (isProxyObject)
                pageDataType = pageDataType.BaseType;

            
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
            var genericPresenterViewType = abstractionInterface ?? viewType;

            // Validate and check the Presenter type.
            var correctPresenterType = typeof(EPiPresenter<,>).MakeGenericType(new Type[] { genericPresenterViewType, pageDataType });
            if (!presenterType.IsSubclassOf(correctPresenterType))
                throw new InvalidCastException("Tried to create presenter of type " +presenterType + 
                    ". This kernel can (and should) only create presenters that are a subclass of " + correctPresenterType + "."+
                    " This bugger, however, is a subclass of " + presenterType.BaseType);
            
            // Check if the Presenter has a usable constructor.
            var constructors = presenterType.GetConstructors();
            var foundValidConstructor = false;
            foreach (var constructor in constructors)
            {
                var constructorParameters = constructor.GetParameters();
                var signatureIsUsable = constructorParameters[0].ParameterType.IsAssignableFrom(viewType) &&
                                        constructorParameters[1].ParameterType.IsAssignableFrom(pageDataType) &&
                                        constructorParameters.Length == 2;
                if (signatureIsUsable)
                {
                    foundValidConstructor = true;
                    break;
                }
            }
            if (!foundValidConstructor)
                throw new NullReferenceException("Did not find a suitable constructor on the presenter of type " + presenterType + ". "
                                                 + "The presenter constructor requires two parameters, the FIRST one accepting a " + viewType + " and a the SECOND one a " + pageDataType + ".");
            return (IPresenter)CreatePresenterInstance(presenterType, (TypedPageData)epiView.CurrentPage, epiView);
        }

        /// <summary>
        /// This is the actual method that creates the instance. 
        /// Called by Create. It can be 
        /// overridden in case you need to do more stuff to the Presenter
        /// when creating it (injecting services using an IOC container, for instance).
        /// </summary>
        /// <param name="presenterType"></param>
        /// <param name="pageData"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        protected virtual IPresenter CreatePresenterInstance(Type presenterType, TypedPageData pageData, IEPiView view)
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

        #endregion


    }
}