﻿using System.Windows;
using UBB_SE_2024_Team_42.Domain.Post.Interfaces;
using UBB_SE_2024_Team_42.Domain.Posts;
using UBB_SE_2024_Team_42.Service;

namespace UBB_SE_2024_Team_42.GUI
{
    /// <summary>
    /// Interaction logic for AddComment.xaml
    /// </summary>
    public partial class EditPost : Window
    {
        private readonly IPost post;
        private readonly IService iservice;
        public EditPost(IService service, IPost post)
        {
            this.iservice = service;
            this.post = post;
            InitializeComponent();
        }

        private void Submit_Button_Click(object sender, RoutedEventArgs e)
        {
            string text = Coolest_TextBox_Ever.Text;
            post.Content = text;
            post.DateOfLastEdit = DateTime.Now;
            // This is not fine leaving this here until someone fixes
            TextPost newPost = new (post.UserID, text);
            iservice.UpdatePost(post, newPost);
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
