using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK_UI3.Views.Tasks
{
    enum Statuses
    {
        Resume,
        Pause,
        Completed,
        error
    }
    internal class Task
    {
        public static ObservableCollection<Task> tasks = new ObservableCollection<Task>();

        public Task(string name, string description, int maxTaskDoing)
        {
            Name = name;
            Description = description;
            this.maxTaskDoing = maxTaskDoing;
            Status = Statuses.Resume;
            doingStatus = 0;
            tasks.Add(this);
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public int maxTaskDoing { get; set; }
        private int _doingStatus { get; set; }
        public int doingStatus { get { return _doingStatus; } set { _doingStatus = value; onStatusUpdate?.Invoke(this, EventArgs.Empty); } }

        private Statuses _Status;
        public Statuses Status { get { return _Status; } set {

                _Status = value;
                onStatusUpdate?.Invoke(this, EventArgs.Empty);

            }
        }




        public event EventHandler Cancel;
        public event EventHandler onStatusUpdate;
        public event EventHandler onDoingStatusUpdate;
        public event EventHandler ClickTask;

        public void ClickTaskInvoke() {
            ClickTask?.Invoke(this, EventArgs.Empty);
        }
        public void CancelTask() { 
            this.Status = Statuses.Pause;
            Cancel?.Invoke(this, EventArgs.Empty);  
        }

        internal void Resume()
        {
           
        }

        internal void Pause()
        {
           
        }
    }
}
