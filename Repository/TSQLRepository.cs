﻿using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using UBB_SE_2024_Team_42.Domain.Badge;
using UBB_SE_2024_Team_42.Domain.Category;
using UBB_SE_2024_Team_42.Domain.Notification;
using UBB_SE_2024_Team_42.Domain.Post;
using UBB_SE_2024_Team_42.Domain.Post.Interfaces;
using UBB_SE_2024_Team_42.Domain.Posts;
using UBB_SE_2024_Team_42.Domain.Reactions;
using UBB_SE_2024_Team_42.Domain.Tag;
using UBB_SE_2024_Team_42.Domain.User;
using UBB_SE_2024_Team_42.Service.EntityCreationServices;
using UBB_SE_2024_Team_42.Utils.Functionals;

namespace UBB_SE_2024_Team_42.Repository
{
    public class TSQLRepository : IRepository
    {
        private readonly string sqlConnectionString;
        private static Image? CellInDBToBadgeImage(object dataRowCell) => Image.FromStream(new MemoryStream((byte[])dataRowCell));
        public TSQLRepository()
        {
            sqlConnectionString = @"Data Source = CAMFRIGLACLUJ; Initial Catalog = Team42DB;Integrated Security = True";
        }
        public TSQLRepository(string customConnectionString)
        {
            sqlConnectionString = customConnectionString;
        }
        private EnumerableRowCollection<DataRow> QueryDB(string sqlStatement)
        {
            SqlConnection connection = new (sqlConnectionString);
            connection.Open();
            SqlCommand command = new (sqlStatement, connection);
            SqlDataAdapter dataAdapter = new (command);
            DataTable dataTable = new ();
            dataAdapter.Fill(dataTable);
            connection.Close();
            return dataTable.AsEnumerable();
        }
        private INotification RowInDBToNotification(DataRow row)
            => new NotificationBuilder().Begin()
                .SetID(Convert.ToInt64(row["id"]))
                .SetUserID(Convert.ToInt64(row["userId"]))
                .SetPostID(Convert.ToInt64(row["postId"]))
                .SetBadgeId(Convert.ToInt64(row["badgeId"]))
                .End();
        private IBadge RowInDBToBadge(DataRow row)
            => new BadgeBuilder().Begin()
                .SetID(Convert.ToInt64(row["id"]))
                .SetName(row["name"].ToString() ?? string.Empty)
                .SetDescription(row["description"].ToString() ?? string.Empty)
                .SetImage(CellInDBToBadgeImage(row["image"]))
                .End();
        private ICategory RowInDBToCategory(DataRow row)
            => new CategoryBuilder().Begin()
                .SetID(Convert.ToInt64(row["id"]))
                .SetName(row["name"].ToString() ?? string.Empty)
                .End();
        private IUser RowInDBToUser(DataRow row)
        {
            long userId = Convert.ToInt64(row["id"]);
            return new UserBuilder().Begin()
                              .SetName(row["name"].ToString() ?? string.Empty)
                              .SetNotificationList(GetNotificationsOfUser(userId).ToList())
                              .SetCategoriesModeratedList(GetCategoriesModeratedByUser(userId).ToList())
                              .SetBadgeList(GetBadgesOfUser(userId).ToList())
                              .End();
        }
        private IReaction RowInDBToIReaction(DataRow row)
            => new ReactionBuilder().Begin()
                .SetReacterUserId(Convert.ToInt64(row["userId"]))
                .SetReactionValue(Convert.ToInt32(row["value"]))
                .End();
        private ITag RowInDBToTag(DataRow row)
            => new TagBuilder().Begin()
                .SetID(Convert.ToInt64(row["id"]))
                .SetName(row["name"].ToString() ?? string.Empty)
                .End();
        private IAnswer RowInDBToAnswer(DataRow row)
            => new AnswerBuilder().Begin()
               .SetId(Convert.ToInt64(row["id"]))
               .SetUserId(Convert.ToInt64(row["userId"]))
               .SetContent(Convert.ToString(row["content"]) ?? string.Empty)
               .SetDatePosted(Convert.ToDateTime(row["datePosted"]))
               .SetDateOfLastEdit(Convert.ToDateTime(row["dateOfLastEdit"]))
               .SetReactions(GetReactionsOfPostByPostID(Convert.ToInt64(row["id"])).ToList())
               .End();
        private IComment RowInDBToComment(DataRow row)
            => new CommentBuilder().Begin()
                .SetId(Convert.ToInt64(row["id"]))
                .SetUserId(Convert.ToInt64(row["userId"]))
                .SetContent(Convert.ToString(row["content"]) ?? string.Empty)
                .SetDatePosted(Convert.ToDateTime(row["datePosted"]))
                .SetDateOfLastEdit(DateTime.TryParse(row["dateOfLastEdit"].ToString(), out DateTime parsingResult)
                                                                                ? parsingResult
                                                                                : Convert.ToDateTime(row["datePosted"]))
                .End();
        private IQuestion RowInDBToQuestion(DataRow row)
        {
            long questionId = Convert.ToInt64(row["id"]);
            long userId = Convert.ToInt64(row["userId"]);
            List<ITag> tagList = GetTagsOfQuestion(questionId).ToList();
            List<IReaction> voteList = GetReactionsOfPostByPostID(questionId).ToList();
            ICategory category = GetCategoryByID(Convert.ToInt64(row["categoryId"]));
            DateTime postDate = Convert.ToDateTime(row["datePosted"]);
            DateTime lastEditDate = DateTime.TryParse(row["dateOfLastEdit"].ToString(), out DateTime editDate)
                ? editDate
                : postDate;
            string title = row["title"]?.ToString() ?? string.Empty;
            string content = row["content"]?.ToString() ?? string.Empty;
            return new QuestionBuilder().Begin()
                                  .SetId(questionId)
                                  .SetTitle(title)
                                  .SetCategory(category)
                                  .SetTags(tagList)
                                  .SetUserId(userId)
                                  .SetContent(content)
                                  .SetPostTime(postDate)
                                  .SetEditTime(lastEditDate)
                                  .SetReactions(voteList)
                                  .End();
        }
        private IPost RowInDBToReply(DataRow row)
        {
            long id = Convert.ToInt64(row["id"]);
            long userId = Convert.ToInt64(row["userId"]);
            DateTime datePosted = Convert.ToDateTime(row["datePosted"]);
            DateTime dateOfLastEdit = row["dateOfLastEdit"] == DBNull.Value
                ? datePosted
                : Convert.ToDateTime(row["dateOfLastEdit"]);
            string type = row["type"].ToString() ?? string.Empty;
            string content = row["content"].ToString() ?? string.Empty;
            List<IReaction> reactions = GetReactionsOfPostByPostID(id).ToList();
            if (new List<string>() { "post", "comment", "answer" }.Contains(type))
            {
                return new TextPostBuilder().Begin()
                    .SetID(id).SetUserId(userId).SetReactions(reactions).SetContent(content)
                    .SetDatePosted(datePosted).SetDateOfLastEdit(dateOfLastEdit).End();
            }
            else
            {
                string title = row["title"].ToString() ?? string.Empty;
                ICategory category = GetCategoryByID(Convert.ToInt64(row["categoryId"]));
                List<ITag> tags = GetTagsOfQuestion(id).ToList();
                return new QuestionBuilder().Begin().
                    SetId(id).SetUserId(userId).SetTitle(title).SetCategory(category).
                    SetTags(tags).SetReactions(reactions).SetContent(content).
                    SetPostTime(datePosted).SetEditTime(dateOfLastEdit).End();
            }
        }
        public IEnumerable<INotification> GetNotificationsOfUser(long userId)
            => StreamProcessor<DataRow, INotification>.MapCollection(QueryDB("select * from dbo.getNotificationsOfUser(" + userId + ")"), RowInDBToNotification);

        public IEnumerable<ICategory> GetCategoriesModeratedByUser(long userId)
            => StreamProcessor<DataRow, ICategory>.MapCollection(QueryDB("select * from dbo.getCategoriesModeratedByUser(" + userId + ")"), RowInDBToCategory);

        public IEnumerable<IBadge> GetBadgesOfUser(long userId)
            => StreamProcessor<DataRow, IBadge>.MapCollection(QueryDB("select * from dbo.getBadgesOfUser(" + userId + ")"), RowInDBToBadge);

        public IEnumerable<IUser> GetAllUsers()
            => StreamProcessor<DataRow, IUser>.MapCollection(QueryDB("select * from dbo.getAllUsers()"), RowInDBToUser);

        public IEnumerable<IReaction> GetReactionsOfPostByPostID(long postId)
            => StreamProcessor<DataRow, IReaction>.MapCollection(QueryDB("select * from dbo.getVotesOfPost(" + postId + ")"), RowInDBToIReaction);

        public IEnumerable<ICategory> GetAllCategories()
            => StreamProcessor<DataRow, ICategory>.MapCollection(QueryDB("select * from dbo.getAllCategories()"), RowInDBToCategory);

        public IEnumerable<ITag> GetTagsOfQuestion(long questionId)
            => StreamProcessor<DataRow, ITag>.MapCollection(QueryDB("select * from dbo.getTagById(" + questionId + ")"), RowInDBToTag);
        public IEnumerable<IQuestion> GetAllQuestions()
            => StreamProcessor<DataRow, IQuestion>.MapCollection(QueryDB("select * from dbo.getAllQuestions()"), RowInDBToQuestion);
        public IEnumerable<IAnswer> GetAnswersOfUser(long userId)
            => StreamProcessor<DataRow, IAnswer>.FilterAndMapCollection(QueryDB("select * from dbo.getPostsByUserId(" + userId + ")"), Filters.DataRowRepresentsAnswer, RowInDBToAnswer);
        public IEnumerable<IComment> GetCommentsOfUser(long userId)
            => StreamProcessor<DataRow, IComment>.FilterAndMapCollection(QueryDB("select * from dbo.getPostsByUserId(" + userId + ")"), Filters.DataRowRepresentsComment, RowInDBToComment);
        public IEnumerable<IQuestion> GetQuestionsOfUser(long userId)
            => StreamProcessor<DataRow, IQuestion>.FilterAndMapCollection(QueryDB("select * from dbo.getPostsByUserId(" + userId + ")"), Filters.DataRowRepresentsQuestion, RowInDBToQuestion);
        public IQuestion GetQuestion(long questionId)
            => StreamProcessor<DataRow, IQuestion>.MapOne(QueryDB("select * from dbo.getQuestionByID(" + questionId + ")"), RowInDBToQuestion);

        public IUser GetUser(long userId)
            => StreamProcessor<DataRow, IUser>.MapOne(QueryDB("select * from dbo.getUser(" + userId + ")"), RowInDBToUser);
        public ICategory GetCategoryByID(long categoryId)
            => StreamProcessor<DataRow, ICategory>.MapOne(QueryDB("select * from dbo.getCategoryByID(" + categoryId + ")"), RowInDBToCategory);
        public void AddPostReply(IPost reply, long postId)
        {
            SqlConnection sqlConnection = new (sqlConnectionString);
            sqlConnection.Open();
            SqlCommand command = new ("INSERT INTO dbo.Replies (idOfPostRepliedOn, idOfReply) VALUES (@idOfPostRepliedOn, @idOfReply)");
            command.Parameters.AddWithValue("@idOfPostRepliedOn", postId);
            command.Parameters.AddWithValue("@idOfReply", reply.ID);
            command.ExecuteNonQuery();
            sqlConnection.Close();
        }
        public IEnumerable<IPost> GetRepliesOfPost(long postId)
            => StreamProcessor<DataRow, IPost>.MapCollection(QueryDB("select * from dbo.GetAllRepliesOfPost(" + postId + ")"), RowInDBToReply);
        public void AddQuestion(IQuestion question)
        {
            SqlConnection sqlConnection = new (sqlConnectionString);
            sqlConnection.Open();
            SqlCommand command = new ("addQuestion", sqlConnection);
            command.Parameters.AddWithValue("@userID", question.UserID);
            command.Parameters.AddWithValue("@content", question.Content);
            command.Parameters.AddWithValue("@title", question.Title);
            command.Parameters.AddWithValue("@categoryId", question.Category?.ID);
            command.CommandType = CommandType.StoredProcedure;
            command.ExecuteNonQuery();
            sqlConnection.Close();
        }
        public void UpdatePost(IPost oldPost, IPost newPost)
        {
            if (oldPost is not Answer && oldPost is not Comment)
            {
                return;
            }
            SqlConnection connection = new (sqlConnectionString);
            connection.Open();
            SqlCommand command;
            switch (oldPost.GetType())
            {
                case Type t when t == typeof(Answer):
                    command = new SqlCommand("UpdateAnswer", connection);
                    command.Parameters.AddWithValue("@answerId", newPost.ID);
                    break;
                case Type t when t == typeof(Comment):
                    command = new SqlCommand("UpdateComment", connection);
                    command.Parameters.AddWithValue("@commentId", newPost.ID);
                    break;
                default:
                    return;
            }
            command.Parameters.AddWithValue("@content", newPost.Content);
            command.CommandType = CommandType.StoredProcedure;
            command.ExecuteNonQuery();
            connection.Close();
        }

        public object GetQuestionOfUser(int v)
        {
            throw new NotImplementedException();
        }

        public void AddPost(IPost post)
        {
            SqlConnection sqlConnection = new (sqlConnectionString);
            sqlConnection.Open();
            SqlCommand command = new (
                "INSER INTO Post (id, userId, content, datePosted, dateOfLastEdit, type, title, categoryId)" +
                "VALUES (@id, @userId, @content, @datePosted, @dateOfLastEdit, @type, @title, @categoryId)");
            command.Parameters.AddWithValue("@id", post.ID);
            command.Parameters.AddWithValue("@userId", post.UserID);
            command.Parameters.AddWithValue("@content", post.Content);
            command.Parameters.AddWithValue("@datePosted", post.DatePosted);
            command.Parameters.AddWithValue("@dateOfLastEdit", post.DateOfLastEdit);
            command.Parameters.AddWithValue("@type", post.GetType());
            command.Parameters.AddWithValue("@title", string.Empty);
            command.Parameters.AddWithValue("@categoryId", string.Empty);
            sqlConnection.Close();
        }

        public IPost GetPost(long postId)
        {
            throw new NotImplementedException();
        }
    }
}