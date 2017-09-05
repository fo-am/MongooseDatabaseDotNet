using Dapper.Contrib.Extensions;

namespace DataPipe.Main
{
    public interface ISendable
    {
        int sent { get; set; }
    }

    [Table("sync_entity")]
    public class sync_entity : ISendable
    {
        [Key]
        public int entity_id { get; set; }

        public string entity_type { get; set; }
        public string unique_id { get; set; }
        public int dirty { get; set; }
        public int version { get; set; }
        public int sent { get; set; }
    }



    [Table("stream_attribute")]
    public class stream_attribute : ISendable
    {
        [Key]
        public int id { get; set; }

        public string attribute_id { get; set; }
        public string entity_type { get; set; }
        public string attribute_type { get; set; }
        public int sent { get; set; }
    }

    [Table("stream_entity")]
    public class stream_entity : ISendable
    {
        [Key]
        public int id { get; set; }

        public int entity_id { get; set; }
        public string entity_type { get; set; }
        public string unique_id { get; set; }
        public int dirty { get; set; }
        public int version { get; set; }
        public int sent { get; set; }
    }

    [Table("stream_value_file")]
    public class stream_value_file : ISendable
    {
        [Key]
        public int id { get; set; }

        public int entity_id { get; set; }
        public string attribute_id { get; set; }
        public string value { get; set; }
        public int dirty { get; set; }
        public int version { get; set; }
        public int sent { get; set; }
    }

    [Table("stream_value_int")]
    public class stream_value_int : ISendable
    {
        [Key]
        public int id { get; set; }

        public int entity_id { get; set; }
        public string attribute_id { get; set; }
        public int value { get; set; }
        public int dirty { get; set; }
        public int version { get; set; }
        public int sent { get; set; }
    }

    [Table("stream_value_real")]
    public class stream_value_real : ISendable
    {
        [Key]
        public int id { get; set; }

        public int entity_id { get; set; }
        public string attribute_id { get; set; }
        public double value { get; set; }
        public int dirty { get; set; }
        public int version { get; set; }
        public int sent { get; set; }
    }

    [Table("stream_value_varchar")]
    public class stream_value_varchar : ISendable
    {
        [Key]
        public int id { get; set; }

        public int entity_id { get; set; }
        public string attribute_id { get; set; }
        public string value { get; set; }
        public int dirty { get; set; }
        public int version { get; set; }
        public int sent { get; set; }
    }

    [Table("sync_attribute")]
    public class sync_attribute : ISendable
    {
        [Key]
        public int id { get; set; }

        public string attribute_id { get; set; }
        public string entity_type { get; set; }
        public int attribute_type { get; set; }
        public int sent { get; set; }
    }

    [Table("sync_value_file")]
    public class sync_value_file : ISendable
    {
        [Key]
        public int id { get; set; }

        public int entity_id { get; set; }
        public string attribute_id { get; set; }
        public string value { get; set; }
        public int dirty { get; set; }
        public int version { get; set; }
        public int sent { get; set; }
    }

    [Table("sync_value_int")]
    public class sync_value_int : ISendable
    {
        [Key]
        public int id { get; set; }

        public int entity_id { get; set; }
        public string attribute_id { get; set; }
        public int value { get; set; }
        public int dirty { get; set; }
        public int version { get; set; }
        public int sent { get; set; }
    }

    [Table("sync_value_real")]
    public class sync_value_real : ISendable
    {
        [Key]
        public int id { get; set; }

        public int entity_id { get; set; }
        public string attribute_id { get; set; }
        public double value { get; set; }
        public int dirty { get; set; }
        public int version { get; set; }
        public int sent { get; set; }
    }
    [Table("sync_value_varchar")]
    public class sync_value_varchar : ISendable
    {
        [Key]
        public int id { get; set; }

        public int entity_id { get; set; }
        public string attribute_id { get; set; }
        public string value { get; set; }
        public int dirty { get; set; }
        public int version { get; set; }
        public int sent { get; set; }
    }

}