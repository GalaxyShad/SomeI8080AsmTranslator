﻿using I8080Translator;
using SomeAsmTranslator.Operands;

namespace SomeAsmTranslator.Source;

class Parser
{
    private readonly LabelTable _labelTable;
    private readonly Lexer _lexer;

    public Parser(Lexer lexer, LabelTable labelTable)
    {
        _labelTable = labelTable;
        _lexer = lexer;
        _currentToken = _lexer.Next();
    }

    public AssemblyStatement Next()
    {
        var label = ParseLabel();
        var instruction = ParseInstruction();
        var operands = ParseOperands();
        var comments = ParseComment();

        return new(label, instruction, operands, comments);
    }

    private Token _currentToken = Token.EOF;
      
    private Token TokenAt() => _currentToken;

    private Token TokenEat()
    {
        var prev = new Token 
        {
            Line = _currentToken.Line,
            TokenType = _currentToken.TokenType,
            Value = _currentToken.Value
        };

        _currentToken = _lexer.Next();

        while (_currentToken.TokenType == TokenType.NewLine)
            _currentToken = _lexer.Next();

        return prev;
    }

    private OperandList ParseOperands()
    {
        var operandList = new OperandList();

        var left = ParseOperand();
        if (left != null)
            operandList.Add(left);

        while (TokenAt().TokenType is TokenType.Comma)
        {
            TokenEat();
            left = ParseOperand();

            if (left != null)
                operandList.Add(left);
        }

        return operandList;
    }

    private IOperand? ParseOperand()
    {
        IOperand operand;

        switch (TokenAt().TokenType)
        {
            case TokenType.ProgramCounterData:
                operand = new OperandProgramCounter();
                break;
            case TokenType.Number:
                operand = new OperandNumeric(TokenAt().Value);
                break;
            case TokenType.String:
                throw new NotImplementedException($"Strings are not implemented yet -> {TokenAt().Value}");
            case TokenType.Symbol:
                operand = new OperandLabel(_labelTable.AddOrUpdateLabel(new Label { Name = TokenAt().Value }));
                break;
            default:
                return null;
        }

        TokenEat();

        return operand;
    }

    private Label? ParseLabel() =>
        TokenAt().TokenType is TokenType.Label 
            ? _labelTable.AddOrUpdateLabel(new Label { Name = TokenEat().Value }) 
            : null;

    private string? ParseInstruction() =>
        TokenAt().TokenType is TokenType.Instruction ? TokenEat().Value : null;

    private string? ParseComment() =>
        TokenAt().TokenType is TokenType.Comment ? TokenEat().Value : null;
}
