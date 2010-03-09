using System;
using EPiServer;
using EPiServer.Core;
using WebFormsMvp;
using WebFormsMvp.Web;

namespace EPiMVP
{

    public interface IEPiView<TModel> : IEPiView, IView<TModel> where TModel : class, new()
    {

    }

    public interface IEPiView : IView
    {
        PageData CurrentPage { get; set; }
    }

    /// <summary>
    /// Represents a page that is a view in a Web Forms Model-View-Presenter application.
    /// </summary>
    public abstract class EPiView : TemplatePage, IEPiView
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event to initialize the page.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            PageViewHost.Register(this, Context);
            base.OnInit(e);
        }
    }


    /// <summary>
    /// Represents a page that is a view with a strongly typed model in a Web Forms Model-View-Presenter application.
    /// </summary>
    public abstract class EPiView<TModel> : EPiView, IEPiView<TModel> where TModel : class, new()
    {
        TModel _model;

        /// <summary>
        /// Gets or sets the view model.
        /// </summary>
        /// <value>The view model.</value>
        public TModel Model
        {
            get
            {
                if (_model == null)
                    throw new InvalidOperationException("The Model property is currently null, however it should have been automatically initialized by the presenter. This most likely indicates that no presenter was bound to the control. Check your presenter bindings.");

                return _model;
            }
            set
            {
                _model = value;
            }
        }

        protected override void OnPreRenderComplete(EventArgs e)
        {
            DataBind();
            base.OnPreRenderComplete(e);
        }
    }
}