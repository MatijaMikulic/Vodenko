using SharedLibrary.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SharedLibrary.Entities
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        public int MessageId { get; set; }

        [ForeignKey("MessageId")]
        public virtual MessageHeader MessageHeader { get; set; }

        [Required]
        public int Status { get; set; }

        public DateTime EnqueueDT { get; set; }
        
        public DateTime DequeueDT { get; set; }

        [Required]
        public string Payload { get; set; }

        [NotMapped]
        public Dictionary<string, string> PayloadDictionary
        {
            get { return JsonSerializer.Deserialize<Dictionary<string, string>>(Payload); }
            set { Payload = JsonSerializer.Serialize(value); }
        }
        [NotMapped]
        public Dictionary<string, string> GetPayloadDictionary
        {
            get { return JsonSerializer.Deserialize<Dictionary<string, string>>(Payload); }
        }
        public int RetryCount { get; set; }
        public string ErrorLog { get; set; }
    }
}
