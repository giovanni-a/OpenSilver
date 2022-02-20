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

using System;
using System.Collections.Generic;
using CSHTML5.Internal;

#if MIGRATION
namespace System.Windows
#else
namespace Windows.UI.Xaml
#endif
{
    public partial class UIElement
    {
        private static readonly Dictionary<RoutedEvent, Func<UIElement, DOMEventManager>> DOMEventManagerFactory;

        private Dictionary<RoutedEvent, DOMEventManager> _domEventManagersStore;

        private bool ShouldHookUpRoutedEvent(RoutedEvent routedEvent, Type ownerType, string methodName, Type[] methodParameters)
        {
            if (_eventHandlersStore != null)
            {
                List<RoutedEventHandlerInfo> handlers = _eventHandlersStore[routedEvent];
                if (handlers != null && handlers.Count > 0)
                {
                    return true;
                }
            }

            return INTERNAL_EventsHelper.IsEventCallbackOverridden(this, ownerType, methodName, methodParameters);
        }

        private void HookUpRoutedEvent(RoutedEvent routedEvent)
        {
            if (_domEventManagersStore == null)
            {
                _domEventManagersStore = new Dictionary<RoutedEvent, DOMEventManager>(1);
            }

            if (!_domEventManagersStore.TryGetValue(routedEvent, out DOMEventManager eventManager))
            {
                _domEventManagersStore[routedEvent] = eventManager = CreateDOMEventManager(routedEvent);
            }

            if (eventManager != null)
            {
                eventManager.AttachToDomEvents();
            }
        }

        private void UnHookRoutedEvent(RoutedEvent routedEvent)
        {
            if (_domEventManagersStore == null)
            {
                return;
            }

            if (_domEventManagersStore.TryGetValue(routedEvent, out DOMEventManager eventManager))
            {
                if (eventManager != null)
                {
                    eventManager.DetachFromDomEvents();
                }
            }
        }

        private DOMEventManager CreateDOMEventManager(RoutedEvent routedEvent)
        {
            if (DOMEventManagerFactory.TryGetValue(routedEvent, out Func<UIElement, DOMEventManager> factory))
            {
                if (factory != null)
                {
                    return factory(this);
                }
            }

            return null;
        }

        private static DOMEventManager CreateMouseMoveManager(UIElement uie)
        {
#if MIGRATION
            return new DOMEventManager(
                () => uie.INTERNAL_OuterDomElement,
                new string[2] { "mousemove", "touchmove" },
                jsEventArg => uie.ProcessPointerEvent(
                    jsEventArg,
                    uie.OnMouseMove,
                    preventTextSelectionWhenPointerIsCaptured: true)
                );
#else
            return new DOMEventManager(
                () => uie.INTERNAL_OuterDomElement,
                new string[2] { "mousemove", "touchmove" },
                jsEventArg => uie.ProcessPointerEvent(
                    jsEventArg,
                    uie.OnPointerMoved,
                    preventTextSelectionWhenPointerIsCaptured: true)
                );
#endif
        }

        private static DOMEventManager CreateMouseLeftButtonDownManager(UIElement uie)
        {
            return new DOMEventManager(
                () => uie.INTERNAL_OuterDomElement,
                new string[2] { "mousedown", "touchstart" },
                jsEventArg =>
                {
                    // We shouldn't trigger OnMouseLeftButtonDown if only right mouse button has been 
                    // triggered, continue as before otherwise
                    //
                    // Javascript Mouse events have a buttons property that can be a bitmask of:
                    //
                    // 0 : No button or un-initialized
                    // 1 : Primary button (usually the left button)
                    // 2 : Secondary button (usually the right button)
                    // 4 : Auxiliary button (usually the mouse wheel button or middle button)
                    // 8 : 4th button (typically the "Browser Back" button)
                    // 16 : 5th button (typically the "Browser Forward" button)
                    int mouseBtn = 0;
                    int.TryParse((OpenSilver.Interop.ExecuteJavaScript("$0.buttons", jsEventArg) ?? 0).ToString(), out mouseBtn);
                    if (mouseBtn != 2)
                    {
#if MIGRATION
                        uie.ProcessMouseButtonEvent(
                            jsEventArg,
                            uie.OnMouseLeftButtonDown,
                            preventTextSelectionWhenPointerIsCaptured: true,
                            checkForDivsThatAbsorbEvents: true,
                            refreshClickCount: true);
#else
                        uie.ProcessMouseButtonEvent(
                            jsEventArg,
                            uie.OnPointerPressed,
                            preventTextSelectionWhenPointerIsCaptured: true,
                            checkForDivsThatAbsorbEvents: true,
                            refreshClickCount: true);
#endif
                    }
                });
        }

#if MIGRATION
        private static DOMEventManager CreateMouseRightButtonDownManager(UIElement uie)
        {
            return new DOMEventManager(
                () => uie.INTERNAL_OuterDomElement,
                new string[2] { "mousedown", "touchstart" },
                jsEventArg =>
                {
                    // We trigger OnMouseRightButtonDown only if right mouse button has been triggered.

                    // Javascript Mouse events have a buttons property that can be a bitmask of:

                    // 0 : No button or un-initialized
                    // 1 : Primary button (usually the left button)
                    // 2 : Secondary button (usually the right button)
                    // 4 : Auxiliary button (usually the mouse wheel button or middle button)
                    // 8 : 4th button (typically the "Browser Back" button)
                    // 16 : 5th button (typically the "Browser Forward" button)
                    int mouseBtn = 0;
                    int.TryParse((OpenSilver.Interop.ExecuteJavaScript("$0.buttons", jsEventArg) ?? 0).ToString(), out mouseBtn);
                    if (mouseBtn == 2)
                    {
                        uie.ProcessMouseButtonEvent(
                            jsEventArg,
                            uie.OnMouseRightButtonDown,
                            preventTextSelectionWhenPointerIsCaptured: true,
                            checkForDivsThatAbsorbEvents: true,
                            refreshClickCount: true);
                    }
                });
        }
#endif

        private static DOMEventManager CreateMouseWheelManager(UIElement uie)
        {
            return new DOMEventManager(
                () => uie.INTERNAL_OuterDomElement,
                "wheel",
#if MIGRATION
                uie.ProcessOnPointerWheelChangedEvent
#else
                jsEventArg => uie.ProcessPointerEvent(
                    jsEventArg, 
                    uie.OnPointerWheelChanged, 
                    preventTextSelectionWhenPointerIsCaptured: false, 
                    checkForDivsThatAbsorbEvents: false, 
                    refreshClickCount: false)
#endif
            );
        }

        private static DOMEventManager CreateMouseLeftButtonUpManager(UIElement uie)
        {
#if MIGRATION
            return new DOMEventManager(
                () => uie.INTERNAL_OuterDomElement,
                new string[2] { "mouseup", "touchend" },
                jsEventArg => uie.ProcessMouseButtonEvent(
                    jsEventArg,
                    uie.OnMouseLeftButtonUp,
                    checkForDivsThatAbsorbEvents: true),
                true);
#else
            return new DOMEventManager(
                () => uie.INTERNAL_OuterDomElement,
                new string[2] { "mouseup", "touchend" },
                jsEventArg => uie.ProcessMouseButtonEvent(
                    jsEventArg,
                    uie.OnPointerReleased,
                    checkForDivsThatAbsorbEvents: true),
                true);
#endif
        }

        private static DOMEventManager CreateMouseEnterManager(UIElement uie)
        {
#if MIGRATION
            return new DOMEventManager(
                () => uie.INTERNAL_OuterDomElement,
                new string[1] { "mouseenter" },
                uie.ProcessOnMouseEnter
            );
#else
            return new DOMEventManager(
                () => uie.INTERNAL_OuterDomElement,
                new string[1] { "mouseenter" },
                uie.ProcessOnPointerEntered
            );
#endif
        }

        private static DOMEventManager CreateMouseLeaveManager(UIElement uie)
        {
#if MIGRATION
            return new DOMEventManager(
                () => uie.INTERNAL_OuterDomElement,
                new string[1] { "mouseleave" },
                uie.ProcessOnMouseLeave
            );
#else
            return new DOMEventManager(
                () => uie.INTERNAL_OuterDomElement,
                new string[1] { "mouseleave" },
                uie.ProcessOnPointerExited
            );
#endif
        }

        private static DOMEventManager CreateTextInputManager(UIElement uie)
        {
            return new DOMEventManager(
                () => uie.INTERNAL_OuterDomElement,
                "input",
                uie.ProcessOnTextInput);
        }

        private static DOMEventManager CreateTextInputStartManager(UIElement uie)
        {
            return null;
        }

        private static DOMEventManager CreateTextInputUpdateManager(UIElement uie)
        {
            return null;
        }

        private static DOMEventManager CreateTappedManager(UIElement uie)
        {
            return new DOMEventManager(
                () => uie.INTERNAL_OuterDomElement,
                "mouseup",
                uie.ProcessOnTapped);
        }

        private static DOMEventManager CreateMouseRightButtonUpManager(UIElement uie)
        {
#if MIGRATION
            return new DOMEventManager(
                () => uie.INTERNAL_OuterDomElement,
                "contextmenu",
                uie.ProcessOnMouseRightButtonUp
            );
#else
            return new DOMEventManager(
                () => uie.INTERNAL_OuterDomElement,
                "contextmenu",
                uie.ProcessOnRightTapped
            );
#endif
        }

        private static DOMEventManager CreateKeyDownManager(UIElement uie)
        {
            return new DOMEventManager(
                () => uie.INTERNAL_OuterDomElement,
                "keydown",
                uie.ProcessOnKeyDown,
                true);
        }

        private static DOMEventManager CreateKeyUpManager(UIElement uie)
        {
            return new DOMEventManager(
                () => uie.INTERNAL_OuterDomElement,
                "keyup",
                uie.ProcessOnKeyUp);
        }

        private static DOMEventManager CreateGotFocusManager(UIElement uie)
        {
            return new DOMEventManager(
                () => uie.INTERNAL_OuterDomElement,
                "focusin",
                uie.ProcessOnGotFocus);
        }

        private static DOMEventManager CreateLostFocusManager(UIElement uie)
        {
            return new DOMEventManager(
                () => uie.INTERNAL_OuterDomElement,
                "focusout",
                uie.ProcessOnLostFocus);
        }

        private static DOMEventManager CreateGotFocusForIsTabStopManager(UIElement uie)
        {
            return new DOMEventManager(
                () => uie.INTERNAL_OuterDomElement,
                "focusin",
                uie.ProcessOnGotFocusForIsTabStop);
        }
    }
}
