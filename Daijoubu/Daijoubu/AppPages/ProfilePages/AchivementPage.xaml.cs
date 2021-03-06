﻿using Daijoubu.AppLibrary;
using Daijoubu.AppModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using static Daijoubu.AppLibrary.Categories;

namespace Daijoubu.AppPages.ProfilePages
{
    public partial class AchivementPage : ContentPage
    {
        ObservableCollection<UserAchivements> UserCard_Kana;
        List<AbstractCardTable> GeneralTable;
        GeneralType _WordType;

        public AchivementPage(GeneralType WordType, bool n5 = true)
        {
            InitializeComponent();

            listview_achivements.ItemTapped += (s, e) =>
            {
                // don't do anything if we just de-selected the row
                if (e.Item == null) return;
                // do something with e.SelectedItem

                ((ListView)s).SelectedItem = null; // de-select the row
            };
            _WordType = WordType;
            if (n5)
            {
                
                if (_WordType == GeneralType.Hiragana)
                {
                    GeneralTable = UserDatabase.Table_UserKanaCardsN5.ToList<AbstractCardTable>();
                }
                else if (_WordType == GeneralType.Katakana)
                {
                    GeneralTable = UserDatabase.Table_UserKataKanaCardsN5.ToList<AbstractCardTable>();
                }
                else if (_WordType == GeneralType.Vocabulary)
                {
                    GeneralTable = UserDatabase.Table_UserVocabCardsN5.ToList<AbstractCardTable>();
                }
                else
                {
                    throw new Exception("achivement page.cs");
                }
            }
            else
            {
                //n4
                if (_WordType == GeneralType.Vocabulary)
                {
                    GeneralTable = UserDatabase.Table_UserVocabCardsN4.ToList<AbstractCardTable>();
                }
                else if (_WordType == GeneralType.Grammar)
                {
                    GeneralTable = UserDatabase.Table_UserGrammCardsN4.ToList<AbstractCardTable>();
                }
                else
                {
                    throw new Exception("achivement page.cs");
                }
            }

            UserCard_Kana = new ObservableCollection<UserAchivements>();

            //bool checkCount = (GeneralTable.Count == JapaneseDatabase.Table_Kana.Count);

            for (int i = 0; i < GeneralTable.Count; i++)
            {
                DateTime _LastView;
                try
                {
                    _LastView = Convert.ToDateTime(GeneralTable[i].LastView);
                }
                catch { _LastView = DateTime.Now; }
                if (GeneralTable[i].CorrectCount != 0 || GeneralTable[i].MistakeCount != 0)
                {
                    string _item;
                    int id = -1;
                    string _subtitle;
                    string _reading;
                    if (n5)
                    {
                        if (_WordType == GeneralType.Hiragana)
                        {
                            _item = JapaneseDatabase.Table_Kana[i].hiragana;
                            id = JapaneseDatabase.Table_Kana[i].Id;
                            _reading = string.Format("\"{0}\"", JapaneseDatabase.Table_Kana[i].romaji.ToUpper());
                            _subtitle = "";
                        }
                        else if (_WordType == GeneralType.Katakana)
                        {
                            _item = JapaneseDatabase.Table_Kana[i].katakana;
                            id = JapaneseDatabase.Table_Kana[i].Id;
                            _reading = string.Format("\"{0}\"", JapaneseDatabase.Table_Kana[i].romaji.ToUpper());
                            _subtitle = "";
                        }
                        else if (_WordType == GeneralType.Vocabulary)
                        {
                            _item = JapaneseDatabase.Table_Vocabulary_N5[i].kanji;
                            id = JapaneseDatabase.Table_Vocabulary_N5[i].Id;
                            _reading = JapaneseDatabase.Table_Vocabulary_N5[i].furigana;
                            _subtitle = JapaneseDatabase.Table_Vocabulary_N5[i].meaning;
                        }
                        else
                        {
                            throw new Exception("achivement page.cs");
                        }
                    }else
                    {
                        if (_WordType == GeneralType.Vocabulary)
                        {
                            _item = JapaneseDatabase.Table_Vocabulary_N4[i].kanji;
                            id = JapaneseDatabase.Table_Vocabulary_N4[i].Id;
                            _reading = JapaneseDatabase.Table_Vocabulary_N4[i].furigana;
                            _subtitle = JapaneseDatabase.Table_Vocabulary_N4[i].meaning;
                        }
                        else if (_WordType == GeneralType.Grammar)
                        {
                            _item = JapaneseDatabase.Table_Grammar_N4[i].sentence_jp;
                            id = JapaneseDatabase.Table_Grammar_N4[i].Id;
                            _reading = JapaneseDatabase.Table_Grammar_N4[i].sentence_en;
                            _subtitle = "";
                        }
                        else
                        {
                            throw new Exception("achivement page.cs");
                        }
                    }

                    UserCard_Kana.Add(new UserAchivements
                    {
                        ItemID = id,
                        Item = _item,
                        SubTitle = _subtitle,
                        Reading = _reading,
                        Correct = GeneralTable[i].CorrectCount,
                        Mistake = GeneralTable[i].MistakeCount,
                        LastView = Computer.SemanticTimespan(Convert.ToDateTime(GeneralTable[i].LastView)),
                        Percent = Computer.ForPercentage(GeneralTable[i].CorrectCount, GeneralTable[i].MistakeCount),
                        NextQueue = Computer.NextQueingSpanToString(Computer.NextQueingSpan(_LastView, GeneralTable[i].CorrectCount, GeneralTable[i].MistakeCount))
                    });
                }


            }
            if (UserCard_Kana.Count == 0)
            {
                UserCard_Kana.Add(new UserAchivements
                {
                    ItemID = 0,
                    Item = "There are no achievements... yet!",
                    SubTitle = "Why don't you take the quiz",
                    Reading = "It's never too late",
                    Correct = 0,
                    Mistake = 0,
                    LastView = null,
                    Percent = 0,
                    NextQueue = null
                });
            }

            listview_achivements.HasUnevenRows = true;
            listview_achivements.ItemsSource = UserCard_Kana.Reverse();
        }

        public class UserAchivements
        {
            public int ItemID { get; set; }
            public string Item { get; set; }
            public string SubTitle { get; set; }
            public string Reading { get; set; }
            public int Correct { get; set; }
            public int Mistake { get; set; }
            public string LastView { get; set; }
            public string NextQueue { get; set; }
            public double Percent { get; set; }

        }
    }


}
