using UnityEngine.Networking;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public static class UnityWebRequestAsyncOperationExtensions
{
    public static TaskAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
    {
        var tcs = new TaskCompletionSource<object>();
        asyncOp.completed += obj => { tcs.SetResult(null); };
        return ((Task)tcs.Task).GetAwaiter();
    }
} 