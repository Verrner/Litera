using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Next.Litera
{
    public sealed class ScopeChildrenContentContainer : PointerManipulator
    {
        private ScopeElement m_element;
        private GraphWindow m_window;

        public bool NeedToAdd;

        public ScopeChildrenContentContainer(ScopeElement element, GraphWindow window)
        {
            m_element = element;
            m_window = window;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerEnterEvent>(PointerEnter);
            target.RegisterCallback<PointerLeaveEvent>(PointerLeave);
            target.RegisterCallback<PointerDownEvent>(PointerDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerEnterEvent>(PointerEnter);
            target.UnregisterCallback<PointerLeaveEvent>(PointerLeave);
            target.UnregisterCallback<PointerDownEvent>(PointerDown);
        }

        private void PointerDown(PointerDownEvent evt)
        {
            if (m_window.Selection.Count > (m_element.Selected ? 1 : 0) && NeedToAdd)
            {
                m_element.AddChildren(m_window.Selection.Where(e => e != m_element));
            }
            NeedToAdd = false;
        }

        private void PointerLeave(PointerLeaveEvent evt)
        {
            Color color = new(m_element.Color.r, m_element.Color.g, m_element.Color.b, .1f);
            target.style.backgroundColor = color;
            target.style.borderRightColor = color;
            target.style.borderLeftColor = color;
            target.style.borderBottomColor = color;
            target.style.borderTopColor = color;
            target.style.borderRightWidth = 0;
            target.style.borderLeftWidth = 0;
            target.style.borderBottomWidth = 0;
            target.style.borderTopWidth = 0;
            NeedToAdd = false;
        }

        private void PointerEnter(PointerEnterEvent evt)
        {
            if (m_window.Selection.Count > (m_element.Selected ? 1 : 0) && m_window.MovingSelection)
            {
                target.style.backgroundColor = new Color(50f / 255f, 112f / 255f, 144f / 255f);
                target.style.borderRightColor = new Color(68f / 255f, 192f / 255f, 1);
                target.style.borderLeftColor = new Color(68f / 255f, 192f / 255f, 1);
                target.style.borderBottomColor = new Color(68f / 255f, 192f / 255f, 1);
                target.style.borderTopColor = new Color(68f / 255f, 192f / 255f, 1);
                target.style.borderRightWidth = 2;
                target.style.borderLeftWidth = 2;
                target.style.borderBottomWidth = 2;
                target.style.borderTopWidth = 2;
                NeedToAdd = true;
            }
        }
    }
}