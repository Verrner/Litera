using UnityEngine.UIElements;

namespace Next.Litera
{
    public sealed class GraphWindowZoomManipulator : Manipulator
    {
        private GraphWindow m_window;

        public GraphWindowZoomManipulator(GraphWindow window)
        {
            m_window = window;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<WheelEvent>(Wheel);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<WheelEvent>(Wheel);
        }

        private void Wheel(WheelEvent evt)
        {
            m_window.Zoom -= evt.mouseDelta.y * .05f;
        }
    }
}