using Common.Utils;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Common.Controls
{
    public enum WindowState
	{
		Closed,
		Open
	}


	[TemplatePart(Name = PART_WindowRoot, Type = typeof(FrameworkElement))]
	[TemplatePart(Name = PART_Root, Type = typeof(Canvas))]
	[TemplatePart(Name = PART_WindowControl, Type = typeof(ContentControl))]
	public class ChildWindow : ContentControl, IDisposable
	{
		private const string PART_WindowRoot = "PART_WindowRoot";
		private const string PART_Root = "PART_Root";
		private const string PART_WindowControl = "PART_WindowControl";
		private const int _horizontalOffset = 3;
		private const int _verticalOffset = 3;

		#region Private Members

		private Canvas _CanvasRoot;
		private FrameworkElement _windowRoot;
		private ContentControl _windowControl;
		private bool _ignorePropertyChanged;

		#endregion //Private Members

		#region Public Properties

		#region DialogResult

		private bool? _dialogResult;
		/// <summary>
		/// Gets or sets a value indicating whether the ChildWindow was accepted or canceled.
		/// </summary>
		/// <value>
		/// True if the child window was accepted; false if the child window was
		/// canceled. The default is null.
		/// </value>
		[TypeConverter(typeof(NullableBoolConverter))]
		public bool? DialogResult
		{
			get { return _dialogResult; }
			set
			{
				if (_dialogResult != value)
				{
					_dialogResult = value;
					this.Close();
				}
			}
		}

		#endregion //DialogResult


		#region DesignerWindowState

		public static readonly DependencyProperty DesignerWindowStateProperty = DependencyProperty.Register("DesignerWindowState", typeof(WindowState), typeof(ChildWindow), new PropertyMetadata(WindowState.Closed, OnDesignerWindowStatePropertyChanged));
		public WindowState DesignerWindowState
		{
			get { return (WindowState)GetValue(DesignerWindowStateProperty); }
			set { SetValue(DesignerWindowStateProperty, value); }
		}

		private static void OnDesignerWindowStatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ChildWindow childWindow = d as ChildWindow;
			if (childWindow != null)
				childWindow.OnDesignerWindowStatePropertyChanged((WindowState)e.OldValue, (WindowState)e.NewValue);
		}

		protected virtual void OnDesignerWindowStatePropertyChanged(WindowState oldValue, WindowState newValue)
		{
			if (DesignerProperties.GetIsInDesignMode(this))
			{
				Visibility = newValue == WindowState.Open ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		#endregion //DesignerWindowState

		#region FocusedElement

		public static readonly DependencyProperty FocusedElementProperty = DependencyProperty.Register("FocusedElement", typeof(FrameworkElement), typeof(ChildWindow), new UIPropertyMetadata(null));
		public FrameworkElement FocusedElement
		{
			get { return (FrameworkElement)GetValue(FocusedElementProperty); }
			set { SetValue(FocusedElementProperty, value); }
		}

		#endregion //FocusedElement


		#region WindowStartupLocation

		public static readonly DependencyProperty WindowStartupLocationProperty = DependencyProperty.Register("WindowStartupLocation", typeof(WindowStartupLocation), typeof(ChildWindow), new UIPropertyMetadata(WindowStartupLocation.Manual, OnWindowStartupLocationChanged));
		public WindowStartupLocation WindowStartupLocation
		{
			get { return (WindowStartupLocation)GetValue(WindowStartupLocationProperty); }
			set { SetValue(WindowStartupLocationProperty, value); }
		}

		private static void OnWindowStartupLocationChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			ChildWindow childWindow = o as ChildWindow;
			if (childWindow != null)
				childWindow.OnWindowStartupLocationChanged((WindowStartupLocation)e.OldValue, (WindowStartupLocation)e.NewValue);
		}

		protected virtual void OnWindowStartupLocationChanged(WindowStartupLocation oldValue, WindowStartupLocation newValue)
		{
			// TODO: Add your property changed side-effects. Descendants can override as well.
		}

		#endregion //WindowStartupLocation

		#region WindowState

		public static readonly DependencyProperty WindowStateProperty = DependencyProperty.Register("WindowState", typeof(WindowState), typeof(ChildWindow), new FrameworkPropertyMetadata(WindowState.Closed, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnWindowStatePropertyChanged));
		public WindowState WindowState
		{
			get { return (WindowState)GetValue(WindowStateProperty); }
			set { SetValue(WindowStateProperty, value); }
		}

		private static void OnWindowStatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ChildWindow childWindow = d as ChildWindow;
			if (childWindow != null)
				childWindow.OnWindowStatePropertyChanged((WindowState)e.OldValue, (WindowState)e.NewValue);
		}

		protected virtual void OnWindowStatePropertyChanged(WindowState oldValue, WindowState newValue)
		{
			if (!DesignerProperties.GetIsInDesignMode(this))
			{
				if (!_ignorePropertyChanged)
					SetWindowState(newValue);
			}
			else
			{
				Visibility = DesignerWindowState == WindowState.Open ? Visibility.Visible : System.Windows.Visibility.Collapsed;
			}
		}

		#endregion //WindowState

		public string Caption
		{
			get { return (string)GetValue(CaptionProperty); }
			set { SetValue(CaptionProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Caption.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CaptionProperty =
			DependencyProperty.Register("Caption", typeof(string), typeof(ChildWindow), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCaptionPropertyChanged));

		private static void OnCaptionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ChildWindow childWindow = d as ChildWindow;
			if (childWindow != null)
			{ }
		}

		public string Caption2
		{
			get { return (string)GetValue(Caption2Property); }
			set { SetValue(Caption2Property, value); }
		}

		// Using a DependencyProperty as the backing store for Caption.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty Caption2Property =
			DependencyProperty.Register("Caption2", typeof(string), typeof(ChildWindow), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public double ChildWidth
		{
			get { return (double)GetValue(ChildWidthProperty); }
			set { SetValue(ChildWidthProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ChildWidth.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ChildWidthProperty =
			DependencyProperty.Register("ChildWidth", typeof(double), typeof(ChildWindow), new FrameworkPropertyMetadata((double)0));

		public double ChildHeight
		{
			get { return (double)GetValue(ChildHeightProperty); }
			set { SetValue(ChildHeightProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ChildHeight.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ChildHeightProperty =
			DependencyProperty.Register("ChildHeight", typeof(double), typeof(ChildWindow), new FrameworkPropertyMetadata((double)0));

		public double ChildLeft
		{
			get { return (double)GetValue(ChildLeftProperty); }
			set { SetValue(ChildLeftProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ChildHeight.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ChildLeftProperty =
			DependencyProperty.Register(nameof(ChildLeft), typeof(double), typeof(ChildWindow), new FrameworkPropertyMetadata(double.NaN));

		#endregion //Public Properties

		#region Constructors

		static ChildWindow()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ChildWindow), new FrameworkPropertyMetadata(typeof(ChildWindow)));
		}

		public ChildWindow()
		{
			DesignerWindowState = WindowState.Open;
		}


		#endregion //Constructors

		#region Base Class Overrides

		private void CloseButton_OnClick(object sender, RoutedEventArgs e)
		{
			Close();
			RoutedEventArgs args = new RoutedEventArgs(CloseButtonClickedEvent, this);
			this.RaiseEvent(args);
			e.Handled = true;
		}

		Button _closeButton;
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_CanvasRoot = this.GetTemplateChild(PART_Root) as Canvas;
			if (_CanvasRoot != null) _CanvasRoot.SizeChanged += _CanvasRoot_SizeChanged;
			_windowRoot = this.GetTemplateChild(PART_WindowRoot) as FrameworkElement;

			try
			{
				SetLayout();

				if (_closeButton != null)
				{
					_closeButton.Click -= CloseButton_OnClick;
				}

				_closeButton = this.GetTemplateChild("CloseButton") as Button;

				if (_closeButton != null)
				{
					_closeButton.Click += CloseButton_OnClick;
				}
			}
			catch (Exception ex)
			{
				LogManager.WriteExceptionLog(ex.ToString());
			}
		}

		private void _CanvasRoot_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			SetLayout();
		}

		private void SetLayout()
		{
			if (_windowRoot != null && _CanvasRoot != null)
			{
				double screenCenterX = _CanvasRoot.ActualWidth / 2;
				double screenCenterY = _CanvasRoot.ActualHeight / 2;
				//double screenCenterX = (this.Parent as FrameworkElement).ActualWidth / 2;
				//double screenCenterY = (this.Parent as FrameworkElement).ActualHeight / 2;
				double halfWidth = _windowRoot.Width / 2;
				double halfHeight = _windowRoot.Height / 2;

				if (ChildLeft == double.NaN)
					Canvas.SetLeft(_windowRoot, screenCenterX - halfWidth);
				else
					Canvas.SetLeft(_windowRoot, ChildLeft);
				Canvas.SetTop(_windowRoot, screenCenterY - halfHeight);
			}
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);
			SetLayout();
		}

		#region override

		protected override void OnGotFocus(RoutedEventArgs e)
		{
			base.OnGotFocus(e);

			Action action = () =>
			{
				if (FocusedElement != null)
					FocusedElement.Focus();
			};

			Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, action);
		}

		#endregion //override

		#endregion //Base Class Overrides

		#region Event Handlers

		public static readonly RoutedEvent CloseButtonClickedEvent = EventManager.RegisterRoutedEvent("CloseButtonClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ChildWindow));

		// Provide CLR accessors for the event 
		public event RoutedEventHandler CloseButtonClicked
		{
			add { AddHandler(CloseButtonClickedEvent, value); }
			remove { RemoveHandler(CloseButtonClickedEvent, value); }
		}

		protected virtual void OnCloseButtonClicked(RoutedEventArgs e)
		{
			//if (!this.IsCurrentWindow(e.OriginalSource))
			//    return;

			e.Handled = true;

			RoutedEventArgs args = new RoutedEventArgs(CloseButtonClickedEvent, this);
			this.RaiseEvent(args);

			if (!args.Handled)
			{
				this.Close();
			}
		}

		#endregion //Event Handlers

		#region Methods

		#region Private

		private void SetWindowState(WindowState state)
		{
			switch (state)
			{
				case WindowState.Closed:
					{
						ExecuteClose();
						break;
					}
				case WindowState.Open:
					{
						ExecuteOpen();
						break;
					}
			}
		}

		private void ExecuteClose()
		{
			CancelEventArgs e = new CancelEventArgs();
			OnClosing(e);

			if (!e.Cancel)
			{
				if (!_dialogResult.HasValue)
					_dialogResult = false;

				OnClosed(EventArgs.Empty);
			}
			else
			{
				CancelClose();
			}
		}

		private void CancelClose()
		{
			_dialogResult = null; //when the close is cancelled, DialogResult should be null

			_ignorePropertyChanged = true;
			WindowState = WindowState.Open; //now reset the window state to open because the close was cancelled
			_ignorePropertyChanged = false;
		}

		private void ExecuteOpen()
		{
			SetLayout();
		}

		private bool IsCurrentWindow(object windowtoTest)
		{
			return object.Equals(_windowControl, windowtoTest);
		}

		#endregion //Private

		#region Public

		public void Show()
		{
			WindowState = WindowState.Open;
		}

		public void Close()
		{
			WindowState = WindowState.Closed;
		}

		public virtual void Dispose()
		{
			_dialogResult = null;
			if (_CanvasRoot != null) _CanvasRoot = null;
			if (_windowRoot != null) _windowRoot = null;
			if (_windowControl != null) _windowControl = null;
		}

		#endregion //Public

		#endregion //Methods

		#region Events

		/// <summary>
		/// Occurs when the ChildWindow is closed.
		/// </summary>
		public event EventHandler Closed;
		protected virtual void OnClosed(EventArgs e)
		{
			if (Closed != null)
				Closed(this, e);
		}

		/// <summary>
		/// Occurs when the ChildWindow is closing.
		/// </summary>
		public event EventHandler<CancelEventArgs> Closing;
		protected virtual void OnClosing(CancelEventArgs e)
		{
			if (Closing != null)
				Closing(this, e);
		}

		#endregion //Events
	}
}
