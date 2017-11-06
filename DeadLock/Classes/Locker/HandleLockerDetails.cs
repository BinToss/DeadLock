namespace DeadLock.Classes.Locker
{
    /// <summary>
    /// This class represents the details of a process or handle that is currently locking another file
    /// </summary>
    internal sealed class HandleLockerDetails
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string ProcessId { get; set; }
    }
}
