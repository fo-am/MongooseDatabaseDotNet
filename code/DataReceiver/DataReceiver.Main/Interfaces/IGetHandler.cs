namespace DataReceiver.Main.Interfaces
{
    public interface IGetHandler
    {
        void Handle<T>(T output);
    }
}