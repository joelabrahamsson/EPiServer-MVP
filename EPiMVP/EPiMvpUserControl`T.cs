using System;

namespace EPiMVP
{
    public abstract class EPiMvpUserControl<TModel> : EPiMvpUserControl, IEPiView<TModel> where TModel : class, new()
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
    }
}
