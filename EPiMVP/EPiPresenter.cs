using PageTypeBuilder;
using WebFormsMvp;

namespace EPiMVP
{
    public abstract class EPiPresenter<TView, TPageDataType> : Presenter<TView> where TView : class, IView where TPageDataType : TypedPageData
    {
        public TPageDataType CurrentPage { get; private set; }

        protected EPiPresenter(TView view, TPageDataType pageData) : base(view)
        {
            CurrentPage = (TPageDataType)pageData;
        }

        public override void ReleaseView() { }
    }

    /// <summary>
    /// This is a Presenter without a page type. 
    /// </summary>
    /// <typeparam name="TView"></typeparam>
    public abstract class EPiPresenter<TView> : Presenter<TView> where TView : class, IView
    {
        protected EPiPresenter(TView view) : base(view) { }

        public override void ReleaseView() { }
    }
}