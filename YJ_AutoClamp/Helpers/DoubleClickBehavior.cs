using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace YJ_AutoClamp.Helpers
{
    public static class DoubleClickBehavior
    {
        // Command Attached Property
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached(
                "Command",
                typeof(ICommand),
                typeof(DoubleClickBehavior),
                new PropertyMetadata(null, OnCommandChanged));

        // CommandParameter Attached Property
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "CommandParameter",
                typeof(object),
                typeof(DoubleClickBehavior),
                new PropertyMetadata(null));

        public static void SetCommand(UIElement element, ICommand value)
        {
            element.SetValue(CommandProperty, value);
        }

        public static ICommand GetCommand(UIElement element)
        {
            return (ICommand)element.GetValue(CommandProperty);
        }

        public static void SetCommandParameter(UIElement element, object value)
        {
            element.SetValue(CommandParameterProperty, value);
        }

        public static object GetCommandParameter(UIElement element)
        {
            return element.GetValue(CommandParameterProperty);
        }

        // Command 변경 시 이벤트 핸들러 추가/제거
        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                // 기존 이벤트 핸들러 제거
                element.RemoveHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnMouseLeftButtonDown));

                // 새로운 Command가 설정된 경우에만 이벤트 핸들러 추가
                if (e.NewValue is ICommand)
                {
                    element.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnMouseLeftButtonDown), true);
                }
            }
        }

        // 더블 클릭 감지 로직
        private static DateTime _lastClickTime;
        private static UIElement _lastClickedElement;

        private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is UIElement element)
            {
                var currentTime = DateTime.Now;
                int doubleClickTime = GetDoubleClickTime(); // Win32 API 호출

                // 동일한 UI 요소에서 클릭 간격이 더블 클릭 시간 이내인지 확인
                if (_lastClickedElement == element && (currentTime - _lastClickTime).TotalMilliseconds <= doubleClickTime)
                {
                    // 더블 클릭으로 간주
                    var command = GetCommand(element);
                    var parameter = GetCommandParameter(element);

                    if (command != null && command.CanExecute(parameter))
                    {
                        ExecuteCommandAsync(command, parameter);
                    }
                }

                // 마지막 클릭 시간과 클릭된 요소 업데이트
                _lastClickTime = currentTime;
                _lastClickedElement = element;
            }
        }

        // 비동기 Command 실행
        private static async void ExecuteCommandAsync(ICommand command, object parameter)
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                command.Execute(parameter);
            });
        }

        // Win32 API를 사용하여 더블 클릭 시간 가져오기
        [DllImport("user32.dll")]
        private static extern int GetDoubleClickTime();
    }
}