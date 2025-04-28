using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// calculator class
class Calculator {
    // stores result of calculation
    private Dictionary<string, double> variables;
    private Dictionary<string, string> functions;
    private string[] valid_operators;
    private string[] reserved_commands;

    public Calculator(Dictionary<string, double> vars, Dictionary<string, string> funcs) {
        // default calculation result 0
        this.variables = vars;
        this.functions = funcs;
        this.valid_operators = new string[] {"+", "-", "*", "/", "^", "%", "(", ")"};
        this.reserved_commands = new string[] {"q", "quit", "h", "help", "s", "save"};
    }

    // get precedence on an operator
    private int precedence(string c) {
        int p = 0;
        switch (c) {
            case "+":
            case "-":
                p = 1;
                break;
            case "*":
            case "/":
            case "_":
            case "%":
                p = 2;
                break;
            case "^":
                p = 3;
                break;
            case "(":
                p = 4;
                break;
            default:
                throw new ArgumentException($"Invalid Operator '{c}'");
        }
        return p;
    }

    // break operation string into separate numbers and ops
    // returns array of string in reverse polish notation
    // returns name of var to be stored (default 'r')
    private (string, List<string>) Parse(string expression) {
        // handle variable and function assignment 
        string[] equation = expression.Split("=");
        string var_name = "r";
        if (equation.Count() == 2) {
            string n = equation[0];
            n = n.Trim();
            
            // check if var name is valid
            if (this.reserved_commands.Contains(n)) {
                throw new ArgumentException("Variable Cannot Have a Reserved Command Name");
            }
            else if (Regex.IsMatch(n, @"^\$[a-zA-Z][\w]*$")) {
                // assign a function 
                if (equation[1].Contains("$")) {
                    throw new ArgumentException("Cannot Nest Functions");
                }
                this.functions[n] = equation[1].Trim();
            }
            else if (Regex.IsMatch(n, @"^[a-zA-Z][\w]*$")) {
                var_name = n;
            }
            else {
                throw new ArgumentException("Invalid Format for Variable/Function Name");
            }
            
            expression = equation[1];
        }
        else if (equation.Count() == 1) {
            expression = equation[0];
        }
        else {
            throw new ArgumentException("Invalid Number of '='");
        }

        // remove whitespace
        expression = expression.Trim();
        // split expression by operators
        List<string> exp = new List<string>(Regex.Split(expression, @"([\-+*/^%() ])"));

        if (exp.Count() == 0) {
            return (var_name, new List<string>());
        }

        // search for and expand functions
        for (int i = 0; i < exp.Count(); i++) {
            if (exp[i].StartsWith("$")) {
                if (this.functions.TryGetValue(exp[i], out var expanded)) {
                    exp.RemoveAt(i);
                    exp.InsertRange(i, Regex.Split(expanded, @"([\-+*/^%() ])"));
                    i += expanded.Count() - 1; // adjust index
                }
                else {
                    throw new ArgumentException($"Function '{exp[i]}' Does Not Exist");
                }
            }
        }

        // Parse expression
        Stack<string> ops = new Stack<string>();
        bool op_last = true; // was an operator encountered last?
        List<string> r = new List<string>();
        foreach (var t in exp) {
            if (t == "" || t == " ") {
                continue;
            }
            else if (t == "-" && op_last) {
                op_last = true;
                // handle minus as a unary op (negative nums)
                ops.Push("_");
            }
            else if (this.valid_operators.Contains(t)) {
                // is op valid?
                if (op_last && t != "(" && t != ")") {
                    throw new ArgumentException($"Missing Token Before '{t}'");
                }


                // add ops to output
                if (t == ")") {
                    op_last = false;
                    while (ops.Count > 0 && ops.Peek() != "(") {
                        r.Add(ops.Pop().ToString());
                    }
                    if (ops.Count == 0) {
                        throw new ArgumentException("Missing Opening Parentheses");
                    }
                    ops.Pop();
                }
                else {
                    op_last = true;
                    while (ops.Count > 0 && this.precedence(ops.Peek()) >= this.precedence(t) && ops.Peek() != "(") {
                        r.Add(ops.Pop().ToString());
                    }
                    ops.Push(t);
                }
            }
            else if (op_last) {
                op_last = false;

                // handle num/var and invalid input
                if (this.variables.ContainsKey(t)) {
                    r.Add(this.variables[t].ToString());
                }
                else if (double.TryParse(t, out _)) {
                    r.Add(t);
                }
                else {
                    throw new ArgumentException($"Invalid Token '{t}'");
                }
            }
            else {
                throw new ArgumentException($"Missing Operator Before '{t}'");
            }
        }

        // add last ops
        while (ops.Count > 0) {
            var op = ops.Pop();
            if (op == "(") {
                throw new ArgumentException("Missing Closing Parentheses");
            }
            else {
                r.Add(op.ToString());
            }
        }

        return (var_name, r);
    }

    // perform an operation on 2 numbers (ex. 2 + 2)
    private double Calculate(double x, double y, string op) {
        double r;
        switch (op) {
            case "+":
                r = y + x;
                break;
            case "-":
                r = y - x;
                break;
            case "*":
                r = y * x;
                break;
            case "/":
                if (x == 0) throw new ArgumentException("Cannot Divide by 0");
                r = y / x;
                break;
            case "_":
                r = -x;
                break;
            case "^":
                r = Math.Pow(y, x);
                break;
            case "%":
                r = y % x;
                break;
            default:
                throw new ArgumentException($"Invalid Operator Encountered '{op}'");
        }
        return r;
    }

    // calaculate expression
    public (string, double) Evaluate(string expression) {
        // parse expression
        var (var_name, exp) = this.Parse(expression);

        // return if no operators
        if (exp.Count == 1) {
            this.variables[var_name] = double.Parse(exp[0]);
            return (var_name, this.variables[var_name]);
        }
        else if (exp.Count == 1) {
            this.variables[var_name] = 0;
            return (var_name, 0);
        }

        // evaluate expression
        double r = 0;
        Stack<string> stack = new Stack<string>();
        foreach (string t in exp) {
            if (this.valid_operators.Contains(t)) {
                r = this.Calculate(double.Parse(stack.Pop()), double.Parse(stack.Pop()), t);
                stack.Push(r.ToString());
            }
            else if (t == "_") {
                r = this.Calculate(double.Parse(stack.Pop()), 0, t);
                stack.Push(r.ToString());
            }
            else if (double.TryParse(t, out _)) {
                stack.Push(t);
            }
            else {
                throw new ArgumentException($"Invalid Token '{t}'");
            }
        }

        // return results
        this.variables[var_name] = r;
        return (var_name, r);
    }
}

class Program {
    // display help menu
    static void DisplayMenu(int padding_size) {
        // display valid operators
        Console.WriteLine("Operators:"); // *, +, -, /, ^, %
        Console.Write("  +".PadRight(padding_size));
        Console.WriteLine("Add x and y (x + y)");
        Console.Write("  -".PadRight(padding_size));
        Console.WriteLine("Subtract y from x (x - y)");
        Console.Write("  *".PadRight(padding_size));
        Console.WriteLine("Multiply x by y (x * y)");
        Console.Write("  /".PadRight(padding_size));
        Console.WriteLine("Divide x by y (x / y)");
        Console.Write("  ^".PadRight(padding_size));
        Console.WriteLine("Get x to the power of y (x^y)");
        Console.Write("  %".PadRight(padding_size));
        Console.WriteLine("Get remainder of x divided by y (x % y)");
        Console.Write("  - (unary)".PadRight(padding_size));
        Console.WriteLine("Negate x (-x)");
        Console.Write("  ()".PadRight(padding_size));
        Console.WriteLine("Group expression ((x + y) * z)");
        // explain variable use
        Console.WriteLine("Variables:");
        Console.Write("  Use".PadRight(padding_size));
        Console.WriteLine("var_name = <expression>");
        Console.Write("  Valid Chars".PadRight(padding_size));
        Console.WriteLine("a-z, A-Z, 0-9, _ (first char must be a-z or A-Z)");
        Console.Write("  Default".PadRight(padding_size));
        Console.WriteLine("Result is saved to a varible name 'r' when no variable is specified");
        // explain function use
        Console.WriteLine("Functions:");
        Console.Write("  Use".PadRight(padding_size));
        Console.WriteLine("$fn = <expression>");
        Console.Write("  Valid Chars".PadRight(padding_size));
        Console.WriteLine("Must start with $. Must have 2+ chars (a-z, A-Z, 0-9, _)");
        // display commands
        Console.WriteLine("Commands:");
        Console.Write("  <ENTER>".PadRight(padding_size));
        Console.WriteLine("Execute command or equation");
        Console.Write("  quit (q)".PadRight(padding_size));
        Console.WriteLine("Exit program");
        Console.Write("  help (h)".PadRight(padding_size));
        Console.WriteLine("Display menu");
        Console.Write("  save (s)".PadRight(padding_size));
        Console.WriteLine("write variables and functions to file (coming soon)");
        Console.WriteLine();
    }

    static void Main() {
        // display welcome
        Console.WriteLine("The Calculator");
        Console.WriteLine("--------------");

        // run program until interupt
        Calculator calc = new Calculator(new Dictionary<string, double> {}, new Dictionary<string, string> {});
        while (true) {
            // get input
            Console.Write("-> ");
            string expression = Console.ReadLine() ?? "";

            // check for exit
            if (expression.ToLower() == "q" || expression.ToLower() == "quit") {
                break;
            }
            else if (expression.ToLower() == "h" || expression.ToLower() == "help") {
                Console.WriteLine();
                DisplayMenu(20);
                continue;
            }

            // perform calculations
            try {
                var (var_name, result) = calc.Evaluate(expression);

                // print result
                Console.WriteLine($"{var_name} = {result}");
            }
            catch (Exception e) {
                Console.WriteLine($"Error: {e.Message}");
            }
        }
    }
}
