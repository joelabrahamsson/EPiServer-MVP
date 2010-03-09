using EPiServer.Core;
using WebFormsMvp;

namespace EPiMVP
{
    public interface IEPiView<TModel> : IEPiView, IView<TModel> where TModel : class, new()
    {

    }

    public interface IEPiView : IView
    {
        PageData CurrentPage { get; set; }
    }
}
