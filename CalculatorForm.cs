using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace CalculatorApp
{
    public partial class CalculatorForm : Form
    {
        private readonly CalculatorStateMachine.Calculator _calculator;
        private string _currentInput = "";
        private TextBox _display;
        private Label _stateDisplay;
        private bool _newNumber = true;

        public CalculatorForm()
        {
            _calculator = new CalculatorStateMachine.Calculator();
            InitializeCalculatorComponents();
        }

        private void InitializeCalculatorComponents()
        {
            // Form settings
            this.Text = "State Machine Calculator";
            this.Width = 300;
            this.Height = 400;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);

            // Create display panel
            var displayPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(10)
            };

            // Create state display
            _stateDisplay = new Label
            {
                Dock = DockStyle.Top,
                Height = 20,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray
            };
            displayPanel.Controls.Add(_stateDisplay);

            // Create main display
            _display = new TextBox
            {
                Dock = DockStyle.Fill,
                Text = "0",
                TextAlign = HorizontalAlignment.Right,
                Font = new Font("Segoe UI", 20, FontStyle.Regular),
                ReadOnly = true,
                BackColor = Color.White
            };
            displayPanel.Controls.Add(_display);

            this.Controls.Add(displayPanel);

            // Create button panel
            var buttonPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 5,
                ColumnCount = 4,
                Padding = new Padding(5),
                BackColor = Color.FromArgb(240, 240, 240)
            };

            // Set row and column styles
            for (int i = 0; i < 5; i++)
                buttonPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            for (int i = 0; i < 4; i++)
                buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            // Define button layout
            var buttonLayout = new string[,]
            {
                { "CE", "C", "⌫", "÷" },
                { "7", "8", "9", "×" },
                { "4", "5", "6", "-" },
                { "1", "2", "3", "+" },
                { "±", "0", ".", "=" }
            };

            // Create buttons
            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    var button = CreateCalculatorButton(buttonLayout[row, col]);
                    buttonPanel.Controls.Add(button, col, row);
                }
            }

            this.Controls.Add(buttonPanel);
        }

        private Button CreateCalculatorButton(string text)
        {
            var button = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                Margin = new Padding(2),
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat
            };

            // Style the button based on its type
            if ("0123456789.".Contains(text))
            {
                button.BackColor = Color.White;
                button.Click += NumberButton_Click;
            }
            else if ("+-×÷".Contains(text))
            {
                button.BackColor = Color.FromArgb(230, 230, 230);
                button.Click += OperatorButton_Click;
            }
            else if (text == "=")
            {
                button.BackColor = Color.FromArgb(120, 162, 204);
                button.ForeColor = Color.White;
                button.Click += EqualsButton_Click;
            }
            else
            {
                button.BackColor = Color.FromArgb(230, 230, 230);
                switch (text)
                {
                    case "CE":
                        button.Click += (s, e) => ClearEntry();
                        break;
                    case "C":
                        button.Click += (s, e) => ClearAll();
                        break;
                    case "⌫":
                        button.Click += (s, e) => Backspace();
                        break;
                    case "±":
                        button.Click += (s, e) => ToggleSign();
                        break;
                }
            }

            // Add hover effects
            button.MouseEnter += (s, e) => button.BackColor = DarkenColor(button.BackColor);
            button.MouseLeave += (s, e) => button.BackColor = LightenColor(button.BackColor);

            return button;
        }

        private void NumberButton_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            if (_newNumber)
            {
                _display.Text = button.Text;
                _newNumber = false;
            }
            else
            {
                _display.Text += button.Text;
            }
            _currentInput = _display.Text;
            ProcessCurrentInput();
        }

        private void OperatorButton_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            string op = button.Text.Replace("×", "*").Replace("÷", "/");
            _currentInput += " " + op + " ";
            _newNumber = true;
            ProcessCurrentInput();
        }

        private void EqualsButton_Click(object sender, EventArgs e)
        {
            ProcessCurrentInput(true);
            _newNumber = true;
            _currentInput = _display.Text;
        }

        private void ProcessCurrentInput(bool isFinal = false)
        {
            var results = _calculator.ProcessInput(_currentInput);
            CalculatorStateMachine.CalculationResult lastResult = null;

            foreach (var result in results)
            {
                if (result.Success)
                {
                    lastResult = result;
                }
                else
                {
                    _display.Text = "Error: " + result.Error;
                    _stateDisplay.Text = "Error";
                    return;
                }
            }

            if (lastResult != null && lastResult.Success)
            {
                _display.Text = lastResult.Value?.ToString() ?? "0";
                _stateDisplay.Text = _calculator.GetType().GetField("_currentState", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance)?.GetValue(_calculator).ToString();
            }
        }

        private void ClearEntry()
        {
            _display.Text = "0";
            _newNumber = true;
            _currentInput = "";
        }

        private void ClearAll()
        {
            ClearEntry();
            _calculator.Reset();
            _stateDisplay.Text = "Start";
        }

        private void Backspace()
        {
            if (_display.Text.Length > 1)
            {
                _display.Text = _display.Text.Substring(0, _display.Text.Length - 1);
                _currentInput = _display.Text;
            }
            else
            {
                _display.Text = "0";
                _newNumber = true;
            }
        }

        private void ToggleSign()
        {
            if (_display.Text != "0" && _display.Text.Length > 0)
            {
                if (_display.Text[0] == '-')
                    _display.Text = _display.Text.Substring(1);
                else
                    _display.Text = "-" + _display.Text;
                
                _currentInput = _display.Text;
            }
        }

        private Color DarkenColor(Color color)
        {
            return Color.FromArgb(color.R - 20, color.G - 20, color.B - 20);
        }

        private Color LightenColor(Color color)
        {
            return Color.FromArgb(Math.Min(color.R + 20, 255), 
                                Math.Min(color.G + 20, 255), 
                                Math.Min(color.B + 20, 255));
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new CalculatorForm());
        }
    }
}