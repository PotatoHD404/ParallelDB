using Antlr4.Runtime;

namespace Parser;

public class ParserErrorListener : IAntlrErrorListener<IToken>
{
    public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine,
        string msg, RecognitionException e)
    {
        throw new Exception(msg);
    }
}

public class LexerErrorListener : IAntlrErrorListener<int>
{
    public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine,
        string msg, RecognitionException e)
    {
        throw new Exception(msg);
    }
}
