# The-Calculator
Console based calculator build with C# (.NET). Supports about 10 operators, with variable and function capabilities. Expressions are parsed and converted to Reverse Polish Notation (RPN) using the Shunting Yard algorithm to streamline evaluation.

## Features
### Operators
-  `+`                 `Add x and y (x + y)`
-  `-`                 `Subtract y from x (x - y)`
-  `*`                 `Multiply x by y (x * y)`
-  `/`                 `Divide x by y (x / y)`
-  `^`                 `Get x to the power of y (x^y)`
-  `%`                 `Get remainder of x divided by y (x % y)`
-  `-` `(unary)         Negate x (-x)`
-  `()`                `Group expression ((x + y) * z)`

### Variables
-  `Use`               `var_name = <expression>`
-  `Valid Chars`       `a-z, A-Z, 0-9, _ (first char must be a-z or A-Z)`
-  `Default`           `Result is saved to a variable name 'r' when no variable is specified`

### Functions
-  `Assignment`        `$fn = <expression>`
-  `Usage`             `var = $fn + $fn`
-  `Valid Chars`       `Must start with $. Must have 2+ chars (a-z, A-Z, 0-9, _)`

-  Functions cannot be assigned to other functions
-  The result of a function can be assigned to any variable
-  Changing a variable that a function uses will change the result of the function when it is run again

### Commands
-  `<ENTER>`           `Execute command or equation`
-  `quit (q)`          `Exit program`
-  `help (h)`          `Display menu`
-  `save (s)`          `write variables and functions to file (coming soon)`

## Usage
1. from the project root directory, enter `dotnet run`
2. enter expressions or commands
```
-> help
-> 2 * 3
r = 6
-> x = 10
-> x_2 = x^2
x_2 = 20
-> $y = (-3*x_2) + x
r = -50
-> x = 5
-> y = $y
y = -55
```

## Installation
### Dependencies
- .Net

### Instructions
1. open the Terminal application
2. enter `git clone https://github.com/jonahcragun/The-Calculator`
3. enter `cd The-Calculator/calculator`
4. enter `dotnet run`

## Implementation
- c# with .Net 9
- Regex used for tokenization
- Stack used to parse and evaluate RPN expression (Shunting Yard Algorithm)
- Dictionaries used to store variable and function names
- Object oriented using a class structure to represent the calculator

## Future Updates
- Add root operator for negative numbers (ex. (-2)^(1/3))
- Add 'save' command to save variables between uses
- Add support for trig operations
