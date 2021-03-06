﻿using Daijoubu.AppModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Daijoubu.AppLibrary
{
    public static class Computer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="High">Max Card to review</param>
        /// <returns></returns>
        public static Queue<Card> CreateQueue(List<AbstractCardTable> CardTable, int High = 5)
        {
            var queue = new Queue<Card>();

            //List<AbstractCardTable> CardTable = TheCardTable.OrderBy(i => (Computer.));

            for (int i = 0; High != 0; i++)
            {
                //UserDatabase.KanaCardQueueHigh = i;
                DateTime LastView;
                try
                { LastView = Convert.ToDateTime(CardTable[i].LastView); }
                catch
                { LastView = DateTime.Now; }
                //compute date vs correct items and mistakes 
                if (Computer.ForQueuing(LastView, CardTable[i].CorrectCount, CardTable[i].MistakeCount))
                {
                    Card NewCard = new Card();
                    NewCard.Id = CardTable[i].Id;
                    NewCard.MistakeCount = CardTable[i].MistakeCount;
                    NewCard.CorrectCount = CardTable[i].CorrectCount;
                    NewCard.LastView = LastView;

                    queue.Enqueue(NewCard);
                    High--;
                }
            }
            return queue;
        }
        public static double ForPercentage(int numerator, int denominator)
        {
            double percent = ((double)numerator) / ((double)numerator + (double)denominator);
            if (double.IsNaN(percent))
            {
                percent = new double();
                percent = 0;
            }
            return percent*100.0;
        }
        public static DateTime NextQueuing2(DateTime LastView, int correct, int mistake)
        {
            DateTime TimeDiff;
            
            mistake = mistake <= 0 ? 1 : mistake;
            correct = correct <= 0 ? 1 : correct;


            double number = (13.3333 * Math.Pow(correct, 2) * (correct + (3 * mistake)) / (correct + mistake));
            double fixed_multiplier = .4;////////////////////////////////////////////////////////////////////////////
            try
            {
                double _minutes = number * fixed_multiplier;
                TimeDiff = LastView.AddMinutes(_minutes);
            }
            catch
            {
                TimeDiff = DateTime.Now;
            }
            return TimeDiff;
        }
        public static DateTime NextQueuing(DateTime LastView, int correct,int mistake)
        {
            DateTime TimeDiff;
            
            double _Percent = ForPercentage( correct,  mistake);
            mistake = mistake <= 0 ? 1 : mistake;
            correct = correct <= 0 ? 1 : correct;
            _Percent = _Percent <= 0 ? 1 : _Percent;

            //double _multiplier = ((double)correct + (mistake * 3.0)) / (correct * 1.5);
            //double fixed_multiplier = .4;////////////////////////////////////////////////////////////////////////////
            try
            {
                //double _minutes = _Percent * fixed_multiplier * _multiplier;
                double number = ( SRSsettings.PercentageMultiplier * (5*correct + (mistake)) ) / (correct + 10*mistake);
 
                number *= SRSsettings.Multiplier;
                number *= _Percent/100.0;
                //_minutes *= SRSsettings.Multiplier;
                TimeDiff = LastView.AddMinutes(number);
            }
            catch
            {
                TimeDiff = DateTime.Now;
            }
            return TimeDiff;
        }
        public static TimeSpan NextQueingSpan(DateTime LastView, int correct, int mistake)
        {
            var nextq = NextQueuing(LastView, correct, mistake);
            TimeSpan span = (nextq - DateTime.Now);
            return span;
        }

        public static string NextQueingSpanToString(TimeSpan span)
        {
            string result = "";
            if (Math.Abs(span.Days) > 0)
            {
                result = String.Format("{0}d, {1}hr, {2}m, {3}s",
                    span.Days, span.Hours, span.Minutes, span.Seconds);
            }else if (Math.Abs(span.Hours) > 0)
            {
                result = String.Format("{0}h, {1}m, {2}s",
                    span.Hours, span.Minutes, span.Seconds);
            }
            else if (Math.Abs(span.Minutes) > 0)
            {
                result = String.Format("{0}m, {1}s",
                    span.Minutes, span.Seconds);
            }else
            {
                result = String.Format("{0}s",
                    span.Seconds);
            }
            return result;
        }

        public static bool ForQueuing(DateTime LastView, int correct, int mistake)
        {
            var TimeDiff = NextQueingSpan( LastView, correct, mistake);

            var timelapse = TimeDiff.TotalSeconds <= 0;
            var percentdiff = (ForPercentage(correct, mistake) < SRSsettings.PercentageQuota);///////////////////////////////////////////////////////////////////////////
            return (percentdiff || timelapse);
        }

        public static CardAssesment totalcardproficiency(List<AbstractCardTable> CardTable)
        {
            CardAssesment assesment = new CardAssesment();
            assesment.TotalItems = CardTable.Count;
            double total_percent = 0;
            foreach(var item in CardTable)
            {
                var percent = ForPercentage(item.CorrectCount, item.MistakeCount);

                if(item.CorrectCount>0 || item.MistakeCount > 0)
                {
                    assesment.TotalReviewed++;
                    if (percent < 70)
                    {
                        //failed
                        assesment.TotalFailed++;
                    }
                    else
                    {
                        assesment.TotalPassed++;
                    }
                }              
                total_percent += percent;
            }
            assesment.TotalProficiency = total_percent / assesment.TotalItems;

            return assesment;
        }

        public static string SemanticTimespan(TimeSpan tspan)
        {
            var ts = tspan.Duration();
            string due = tspan.TotalSeconds < 0 ? "Overdue" : "To go";
            if (ts.Days % 30 > 0)
            {
                var x = ts.Days % 30;
                return string.Format("{0} month{1} {2}", x, x > 1 ? "s" : "", due);
            }
            else if (ts.Days % 7 > 0)
            {
                var x = ts.Days % 7;
                return string.Format("{0} week{1} {2}", x, x > 1 ? "s" : "", due);
            }
            else if (ts.Days > 0)
            {
                var x = ts.Days;
                return string.Format("{0} day{1} {2}", x, x > 1 ? "s" : "", due);
            }
            else if (ts.Hours > 0)
            {
                var x = ts.Hours;
                return string.Format("{0} hour{1} {2}", x, x > 1 ? "s" : "", due);
            }
            else if (ts.Hours > 0)
            {
                var x = ts.Hours;
                return string.Format("{0} hour{1} {2}", x, x > 1 ? "s" : "", due);
            }
            else if (ts.Minutes > 0)
            {
                var x = ts.Minutes;
                return string.Format("{0} minute{1} {2}", x, x > 1 ? "s" : "", due);
            }
            else
            {
                var x = ts.Seconds;
                return string.Format("{0} Second{1} {2}", x, x > 1 ? "s" : "", due);
            }
        }

        public static string SemanticTimespan(DateTime dt)
        {
            TimeSpan ts = (DateTime.Now - dt);
            if(ts.Days % 30 > 0)
            {
                var x = ts.Days % 30;
                return string.Format("{0} month{1} ago", x, x > 1 ? "s" : "" );
            }else if (ts.Days % 7 > 0)
            {
                var x = ts.Days % 7;
                return string.Format("{0} week{1} ago", x, x > 1 ? "s" : "");
            }
            else if (ts.Days > 0)
            {
                var x = ts.Days;
                return string.Format("{0} day{1} ago", x, x > 1 ? "s" : "");
            }
            else if (ts.Hours > 0)
            {
                var x = ts.Hours;
                return string.Format("{0} hour{1} ago", x, x > 1 ? "s" : "");
            }
            else if (ts.Hours > 0)
            {
                var x = ts.Hours;
                return string.Format("{0} hour{1} ago", x, x > 1 ? "s" : "");
            }
            else if (ts.Minutes > 0)
            {
                var x = ts.Minutes;
                return string.Format("{0} minute{1} ago", x, x > 1 ? "s" : "");
            }
            else
            {
                var x = ts.Seconds;
                return string.Format("{0} Second{1} ago", x, x > 1 ? "s" : "");
            }
        }

        public static double AnswerButtonFontSize(int strlen,double multiplier)
        {
            var button_size_large = Device.GetNamedSize(NamedSize.Small, typeof(Button)) * multiplier;
            var button_size_med = Device.GetNamedSize(NamedSize.Small, typeof(Button)) * (multiplier / 2);
            var button_size_small = Device.GetNamedSize(NamedSize.Large, typeof(Button));
            return strlen > 3 ? button_size_small : strlen > 2 ? button_size_med : button_size_large;
        }

        public static double LabelFontSize(int strlen, double multiplier)
        {
            var size_large = Device.GetNamedSize(NamedSize.Small, typeof(Button)) * multiplier;
            //var size_med = Device.GetNamedSize(NamedSize.Small, typeof(Button)) * (multiplier / 2);
            var size_small = Device.GetNamedSize(NamedSize.Large, typeof(Button));
            return strlen > 4 ? size_small : size_large;
        }

        public static string RandomJapaneseProverbs()
        {
            string greeting = "";
            List<string> possible_greetings = new List<string>();
            possible_greetings.Add("継続は力なり");
            possible_greetings.Add("早起きは三文の徳");

            possible_greetings.Add("本末転倒");
            possible_greetings.Add("早い者勝ち");
            possible_greetings.Add("愚公山を移す");
            possible_greetings.Add("井戸の中の独言も三年たてば知れる");
            possible_greetings.Add("亀の甲より年の功");

            possible_greetings.Add("進み続けてさえいれば、遅くとも関係ない。");
            possible_greetings.Add("この世界の内に望む変化に、あなた自身が成ってみせなさい。");
            possible_greetings.Add("きっと成功してみせる、と決心する事が何よりも重要だ。");

            int rand = new Random().Next(0, possible_greetings.Count);
            greeting += possible_greetings[rand];

            return greeting;
        }

        public static string ApplicationInitialGreeting()
        {
            string greeting = "";
            var hours = DateTime.Now.Hour;
            if (hours > 16)
            {
                greeting += "こんばんは、"; //konbanwa
            }
            else if (hours > 11)
            {
                greeting += "こんにちは、"; //konichiwa
            }
            else
            {
                greeting += "お早うございます、"; // ohayou
            }

            List<string> possible_greetings = new List<string>();
            possible_greetings.Add("早起きは三文の徳");
            possible_greetings.Add("継続は力なり");
            possible_greetings.Add("本末転倒");
            possible_greetings.Add("早い者勝ち");
            possible_greetings.Add("愚公山を移す");
            possible_greetings.Add("井戸の中の独言も三年たてば知れる");
            possible_greetings.Add("亀の甲より年の功");
            possible_greetings.Add("べんきょうしましょう！"); //benkyou shimashou
            possible_greetings.Add("お元気ですか？"); //ogenki desu ka
            possible_greetings.Add("がんばってね！"); //ganbatte ne

            int rand = new Random().Next(0, possible_greetings.Count);
            greeting += possible_greetings[rand];

            return greeting;
        }

        public static bool IsHiragana(char x)
        {
            return (x >= 0x3040 && x <= 0x309F);
        }

        public static string MakeConsistentHiraganaEnding(string h,string a, string q)
        {
            char ans = a[a.Length - 1];

            if (a.Length > 1 && IsHiragana(ans) && IsHiragana(q[q.Length - 1]) )
            {
                if (h.Length > 1 && IsHiragana(h[h.Length - 1]))
                {
                    string r = "";

                    for (int i = 0; i < h.Length; i++)
                    {
                        if (i != h.Length - 1)
                        {
                            r += h[i];
                        }
                        else
                        {
                            r += ans;
                        }
                    }
                    return r;
                }
            }
            return h;
        }
    }
}
