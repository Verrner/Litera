using UnityEngine;

namespace Next.Act
{
    [AddComponentMenu("Next/Act/Executer")]
    public class ActExecuter : MonoBehaviour
    {
        public ActExecutionProject Project;
        public bool ActivateOnAwake = true;

        public bool Activated { get; private set; }

        private void Awake()
        {
            if (ActivateOnAwake) ActivateProjectExecution();
        }

        public void ActivateProjectExecution()
        {
            if (Project == null) return;

            Project.Execute();
            Activated = true;
        }

        private void Update()
        {
            if (Project == null || !Activated)
            {
                Activated = false;
                return;
            }

            if (!Project.UpdateExecution()) Activated = false;
        }
    }
}