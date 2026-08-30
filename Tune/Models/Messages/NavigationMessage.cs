using System;
using System.Collections.Generic;
using System.Text;

namespace Tune.Models.Messages
{
    public class NavigationMessage
    {
        public Type PageType { get; }

        public NavigationMessage(Type pageType)
        {
            PageType = pageType;
        }
    }
}
