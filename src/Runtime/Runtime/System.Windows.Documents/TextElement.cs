﻿
/*===================================================================================
* 
*   Copyright (c) Userware/OpenSilver.net
*      
*   This file is part of the OpenSilver Runtime (https://opensilver.net), which is
*   licensed under the MIT license: https://opensource.org/licenses/MIT
*   
*   As stated in the MIT license, "the above copyright notice and this permission
*   notice shall be included in all copies or substantial portions of the Software."
*  
\*====================================================================================*/

using CSHTML5.Internal;

#if MIGRATION
using System.Windows.Controls;
#else
using Windows.UI.Xaml.Controls;
#endif

#if MIGRATION
namespace System.Windows.Documents
#else
namespace Windows.UI.Xaml.Documents
#endif
{
    /// <summary>
    /// An abstract class used as the base class for the also-abstract Block and Inline classes. 
    /// TextElement supports common API for classes involved in the XAML text object model,
    /// such as properties that control text size, font families and so on.
    /// </summary>
    public abstract class TextElement : Control
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextElement"/> class.
        /// </summary>
        protected TextElement()
        {
            // TextElement should not be a Control, ence it should not be able to
            // get focus.
            IsTabStop = false;
        }

        internal virtual string TagName => "span";

        internal sealed override void AddEventListeners() { }

        internal sealed override UIElement MouseTarget => null;

        internal sealed override UIElement KeyboardTarget => null;

        public override object CreateDomElement(object parentRef, out object domElementWhereToPlaceChildren)
            => domElementWhereToPlaceChildren = INTERNAL_HtmlDomManager.CreateTextElementDomElementAndAppendIt(parentRef, this);

        [OpenSilver.NotImplemented]
        public static readonly DependencyProperty CharacterSpacingProperty = 
            DependencyProperty.Register(
                nameof(CharacterSpacing), 
                typeof(int), 
                typeof(TextElement), 
                null);

        [OpenSilver.NotImplemented]
        public int CharacterSpacing
        {
            get { return (int)GetValue(CharacterSpacingProperty); }
            set { SetValue(CharacterSpacingProperty, value); }
        }
    }
}
