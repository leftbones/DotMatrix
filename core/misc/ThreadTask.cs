using System.Threading;

namespace DotMatrix;

class ThreadTask : IThreadPoolWorkItem {
    public int Index { get; private set; }
    public Action? Action { get; private set; }
    public Chunk? Chunk { get; private set; }

    public ThreadTask(int index) {
        Index = index;
    }

    public void SetChunk(Chunk C) {
        Chunk = C;
    }

    public void Execute() {
        if (Action is not null)
            Action.Invoke();
    }
}