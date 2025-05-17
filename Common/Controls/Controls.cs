using Common.Commands;
using Common.Utils;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Common.Controls
{
    #region Border
    public class WindowSubHeader : Border
	{

	}
	public class MessageBorder : Border
	{
	}
	public class SearchBorder : Border
	{
	}
	#endregion //Border
	#region Button
	public class CustomImageButton : Button
	{
		#region Properties

		public static readonly DependencyProperty ImageSourceProperty =
			DependencyProperty.Register(nameof(ImageSource), typeof(ImageSource), typeof(CustomImageButton), new PropertyMetadata(null));

		public ImageSource ImageSource
		{
			get { return GetValue(ImageSourceProperty) as ImageSource; }
			set { SetValue(ImageSourceProperty, value); }
		}

		#endregion //Properties


		public CustomImageButton() { }
	}
	public class WinMinButton : Button
	{
	}
	public class WinCloseButton : Button
	{
	}
	public class SearchButton : Button
	{
	}
	public class TeamViewerButton : Button
	{
	}
	public class MainButton : Button
	{

	}
	public class ExitButton : Button
	{

	}
	public class SetGISButton : Button
	{

	}
	public class CommonButton : Button
	{

	}
	public class PopupCommonButton : Button
	{
	}
	public class PopupInitialButton : Button
	{
	}
	public class PopupSearchButton : Button
	{
	}
	public class SearchPaggingButton : Button
	{
		public static readonly DependencyProperty CurrentPageProperty = DependencyProperty.Register("CurrentPage", typeof(int), typeof(SearchPaggingButton), new PropertyMetadata(0, onCurrentPageChanged, coerceValueChanged));

		private static object coerceValueChanged(DependencyObject d, object baseValue)
		{
			//((SearchPaggingButton)d).onCurrentPageChanged((int)baseValue, (int)baseValue);
			return baseValue;
		}

		private static void onCurrentPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((SearchPaggingButton)d).onCurrentPageChanged((int)e.OldValue, (int)e.NewValue);
		}
		private void onCurrentPageChanged(int oldValue, int newValue)
		{
			if (this.Content == null) return;

			int value = 0;
			if (int.TryParse(this.DataContext.ToString(), out value))
			{
				try
				{
					if (CurrentPage == value)
					{
						this.Background = (SolidColorBrush)Application.Current.FindResource("bgPopupCommonButtonSelect");
						this.FontSize = 14;
						this.FontWeight = FontWeights.Bold;
					}
					else
					{
						this.Background = null;
						this.FontSize = 12;
						this.FontWeight = FontWeights.Normal;
					}
				}
				catch (Exception ex)
				{
					LogManager.WriteExceptionLog(ex.ToString());
				}
			}
		}

		public int CurrentPage
		{
			get { return (int)GetValue(CurrentPageProperty); }
			set { SetValue(CurrentPageProperty, value); }
		}
	}
	public class SearchPaggingFirst : Button
	{
	}
	public class SearchPaggingLast : Button
	{
	}
	public class SearchPaggingNext : Button
	{
	}
	public class SearchPaggingPrev : Button
	{
	}
	public class ChildPopupButton : Button
	{

	}
	public class UpDownButton : ButtonBase
	{
		public bool PressButton
		{
			set { this.IsPressed = value; }
		}


		static UpDownButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(UpDownButton), new FrameworkPropertyMetadata(typeof(ButtonBase)));
		}


		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.Key == Key.Return)
			{
				this.IsPressed = true;
			}
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);
			if (e.Key == Key.Return)
			{
				this.IsPressed = false;
			}
		}
	}
	#endregion //Button
	#region ComboBox
	public class BaseComboBox : ComboBox
	{

	}
	public class SearchComboBox : ComboBox
	{
	}
	public class ChildHeaderComboBox : ComboBox
	{
	}
	#endregion //ComboBox
	#region Grid
	public class PopupGrid : Grid
	{
	}
	#endregion //Grid
	#region Label
	public class SearchTitle : Label
	{
	}
	#endregion //Label
	#region ListBox
	public class BaseListBoxItem : ListBoxItem
	{
	}
	public class BaseListBox : ListBox
	{
		protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
		{
			e.Handled = true;
			base.OnPreviewMouseRightButtonDown(e);
		}
	}
	public class MultiListBox : BaseListBox
	{
		#region Properties

		/// <summary>
		/// The SelectedItems dependency property. Access to the values of the items that are 
		/// selected in the selectedItems box. If SelectionMode is Single, this property returns an array
		/// of length one.
		/// </summary>
		public static new readonly DependencyProperty SelectedItemsProperty =
		   DependencyProperty.Register(nameof(SelectedItems), typeof(ObservableCollection<Object>), typeof(MultiListBox), new PropertyMetadata(new ObservableCollection<Object>()));

		/// <summary>
		/// Get or set the selected items.
		/// </summary>
		public new ObservableCollection<Object> SelectedItems
		{
			get { return GetValue(SelectedItemsProperty) as ObservableCollection<Object>; }
			set { SetValue(SelectedItemsProperty, value); }
		}

		#endregion //Properties



		public MultiListBox()
		{
			base.SelectionChanged += new SelectionChangedEventHandler(UpdateNewClassSelectedItems);
		}



		#region Methods
		/// <summary>
		/// Synchronizes the selected items of this class with the selected items of the base class.
		/// </summary>
		private void UpdateNewClassSelectedItems(object sender, SelectionChangedEventArgs e)
		{
			// If null, then we aren't bound to.
			if (SelectedItems == null)
				return;

			try
			{
				foreach (var o in e.AddedItems)
				{
					SelectedItems.Add(o);
				}
				foreach (var o in e.RemovedItems)
				{
					SelectedItems.Remove(o);
				}
			}
			catch (Exception ex)
			{
				LogManager.WriteExceptionLog(ex.ToString());
			}
		}

		/// <summary>
		/// Synchronizes the selected items with the selected values. 
		/// </summary>
		protected virtual void SetSelectedItemsNew(ObservableCollection<Object> newSelectedItems)
		{
			if (newSelectedItems == null)
				throw new InvalidOperationException("Collection cannot be null");

			// Remove the event handler to prevent recursion.
			base.SelectionChanged -= new SelectionChangedEventHandler(UpdateNewClassSelectedItems);

			base.SetSelectedItems(SelectedItems);

			// Reestablish the event handler.
			base.SelectionChanged += new SelectionChangedEventHandler(UpdateNewClassSelectedItems);

			// Add a collection changed handler to the new list, if it supports the interface.
			AddSelectedItemsChangedHandler(newSelectedItems as INotifyCollectionChanged);
		}

		private void AddSelectedItemsChangedHandler(INotifyCollectionChanged collection)
		{
			if (collection != null)
				collection.CollectionChanged += new NotifyCollectionChangedEventHandler(SelectedItems_CollectionChanged);
		}

		private void RemoveSelectedItemsChangedHandler(INotifyCollectionChanged collection)
		{
			if (collection != null)
				collection.CollectionChanged -= new NotifyCollectionChangedEventHandler(SelectedItems_CollectionChanged);
		}

		void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			base.SelectionChanged -= new SelectionChangedEventHandler(UpdateNewClassSelectedItems);
			base.SetSelectedItems(SelectedItems);
			base.SelectionChanged += new SelectionChangedEventHandler(UpdateNewClassSelectedItems);
		}

		#endregion //Methods
	}
	#endregion //ListBox
	#region RadioButton
	public class SeletedMapButton : RadioButton
	{
	}
	#endregion //RadioButton
	#region ScrollBar
	public class BaseScrollBar : ScrollBar
	{
	}
	#endregion //ScrollBar
	#region ScrollViewer
	public class BaseScrollViewer : ScrollViewer
	{
	}
	#endregion //ScrollViewer
	#region StackPanel
	public class HorizonPanel : StackPanel
	{
	}
	public class VerticalPanel : StackPanel
	{
	}
	public class RightPanel : StackPanel
	{
	}
	public class PopupBottomPanel : StackPanel
	{
	}
	#endregion //StackPanel
	#region TextBlock
	/// <summary>
	/// TextBlock의 TextTrimed를 확인하기 위한 서비스
	/// </summary>
	public class TextBlockService
	{
		static TextBlockService()
		{
			// Register for the SizeChanged event on all TextBlocks, even if the event was handled.
			EventManager.RegisterClassHandler(typeof(TextBlock), FrameworkElement.SizeChangedEvent, new SizeChangedEventHandler(OnTextBlockSizeChanged), true);
		}

		public static readonly DependencyPropertyKey IsTextTrimmedKey = DependencyProperty.RegisterAttachedReadOnly(
			"IsTextTrimmed",
			typeof(bool),
			typeof(TextBlockService),
			new PropertyMetadata(false));

		public static readonly DependencyProperty IsTextTrimmedProperty = IsTextTrimmedKey.DependencyProperty;

		[AttachedPropertyBrowsableForType(typeof(TextBlock))]
		public static Boolean GetIsTextTrimmed(TextBlock target)
		{
			return (Boolean)target.GetValue(IsTextTrimmedProperty);
		}

		public static void OnTextBlockSizeChanged(object sender, SizeChangedEventArgs e)
		{
			BaseTextBlock textBlock = sender as BaseTextBlock;
			if (null == textBlock)
				return;
			if (textBlock.IsTrimmingToolTip)
				textBlock.SetValue(IsTextTrimmedKey, calculateIsTextTrimmed(textBlock));
		}

		private static bool calculateIsTextTrimmed(TextBlock textBlock)
		{
			double width = textBlock.ActualWidth;
			if (textBlock.TextTrimming == TextTrimming.None)
				return false;
			if (textBlock.TextWrapping != TextWrapping.NoWrap)
				return false;
			textBlock.Measure(new Size(double.MaxValue, double.MaxValue));
			double totalWidth = textBlock.DesiredSize.Width;
			return width < totalWidth;
		}
	}

	public class BaseTextBlock : TextBlock
	{
		#region IsTrimmingToolTip
		public static readonly DependencyProperty IsTrimmingToolTipProperty =
			DependencyProperty.Register("IsTrimmingToolTip", typeof(Boolean), typeof(BaseTextBlock), new PropertyMetadata(false));

		/// <summary>
		/// TextTrimming 속성이 CharacterEllipsis 일 때 툴팁 표시 여부
		/// </summary>
		public Boolean IsTrimmingToolTip
		{
			get { return (Boolean)this.GetValue(IsTrimmingToolTipProperty); }
			set { this.SetValue(IsTrimmingToolTipProperty, value); }
		}
		#endregion //IsTrimmingToolTip


		/*
		protected override void OnToolTipOpening(ToolTipEventArgs e)
		{
			if (TextTrimming != TextTrimming.None && ToolTip != null)
				e.Handled = !IsTextTrimmed();
		}

		private bool IsTextTrimmed()
		{
			var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
			var formattedText = new FormattedText(Text, System.Globalization.CultureInfo.CurrentCulture, FlowDirection, typeface, FontSize, Foreground);
			return formattedText.Width > ActualWidth;
		}
		*/
	}
	public class PopupTextBlockHeader : BaseTextBlock
	{
	}
	public class DGTextBlock : TextBlock
	{
	}
	#endregion //TextBlock
	#region TextBox
	public class BaseTextBox : TextBox
	{
	}
	/// <summary>
	/// 검색 텍스트 박스 컨트롤
	/// </summary>
	public class SearchTextBox : UserControl
	{
		#region Fields
		public const int LIST_COUNT = 10;
		#endregion //Fields


		#region Properties

		public ObservableCollection<String> SearchList
		{
			get { return (ObservableCollection<String>)GetValue(SearchListProperty); }
			set { SetValue(SearchListProperty, value); }
		}

		public String SearchText
		{
			get { return (String)GetValue(SearchTextProperty); }
			set { SetValue(SearchTextProperty, value); }
		}

		public String SearchSelectedItem
		{
			get { return (String)GetValue(SearchSelectedItemProperty); }
			set { SetValue(SearchSelectedItemProperty, value); }
		}

		public Boolean IsOpen
		{
			get { return (Boolean)GetValue(IsOpenProperty); }
			set { SetValue(IsOpenProperty, value); }
		}

		#endregion //Properties

		#region DependencyProperty

		public static readonly DependencyProperty SearchTextProperty;
		public static readonly DependencyProperty SearchListProperty;
		public static readonly DependencyProperty SearchSelectedItemProperty;
		public static readonly DependencyProperty IsOpenProperty;


		#region Dependency Changed Methods

		private static void OnSearchSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			SearchTextBox control = (d as SearchTextBox);
			if (control != null)
			{
				control.OnSearchSelectedItemChanged(e.OldValue, e.NewValue);
			}
		}

		private void OnSearchSelectedItemChanged(object oldValue, object newValue)
		{
			if (newValue == null) return;

			this.SearchText = newValue.ToString();
		}

		#endregion //Dependency Changed Methods

		#endregion //DependencyProperty

		public ICommand SearchCommand { get; private set; }



		static SearchTextBox()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchTextBox), new FrameworkPropertyMetadata(typeof(SearchTextBox)));

			SearchTextProperty = DependencyProperty.Register("SearchText", typeof(String), typeof(SearchTextBox), new FrameworkPropertyMetadata(String.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
			SearchListProperty = DependencyProperty.Register("SearchList", typeof(ObservableCollection<String>), typeof(SearchTextBox), new UIPropertyMetadata(new ObservableCollection<String>()));
			SearchSelectedItemProperty = DependencyProperty.Register("SearchSelectedItem", typeof(String), typeof(SearchTextBox), new UIPropertyMetadata(null, OnSearchSelectedItemChanged));
			IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(Boolean), typeof(SearchTextBox), new UIPropertyMetadata(false));
		}
		public SearchTextBox()
		{
			SearchCommand = new RelayCommand(onSearch);
		}



		#region override

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			try
			{
				ControlTemplate template = Template as ControlTemplate;
				if (template != null)
				{
					ScrollViewer sv = template.FindName("PART_ContentHost", this) as ScrollViewer;
				}
			}
			catch (Exception ex)
			{
				LogManager.WriteExceptionLog(ex.ToString());
			}
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (String.IsNullOrEmpty(SearchText))
				return;
			if (e.Key == Key.Enter)
			{
				SetSearchText();
			}
		}

		private void SetSearchText()
		{
			if (SearchList.Contains(SearchText))
				SearchList.Remove(SearchText);
			// 지정된 갯수보다 많아 질 경우 가장 아래의 목록을 삭제 한다.
			if (SearchList.Count > LIST_COUNT)
				SearchList.RemoveAt(SearchList.Count - 1);
			SearchList.Insert(0, SearchText);
		}

		#endregion //override

		#region Methods

		private void onSearch(object obj)
		{
			SetSearchText();
		}

		#endregion //Methods
	}
	public class NumericTextBox : BaseTextBox
	{
		#region Fields
		private Regex numericRegex = null;
		private bool IsKeyDown = false;
		private object oldValue = null;
		//private object newValue = null;
		#endregion //Fields


		#region DependencyProperty
		// Using a DependencyProperty as the backing store for Maximum.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(nameof(Maximum), typeof(int), typeof(NumericTextBox), new FrameworkPropertyMetadata(int.MaxValue));
		// Using a DependencyProperty as the backing store for Minimum.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(nameof(Minimum), typeof(int), typeof(NumericTextBox), new FrameworkPropertyMetadata(0));
		// Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(int), typeof(NumericTextBox),
			new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

		private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			(d as NumericTextBox).OnValueChanged((int)e.NewValue);
		}
		private void OnValueChanged(int newValue)
		{
			this.Text = newValue.ToString();
		}
		#endregion //DependencyProperty


		#region Properties
		/// <summary>
		/// Maximum value for the Numeric Up Down control
		/// </summary>
		public int Maximum
		{
			get { return (int)GetValue(MaximumProperty); }
			set { SetValue(MaximumProperty, value); }
		}

		/// <summary>
		/// Minimum value of the numeric up down conrol.
		/// </summary>
		public int Minimum
		{
			get { return (int)GetValue(MinimumProperty); }
			set { SetValue(MinimumProperty, value); }
		}
		/// <summary>The Value property represents the TextBoxValue of the control.</summary>
		/// <returns>The current TextBoxValue of the control</returns>      
		public int Value
		{
			get { return (int)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}
		#endregion //Properties


		#region Events
		//Increase button clicked
		private static readonly RoutedEvent IncreaseClickedEvent =
			EventManager.RegisterRoutedEvent(nameof(IncreaseClicked), RoutingStrategy.Bubble,
			typeof(RoutedEventHandler), typeof(NumericTextBox));

		/// <summary>The IncreaseClicked event is called when the Increase button clicked</summary>
		public event RoutedEventHandler IncreaseClicked
		{
			add { AddHandler(IncreaseClickedEvent, value); }
			remove { RemoveHandler(IncreaseClickedEvent, value); }
		}

		//Increase button clicked
		private static readonly RoutedEvent DecreaseClickedEvent =
			EventManager.RegisterRoutedEvent(nameof(DecreaseClicked), RoutingStrategy.Bubble,
			typeof(RoutedEventHandler), typeof(NumericTextBox));

		/// <summary>The DecreaseClicked event is called when the Decrease button clicked</summary>
		public event RoutedEventHandler DecreaseClicked
		{
			add { AddHandler(DecreaseClickedEvent, value); }
			remove { RemoveHandler(DecreaseClickedEvent, value); }
		}
		#endregion //Events



		static NumericTextBox()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericTextBox), new FrameworkPropertyMetadata(typeof(TextBox)));
		}
		public NumericTextBox()
		{
			numericRegex = new Regex(@"^(0|-?[1-9]\d*)$");
			this.TextChanged += NumericTextBox_TextChanged;
		}
		~NumericTextBox()
		{
			this.TextChanged -= NumericTextBox_TextChanged;
		}




		protected override void OnPreviewTextInput(TextCompositionEventArgs e)
		{
			var tb = (TextBox)this;
			var text = tb.Text;
			text = text.Remove(tb.SelectionStart, tb.SelectionLength);
			text = text.Insert(tb.SelectionStart, e.Text);

			e.Handled = !numericRegex.IsMatch(text);

			var parsedValue = -1;

			int.TryParse(text, out parsedValue);

			if (parsedValue < Minimum)
			{
				tb.Text = Minimum.ToString();

				e.Handled = true;
			}
			else if (parsedValue > Maximum)
			{
				tb.Text = Maximum.ToString();
				e.Handled = true;
			}

			base.OnPreviewTextInput(e);
		}
		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			if (e.Key == Key.Space)
			{
				e.Handled = true;
				return;
			}

			if (e.IsDown && e.Key == Key.Up && Value < Maximum)
			{
				Value++;
				RaiseEvent(new RoutedEventArgs(IncreaseClickedEvent));
			}
			else if (e.IsDown && e.Key == Key.Down && Value > Minimum)
			{
				Value--;
				RaiseEvent(new RoutedEventArgs(DecreaseClickedEvent));
			}

			if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down)
			{
				if (IsKeyDown == false)
				{
					IsKeyDown = true;
					System.Diagnostics.Debug.WriteLine("oldValue : " + oldValue);
				}
			}
			base.OnPreviewKeyDown(e);
		}
		protected override void OnPreviewKeyUp(KeyEventArgs e)
		{
			if (oldValue != null)
			{
				System.Diagnostics.Debug.WriteLine("Value : " + this.Value);
			}
			oldValue = null;
			IsKeyDown = false;
			base.OnPreviewKeyUp(e);
		}


		private void NumericTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			var tb = (TextBox)this;
			if (!numericRegex.IsMatch(tb.Text)) ResetText(tb);

			int parsedValue;

			try
			{
				parsedValue = Convert.ToInt32(tb.Text);
			}
			catch (OverflowException)
			{
				if (tb.Text.Length > 0 && tb.Text[0] == '-')
					parsedValue = Minimum;
				else
					parsedValue = Maximum;

				// 강제로 갱신해준다.
				if (Value == parsedValue)
				{
					tb.Text = parsedValue.ToString(CultureInfo.InvariantCulture);
				}
			}

			if (Value != parsedValue)
			{
				if (parsedValue < Minimum)
				{
					parsedValue = Minimum;
				}
				else if (parsedValue > Maximum)
				{
					parsedValue = Maximum;
				}

				Value = parsedValue;
			}

			//RaiseEvent(new RoutedEventArgs(ValueChangedEvent));
		}

		private void ResetText(TextBox tb)
		{
			tb.Text = 0 < Minimum ? Minimum.ToString(CultureInfo.InvariantCulture) : "0";
			tb.SelectAll();
		}
	}
	#endregion //TextBox

	#region UserControl
	public class WaitingBar : ContentControl
	{
		public WaitingBar()
		{

		}
	}
	public class MainAlarmGrid : ContentControl
	{
		public static readonly DependencyProperty IsOpenProperty;

		#region Properties
		public Boolean IsOpen
		{
			get { return (Boolean)GetValue(IsOpenProperty); }
			set { SetValue(IsOpenProperty, value); }
		}
		#endregion //Properties


		static MainAlarmGrid()
		{
			IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(Boolean), typeof(MainAlarmGrid), new UIPropertyMetadata(false));
		}

	}
	#endregion //UserControl
}
