using Dapper.Contrib.Extensions;

namespace DataReciever.Main
{
   
    public class sync_entity
    {
       
        public int entity_id { get; set; }
        public string entity_type { get; set; }
        public string unique_id { get; set; }
        public int dirty { get; set; }
        public int version { get; set; }

    }

    
    public class sync_value_varchar
    {
        
        public int id { get; set; }
        public int entity_id { get; set; }
        public string attribute_id { get; set; }
        public string value { get; set; }
        public int dirty { get; set; }
        public int version { get; set; }

    }
}
