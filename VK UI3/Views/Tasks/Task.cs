using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet.Model.RequestParams.Ads;

namespace VK_UI3.Views.Tasks
{


    public class ProgressEventArgs : EventArgs
    {
        public int Processed { get; }
        public int Total { get; }

        public ProgressEventArgs(int processed, int total)
        {
            Processed = processed;
            Total = total;
        }
    }

    public abstract class TaskAction
    {
        public event EventHandler<ProgressEventArgs> ProgressChanged;
        public event EventHandler Cancelled;
        public event EventHandler Completed;
        public event EventHandler<StatusChangedEventArgs> StatusChanged;

      

        protected void OnProgressChanged(ProgressEventArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }
        protected void OnCancelled(EventArgs e)
        {
            Cancelled?.Invoke(this, e);
        }
        protected void OnCompleted(EventArgs e)
        {
            Completed?.Invoke(this, e);
        }
        protected void OnStatusChanged(StatusChangedEventArgs e)
        {
            StatusChanged?.Invoke(this, e);
        }

        private Statuses _status;
        public Statuses Status
        {
            get => _status;
            set
            {
                if (_status == Statuses.Completed) return;
                if (_status != value)
                {
                    _status = value;
                    OnStatusChanged(new StatusChangedEventArgs(_status));
                }
            }
        }
        

        private int _progress;
        internal static ObservableCollection<TaskAction> tasks = new ObservableCollection<TaskAction>();
        public string nameTask { get; set; }
        public string taskID { get; set; }  
        protected TaskAction(int total, string nameTask, string taskID)
        {
            this.total = total;
            this.nameTask = nameTask;
            this.taskID = taskID;
            bool exists = tasks.Any(task => task.taskID == this.taskID);
        
            if (exists)
                return;

            tasks.Add(this);
        }

        public int total { get; set; }

        public int Progress
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    OnProgressChanged(new ProgressEventArgs(_progress, total));
                }
            }
        }
        public abstract void onClick();
        public abstract void Cancel();
        public abstract void Pause();
        public abstract void Resume();

        public void delete() 
        {
            if (Status != Statuses.Error && Status != Statuses.Completed && Status != Statuses.Cancelled)
            {
                Cancel();
            }
            ProgressChanged = null;
            Cancelled = null;
            Completed = null;
            StatusChanged = null;
            tasks.Remove(this);
        }
    }

    public class StatusChangedEventArgs : EventArgs
    {
        public Statuses Status { get; }

        public StatusChangedEventArgs(Statuses status)
        {
            Status = status;
        }
    }


    public enum Statuses
    {
        Resume,
        Pause,
        Completed,
        Error,
        Cancelled
    }

   

}
