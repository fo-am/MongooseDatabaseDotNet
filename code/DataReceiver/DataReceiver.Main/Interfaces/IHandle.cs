namespace DataReceiver.Main.Interfaces
{
    internal interface IHandle<in T>
    {
        void HandleMessage(T message);
    }
}