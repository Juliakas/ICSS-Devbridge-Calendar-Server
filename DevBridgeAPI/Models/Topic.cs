﻿using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PostTopic = DevBridgeAPI.Models.Post.Topic;

namespace DevBridgeAPI.Models
{
    /// <summary>
    /// Information about a subject that can be taken as assignment and used by users to
    /// track their learning progress.
    /// </summary>
    public class Topic : IModel
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        [Key]
        public int TopicId { get; set; }
        /// <summary>
        /// Topic name (ex. Javascript basics, Team management basics etc..)
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// A free text field used for detailed description of topic
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Used for making topic learning hierarchy. May be used to specify
        /// another topic that is recommended requirement before this topic.
        /// May be null if there is no such topic.
        /// </summary>
        public int? ParentTopicId { get; set; }
        /// <summary>
        /// ID of user that updated/created this version of topic
        /// </summary>
        public int ChangeByUserId { get; set; }
        /// <summary>
        /// Start date of this topic version
        /// </summary>
        [Computed]
        public DateTime SysStart { get; set; }
        /// <summary>
        /// End date of this topic version
        /// </summary>
        [Computed]
        public DateTime SysEnd { get; set; }

        public void UpdateFields (PostTopic updatedTopic)
        {
            Name = updatedTopic.Name;
            Description = updatedTopic.Description;
            ParentTopicId = updatedTopic.ParentTopicId;
            ChangeByUserId = updatedTopic.ChangeByUserId.Value; // Can never be null at this point
        }
    }
}