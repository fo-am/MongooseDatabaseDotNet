namespace DataReciever.Main.Interfaces
{
    internal interface IHandle<in T>
    {
        void HandleMessage(T message);
    }
}