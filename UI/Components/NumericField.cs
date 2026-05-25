using System.Text;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

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

    public bool ExpressionValid
    {
        get;
        private set;
    } = false;

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

    public NumericField(Vector4 bounds, float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : base(bounds, inset, uvInset, colour)
    {
        Mode = TextFieldMode.SingleLine;
        LockMode = true;
    }

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

    public override void OnTextInput(TextInputEventArgs e)
    {
        if (!IsVisible || !IsFocused) return;
        if (GetTokenType((char)e.Unicode) == TokenType.Invalid) return;
        if ((char)e.Unicode == '.' && HasDecimalAtCaretGroup()) return;
        base.OnTextInput(e);
    }

    public override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        if (!IsVisible || !IsFocused) return;
        base.OnKeyDown(e);
        if (e.Key == Keys.Enter)
        {
            EvaluateExpression();
        }
    }

    public override bool OnClickDown(MouseState mouse)
    {
        if (!IsVisible) return false;
        if (!WithinBounds(mouse) && IsFocused) EvaluateExpression();
        return base.OnClickDown(mouse);
    }

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