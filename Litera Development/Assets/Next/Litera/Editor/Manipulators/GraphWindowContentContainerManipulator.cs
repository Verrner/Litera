using Next.Litera.Scripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Next.Litera
{
    public sealed class GraphWindowContentContainerManipulator : Manipulator
    {
        private bool m_selecting;
        private bool m_shift;
        private bool m_ctrl;
        private Vector2 m_startPosition;
        private Vector2 m_startLocalPosition;
        private VisualElement m_indicator;

        private GraphWindow m_window;

        private bool m_dragging;
        private int m_pointerId;

        public bool MovingSelection { get; private set; }
        private Vector2 m_previousSelectionPosition;
        private Vector2 m_startSelectionMovementPosition;

        public GraphWindowContentContainerManipulator(GraphWindow window)
        {
            m_window = window;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.focusable = true;

            target.RegisterCallback<PointerDownEvent>(PointerDown);
            target.RegisterCallback<PointerMoveEvent>(PointerMove);
            target.RegisterCallback<PointerUpEvent>(PointerUp);
            target.RegisterCallback<KeyDownEvent>(KeyDown);
            target.RegisterCallback<KeyUpEvent>(KeyUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(PointerDown);
            target.UnregisterCallback<PointerMoveEvent>(PointerMove);
            target.UnregisterCallback<PointerUpEvent>(PointerUp);
            target.UnregisterCallback<KeyDownEvent>(KeyDown);
            target.UnregisterCallback<KeyUpEvent>(KeyUp);
        }

        private void PointerDown(PointerDownEvent evt)
        {
            if (evt.button == 0)
            {
                m_shift = evt.shiftKey;
                m_ctrl = evt.ctrlKey;
                m_selecting = true;
                m_pointerId = evt.pointerId;
                target.CapturePointer(m_pointerId);
                m_startLocalPosition = new(evt.localPosition.x, evt.localPosition.y);
                m_startPosition = m_window.LocalToGridPosition(m_startLocalPosition);
                m_indicator = new();
                m_window.ContentContainer.Add(m_indicator);
                RefreshSelectionIndicatorColor();
                RefreshIndicatorSizeAndPosition(m_startLocalPosition);
            }
            else if (evt.button == 1)
            {
                GenericMenu menu = m_window.GetGenericMenu();
                if (m_window.Selection.Count == 1)
                {
                    menu.AddSeparator("");
                    m_window.Selection[0].GetGenericMenu(menu);
                }
                if (menu != null)
                {
                    menu.ShowAsContext();
                }
            }
            else if (evt.button == 2)
            {
                m_dragging = true;
                m_pointerId = evt.pointerId;
                target.CapturePointer(m_pointerId);
            }
            evt.StopPropagation();
        }

        private void PointerMove(PointerMoveEvent evt)
        {
            Vector2 currentGridPosition = m_window.LocalToGridPosition(new(evt.localPosition.x, evt.localPosition.y));
            if (MovingSelection)
            {
                m_window.Selection.ForEach(e =>
                {
                    if (e is not IConditionalDraggable draggable || draggable.Draggable) e.MoveElement(currentGridPosition - m_startSelectionMovementPosition);
                });
                m_window.SelectionMoved?.Invoke();
            }
            m_previousSelectionPosition = currentGridPosition;

            if (!target.HasPointerCapture(m_pointerId))
            {
                evt.StopImmediatePropagation();
                return;
            }

            if (m_dragging)
            {
                m_window.CameraPosition += new Vector2(evt.deltaPosition.x, evt.deltaPosition.y) / m_window.Zoom;
            }
            else if (m_selecting)
            {
                RefreshIndicatorSizeAndPosition(new(evt.localPosition.x, evt.localPosition.y));
            }

            evt.StopPropagation();
        }

        private void PointerUp(PointerUpEvent evt)
        {
            if (!target.HasPointerCapture(m_pointerId))
            {
                evt.StopImmediatePropagation();
                return;
            }

            if (evt.button == 2 && m_dragging)
            {
                m_dragging = false;
                target.ReleasePointer(m_pointerId);
            }
            if (evt.button == 0 && m_selecting)
            {
                m_selecting = false;
                target.ReleasePointer(m_pointerId);

                if (!MovingSelection)
                {
                    if (!m_shift && !m_ctrl) m_window.ClearSelection();

                    for (int i = m_window.Elements.Count - 1; i >= 0; i--)
                    {
                        GraphElement element = m_window.Elements[i];

                        Vector2 colliderPosition = element.Position;
                        Vector2 colliderSize = new(element.ElementContent.resolvedStyle.width + 20, element.ElementContent.resolvedStyle.height + 20);
                        Vector2 position = m_window.LocalToGridPosition(new(evt.localPosition.x, evt.localPosition.y));

                        if (new Rect(colliderPosition, colliderSize).Overlaps(new(Vector2.Min(m_startPosition, position), new(Mathf.Abs(m_startPosition.x - position.x), Mathf.Abs(m_startPosition.y - position.y)))))
                        {
                            if (!m_ctrl && !element.Selected) element.Select();
                            else if (m_ctrl && element.Selected) element.Deselect();

                            if (m_startPosition == position) break;
                        }
                    }
                }

                m_window.ContentContainer.Remove(m_indicator);
                m_indicator = null;

                m_shift = false;
                m_ctrl = false;
            }
            if (evt.button == 0 && MovingSelection)
            {
                MovingSelection = false;
                m_window.Selection.ForEach(e => e.FinishMovement());
            }
            evt.StopPropagation();
        }

        private void KeyUp(KeyUpEvent evt)
        {
            if (!evt.shiftKey) m_shift = false;
            if (!evt.ctrlKey) m_ctrl = false;

            evt.StopPropagation();
            RefreshSelectionIndicatorColor();
        }

        private void KeyDown(KeyDownEvent evt)
        {
            if (evt.shiftKey) m_shift = true;
            if (evt.ctrlKey) m_ctrl = true;

            if (evt.keyCode == KeyCode.G && !MovingSelection && m_window.Selection.Count > 0)
            {
                MovingSelection = true;
                m_window.Selection.ForEach(s => s.BeginMovement());
                m_startSelectionMovementPosition = m_previousSelectionPosition;
            }
            else if (evt.keyCode == KeyCode.Delete)
            {
                while (m_window.Selection.Count > 0) m_window.RemoveElement(m_window.Selection[0]);
            }
            else if (evt.keyCode == KeyCode.F)
            {
                m_window.FocusOnElements(m_window.Selection);
            }
            else if (evt.keyCode == KeyCode.Z)
            {
                m_window.Zoom = 1;
                m_window.CameraPosition = Vector2.zero;
            }

            evt.StopPropagation();
            RefreshSelectionIndicatorColor();
        }

        private void RefreshSelectionIndicatorColor()
        {
            if (m_indicator == null) return;

            m_indicator.style.backgroundColor = m_ctrl ? new Color(1, 0, 0, .5f) : m_shift ? new Color(0, 1, 0, .5f) : new Color(0, 0, 1, .5f);

            Color borderColor = m_ctrl ? Color.red : m_shift ? Color.green : Color.blue;

            m_indicator.style.borderBottomColor = borderColor;
            m_indicator.style.borderTopColor = borderColor;
            m_indicator.style.borderLeftColor = borderColor;
            m_indicator.style.borderRightColor = borderColor;

            m_indicator.style.borderBottomWidth = 1;
            m_indicator.style.borderTopWidth = 1;
            m_indicator.style.borderLeftWidth = 1;
            m_indicator.style.borderRightWidth = 1;
        }

        private void RefreshIndicatorSizeAndPosition(Vector2 localPosition)
        {
            if (m_indicator == null) return;

            m_indicator.transform.position = new(Mathf.Min(localPosition.x, m_startLocalPosition.x), Mathf.Min(localPosition.y, m_startLocalPosition.y));
            m_indicator.style.width = Mathf.Abs(m_startLocalPosition.x - localPosition.x);
            m_indicator.style.height = Mathf.Abs(m_startLocalPosition.y - localPosition.y);
        }
    }
}