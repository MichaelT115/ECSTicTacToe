using Unity.Collections;
using Unity.Jobs;

public struct ConvertNativeQueueToNativeList<T> : IJob 
    where T : struct
{
    [ReadOnly] public NativeQueue<T> queue;
    [WriteOnly] public NativeList<T> list;

    public void Execute()
    {
        while (queue.Count > 0)
        {
            list.Add(queue.Dequeue());
        }
    }
}