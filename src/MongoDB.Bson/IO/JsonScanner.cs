/* Copyright 2010-2014 MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.IO;
using System.Text;
using System.Xml;

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// A static class that represents a JSON scanner.
    /// </summary>
    public static class JsonScanner
    {
        // public static methods
        /// <summary>
        /// Gets the next JsonToken from the input.
        /// </summary>
        /// <param name="bufferedReader">The buffered reader.</param>
        /// <returns>The next token.</returns>
        public static JsonToken GetNextToken(BufferedTextReader bufferedReader)
        {
            // skip leading whitespace
            var c = bufferedReader.Read();
            while (c != -1 && char.IsWhiteSpace((char)c))
            {
                c = bufferedReader.Read();
            }
            if (c == -1)
            {
                return new JsonToken(JsonTokenType.EndOfFile, "<eof>");
            }

            // leading character determines token type
            switch (c)
            {
                case '{': return new JsonToken(JsonTokenType.BeginObject, "{");
                case '}': return new JsonToken(JsonTokenType.EndObject, "}");
                case '[': return new JsonToken(JsonTokenType.BeginArray, "[");
                case ']': return new JsonToken(JsonTokenType.EndArray, "]");
                case '(': return new JsonToken(JsonTokenType.LeftParen, "(");
                case ')': return new JsonToken(JsonTokenType.RightParen, ")");
                case ':': return new JsonToken(JsonTokenType.Colon, ":");
                case ',': return new JsonToken(JsonTokenType.Comma, ",");
                case '\'':
                case '"':
                    return GetStringToken(bufferedReader, (char)c);
                case '/': return GetRegularExpressionToken(bufferedReader);
                default:
                    if (c == '-' || char.IsDigit((char)c))
                    {
                        return GetNumberToken(bufferedReader, c);
                    }
                    else if (c == '$' || c == '_' || char.IsLetter((char)c))
                    {
                        return GetUnquotedStringToken(bufferedReader);
                    }
                    else
                    {
                        bufferedReader.Unread(c);
                        throw new FileFormatException(FormatMessage("Invalid JSON input", bufferedReader, bufferedReader.Position));
                    }
            }
        }

        // private methods
        private static string FormatMessage(string message, BufferedTextReader bufferedReader, int start)
        {
            var maxLength = 20;
            var snippet = bufferedReader.GetBufferedSnippet(start, maxLength);
            return string.Format("{0} '{1}'.", message, snippet);
        }

        private static JsonToken GetNumberToken(BufferedTextReader bufferedReader, int firstChar)
        {
            var c = firstChar;

            // leading digit or '-' has already been read
            var start = bufferedReader.Position - 1;
            NumberState state;
            switch (c)
            {
                case '-': state = NumberState.SawLeadingMinus; break;
                case '0': state = NumberState.SawLeadingZero; break;
                default: state = NumberState.SawIntegerDigits; break;
            }
            var type = JsonTokenType.Int64; // assume integer until proved otherwise

            while (true)
            {
                c = bufferedReader.Read();
                switch (state)
                {
                    case NumberState.SawLeadingMinus:
                        switch (c)
                        {
                            case '0':
                                state = NumberState.SawLeadingZero;
                                break;
                            case 'I':
                                state = NumberState.SawMinusI;
                                break;
                            default:
                                if (char.IsDigit((char)c))
                                {
                                    state = NumberState.SawIntegerDigits;
                                }
                                else
                                {
                                    state = NumberState.Invalid;
                                }
                                break;
                        }
                        break;
                    case NumberState.SawLeadingZero:
                        switch (c)
                        {
                            case '.':
                                state = NumberState.SawDecimalPoint;
                                break;
                            case 'e':
                            case 'E':
                                state = NumberState.SawExponentLetter;
                                break;
                            case ',':
                            case '}':
                            case ']':
                            case ')':
                            case -1:
                                state = NumberState.Done;
                                break;
                            default:
                                if (char.IsWhiteSpace((char)c))
                                {
                                    state = NumberState.Done;
                                }
                                else
                                {
                                    state = NumberState.Invalid;
                                }
                                break;
                        }
                        break;
                    case NumberState.SawIntegerDigits:
                        switch (c)
                        {
                            case '.':
                                state = NumberState.SawDecimalPoint;
                                break;
                            case 'e':
                            case 'E':
                                state = NumberState.SawExponentLetter;
                                break;
                            case ',':
                            case '}':
                            case ']':
                            case ')':
                            case -1:
                                state = NumberState.Done;
                                break;
                            default:
                                if (char.IsDigit((char)c))
                                {
                                    state = NumberState.SawIntegerDigits;
                                }
                                else if (char.IsWhiteSpace((char)c))
                                {
                                    state = NumberState.Done;
                                }
                                else
                                {
                                    state = NumberState.Invalid;
                                }
                                break;
                        }
                        break;
                    case NumberState.SawDecimalPoint:
                        type = JsonTokenType.Double;
                        if (char.IsDigit((char)c))
                        {
                            state = NumberState.SawFractionDigits;
                        }
                        else
                        {
                            state = NumberState.Invalid;
                        }
                        break;
                    case NumberState.SawFractionDigits:
                        switch (c)
                        {
                            case 'e':
                            case 'E':
                                state = NumberState.SawExponentLetter;
                                break;
                            case ',':
                            case '}':
                            case ']':
                            case ')':
                            case -1:
                                state = NumberState.Done;
                                break;
                            default:
                                if (char.IsDigit((char)c))
                                {
                                    state = NumberState.SawFractionDigits;
                                }
                                else if (char.IsWhiteSpace((char)c))
                                {
                                    state = NumberState.Done;
                                }
                                else
                                {
                                    state = NumberState.Invalid;
                                }
                                break;
                        }
                        break;
                    case NumberState.SawExponentLetter:
                        type = JsonTokenType.Double;
                        switch (c)
                        {
                            case '+':
                            case '-':
                                state = NumberState.SawExponentSign;
                                break;
                            default:
                                if (char.IsDigit((char)c))
                                {
                                    state = NumberState.SawExponentDigits;
                                }
                                else
                                {
                                    state = NumberState.Invalid;
                                }
                                break;
                        }
                        break;
                    case NumberState.SawExponentSign:
                        if (char.IsDigit((char)c))
                        {
                            state = NumberState.SawExponentDigits;
                        }
                        else
                        {
                            state = NumberState.Invalid;
                        }
                        break;
                    case NumberState.SawExponentDigits:
                        switch (c)
                        {
                            case ',':
                            case '}':
                            case ']':
                            case ')':
                            case -1:
                                state = NumberState.Done;
                                break;
                            default:
                                if (char.IsDigit((char)c))
                                {
                                    state = NumberState.SawExponentDigits;
                                }
                                else if (char.IsWhiteSpace((char)c))
                                {
                                    state = NumberState.Done;
                                }
                                else
                                {
                                    state = NumberState.Invalid;
                                }
                                break;
                        }
                        break;
                    case NumberState.SawMinusI:
                        var sawMinusInfinity = true;
                        var nfinity = new char[] { 'n', 'f', 'i', 'n', 'i', 't', 'y' };
                        for (var i = 0; i < nfinity.Length; i++)
                        {
                            if (c != nfinity[i])
                            {
                                sawMinusInfinity = false;
                                break;
                            }
                            c = bufferedReader.Read();
                        }
                        if (sawMinusInfinity)
                        {
                            type = JsonTokenType.Double;
                            switch (c)
                            {
                                case ',':
                                case '}':
                                case ']':
                                case ')':
                                case -1:
                                    state = NumberState.Done;
                                    break;
                                default:
                                    if (char.IsWhiteSpace((char)c))
                                    {
                                        state = NumberState.Done;
                                    }
                                    else
                                    {
                                        state = NumberState.Invalid;
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            state = NumberState.Invalid;
                        }
                        break;
                }

                switch (state)
                {
                    case NumberState.Done:
                        bufferedReader.Unread(c);
                        var lexeme = bufferedReader.GetBufferedSubstring(start, bufferedReader.Position - start);
                        if (type == JsonTokenType.Double)
                        {
                            var value = XmlConvert.ToDouble(lexeme);
                            return new DoubleJsonToken(lexeme, value);
                        }
                        else
                        {
                            var value = XmlConvert.ToInt64(lexeme);
                            if (value < int.MinValue || value > int.MaxValue)
                            {
                                return new Int64JsonToken(lexeme, value);
                            }
                            else
                            {
                                return new Int32JsonToken(lexeme, (int)value);
                            }
                        }
                    case NumberState.Invalid:
                        throw new FileFormatException(FormatMessage("Invalid JSON number", bufferedReader, start));
                }
            }
        }

        private static JsonToken GetRegularExpressionToken(BufferedTextReader bufferedReader)
        {
            // opening slash has already been read
            var start = bufferedReader.Position - 1;
            var state = RegularExpressionState.InPattern;
            while (true)
            {
                var c = bufferedReader.Read();
                switch (state)
                {
                    case RegularExpressionState.InPattern:
                        switch (c)
                        {
                            case '/': state = RegularExpressionState.InOptions; break;
                            case '\\': state = RegularExpressionState.InEscapeSequence; break;
                            default: state = RegularExpressionState.InPattern; break;
                        }
                        break;
                    case RegularExpressionState.InEscapeSequence:
                        state = RegularExpressionState.InPattern;
                        break;
                    case RegularExpressionState.InOptions:
                        switch (c)
                        {
                            case 'i':
                            case 'm':
                            case 'x':
                            case 's':
                                state = RegularExpressionState.InOptions;
                                break;
                            case ',':
                            case '}':
                            case ']':
                            case ')':
                            case -1:
                                state = RegularExpressionState.Done;
                                break;
                            default:
                                if (char.IsWhiteSpace((char)c))
                                {
                                    state = RegularExpressionState.Done;
                                }
                                else
                                {
                                    state = RegularExpressionState.Invalid;
                                }
                                break;
                        }
                        break;
                }

                switch (state)
                {
                    case RegularExpressionState.Done:
                        bufferedReader.Unread(c);
                        var lexeme = bufferedReader.GetBufferedSubstring(start, bufferedReader.Position - start);
                        var regex = new BsonRegularExpression(lexeme);
                        return new RegularExpressionJsonToken(lexeme, regex);
                    case RegularExpressionState.Invalid:
                        throw new FileFormatException(FormatMessage("Invalid JSON regular expression", bufferedReader, start));
                }
            }
        }

        private static JsonToken GetStringToken(BufferedTextReader bufferedReader, char quoteCharacter)
        {
            // opening quote has already been read
            var start = bufferedReader.Position - 1;
            var sb = new StringBuilder();
            while (true)
            {
                var c = bufferedReader.Read();
                switch (c)
                {
                    case '\\':
                        c = bufferedReader.Read();
                        switch (c)
                        {
                            case '\'': sb.Append('\''); break;
                            case '"': sb.Append('"'); break;
                            case '\\': sb.Append('\\'); break;
                            case '/': sb.Append('/'); break;
                            case 'b': sb.Append('\b'); break;
                            case 'f': sb.Append('\f'); break;
                            case 'n': sb.Append('\n'); break;
                            case 'r': sb.Append('\r'); break;
                            case 't': sb.Append('\t'); break;
                            case 'u':
                                var u1 = bufferedReader.Read();
                                var u2 = bufferedReader.Read();
                                var u3 = bufferedReader.Read();
                                var u4 = bufferedReader.Read();
                                if (u4 != -1)
                                {
                                    var hex = new string(new char[] { (char)u1, (char)u2, (char)u3, (char)u4 });
                                    var n = Convert.ToInt32(hex, 16);
                                    sb.Append((char)n);
                                }
                                break;
                            default:
                                if (c != -1)
                                {
                                    var message = string.Format("Invalid escape sequence in JSON string '\\{0}'.", (char)c);
                                    throw new FileFormatException(message);
                                }
                                break;
                        }
                        break;
                    default:
                        if (c == quoteCharacter)
                        {
                            var lexeme = bufferedReader.GetBufferedSubstring(start, bufferedReader.Position - start);
                            return new StringJsonToken(JsonTokenType.String, lexeme, sb.ToString());
                        }
                        if (c != -1)
                        {
                            sb.Append((char)c);
                        }
                        break;
                }
                if (c == -1)
                {
                    throw new FileFormatException(FormatMessage("End of file in JSON string.", bufferedReader, start));
                }
            }
        }

        private static JsonToken GetUnquotedStringToken(BufferedTextReader bufferedReader)
        {
            // opening letter or $ has already been read
            var start = bufferedReader.Position - 1;
            var c = bufferedReader.Read();
            while (c == '$' || c == '_' || char.IsLetterOrDigit((char)c))
            {
                c = bufferedReader.Read();
            }
            bufferedReader.Unread(c);
            var lexeme = bufferedReader.GetBufferedSubstring(start, bufferedReader.Position - start);
            return new StringJsonToken(JsonTokenType.UnquotedString, lexeme, lexeme);
        }

        // nested types
        private enum NumberState
        {
            SawLeadingMinus,
            SawLeadingZero,
            SawIntegerDigits,
            SawDecimalPoint,
            SawFractionDigits,
            SawExponentLetter,
            SawExponentSign,
            SawExponentDigits,
            SawMinusI,
            Done,
            Invalid
        }

        private enum RegularExpressionState
        {
            InPattern,
            InEscapeSequence,
            InOptions,
            Done,
            Invalid
        }
    }
}
