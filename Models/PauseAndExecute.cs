using System;
using System.Threading.Tasks;

namespace TelstarClient.Models;

public class PauseAndExecuter
{
    public async Task Execute(Action action, int timeoutInMilliseconds)
    {
        await Task.Delay(timeoutInMilliseconds);
        action();
    }
}