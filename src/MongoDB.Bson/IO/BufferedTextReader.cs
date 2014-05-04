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

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents a wrapper around a TextReader to provide some buffering functionality.
    /// </summary>
    public class BufferedTextReader : TextReader
    {
        // private fields
        private readonly StringBuilder _buffer;
        private readonly bool _ownsReader;
        private int _position;
        private readonly TextReader _reader;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BufferedTextReader"/> class.
        /// </summary>
        /// <param name="json">The json.</param>
        public BufferedTextReader(string json)
        {
            if (json == null)
            {
                throw new ArgumentNullException("json");
            }
            _buffer = new StringBuilder(json);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferedTextReader" /> class.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="ownsReader">Whether the BufferedTextReader owns the wrapped TextReader.</param>
        public BufferedTextReader(TextReader reader, bool ownsReader = true)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            _buffer = new StringBuilder(256); // start out with a reasonable initial capacity
            _ownsReader = ownsReader;
            _reader = reader;
        }

        // public properties
        /// <summary>
        /// Gets or sets the current position.
        /// </summary>
        public int Position
        {
            get { return _position; }
            set
            {
                if (value < 0 || value > _buffer.Length)
                {
                    var message = string.Format("Invalid position: {0}.", value);
                    throw new ArgumentOutOfRangeException("value", message);
                }
                _position = value;
            }
        }

        // public methods
        /// <summary>
        /// Gets a buffered snippet of a maximum length (usually to include in an error message).
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="maxLength">The maximum length.</param>
        /// <returns>The snippet.</returns>
        public string GetBufferedSnippet(int start, int maxLength)
        {
            if (start < 0)
            {
                throw new ArgumentOutOfRangeException("start", "Start cannot be negative.");
            }
            if (maxLength < 0)
            {
                throw new ArgumentOutOfRangeException("maxLength", "MaxLength cannot be negative.");
            }
            if (start > _position)
            {
                throw new ArgumentOutOfRangeException("start", "Start is beyond current position.");
            }
            var availableCount = _position - start;
            var count = Math.Min(availableCount, maxLength);
            return _buffer.ToString(start, count);
        }

        /// <summary>
        /// Gets a buffered substring.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="count">The count.</param>
        /// <returns>The substring.</returns>
        public string GetBufferedSubstring(int start, int count)
        {
            if (start < 0)
            {
                throw new ArgumentOutOfRangeException("start", "Start cannot be negative.");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "Count cannot be negative.");
            }
            if (start > _position)
            {
                throw new ArgumentOutOfRangeException("start", "Start is beyond current position.");
            }
            if (start + count > _position)
            {
                throw new ArgumentOutOfRangeException("start", "End of substring is beyond current position.");
            }
            return _buffer.ToString(start, count);
        }

        /// <summary>
        /// Reads the next character without changing the state of the reader or the character source. Returns the next available character without actually reading it from the reader.
        /// </summary>
        /// <returns>
        /// An integer representing the next character to be read, or -1 if no more characters are available or the reader does not support seeking.
        /// </returns>
        public override int Peek()
        {
            ReadMoreIfAtEndOfBuffer();
            return _position >= _buffer.Length ? -1 : _buffer[_position];
        }

        /// <summary>
        /// Reads the next character from the text reader and advances the character position by one character.
        /// </summary>
        /// <returns>
        /// The next character from the text reader, or -1 if no more characters are available. The default implementation returns -1.
        /// </returns>
        public override int Read()
        {
            ReadMoreIfAtEndOfBuffer();
            return _position >= _buffer.Length ? -1 : _buffer[_position++];
        }

        /// <summary>
        /// Reads a specified maximum number of characters from the current reader and writes the data to a buffer, beginning at the specified index.
        /// </summary>
        /// <param name="buffer">When this method returns, contains the specified character array with the values between <paramref name="index" /> and (<paramref name="index" /> + <paramref name="count" /> - 1) replaced by the characters read from the current source.</param>
        /// <param name="index">The position in <paramref name="buffer" /> at which to begin writing.</param>
        /// <param name="count">The maximum number of characters to read. If the end of the reader is reached before the specified number of characters is read into the buffer, the method returns.</param>
        /// <returns>
        /// The number of characters that have been read. The number will be less than or equal to <paramref name="count" />, depending on whether the data is available within the reader. This method returns 0 (zero) if it is called when no more characters are left to read.
        /// </returns>
        public override int Read(char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", "Index cannot be negative.");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "Count cannot be negative.");
            }
            if (buffer.Length - index < count)
            {
                throw new ArgumentException("The buffer length minus index is less than count.");
            }
            
            var result = 0;
            while (count > 0)
            {
                ReadMoreIfAtEndOfBuffer();
                if (_position >= _buffer.Length)
                {
                    return result;
                }

                var availableCount = _buffer.Length - _position;
                var partialCount = Math.Min(count, availableCount);
                _buffer.CopyTo(_position, buffer, index, partialCount);
                _position += partialCount;
                index += partialCount;
                count -= partialCount;
                result += partialCount;
            }
            return result;
        }

        /// <summary>
        /// Reads all characters from the current position to the end of the text reader and returns them as one string.
        /// </summary>
        /// <returns>
        /// A string that contains all characters from the current position to the end of the text reader.
        /// </returns>
        public override string ReadToEnd()
        {
            if (_reader != null)
            {
                var remainingInput = _reader.ReadToEnd();
                _buffer.Append(remainingInput);
            }

            var result = _buffer.ToString(_position, _buffer.Length - _position);
            _position = _buffer.Length;
            return result;
        }

        /// <summary>
        /// Resets the buffer (clears everything up to the current position).
        /// </summary>
        public void ResetBuffer()
        {
            // only trim the buffer if enough space will be reclaimed to make it worthwhile
            var minimumTrimCount = 256; // TODO: make configurable?
            if (_position >= minimumTrimCount)
            {
                _buffer.Remove(0, _position);
                _position = 0;
            }
        }

        /// <summary>
        /// Unreads one character (moving the current Position back one position).
        /// </summary>
        /// <param name="c">The character.</param>
        public void Unread(int c)
        {
            if (_position == 0)
            {
                throw new InvalidOperationException("Unread called when nothing has been read.");
            }

            if (c == -1)
            {
                if (_position != _buffer.Length)
                {
                    throw new InvalidOperationException("Unread called with -1 when position is not at the end of the buffer.");
                }
            }
            else
            {
                if (_buffer[_position - 1] != c)
                {
                    throw new InvalidOperationException("Unread called with a character that does not match what is in the buffer.");
                }
                _position -= 1;
            }
        }

        // protected methods
        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.IO.TextReader" /> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (_reader != null && _ownsReader)
                    {
                        _reader.Dispose();
                    }
                }
                catch
                {
                    // ignore exceptions
                }
            }
            base.Dispose(disposing);
        }

        // private methods
        private void ReadMoreIfAtEndOfBuffer()
        {
            if (_position >= _buffer.Length)
            {
                if (_reader != null)
                {
                    var blockSize = 1024; // TODO: make configurable?
                    var block = new char[blockSize];
                    var actualCount = _reader.ReadBlock(block, 0, blockSize);

                    if (actualCount > 0)
                    {
                        _buffer.Append(block, 0, actualCount);
                    }
                }
            }
        }
    }
}
