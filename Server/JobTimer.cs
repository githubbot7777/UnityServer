using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;
namespace Server
{
    struct JobTimerElem : IComparable<JobTimerElem>
    {
        public int execTick;//실행 시간
        public Action action;

        public int CompareTo(JobTimerElem other)
        {
            return other.execTick - execTick;
        }
    }
    class JobTimer
    {
        PriorityQueue<JobTimerElem> _pq = new PriorityQueue<JobTimerElem>();//오래된 작업이 우선순위 높음
        object _lock = new object();

        public static JobTimer Instance { get; } = new JobTimer();

        public void Push(Action action, int tickAfter=0)//tickAfter 몇초후에 실행할건지
        {
            JobTimerElem job;
            job.execTick = System.Environment.TickCount + tickAfter;
            job.action = action;

            lock(_lock)
            {
                _pq.Push(job);
            }
        }

        public void Flush()
        {
            while(true)
            {
                int now = System.Environment.TickCount;

                JobTimerElem job;
                lock(_lock)
                {
                    if (_pq.Count == 0)
                        break;

                    job = _pq.Peek();
                    if (job.execTick > now)
                        break;

                    _pq.Pop();
                    
                }

                job.action.Invoke();
            }
        }
    }
}
