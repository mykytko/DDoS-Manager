// See https://aka.ms/new-console-template for more information

using DDoS_Manager;

var threads = 10;
const string targetsFileName = "resources/targets";

var targetSupplier = new TargetSupplier(targetsFileName);
Console.WriteLine("Starting " + threads + " tasks...");

if (args.Length == 1)
{
    var isParsed = int.TryParse(args[0], out threads);
    if (!isParsed)
    {
        Console.WriteLine("Invalid argument!");
        return;
    }
}
var tasks = new AttackTaskManager(threads, targetSupplier);

AppDomain.CurrentDomain.ProcessExit += (_, _) =>
{
    tasks.TerminateAttack();
};

tasks.InitializeAttack();
