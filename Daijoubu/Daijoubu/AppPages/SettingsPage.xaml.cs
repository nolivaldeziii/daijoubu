﻿using Daijoubu.Dependencies;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Daijoubu.AppPages
{
    public partial class SettingsPage : ContentPage
    {
        public Settings setting;
        double progress;
        public SettingsPage()
        {
            InitializeComponent();
            
            setting = new Settings();
            InitializeValues();

            btn_save.Clicked += async (o, e) => {
                var confirm_save = await DisplayAlert("Confirm", "Are you sure you want to save?", "Yes", "No");
                if (confirm_save)
                {
                    progress_saving.IsVisible = true;
                    btn_Discard.IsEnabled = false;
                    btn_save.IsEnabled = false;

                    DependencyService.Get<INotifications>().ToastDependency("Saving Please wait...!");

                    Device.StartTimer(new TimeSpan(0, 0, 0, 0, 250), () =>
                    {
                        Task.Run(() =>
                        {

                            progress_saving.ProgressTo(progress, 2000, Easing.Linear);
                        });
                        return progress_saving.IsVisible;
                    });

                    Saving();

                    await progress_saving.ProgressTo(1, 1000, Easing.Linear);

                    Thread.Sleep(2000);
                    Device.StartTimer(new TimeSpan(0, 0, 0, 3, 250), () =>
                    {
                        progress_saving.IsVisible = false;
                        //lbl_prog_percent.IsVisible = false;
                        DisplayAlert("Saved!", "Some changes will take efect after app restart", "OK");
                        return false;
                    });
                }

            };
        }

        async void Saving()
        {
            await Task.Run(() =>
            {
                try
                {
                    progress = 0;
                    
                    setting.HapticFeedback = switch_HapticFeedback.IsToggled;
                    setting.SpeakWords = switch_TTS.IsToggled;

                    setting.MultipleChoice.AnswerFeedBackDelay = new TimeSpan(0,0,0,0, Convert.ToInt32(slider_AnswerFeedBackDelay.Value));
                    setting.ForceEnableN4 = switch_enablen4.IsToggled;


                    setting.SaveCurrentConfig(ref progress);
                }
                finally
                {
                    
                }
                return false;
            });           
        }

        void InitializeValues()
        {
            switch_HapticFeedback.IsToggled = setting.HapticFeedback;
            switch_TTS.IsToggled = setting.SpeakWords;
            slider_AnswerFeedBackDelay.Value = setting.MultipleChoice.AnswerFeedBackDelay.TotalSeconds * 1000;
            switch_enablen4.IsToggled = setting.ForceEnableN4;
        }
    }
}
