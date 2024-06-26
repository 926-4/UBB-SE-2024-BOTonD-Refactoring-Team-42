﻿using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using UBB_SE_2024_Team_42.Domain.Post.Interfaces;
using UBB_SE_2024_Team_42.Service;

namespace UBB_SE_2024_Team_42.GUI
{
    /// <summary>
    /// Interaction logic for MiniProfile.xaml
    /// </summary>
    public partial class MiniProfile : Window
    {
        // public ObservableCollection<Badge> Badges { get; set; }
        public ObservableCollection<IQuestion> Questions { get; set; }
        public ObservableCollection<IPost> Answers { get; set; }

        private readonly IService iservice;

        public MiniProfile(IService service)
        {
            InitializeComponent();
            this.iservice = service;
            // sorry for what's coming
            // we're getting the profile of the user with id 3
            // ?? idk what is this shit here so i'm sure i won't disturb it's natural habitat - Boti
            Answers = new ObservableCollection<IPost>(service.GetCommentsOfUser(3));
            Questions = new ObservableCollection<IQuestion>(service.GetQuestionsOfUser(1));
            DataContext = this; // Set DataContext to enable data binding
        }

        private void BadgeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void QuestionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void AnswerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}