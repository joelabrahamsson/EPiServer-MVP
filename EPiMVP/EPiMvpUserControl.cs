using System;
using System.Collections.Generic;
using EPiServer;
using WebFormsMvp.Web;

namespace EPiMVP
{
    public abstract class EPiMvpUserControl : UserControlBase, IEPiView
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            PageViewHost.Register(this, Context);

            base.OnInit(e);
        }
    }
}
