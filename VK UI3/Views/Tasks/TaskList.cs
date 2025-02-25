using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK_UI3.Views.Tasks
{
    public class TaskListActions : TaskAction
    {
        private List<Task> _tasks;
        private int _delay;

        public TaskListActions(List<Task> tasks, int total, string nameTask, string taskID, string subTextTask, int delay = 0)
            : base(total, nameTask, taskID, subTextTask)
        {
            _tasks = tasks;
            
            _delay = delay;
            ExecuteTasksAsync();
        }

        public void AddTask(Task task)
        {
            _tasks.Add(task);
        }

        public async Task ExecuteTasksAsync()
        {
            foreach (var task in _tasks)
            {


                while (base.Status != Statuses.Resume)
                {
                    await Task.Delay(100);
                }

                if (base.Status == Statuses.Cancelled)
                {
                    base.delete();
                    break;
                }

                task.Start();
                await task;

                if (task.Exception != null)
                {

                }
                else
                {
                
                }


                    base.Progress++;
                await Task.Delay(_delay);
            }
            if (Status != Statuses.Cancelled)
            {
                Status = Statuses.Completed;
            }
        }

        public override void Cancel()
        {
            base.Status = Statuses.Cancelled;
            base.delete();
        }

        public override void onClick()
        {
       
        }

        public override void Pause()
        {
            base.Status = Statuses.Pause;
        }

        public override void Resume()
        {
            base.Status = Statuses.Resume;
        }
    }

}
