using System;
using System.Collections.Generic;
using System.Text;

namespace CalculatorStateMachine
{
    // Calculator states
    public enum CalculatorState
    {
        Start,
        AccumulatingNumber,
        OperatorEntered,
        Error
    }

    // Calculator tokens
    public enum TokenType
    {
        Number,
        Operator,
        Error
    }

    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
    }

    public class CalculationResult
    {
        public double? Value { get; set; }
        public string Error { get; set; }
        public bool Success => string.IsNullOrEmpty(Error);
    }
    public class Calculator
    {
        private CalculatorState _currentState;
        private StringBuilder _currentNumber;
        private double? _accumulator;
        private string _currentOperator;

        public Calculator()
        {
            Reset();
        }

        public void Reset()
        {
            _currentState = CalculatorState.Start;
            _currentNumber = new StringBuilder();
            _accumulator = null;
            _currentOperator = null;
        }

        // Main calculation method using yield pattern
        public IEnumerable<CalculationResult> ProcessInput(string input)
        {
            foreach (var token in Tokenize(input))
            {
                var result = ProcessToken(token);
                yield return result;

                if (!result.Success)
                {
                    Reset();
                    yield break;
                }
            }

            // Return final result if there's a pending number
            if (_currentNumber.Length > 0)
            {
                yield return FinalizePendingCalculation();
            }
        }

        // Tokenize input string
        private IEnumerable<Token> Tokenize(string input)
        {
            var currentNumber = new StringBuilder();

            foreach (char c in input)
            {
                if (char.IsWhiteSpace(c))
                    continue;

                if (char.IsDigit(c) || c == '.')
                {
                    currentNumber.Append(c);
                }
                else if (IsOperator(c.ToString()))
                {
                    if (currentNumber.Length > 0)
                    {
                        yield return new Token { Type = TokenType.Number, Value = currentNumber.ToString() };
                        currentNumber.Clear();
                    }
                    yield return new Token { Type = TokenType.Operator, Value = c.ToString() };
                }
                else
                {
                    yield return new Token { Type = TokenType.Error, Value = c.ToString() };
                }
            }

            if (currentNumber.Length > 0)
            {
                yield return new Token { Type = TokenType.Number, Value = currentNumber.ToString() };
            }
        }

        // Process individual tokens
        private CalculationResult ProcessToken(Token token)
        {
            switch (_currentState)
            {
                case CalculatorState.Start:
                    return ProcessStartState(token);

                case CalculatorState.AccumulatingNumber:
                    return ProcessAccumulatingState(token);

                case CalculatorState.OperatorEntered:
                    return ProcessOperatorState(token);

                case CalculatorState.Error:
                default:
                    return new CalculationResult { Error = "Calculator in error state" };
            }
        }

        private CalculationResult ProcessStartState(Token token)
        {
            if (token.Type == TokenType.Number)
            {
                _currentNumber.Append(token.Value);
                _currentState = CalculatorState.AccumulatingNumber;
                return new CalculationResult { Value = ParseCurrentNumber() };
            }
            return new CalculationResult { Error = "Expected number" };
        }

        private CalculationResult ProcessAccumulatingState(Token token)
        {
            switch (token.Type)
            {
                case TokenType.Number:
                    _currentNumber.Append(token.Value);
                    return new CalculationResult { Value = ParseCurrentNumber() };

                case TokenType.Operator:
                    if (_accumulator == null)
                    {
                        _accumulator = ParseCurrentNumber();
                    }
                    else
                    {
                        var result = PerformOperation();
                        if (!result.Success) return result;
                        _accumulator = result.Value;
                    }
                    _currentOperator = token.Value;
                    _currentNumber.Clear();
                    _currentState = CalculatorState.OperatorEntered;
                    return new CalculationResult { Value = _accumulator };

                default:
                    return new CalculationResult { Error = "Invalid token" };
            }
        }

        private CalculationResult ProcessOperatorState(Token token)
        {
            if (token.Type == TokenType.Number)
            {
                _currentNumber.Append(token.Value);
                _currentState = CalculatorState.AccumulatingNumber;
                return new CalculationResult { Value = ParseCurrentNumber() };
            }
            return new CalculationResult { Error = "Expected number after operator" };
        }

        private CalculationResult FinalizePendingCalculation()
        {
            if (_currentOperator != null && _accumulator.HasValue)
            {
                return PerformOperation();
            }
            return new CalculationResult { Value = ParseCurrentNumber() };
        }

        private CalculationResult PerformOperation()
        {
            var currentValue = ParseCurrentNumber();
            if (!_accumulator.HasValue) return new CalculationResult { Value = currentValue };

            try
            {
                switch (_currentOperator)
                {
                    case "+":
                        return new CalculationResult { Value = _accumulator + currentValue };
                    case "-":
                        return new CalculationResult { Value = _accumulator - currentValue };
                    case "*":
                        return new CalculationResult { Value = _accumulator * currentValue };
                    case "/":
                        if (currentValue == 0)
                            return new CalculationResult { Error = "Division by zero" };
                        return new CalculationResult { Value = _accumulator / currentValue };
                    default:
                        return new CalculationResult { Error = "Unknown operator" };
                }
            }
            catch (Exception ex)
            {
                return new CalculationResult { Error = ex.Message };
            }
        }

        private double ParseCurrentNumber()
        {
            return double.Parse(_currentNumber.ToString());
        }

        private bool IsOperator(string op)
        {
            return op == "+" || op == "-" || op == "*" || op == "/";
        }
    }

    // Example usage
    public class Program
    {
        public static void Main()
        {
            var calculator = new Calculator();
            
            // Test different expressions
            string[] testExpressions = {
                "123 + 456",
                "10 * 5 - 3",
                "100 / 0",
                "abc",
                "1.5 * 2.5",
                "10 + 20 + 30"
            };

            foreach (var expression in testExpressions)
            {
                Console.WriteLine($"\nCalculating: {expression}");
                foreach (var result in calculator.ProcessInput(expression))
                {
                    if (result.Success)
                        Console.WriteLine($"Intermediate result: {result.Value}");
                    else
                        Console.WriteLine($"Error: {result.Error}");
                }
                calculator.Reset();
            }
        }
    }
}