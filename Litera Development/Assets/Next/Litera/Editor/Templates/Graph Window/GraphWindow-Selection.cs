using Next.Litera.Scripting;
using System;
using System.Collections.Generic;

namespace Next.Litera
{
    public abstract partial class GraphWindow
    {
        private List<GraphElement> m_selection = new();

        public List<GraphElement> Selection => new(m_selection);

        public Action SelectionChanged { get; set; }

        public void Select(GraphElement element)
        {
            if (!element.Selected)
            {
                m_selection.Add(element);
                element.RedrawSelection();
                SelectionChanged?.Invoke();
            }
        }

        public void Deselect(GraphElement element)
        {
            if (element.Selected)
            {
                m_selection.Remove(element);
                element.RedrawSelection();
                SelectionChanged?.Invoke();
            }
        }

        public void ClearSelection()
        {
            while (m_selection.Count > 0) Deselect(m_selection[0]);
            SelectionChanged?.Invoke();
        }

        public void DeleteSelection()
        {
            while (m_selection.Count > 0) RemoveElement(m_selection[0]);
            SelectionChanged?.Invoke();
        }
    }
}