﻿using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevBridgeAPI.Models
{
    public class Goal : IModel
    {
        [Key]
        public int GoalId { get; set; }
        public int? UserId { get; set; }
        public int? TeamManagerId { get; set; }
        public int TopicId { get; set; }
        public string Role { get; set; }
        public DateTime Deadline { get; set; }
    }
}