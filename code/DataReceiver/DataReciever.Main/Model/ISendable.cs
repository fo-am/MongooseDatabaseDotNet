namespace DataReciever.Main.Model
{
    public interface ISendable
    {
        int sent { get; set; }
        string UniqueId { get; set; }
        int entity_id { get; set; }
        string entity_type { get; set; }
    }
}