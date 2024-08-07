﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cadastre.DataProcessor.ImportDtos
{
    public class ImportCitizenDto
    {
        [JsonProperty("FirstName")]
        [Required]
        [MinLength(2)]
        [MaxLength(30)]
        public string FirstName { get; set; } = null!;

        [JsonProperty("LastName")]
        [Required]
        [MinLength(2)]
        [MaxLength(30)]
        public string LastName { get; set; } = null!;

        [JsonProperty("BirthDate")]
        [Required]
        public string BirthDate { get; set; } = null!;

        [JsonProperty("MaritalStatus")]
        [Required]
        public string MaritalStatus { get; set; } = null!;

        [JsonProperty("Properties")]
        public int[] PropertiesIds { get; set; } = null!;
    }
}
