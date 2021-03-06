﻿using Daijoubu.AppLibrary;
using Daijoubu.AppModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using static Daijoubu.AppLibrary.Categories;

namespace Daijoubu.AppPages.QuizPages
{

    public partial class MultipleChoicePageN4 : ContentPage
    {
        private string Answer;
        private bool IsCorrect;

        private static Color PageColorDefault = Settings.PageColorDefault;
        private static Color ButtonColorDefault = Settings.ButtonColorDefault;
        private static Color PageColorCorrect = Settings.PageColorCorrect;
        private static Color PageColorMistake = Settings.PageColorMistake;

        Settings Setting;
        MultipleChoiceQuestionFactoryN4 QuestionFactory;
        Random random;
        Card CurrentQuestion;
        MultipleChoiceCategory QuizCategory;

        private bool _english;
        public MultipleChoicePageN4(MultipleChoiceCategory category,bool en = false)
        {
            InitializeComponent();

            FontChanger();
            btn_choice1.MinimumHeightRequest = Device.GetNamedSize(NamedSize.Large, typeof(Button)) * Settings.FontSizeMultiplier;
            btn_choice2.MinimumHeightRequest = Device.GetNamedSize(NamedSize.Large, typeof(Button)) * Settings.FontSizeMultiplier;
            btn_choice3.MinimumHeightRequest = Device.GetNamedSize(NamedSize.Large, typeof(Button)) * Settings.FontSizeMultiplier;
            btn_choice4.MinimumHeightRequest = Device.GetNamedSize(NamedSize.Large, typeof(Button)) * Settings.FontSizeMultiplier;

            EnableInterfaces(false);
            random = new Random();
            QuizCategory = category;
            QuestionFactory = new MultipleChoiceQuestionFactoryN4();
            btn_choice1.Clicked += CheckAnswer;
            btn_choice2.Clicked += CheckAnswer;
            btn_choice3.Clicked += CheckAnswer;
            btn_choice4.Clicked += CheckAnswer;
            _english = en;
            Setting = new Settings();
            
            Device.StartTimer(Setting.MultipleChoice.AnswerFeedBackDelay, () =>
            {
                EnableInterfaces(false);
                GenerateQuestion(QuizCategory);
                EnableInterfaces(true);
                return false;
            });
        }


        private void CheckAnswer(object sender, EventArgs e)
        {
            CheckAnswer(((Button)(sender)).Text);
            if (!IsCorrect)
            {
                if (btn_choice1.Text == QuestionFactory.Answer)
                {
                    btn_choice1.BackgroundColor = PageColorCorrect;
                }
                else if (btn_choice2.Text == QuestionFactory.Answer)
                {
                    btn_choice2.BackgroundColor = PageColorCorrect;
                }
                else if (btn_choice3.Text == QuestionFactory.Answer)
                {
                    btn_choice3.BackgroundColor = PageColorCorrect;
                }
                else if (btn_choice4.Text == QuestionFactory.Answer)
                {
                    btn_choice4.BackgroundColor = PageColorCorrect;
                }
            }

            if (Setting.SpeakWords)
            {
                string ToSpeak = "";
                
                if (!JapaneseCharacter.ContainsAlphabet(QuestionFactory.Question))
                {
                    ToSpeak = QuestionFactory.Question;
                    if (QuizCategory == MultipleChoiceCategory.Meaning)
                    {
                        ToSpeak = ToSpeak.Replace("＿", QuestionFactory.Answer);
                    }
                    DependencyService.Get<Dependencies.ITextToSpeech>().Speak(ToSpeak);
                }else if (!JapaneseCharacter.ContainsAlphabet(QuestionFactory.Answer))
                {
                    ToSpeak = QuestionFactory.Answer;
                    DependencyService.Get<Dependencies.ITextToSpeech>().Speak(ToSpeak);
                }
            }
        }

        public void GenerateChoices(string[] choices)
        {
            System.Random rnd = new System.Random();
            var numbers = Enumerable.Range(0, 4).OrderBy(r => rnd.Next()).ToArray();

            btn_choice1.Text = choices[numbers[1]];
            btn_choice2.Text = choices[numbers[0]];
            btn_choice3.Text = choices[numbers[2]];
            btn_choice4.Text = choices[numbers[3]];
        }

        public bool GenerateQuestion(MultipleChoiceCategory category)
        {
            bool Threshold = false;
            MultipleChoiceQuestionFactoryN4.QuestionType nextnum;
            Queue<Card> TMPQueueHolder;

            //remove labels
            lbl_meaning.Text = "";
            //lbl_furigana.Text = "";
            lbl_meaning.IsVisible  = false;
            //lbl_furigana.IsVisible = false;

            if (category == MultipleChoiceCategory.Vocabulary)
            {
                if (_english)
                {
                    nextnum = ((MultipleChoiceQuestionFactoryN4.QuestionType)random.Next(7, 9));
                    TMPQueueHolder = UserDatabase.VocabularyCardN4Queue;
                }
                else
                {
                    nextnum = ((MultipleChoiceQuestionFactoryN4.QuestionType)random.Next(5, 7));
                    TMPQueueHolder = UserDatabase.VocabularyCardN4Queue;
                }
                
            }
            else if (category == MultipleChoiceCategory.Meaning)
            {
                nextnum = MultipleChoiceQuestionFactoryN4.QuestionType.Grammar;
                TMPQueueHolder = UserDatabase.GrammarCardN4Queue;
            }
            else
            {
                throw new Exception("MultipleChoicePageN4->GenerateQuestion->QuestionType");
            }

            this.BackgroundColor = PageColorDefault;

            if (!(TMPQueueHolder.Count > 0))
            {
                Threshold = true;
                TMPQueueHolder = ReplenishQueue();
            }

            //0 ,3 is kana
            //3 , 9 is vocabs
            CurrentQuestion = TMPQueueHolder.Dequeue();

            try
            {
                QuestionFactory.GenerateKanaQuestion(CurrentQuestion.Id + 5, CurrentQuestion.Id, nextnum);
            }
            catch
            {
                QuestionFactory.GenerateKanaQuestion(CurrentQuestion.Id, CurrentQuestion.Id, nextnum);
            }

            //generate meaning and furigana
            if (category == MultipleChoiceCategory.Meaning)
            {
                lbl_meaning.IsVisible = true;
                //lbl_furigana.IsVisible = true;
                lbl_meaning.Text = "T: " + JapaneseDatabase.Table_Grammar_N4[QuestionFactory.QuestionID - 1].sentence_en.Replace('_',' ');
                //lbl_furigana.Text = JapaneseDatabase.Table_Grammar_N4[QuestionFactory.QuestionID - 1].sentence_fu;
            }

                label_question.Text = QuestionFactory.Question;
            Answer = QuestionFactory.Answer;
            GenerateChoices(QuestionFactory.Choices);

            lbl_debug_ans.Text = QuestionFactory.Answer.ToString();
            lbl_debug_txt.Text = string.Format("[DEBUG] Question Id: {0}", CurrentQuestion.Id);
            lbl_percent.Text = string.Format("Learn ratio: {0:0.00}%", CurrentQuestion.LearnPercent);
            RefreshItemInfo();

            EnableInterfaces(true);
            SaveQueue(TMPQueueHolder);
            //prepare for next queue

            FontChanger();
            return Threshold;
        }

        private Queue<Card> ReplenishQueue()
        {

            if (QuizCategory == MultipleChoiceCategory.Vocabulary)
            {
                return UserDatabase.VocabularyCardQueue = Computer.CreateQueue(UserDatabase.Table_UserVocabCardsN4.ToList<AbstractCardTable>());
            }
            else if(QuizCategory == MultipleChoiceCategory.Meaning)
            {
                return UserDatabase.GrammarCardN4Queue = Computer.CreateQueue(UserDatabase.Table_UserGrammCardsN4.ToList<AbstractCardTable>());
            }
            else
            {
                throw new Exception("MultipleChoicePageN4->GenerateQuestion->replenish");
            }
        }

        private void SaveQueue(Queue<Card> tmpqueue)
        {
            if (QuizCategory == MultipleChoiceCategory.Vocabulary)
            {
                UserDatabase.VocabularyCardQueue = tmpqueue;
            }
            else if (QuizCategory == MultipleChoiceCategory.Meaning)
            {
                UserDatabase.GrammarCardN4Queue = tmpqueue;
            }
            else
            {
                throw new Exception("MultipleChoicePageN4->GenerateQuestion->savequeue");
            }
        }

        public void CheckAnswer(string user_ans)
        {
            EnableInterfaces(false);
            if (user_ans == Answer)
            {
                //correct answer
                this.BackgroundColor = PageColorCorrect;
                IsCorrect = true;

                CurrentQuestion.CorrectCount++;
                CurrentQuestion.LastView = DateTime.Now;
                //save to sql
                DatabaseManipulator.Update_User_KanaCardN4(QuizCategory, CurrentQuestion.CorrectCount, CurrentQuestion.Id, IsCorrect);
            }
            else
            {
                //wrong answer
                this.BackgroundColor = PageColorMistake;
                IsCorrect = false;
                CurrentQuestion.MistakeCount++;
                CurrentQuestion.LastView = DateTime.Now;

                //save to sql
                DatabaseManipulator.Update_User_KanaCardN4(QuizCategory, CurrentQuestion.MistakeCount, CurrentQuestion.Id, IsCorrect);

                if (Setting.HapticFeedback)
                {
                    DependencyService.Get<Dependencies.INotifications>().Vibrate();
                }
            }

            var cardIndex = CurrentQuestion.Id - 1;

            if (QuizCategory == MultipleChoiceCategory.Vocabulary) 
            {
                UserDatabase.Table_UserVocabCardsN4[cardIndex].CorrectCount = CurrentQuestion.CorrectCount;
                UserDatabase.Table_UserVocabCardsN4[cardIndex].MistakeCount = CurrentQuestion.MistakeCount;
                UserDatabase.Table_UserVocabCardsN4[cardIndex].LastView = CurrentQuestion.LastView.ToString();
            }else if(QuizCategory == MultipleChoiceCategory.Meaning)
            {
                UserDatabase.Table_UserGrammCardsN4[cardIndex].CorrectCount = CurrentQuestion.CorrectCount;
                UserDatabase.Table_UserGrammCardsN4[cardIndex].MistakeCount = CurrentQuestion.MistakeCount;
                UserDatabase.Table_UserGrammCardsN4[cardIndex].LastView = CurrentQuestion.LastView.ToString();
            }

                //Show timespan
            RefreshItemInfo();

            var delay = Setting.MultipleChoice.AnswerFeedBackDelay;
            if (QuizCategory == MultipleChoiceCategory.Meaning)
            {
                int time = QuestionFactory.Question.Length % 5;
                //add 3 secs to delay since this is a sentence
                delay = delay.Add(new TimeSpan(0,0,0, time, 0));
            }

                Device.StartTimer(delay, () =>
            {
                GenerateQuestion(QuizCategory);
                return false;
            });
        }

        private void RefreshItemInfo()
        {
            TimeSpan span = Computer.NextQueingSpan(CurrentQuestion.LastView, CurrentQuestion.CorrectCount, CurrentQuestion.MistakeCount);
            lbl_nextview.Text = Computer.NextQueingSpanToString(span);
            lbl_percent.Text = string.Format("Learn ratio: {0}%", CurrentQuestion.LearnPercent);
        }

        public void EnableInterfaces(bool value)
        {
            btn_choice1.IsEnabled = value;
            btn_choice2.IsEnabled = value;
            btn_choice3.IsEnabled = value;
            btn_choice4.IsEnabled = value;

            if (value == true)
            {
                btn_choice1.BackgroundColor =
                     btn_choice2.BackgroundColor =
                     btn_choice3.BackgroundColor =
                     btn_choice4.BackgroundColor = ButtonColorDefault;
            }
        }

        public void SetQuestionFontSize()
        {
            //label_question.FontSize =;
        }

        protected override bool OnBackButtonPressed()
        {
            if (QuizCategory == MultipleChoiceCategory.Vocabulary)
            {
                var JLPTN4VocabularyAssesment = Computer.totalcardproficiency(UserDatabase.Table_UserVocabCardsN4.ToList<AbstractCardTable>());
                Navigation.PushModalAsync(new ProfilePages.AssesmentPage(
                    JLPTN4VocabularyAssesment
                    , "JLPTN4VocabularyAssesment"
                    , "Vocabulary"
                    , "JLPT N4 Assessment",false)
                {
                    Padding = 0
                });
            }else if (QuizCategory == MultipleChoiceCategory.Meaning)
            {
                var JLPTN4GrammarAssesment = Computer.totalcardproficiency(UserDatabase.Table_UserVocabCardsN4.ToList<AbstractCardTable>());
                Navigation.PushModalAsync(new ProfilePages.AssesmentPage(
                    JLPTN4GrammarAssesment
                    , "JLPTN4GrammarAssesment"
                    , "Grammar"
                    , "JLPT N4 Assessment", false)
                {
                    Padding = 0
                });
            }else
            {
                Navigation.PushModalAsync(new ProfilePages.AssesmentPage(false));
            }
            return false;
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


            btn_choice1.FontSize = Computer.AnswerButtonFontSize(btn_choice1.Text.Length, Settings.FontSizeMultiplier);
            btn_choice2.FontSize = Computer.AnswerButtonFontSize(btn_choice2.Text.Length, Settings.FontSizeMultiplier);
            btn_choice3.FontSize = Computer.AnswerButtonFontSize(btn_choice3.Text.Length, Settings.FontSizeMultiplier);
            btn_choice4.FontSize = Computer.AnswerButtonFontSize(btn_choice4.Text.Length, Settings.FontSizeMultiplier);
        }
    }
}
