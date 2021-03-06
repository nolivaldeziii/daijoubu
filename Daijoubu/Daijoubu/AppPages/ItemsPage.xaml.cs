﻿using Daijoubu.AppLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Daijoubu.AppPages
{
    public partial class ItemsPage : ContentPage
    {
        private AppLibrary.Categories.Lessons Category;

        Settings Setting;
        int CurrentItem;
        int MaxItem;
        bool Toogle;

        public ItemsPage(AppLibrary.Categories.Lessons Category,int CurrentItem = 0)
        {
            InitializeComponent();
            this.Category = Category;
            this.Setting = new Settings();
            
            this.Toogle = false;

            switch (Category)
            {
                case AppLibrary.Categories.Lessons.Hiragana:
                    MaxItem = AppModel.JapaneseDatabase.Table_Kana.Count;
                    this.Title = "Hiragana Lessons";
                    break;
                case AppLibrary.Categories.Lessons.Katakana:
                    MaxItem = AppModel.JapaneseDatabase.Table_Kana.Count;
                    this.Title = "Katakana Lessons";
                    break;
                case AppLibrary.Categories.Lessons.VocabularyN5:
                    MaxItem = AppModel.JapaneseDatabase.Table_Vocabulary_N5.Count;
                    this.Title = "N5 Vocabulary Lessons";
                    break;
                case AppLibrary.Categories.Lessons.VocabularyN4:
                    MaxItem = AppModel.JapaneseDatabase.Table_Vocabulary_N4.Count;
                    this.Title = "N4 Vocabulary Lessons";
                    break;
                case AppLibrary.Categories.Lessons.SentencesN4:
                    MaxItem = AppModel.JapaneseDatabase.Table_Grammar_N4.Count;
                    this.Title = "N4 Sentences";
                    break;
                default:
                    MaxItem = 1;
                    break;
            }

            this.CurrentItem = CurrentItem < MaxItem ? CurrentItem : 0;

            EnableInterfaces(false);
            InitializeClicks();
            Device.StartTimer(Setting.MultipleChoice.AnswerFeedBackDelay, () =>
            {
                EnableInterfaces(false);

                //generate next item to review
                GenerateItem();

                EnableInterfaces(true);
                return false;
            });
        }

        void InitializeClicks()
        {
            btn_next.Clicked += (o, e) => {
                NextItem(true);
            };
            btn_prev.Clicked += (o, e) => {
                NextItem(false);
            };
            btn_listen.Clicked += (o, e) => {
                if (Setting.SpeakWords)
                {
                    string ToSpeak = AppLibrary.JapaneseCharacter.ContainsAlphabet(label_question.Text) == false ? label_question.Text : "";
                    DependencyService.Get<Dependencies.ITextToSpeech>().Speak(ToSpeak);
                }
                else
                {
                    //show voice is disabled
                }
            };
            btn_meaning.Clicked += (o, e) => {
                ToggleMeaning();
            };
        }


        public void GenerateItem()
        {
            //initialize item
            frame_info.IsVisible = false;
            label_question.Text = "";
            label_info.Text = "";
            Toogle = false;
            label_question.TextColor = Color.Default;
            switch (Category)
            {
                case AppLibrary.Categories.Lessons.Hiragana:
                    label_question.Text = AppModel.JapaneseDatabase.Table_Kana[CurrentItem].hiragana;
                    LessonProgress.Hiragana = CurrentItem;
                    break;
                case AppLibrary.Categories.Lessons.Katakana:
                    label_question.Text = AppModel.JapaneseDatabase.Table_Kana[CurrentItem].katakana;
                    LessonProgress.Katakana = CurrentItem;
                    break;
                case AppLibrary.Categories.Lessons.VocabularyN5:
                    label_question.Text = AppModel.JapaneseDatabase.Table_Vocabulary_N5[CurrentItem].kanji;
                    LessonProgress.VocabularyN5 = CurrentItem;
                    break;
                case AppLibrary.Categories.Lessons.VocabularyN4:
                    label_question.Text = AppModel.JapaneseDatabase.Table_Vocabulary_N4[CurrentItem].kanji;
                    LessonProgress.VocabularyN4 = CurrentItem;
                    break;
                case AppLibrary.Categories.Lessons.SentencesN4:
                    label_question.Text = AppModel.JapaneseDatabase.Table_Grammar_N4[CurrentItem].sentence_jp.Replace('_', ' ');
                    LessonProgress.GrammarN4 = CurrentItem;
                    break;
                default:
                    break;
            }

            EnableInterfaces(true);
            //prepare for next queue
            FontChanger();
        }

        public void ToggleMeaning()
        {
            Toogle = !Toogle;
            label_question.TextColor = Toogle ? Color.Green : Color.Default;
            switch (Category)
            {
                case AppLibrary.Categories.Lessons.Hiragana:
                    label_question.Text = Toogle ? AppModel.JapaneseDatabase.Table_Kana[CurrentItem].romaji.ToUpper()
                        : AppModel.JapaneseDatabase.Table_Kana[CurrentItem].hiragana;

                    frame_info.IsVisible = Toogle && AppModel.JapaneseDatabase.Table_Lesson_Hiragana[CurrentItem].example != "";

                    label_info.Text = string.Format("Sound: \n{0} \n\n Mnemonic: \n{1} \n\n Example: \n{2}"
                        , AppModel.JapaneseDatabase.Table_Lesson_Hiragana[CurrentItem].sound
                        ,AppModel.JapaneseDatabase.Table_Lesson_Hiragana[CurrentItem].mnemonic
                        ,AppModel.JapaneseDatabase.Table_Lesson_Hiragana[CurrentItem].example);

                    break;
                case AppLibrary.Categories.Lessons.Katakana:
                    label_question.Text = Toogle ? AppModel.JapaneseDatabase.Table_Kana[CurrentItem].romaji.ToUpper()
                        : AppModel.JapaneseDatabase.Table_Kana[CurrentItem].katakana;

                    frame_info.IsVisible = Toogle && AppModel.JapaneseDatabase.Table_Lesson_Katakana[CurrentItem].example != "";
                    label_info.Text = string.Format("Sound: {0} \n\n Mnemonic: {1} \n\n Example: {2}"
                        , AppModel.JapaneseDatabase.Table_Lesson_Katakana[CurrentItem].sound
                        , AppModel.JapaneseDatabase.Table_Lesson_Katakana[CurrentItem].mnemonic
                        , AppModel.JapaneseDatabase.Table_Lesson_Katakana[CurrentItem].example);
                    break;
                case AppLibrary.Categories.Lessons.VocabularyN5:
                    label_question.Text = Toogle ? AppModel.JapaneseDatabase.Table_Vocabulary_N5[CurrentItem].furigana
                        : AppModel.JapaneseDatabase.Table_Vocabulary_N5[CurrentItem].kanji;
                    frame_info.IsVisible = Toogle;
                    label_info.Text = AppModel.JapaneseDatabase.Table_Vocabulary_N5[CurrentItem].meaning.Replace('_', ' ');
                    break;
                case AppLibrary.Categories.Lessons.VocabularyN4:
                    label_question.Text = Toogle ? AppModel.JapaneseDatabase.Table_Vocabulary_N4[CurrentItem].furigana
                        : AppModel.JapaneseDatabase.Table_Vocabulary_N4[CurrentItem].kanji;
                    frame_info.IsVisible = Toogle;
                    label_info.Text = AppModel.JapaneseDatabase.Table_Vocabulary_N4[CurrentItem].meaning.Replace('_', ' ');
                    break;
                case AppLibrary.Categories.Lessons.SentencesN4:
                    label_question.Text = Toogle ? AppModel.JapaneseDatabase.Table_Grammar_N4[CurrentItem].sentence_en.Replace('_', ' ') : AppModel.JapaneseDatabase.Table_Grammar_N4[CurrentItem].sentence_jp.Replace('_', ' ');
                    //frame_info.IsVisible = Toogle;
                    //label_info.Text = AppModel.JapaneseDatabase.Table_Grammar_N4[CurrentItem].sentence_fu;
                    break;
                default:
                    break;
            }
            FontChanger();
        }

        public void NextItem(bool next)
        {
            EnableInterfaces(false);

            CurrentItem = next ? CurrentItem + 1 : CurrentItem - 1;
  
            

            Device.StartTimer(Setting.MultipleChoice.AnswerFeedBackDelay, () =>
            {
                EnableInterfaces(false);

                //generate next item to review
                GenerateItem();

                EnableInterfaces(true);
                return false;
            });
        }

        public void EnableInterfaces(bool value)
        {
            btn_prev.IsEnabled = value && CurrentItem > 0;
            btn_next.IsEnabled = value && CurrentItem < MaxItem;
            btn_meaning.IsEnabled = value;
            btn_listen.IsEnabled = value;

        }

        void FontChanger()
        {
            if (label_question.Text.Length > 20)
            {
                label_question.FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label));
            }
            else if (label_question.Text.Length > 10)
            {
                label_question.FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)) * 1.5;
            }
            else if (label_question.Text.Length > 3)
            {
                label_question.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)) * Settings.FontSizeMultiplier;
            }
            else
            {
                label_question.FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)) * Settings.FontSizeMultiplier;
            }


            //btn_choice1.FontSize = Computer.AnswerButtonFontSize(btn_choice1.Text.Length, Settings.FontSizeMultiplier);
            //btn_choice2.FontSize = Computer.AnswerButtonFontSize(btn_choice2.Text.Length, Settings.FontSizeMultiplier);
            //btn_choice3.FontSize = Computer.AnswerButtonFontSize(btn_choice3.Text.Length, Settings.FontSizeMultiplier);
            //btn_choice4.FontSize = Computer.AnswerButtonFontSize(btn_choice4.Text.Length, Settings.FontSizeMultiplier);
        }
    }
}
