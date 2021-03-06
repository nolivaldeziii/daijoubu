﻿using Daijoubu.AppControls;
using Daijoubu.AppLibrary;
using Daijoubu.AppModel;
using Daijoubu.Dependencies;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using static Daijoubu.AppLibrary.Categories;

namespace Daijoubu
{

    public partial class MainPage : ContentPage
    {
        ObservableCollection<lv_binding_hp_notifications> ListViewNotifications;
        Settings setting;
        DateTime LastGreeting { get; set; }
        public MainPage()
        {
            InitializeComponent();
            //this.Padding = -50;

            ApplicationInitialization();

            ListViewNotifications = new ObservableCollection<lv_binding_hp_notifications>(ListBuilder.HomePageNotifications());
            listview_homepage_notifications.HasUnevenRows = true;
            listview_homepage_notifications.ItemsSource = ListViewNotifications;

            listview_homepage_notifications.ItemTapped += (object sender, ItemTappedEventArgs e) => {
                try
                {
                    // don't do anything if we just de-selected the row
                    if (e.Item == null) return;
                    // do something with e.SelectedItem
                    var item = (e.Item as lv_binding_hp_notifications);
                    switch(item.TableName)
                    {
                        case "Table_UserKanaCardsN5":
                             Navigation.PushAsync(new AppPages.QuizPages.MultipleChoicePage(MultipleChoiceCategory.Hiragana));
                            break;
                        case "Table_UserKataKanaCardsN5":
                             Navigation.PushAsync(new AppPages.QuizPages.MultipleChoicePage(MultipleChoiceCategory.Katakana));
                            break;
                        case "Table_UserVocabCardsN5":
                             Navigation.PushAsync(new AppPages.QuizPages.MultipleChoicePage(MultipleChoiceCategory.Vocabulary));
                            break;
                        case "Table_UserVocabCardsN4":
                             Navigation.PushAsync(new AppPages.QuizPages.MultipleChoicePageN4(MultipleChoiceCategory.Vocabulary));
                            break;
                        default:
                            if (!setting.EnableN4)
                            {
                                 Navigation.PushAsync(new AppPages.QuizPage());
                            }
                            else
                            {
                                 Navigation.PushAsync(new AppPages.QuizPageN4());
                            }
                            break;
                    }

                    //var index = ListViewNotifications.IndexOf(item);
                    // ListViewNotifications.RemoveAt(index);

                    //if (ListViewNotifications.Count <= 0)
                    //{
                    //    ListViewNotifications.Add(lv_binding_hp_notifications.Empty(3.5));
                    //}
                    ((ListView)sender).SelectedItem = null; // de-select the row
                }
                catch { }
            };

            DependencyService.Get<ITextToSpeech>().Speak(" ");
            LastGreeting = DateTime.Now.Subtract(new TimeSpan(0,3,0));

            if (!Settings.Greeted && setting.SpeakWords)
            {
                Device.StartTimer(new TimeSpan(0, 0, 2), () =>
                  {
                      DependencyService.Get<ITextToSpeech>().Speak(Computer.ApplicationInitialGreeting());
                      return false;
                  });
                Settings.Greeted = true;
            }


        }

        protected override void OnAppearing()
        {
            ListViewNotifications = new ObservableCollection<lv_binding_hp_notifications>(ListBuilder.HomePageNotifications());
            listview_homepage_notifications.ItemsSource = ListViewNotifications;

            if (setting.SpeakWords & (DateTime.Now - LastGreeting).TotalMinutes > 3)
            {
                DependencyService.Get<ITextToSpeech>().Speak(Computer.RandomJapaneseProverbs());
                LastGreeting = DateTime.Now;
            }

            base.OnAppearing();
        }

        protected override bool OnBackButtonPressed()
        {
            ThisApp.MasterDetail.IsPresented = !ThisApp.MasterDetail.IsPresented;
            return true;
            //return base.OnBackButtonPressed();
        }

        void ApplicationInitialization()
        {

            //DB Initialization

            //DependencyService.Get<ISQLite>().DeleteUserDB();
            var japaneseDBO = DependencyService.Get<ISQLite>().GetJapaneseDBconnection();

            //save the japanese dictionary to local list
            japaneseDBO.CreateTable<tbl_kana>();
            japaneseDBO.CreateTable<tbl_vocabulary_N5>();

            japaneseDBO.CreateTable<tbl_vocabulary_N4>();
            japaneseDBO.CreateTable<tbl_grammar_N4>();

            japaneseDBO.CreateTable<tbl_lesson_hiragana>();
            JapaneseDatabase.Table_Lesson_Hiragana = japaneseDBO.Table<tbl_lesson_hiragana>().ToList();
            japaneseDBO.CreateTable<tbl_lesson_katakana>();
            JapaneseDatabase.Table_Lesson_Katakana = japaneseDBO.Table<tbl_lesson_katakana>().ToList();

            //N5 Tables declaration
            JapaneseDatabase.Table_Vocabulary_N5 = japaneseDBO.Table<tbl_vocabulary_N5>().ToList();
            JapaneseDatabase.Table_Kana = japaneseDBO.Table<tbl_kana>().ToList();

            //N4 Tables declaration
            JapaneseDatabase.Table_Vocabulary_N4 = japaneseDBO.Table<tbl_vocabulary_N4>().ToList();
            JapaneseDatabase.Table_Grammar_N4 = japaneseDBO.Table<tbl_grammar_N4>().ToList();


            var userDBO = DependencyService.Get<ISQLite>().GetUserDBconnection();
            //N5 user Tables declaration
            userDBO.CreateTable<tbl_us_cardknN5Dt>();
            userDBO.CreateTable<tbl_us_cardktknN5Dt>();
            userDBO.CreateTable<tbl_us_cardvbN5dt>();

            //N4 user Tables declaration
            userDBO.CreateTable<tbl_us_cardvbN4dt>();
            userDBO.CreateTable<tbl_us_cardgrN4dt>();

            //user settings declaration
            userDBO.CreateTable<tbl_user_settings>();

            userDBO.BeginTransaction();
            //N5 Tables initialization
            while (userDBO.Table<tbl_us_cardknN5Dt>().Count() < JapaneseDatabase.Table_Kana.Count)
            {
                userDBO.Execute("insert into tbl_us_cardknN5Dt values (null,0,0,0);");
            }
            while (userDBO.Table<tbl_us_cardktknN5Dt>().Count() < JapaneseDatabase.Table_Kana.Count)
            {
                userDBO.Execute("insert into tbl_us_cardktknN5Dt values (null,0,0,0);");
            }
            while (userDBO.Table<tbl_us_cardvbN5dt>().Count() < JapaneseDatabase.Table_Vocabulary_N5.Count)
            {
                userDBO.Execute("insert into tbl_us_cardvbN5dt values (null,0,0,0);");
            }

            //N4 Tables initialization
            while (userDBO.Table<tbl_us_cardvbN4dt>().Count() < JapaneseDatabase.Table_Vocabulary_N4.Count)
            {
                userDBO.Execute("insert into tbl_us_cardvbN4dt values (null,0,0,0);");
            }
            while (userDBO.Table<tbl_us_cardgrN4dt>().Count() < JapaneseDatabase.Table_Grammar_N4.Count)
            {
                userDBO.Execute("insert into tbl_us_cardgrN4dt values (null,0,0,0);");
            }
            userDBO.Commit();

            //N5 Tables
            UserDatabase.Table_UserKanaCardsN5 = userDBO.Table<tbl_us_cardknN5Dt>().ToList();
            UserDatabase.Table_UserKataKanaCardsN5 = userDBO.Table<tbl_us_cardktknN5Dt>().ToList();
            UserDatabase.Table_UserVocabCardsN5 = userDBO.Table<tbl_us_cardvbN5dt>().ToList();

            //N4 Tables
            UserDatabase.Table_UserVocabCardsN4 = userDBO.Table<tbl_us_cardvbN4dt>().ToList();
            UserDatabase.Table_UserGrammCardsN4 = userDBO.Table<tbl_us_cardgrN4dt>().ToList();

            //User Setting in db
            UserDatabase.Table_UserSettings = userDBO.Table<tbl_user_settings>().ToList();

            //only init settings if db is loaded
            setting = new Settings();

            //queing function

            //queing for N5
            UserDatabase.KanaCardQueue = Computer.CreateQueue(UserDatabase.Table_UserKanaCardsN5.ToList<AbstractCardTable>(), setting.MultipleChoice.QueueCount);
            UserDatabase.KataKanaCardQueue = Computer.CreateQueue(UserDatabase.Table_UserKataKanaCardsN5.ToList<AbstractCardTable>(), setting.MultipleChoice.QueueCount);
            UserDatabase.VocabularyCardQueue = Computer.CreateQueue(UserDatabase.Table_UserVocabCardsN5.ToList<AbstractCardTable>(), setting.MultipleChoice.QueueCount);

            //queing for N4
            UserDatabase.VocabularyCardN4Queue = Computer.CreateQueue(UserDatabase.Table_UserVocabCardsN4.ToList<AbstractCardTable>(), setting.MultipleChoice.QueueCount);
            UserDatabase.GrammarCardN4Queue = Computer.CreateQueue(UserDatabase.Table_UserGrammCardsN4.ToList<AbstractCardTable>(), setting.MultipleChoice.QueueCount);


            //save assesment
            ThisApp.Assessments = new Dictionary<string, CardAssesment>();
            ThisApp.Assessments.Add("JLPTN5KanaAssesment", Computer.totalcardproficiency(UserDatabase.Table_UserKanaCardsN5.ToList<AbstractCardTable>()));
            ThisApp.Assessments.Add("JLPTN5KatakanaAssesment", Computer.totalcardproficiency(UserDatabase.Table_UserKataKanaCardsN5.ToList<AbstractCardTable>()));
            ThisApp.Assessments.Add("JLPTN5VocabularyAssesment", Computer.totalcardproficiency(UserDatabase.Table_UserVocabCardsN5.ToList<AbstractCardTable>()));

            ThisApp.Assessments.Add("JLPTN4VocabularyAssesment", Computer.totalcardproficiency(UserDatabase.Table_UserVocabCardsN4.ToList<AbstractCardTable>()));
            ThisApp.Assessments.Add("JLPTN4GrammarAssesment", Computer.totalcardproficiency(UserDatabase.Table_UserGrammCardsN4.ToList<AbstractCardTable>()));


            ThisApp.TotalJLPTN5 = (ThisApp.Assessments["JLPTN5KanaAssesment"].TotalProficiency + ThisApp.Assessments["JLPTN5KatakanaAssesment"].TotalProficiency + ThisApp.Assessments["JLPTN5VocabularyAssesment"].TotalProficiency) / 3.0;

            ThisApp.TotalJLPTN4 = (ThisApp.Assessments["JLPTN4VocabularyAssesment"].TotalProficiency + ThisApp.Assessments["JLPTN4GrammarAssesment"].TotalProficiency ) / 2.0;
        }


    }
}
