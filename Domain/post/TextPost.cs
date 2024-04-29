﻿using UBB_SE_2024_Team_42.Domain.Reactions;
using UBB_SE_2024_Team_42.Utils;
using UBB_SE_2024_Team_42.Utils.Functionals;

namespace UBB_SE_2024_Team_42.Domain.Posts
{
    public class TextPost : IPost
    {
        public long ID { get; }
        public long UserID { get; set; }
        public string Content { get; set; }
        public DateTime DatePosted { get; set; }
        public DateTime DateOfLastEdit { get; set; }
        public List<IReaction> Reactions { get; set; }
        public TextPost(long postingUserID, string content)
        {
            ID = IDGenerator.RandomLong();
            UserID = postingUserID;
            Content = content;
            DatePosted = DateTime.Now;
            DateOfLastEdit = DateTime.Now;
            Reactions = new ();
        }

        internal TextPost(long postID, long userID, string content, DateTime postTime, DateTime editTime, List<IReaction> reactions)
        {
            ID = postID;
            UserID = userID;
            Content = content;
            DatePosted = postTime;
            DateOfLastEdit = editTime;
            Reactions = reactions;
        }

        public TextPost()
        {
            ID = IDGenerator.RandomLong();
            Content = "None";
            Reactions = new ();
        }

        public override string ToString()
        {
            return $"TextPost {{postID: {ID}, userID: {UserID}, datePosted: {DatePosted}, dateOfLastEdit: {DateOfLastEdit})\n" +
                $"{Content}\n" +
                $"reactions: {CollectionStringifier<IReaction>.ApplyTo(Reactions)}}}";
        }
    }
}
