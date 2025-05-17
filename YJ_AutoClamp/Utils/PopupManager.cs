using System;
using System.Collections.Generic;
using System.Windows;
using YJ_AutoClamp.ViewModels;

namespace YJ_AutoClamp.Utils
{
    public static class PopupManager
    {
        public static void ShowPopupView<TEnum>(Dictionary<TEnum, Func<(Window, Child_ViewModel)>> popupFactories, TEnum popup) where TEnum : Enum
        {
            Window popupWindow = null;
            Child_ViewModel popupViewModel = null;

            try
            {
                if (popupFactories.TryGetValue(popup, out var factory))
                {
                    (popupWindow, popupViewModel) = factory();
                    popupWindow.DataContext = popupViewModel;
                    popupWindow.Owner = Application.Current.MainWindow;

                    // 팝업 창 표시
                    popupWindow.ShowDialog();
                }
            }
            catch (Exception)
            {
                popupViewModel?.Dispose();
                popupWindow?.Close();
                // Todo : Make Log
                //Global.ExceptionLog.ErrorFormat($"{System.Reflection.MethodBase.GetCurrentMethod().Name} - {e}");
            }
            finally
            {
                // DataContext 해제
                if (popupWindow != null)
                {
                    popupWindow.DataContext = null; // 바인딩 해제
                }

                // Window 닫기
                popupWindow?.Close();

                // ViewModel Dispose
                popupViewModel?.Dispose();

                // 참조 해제
                popupWindow = null;
                popupViewModel = null;

            }
        }
    }
}
