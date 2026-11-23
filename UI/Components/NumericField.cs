using System.Text;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OTK.LiteUI.Core;

namespace OTK.LiteUI.UI.Components
{
    /// <summary>
    /// A single-line text field that accepts numeric input and mathematical expressions.
    /// Expressions are evaluated when Enter is pressed or the field loses focus.
    /// </summary>
    public class NumericField : TextField
    {
        private enum TokenType
        {
            Any,
            Numeric,
            OpenBracket,
            CloseBracket,
            Operator,
            Invalid
        }

        /// <summary>
        /// Whether the most recently evaluated expression was valid.
        /// </summary>
        public bool ExpressionValid
        {
            get;
            private set;
        } = false;

        /// <summary>
        /// Gets or sets the numeric value represented by the field's text.
        /// Returns zero if the text cannot be parsed directly as a double.
        /// Setting this property replaces the field's text with the specified value.
        /// </summary>
        [UIProperty]
        public double Value
        {
            get
            {
                var canGetResult = double.TryParse(Text, out var result);
                if (canGetResult) return result;
                return 0;
            }
            set
            {
                Text = $"{value}";
            }
        }

        /// <summary>
        /// Initializes a numeric field on the specified canvas.
        /// </summary>
        /// <param name="canvas">The canvas to register the field with.</param>
        /// <param name="bounds">The bounds of the field.</param>
        /// <param name="inset">The thickness of the nine-slice sections.</param>
        /// <param name="uvInset">The texture coordinate inset of the nine-slice sections.</param>
        /// <param name="colour">The colour applied to the field.</param>
        public NumericField(UICanvas canvas, Vector4 bounds, float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : base(canvas, bounds, inset, uvInset, colour)
        {
            this.Canvas = canvas;
            Mode = TextFieldMode.SingleLine;
            LockMode = true;
            if (GetType() == typeof(NumericField)) Identifier = Canvas.GetNextIdentifier(typeof(NumericField));
        }

        /// <summary>
        /// Initializes a numeric field on the default canvas.
        /// </summary>
        /// <param name="bounds">The bounds of the field.</param>
        /// <param name="inset">The thickness of the nine-slice sections.</param>
        /// <param name="uvInset">The texture coordinate inset of the nine-slice sections.</param>
        /// <param name="colour">The colour applied to the field.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no default canvas is assigned.
        /// </exception>
        public NumericField(Vector4 bounds, float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : this(UICanvas.Default ?? throw new InvalidOperationException("Default Canvas must not be null for this Constructor. Try Creating a new UICanvas or assigning a UICanvas instance to UICanvas.Default"), bounds, inset, uvInset, colour) { }

        private bool HasDecimalAtCaretGroup()
        {
            if (string.IsNullOrEmpty(Text))
                return false;

            int start = caretIndex;
            int end = caretIndex;

            while (start > 0 && GetTokenType(Text[start - 1]) == TokenType.Numeric)
                start--;

            while (end < Text.Length && GetTokenType(Text[end]) == TokenType.Numeric)
                end++;

            for (int i = start; i < end; i++)
            {
                if (Text[i] == '.')
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Handles text input, rejecting characters that are not valid numeric expression characters
        /// and preventing multiple decimal points within the current number.
        /// </summary>
        /// <param name="e">The text input event arguments.</param>
        public override void OnTextInput(TextInputEventArgs e)
        {
            if (!IsVisible || !IsFocused) return;
            if (GetTokenType((char)e.Unicode) == TokenType.Invalid) return;
            if ((char)e.Unicode == '.' && HasDecimalAtCaretGroup()) return;
            base.OnTextInput(e);
        }

        /// <summary>
        /// Handles keyboard input and evaluates the expression when Enter is pressed.
        /// </summary>
        /// <param name="e">The keyboard event arguments.</param>
        public override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            if (!IsVisible || !IsFocused) return;
            base.OnKeyDown(e);
            if (e.Key == Keys.Enter)
            {
                EvaluateExpression();
            }
        }

        /// <summary>
        /// Handles mouse presses and evaluates the expression when the field loses focus.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        /// <returns>Whether the base text field handled the mouse press.</returns>
        public override bool OnClickDown(MouseState mouse)
        {
            if (!IsVisible) return false;
            if (!WithinBounds(mouse) && IsFocused) EvaluateExpression();
            return base.OnClickDown(mouse);
        }

        /// <summary>
        /// Updates the numeric field while it is visible.
        /// </summary>
        /// <param name="deltaTime">The elapsed time since the previous update.</param>
        /// <param name="mouse">The current mouse state.</param>
        /// <param name="keyboard">The current keyboard state.</param>
        public override void OnUpdate(float deltaTime, MouseState mouse, KeyboardState keyboard)
        {
            if (!IsVisible) return;
            base.OnUpdate(deltaTime, mouse, keyboard);
        }

        private void EvaluateExpression()
        {
            try
            {
                var result = 0.0;
                var tokens = ConvertToTokens();
                var rpn = ConvertToRPN(tokens);
                result = SolveRPN(rpn);
                Value = result;
                ExpressionValid = true;
            }
            catch (Exception)
            {
                ExpressionValid = false;
            }
        }

        private static double SolveRPN(List<string> rpn)
        {
            var stack = new Stack<double>();
            foreach (var token in rpn)
            {
                var type = GetTokenType(token);
                if (type == TokenType.Operator && stack.Count > 1)
                {
                    var right = stack.Pop();
                    var left = stack.Pop();
                    if (token == "+") stack.Push(left + right);
                    else if (token == "-") stack.Push(left - right);
                    else if (token == "*") stack.Push(left * right);
                    else if (token == "/") stack.Push(left / right);
                }
                else if (token == "NEG")
                {
                    stack.Push(-stack.Pop());
                }
                else
                {
                    stack.Push(double.Parse(token));
                }
            }
            return stack.Pop();
        }

        private static List<string> ConvertToRPN(List<string> tokens)
        {
            List<string> output = new();
            Stack<string> operators = new();
            TokenType previousType = TokenType.Any;
            foreach (var token in tokens)
            {
                TokenType type = GetTokenType(token);
                if (type == TokenType.Numeric) output.Add(token);
                else if (type == TokenType.Operator)
                {
                    bool unaryMinus = token == "-" &&
                    (previousType == TokenType.Any ||
                    previousType == TokenType.Operator ||
                    previousType == TokenType.OpenBracket);

                    if (unaryMinus)
                    {
                        operators.Push("NEG");
                    }
                    else
                    {
                        while (operators.Count > 0 && GetTokenType(operators.Peek()) == TokenType.Operator && GetOperatorPrecedence(operators.Peek()) >= GetOperatorPrecedence(token))
                        {
                            output.Add(operators.Pop());
                        }
                        operators.Push(token);
                    }
                }
                else if (type == TokenType.OpenBracket) operators.Push(token);
                else if (type == TokenType.CloseBracket)
                {
                    while (operators.Count > 0 && GetTokenType(operators.Peek()) != TokenType.OpenBracket)
                    {
                        output.Add(operators.Pop());
                    }
                    if (operators.Count == 0) throw new Exception("Mismatched parentheses");
                    operators.Pop();
                }
                previousType = type;
            }
            while (operators.Count > 0)
            {
                if (operators.Peek() == "(") throw new Exception("Mismatched parentheses");
                output.Add(operators.Pop());
            }
            return output;
        }

        private List<string> ConvertToTokens()
        {
            var tokens = new List<string>();
            TokenType type = TokenType.Any;
            StringBuilder currentToken = new();
            bool hasDecimal = false;
            for (int i = 0; i < Text.Length; i++)
            {
                if (type == TokenType.Any)
                {
                    type = GetTokenType(Text[i]);
                    if (type == TokenType.Invalid) throw new ArgumentException("Invalid argument provided. Input must be either numeric, brackets or mathematical operators.");
                }
                else if (GetTokenType(Text[i]) != type || !(GetTokenType(Text[i]) == TokenType.Numeric))
                {
                    tokens.Add(currentToken.ToString());
                    currentToken.Clear();
                    type = GetTokenType(Text[i]);
                    hasDecimal = false;
                }
                if (Text[i] == '.')
                {
                    if (hasDecimal)
                    {
                        throw new ArgumentException("Cannot have more than one decimal point inside a number");
                    }
                    hasDecimal = true;
                }
                currentToken.Append(Text[i]);
            }
            if (currentToken.Length > 0)
            {
                tokens.Add(currentToken.ToString());
            }
            return tokens;
        }

        private static TokenType GetTokenType(char character)
        {
            var result = TokenType.Invalid;
            if (character >= 48 && character <= 57 || character == 46) result = TokenType.Numeric;
            else if (character == 40) result = TokenType.OpenBracket;
            else if (character == 41) result = TokenType.CloseBracket;
            else if (character == 42 || character == 43 || character == 47 || character == 45) result = TokenType.Operator;
            return result;
        }

        private static TokenType GetTokenType(string token)
        {
            if (string.IsNullOrEmpty(token)) return TokenType.Invalid;
            return GetTokenType(token[0]);
        }

        private static int GetOperatorPrecedence(string operatorToken)
        {
            if (operatorToken == "+" || operatorToken == "-") return 1;
            else if (operatorToken == "*" || operatorToken == "/") return 2;
            else if (operatorToken == "NEG") return 3;
            else return 0;
        }
    }
}