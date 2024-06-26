﻿namespace UBB_SE_2024_Team_42.Domain.Notification
{
    public interface INotification
    {
        long ID { get; set; }
        long UserID { get; set; }
        string Text { get; set; }
        long? BadgeID { get; set; }
        long? PostID { get; set;  }
    }
}